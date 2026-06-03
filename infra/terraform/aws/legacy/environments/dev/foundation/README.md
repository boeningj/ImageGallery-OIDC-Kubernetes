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
- Amazon S3 Terraform state storage

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

## Terraform State

Foundation uses an Amazon S3 backend for Terraform state storage.

Backend configuration:

```text
Bucket: imagegallery-terraform-state
Key: dev/foundation/terraform.tfstate
Region: us-east-2
```

The state bucket is managed by the Foundation stack because it is a long-lived shared infrastructure resource shared by all environments.

State protection features:

- S3 Versioning enabled
- Server-side encryption enabled
- Public access blocked

Runtime consumes Foundation outputs through Terraform remote state stored in this backend.

This remote backend architecture enables Terraform operations to be executed from developer workstations, GitHub Actions, and future CI/CD automation environments without dependency on local state files.

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