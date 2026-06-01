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
- Shared IAM resources
- ACM certificates (planned)
- Amazon S3 Terraform state storage

## Directory Structure

```text
infra/terraform/aws
├── env
│   ├── dev.foundation.backend.tfvars
│   ├── dev.foundation.tfvars
│   ├── dev.runtime.backend.tfvars
│   ├── dev.runtime.tfvars
│   └── dev.secrets.tfvars
│
├── foundation
│   ├── provider.tf
│   ├── variables.tf
│   ├── vpc.tf
│   ├── rds.tf
│   ├── s3.tf
│   └── outputs.tf
│
└── runtime
    ├── eks.tf
    ├── alb-controller.tf
    ├── namespace.tf
    └── outputs.tf
```

Foundation and Runtime are separate Terraform root modules that maintain independent state files while sharing environment-specific configuration from the `env` directory.

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

## Terraform Initialization

Initialize Foundation using the environment-specific backend configuration:

```powershell
terraform init -backend-config="../env/dev.foundation.backend.tfvars"
```

Validate:

```powershell
terraform validate
```

## Terraform Planning

Generate a plan using environment and secret variables:

```powershell
terraform plan -var-file="../env/dev.foundation.tfvars" -var-file="../env/dev.secrets.tfvars"
```

## Terraform Apply

Apply Foundation infrastructure:

```powershell
terraform apply -var-file="../env/dev.foundation.tfvars" -var-file="../env/dev.secrets.tfvars"
```

## Environment Configuration

Environment-specific values are stored in the `env` directory.

Examples:

```text
dev.foundation.tfvars
qa.foundation.tfvars
prod.foundation.tfvars
```

Sensitive values are stored separately and excluded from source control:

```text
dev.secrets.tfvars
qa.secrets.tfvars
prod.secrets.tfvars
```

Secret variable files are ignored by Git and should never be committed.

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