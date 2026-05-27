# Foundation Infrastructure

Terraform root module for persistent shared infrastructure.

This layer contains long-lived foundational infrastructure resources shared by application runtime environments.

## Resources Managed

- VPC networking
- Public and private subnets
- NAT gateways
- Route tables
- Shared security groups
- Amazon RDS SQL Server
- Amazon ECR repositories
- Shared IAM resources
- ACM certificates (planned)
- Remote Terraform backend resources (planned)

## Runtime Dependency

The Runtime stack consumes outputs from Foundation using Terraform remote state.

Foundation provides:

- VPC
- Public subnets
- Private subnets
- Shared application security groups
- Amazon RDS SQL Server

Runtime consumes these outputs to provision:

- Amazon EKS
- Managed node groups
- AWS Load Balancer Controller
- Kubernetes workloads

## Lifecycle

Foundation resources are intended to persist independently from disposable runtime infrastructure such as Kubernetes clusters, node groups, ingress controllers, and application workloads.

This architecture has been validated through a full Runtime destroy and rebuild cycle.

Destroying Runtime does not impact:

- VPC networking
- Subnets
- Route tables
- NAT Gateway
- Shared security groups
- Amazon RDS SQL Server

This separation enables safer infrastructure lifecycle management, reduced operational blast radius, and independent runtime destroy/rebuild workflows without impacting persistent application data or shared networking resources.