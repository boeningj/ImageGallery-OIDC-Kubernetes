module "vpc" {
  source  = "terraform-aws-modules/vpc/aws"
  version = "5.1.2"

  name = "imagegallery-vpc"
  cidr = "10.0.0.0/16"

  azs             = ["us-east-2a", "us-east-2b"]
  private_subnets = ["10.0.1.0/24", "10.0.2.0/24"]
  public_subnets  = ["10.0.101.0/24", "10.0.102.0/24"]

  enable_nat_gateway = true
  single_nat_gateway = true

  tags = {
    Environment = "dev"
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
  name        = "imagegallery-app-runtime-sg"
  description = "Shared security group for application runtime workloads"
  vpc_id      = module.vpc.vpc_id

  tags = {
    Name = "imagegallery-app-runtime-sg"
  }
}