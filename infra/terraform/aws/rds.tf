# ============================================================
# Database Password Variable
# ============================================================
#
# The actual password value should be stored locally inside:
# terraform.tfvars
#
# Example:
# db_password = "your-password-here"
#
# The terraform.tfvars file is gitignored so secrets are NOT
# committed to source control.
# ============================================================
variable "db_password" {
  description = "SQL Server master password"
  type        = string
  sensitive   = true
}

# ============================================================
# RDS DB Subnet Group
# ============================================================
#
# RDS instances must be associated with a DB subnet group.
#
# This subnet group places SQL Server into the private subnets
# of the ImageGallery VPC so the database is NOT publicly
# accessible from the internet.
#
# The EKS worker nodes and application pods will communicate
# with SQL Server privately inside the VPC over port 1433.
#
# Using module.vpc.private_subnets ensures Terraform
# automatically uses the private subnets created by the VPC
# module instead of hardcoded subnet IDs.
# ============================================================
resource "aws_db_subnet_group" "imagegallery_db_subnet_group" {
  name       = "imagegallery-db-subnet-group"
  subnet_ids = module.vpc.private_subnets

  tags = {
    Name = "imagegallery-db-subnet-group"
  }
}

# ============================================================
# RDS Security Group
# ============================================================
#
# This security group controls inbound access to the
# SQL Server RDS instance.
#
# Only EKS worker nodes are allowed to communicate with
# SQL Server over TCP 1433.
#
# The database is NOT publicly accessible.
# ============================================================
resource "aws_security_group" "imagegallery_rds_sg" {
  name        = "imagegallery-rds-sg"
  description = "Security group for ImageGallery SQL Server RDS"
  vpc_id      = module.vpc.vpc_id

  ingress {
    description     = "SQL Server access from EKS worker nodes"
    from_port       = 1433
    to_port         = 1433
    protocol        = "tcp"
    security_groups = [module.eks.node_security_group_id]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "imagegallery-rds-sg"
  }
}

# ============================================================
# SQL Server RDS Instance
# ============================================================
#
# This provisions the managed SQL Server database used by
# the ImageGallery API.
#
# Key design decisions:
#
# - SQL Server Express Edition
#   Lowest-cost managed SQL Server option.
#
# - db.t3.micro
#   Smallest practical compute size for this low-traffic
#   portfolio application.
#
# - Single AZ
#   Minimizes cost for dev/test usage.
#
# - Private networking only
#   Database is NOT publicly accessible.
#
# - 20 GiB storage
#   More than sufficient because images are stored in S3,
#   not inside SQL Server.
#
# - Storage autoscaling disabled
#   Keeps monthly costs predictable.
#
# This RDS instance will persist independently from the
# EKS cluster lifecycle, allowing Kubernetes infrastructure
# to be destroyed/recreated without losing application data.
# ============================================================
resource "aws_db_instance" "imagegallery_sqlserver" {

  identifier = "imagegallery-db"

  engine         = "sqlserver-ex"
  engine_version = "15.00.4460.4.v1"

  instance_class = "db.t3.micro"

  allocated_storage     = 20
  max_allocated_storage = 20
  storage_type          = "gp3"

  username = "imagegalleryadm"
  password = var.db_password

  port = 1433

  publicly_accessible = false

  multi_az = false

  storage_encrypted = true

  auto_minor_version_upgrade = true

  backup_retention_period = 1
  skip_final_snapshot     = true
  deletion_protection     = false

  performance_insights_enabled = false

  db_subnet_group_name = aws_db_subnet_group.imagegallery_db_subnet_group.name

  vpc_security_group_ids = [
    aws_security_group.imagegallery_rds_sg.id
  ]

  tags = {
    Name = "imagegallery-db"
  }
}