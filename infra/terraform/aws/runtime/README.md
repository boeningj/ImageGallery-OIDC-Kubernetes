# Runtime Infrastructure

Terraform root module for disposable runtime infrastructure.

This layer contains Kubernetes and application runtime resources that can be independently destroyed and recreated without impacting persistent foundation infrastructure.

## Resources Managed

- Amazon EKS cluster
- EKS managed node groups
- AWS Load Balancer Controller
- Kubernetes namespace (`imagegallery`)
- Helm deployments
- Runtime IAM resources

The runtime layer has been validated as independently destroyable and rebuildable without impacting persistent foundation infrastructure such as VPC networking, RDS, shared security groups, or container registries.

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

## Foundation Dependency

This runtime stack consumes outputs from the Foundation stack using Terraform remote state.

Foundation provides:

- VPC
- Public subnets
- Private subnets
- Shared application security groups
- Amazon RDS SQL Server

Runtime provides:

- Amazon EKS
- Managed node groups
- AWS Load Balancer Controller
- Kubernetes application runtime environment

## Terraform State

Runtime uses an Amazon S3 backend for Terraform state storage.

Backend configuration:

```text
Bucket: imagegallery-terraform-state
Key: dev/runtime/terraform.tfstate
Region: us-east-2
```

Runtime consumes Foundation outputs through Terraform remote state stored in the same S3 backend.

Foundation remote state configuration:

```text
Bucket: imagegallery-terraform-state
Key: dev/foundation/terraform.tfstate
Region: us-east-2
```

This architecture enables Terraform operations to be executed from:

- Developer workstations
- GitHub Actions
- CI/CD pipelines
- Future automation environments

without dependency on local Terraform state files.

State protection features:

- S3 Versioning enabled
- Server-side encryption enabled
- Public access blocked

## Terraform Initialization

Initialize Runtime using the environment-specific backend configuration:

```powershell
terraform init -backend-config="../env/dev.runtime.backend.tfvars"
```

Validate:

```powershell
terraform validate
```

## Terraform Planning

Generate a plan using environment-specific variables:

```powershell
terraform plan -var-file="../env/dev.runtime.tfvars"
```

## Terraform Apply

Apply Runtime infrastructure:

```powershell
terraform apply -var-file="../env/dev.runtime.tfvars"
```

## Runtime Rebuild Procedure

The following procedure was successfully validated by destroying and recreating the entire runtime environment while preserving Foundation infrastructure and application data.

### Automated DNS Reconciliation

Runtime ingress DNS is automatically managed using Kubernetes ExternalDNS with Cloudflare.

ExternalDNS monitors Kubernetes ingress resources and automatically reconciles Cloudflare DNS records whenever AWS Application Load Balancer hostnames change.

This eliminates the need for manual DNS updates after Runtime rebuild operations.

### ExternalDNS Workflow

```text
Runtime Startup
    ↓
Ingress Created
    ↓
AWS Load Balancer Controller Creates ALB
    ↓
ExternalDNS Detects Ingress Hostname
    ↓
Cloudflare DNS Updated Automatically
    ↓
Application Becomes Publicly Reachable
```

### Ingress Annotation

Ingress resources intended for public DNS management include:

```yaml
external-dns.alpha.kubernetes.io/hostname: imagegallery.boeninglabs.net
```

### DNS Ownership

ExternalDNS uses TXT ownership records to safely manage Cloudflare DNS reconciliation.

Example ownership record:

```text
cname-imagegallery.boeninglabs.net
```

This ownership model prevents accidental modification of DNS records managed outside Kubernetes.

### Step 1 - Recreate Runtime Infrastructure

From the Runtime directory:

```powershell
terraform apply -var-file="../env/dev.runtime.tfvars"
```

This recreates:

- EKS cluster
- Managed node groups
- IAM resources
- AWS Load Balancer Controller
- Kubernetes namespace (`imagegallery`)
- Supporting runtime infrastructure

Typical rebuild time is approximately 10-15 minutes.

### Step 2 - Refresh Local Kubeconfig

After rebuilding Runtime, update the local kubeconfig to point to the newly created EKS cluster:

```powershell
aws eks update-kubeconfig --region us-east-2 --name imagegallery-cluster --alias imagegallery-eks
```

Verify:

```powershell
kubectl config get-contexts
```

Expected current context:

```text
imagegallery-eks
```

This updates the existing `imagegallery-eks` context and avoids creation of AWS-generated ARN-based context names.

### Step 3 - Redeploy Application Workloads

The Kubernetes namespace (`imagegallery`) is automatically recreated by Terraform as part of the Runtime infrastructure deployment.

From the repository root:

```powershell
kubectl apply -f k8s\aws
```

This recreates:

- Deployments
- Services
- ConfigMaps
- Secrets
- Ingress resources

### Step 4 - Verify Runtime Health

Verify cluster connectivity:

```powershell
kubectl get nodes
```

Expected:

```text
STATUS: Ready
```

Verify application workloads:

```powershell
kubectl get pods -n imagegallery
```

Expected:

```text
dep-imagegallery-api      Running
dep-imagegallery-client   Running
dep-imagegallery-idp      Running
```

Verify ingress:

```powershell
kubectl get ingress -n imagegallery
```

Expected:

```text
imagegallery-ingress
```

### Step 5 - Validate Application

Browse to:

```text
https://imagegallery.boeninglabs.net
```

The application should be fully functional and publicly reachable through the automatically reconciled Cloudflare DNS record.

## Runtime Destroy Procedure

Destroy only runtime resources:

```powershell
terraform destroy `
  -var-file="../env/dev.runtime.tfvars"
```

This removes:

- EKS cluster
- Managed node groups
- Worker nodes
- AWS Load Balancer Controller
- Runtime IAM resources
- Runtime security groups
- Kubernetes namespace (`imagegallery`)

This does NOT remove:

- VPC
- Subnets
- NAT Gateway
- Route tables
- Shared security groups
- Amazon RDS SQL Server

These resources remain managed by the Foundation stack.