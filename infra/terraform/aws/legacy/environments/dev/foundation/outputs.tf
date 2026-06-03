output "vpc_id" {
  description = "VPC ID for shared application infrastructure"
  value       = module.vpc.vpc_id
}

output "private_subnet_ids" {
  description = "Private subnet IDs for runtime workloads"
  value       = module.vpc.private_subnets
}

output "public_subnet_ids" {
  description = "Public subnet IDs for ingress and ALBs"
  value       = module.vpc.public_subnets
}

output "app_runtime_sg_id" {
  description = "Shared application runtime security group ID"
  value       = aws_security_group.app_runtime_sg.id
}