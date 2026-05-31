module "vpc" {
  source  = "terraform-aws-modules/vpc/aws"
  version = "5.1.2"

  name = "${var.project_name}-vpc"
  cidr = var.aws_vpc_cidr

  azs             = var.aws_vpc_azs
  private_subnets = var.aws_vpc_private_subnets
  public_subnets  = var.aws_vpc_public_subnets

  enable_nat_gateway = true
  single_nat_gateway = true

  tags = {
    Environment = var.environment
  }
}

# ============================================================
# Shared Application Runtime Security Group
# ============================================================
# This security group represents trusted application runtime
# workloads (such as EKS nodes or Kubernetes pods) that are
# allowed to communicate with shared infrastructure services
# such as RDS.
#
# The goal is to decouple foundation infrastructure from
# runtime-specific resources like the EKS node security group.
# ============================================================
resource "aws_security_group" "app_runtime_sg" {
  name        = "${var.project_name}-app-runtime-sg"
  description = "Shared security group for application runtime workloads"
  vpc_id      = module.vpc.vpc_id

  tags = {
    Name = "${var.project_name}-app-runtime-sg"
  }
}