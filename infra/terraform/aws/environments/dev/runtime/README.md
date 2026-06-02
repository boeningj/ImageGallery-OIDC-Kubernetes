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

This remote backend architecture is a prerequisite for GitHub Actions and future infrastructure automation because Terraform state is no longer tied to a specific workstation.

## Runtime Rebuild Procedure

The following procedure was successfully validated by destroying and recreating the entire runtime environment while preserving Foundation infrastructure and application data.

### Step 1 - Recreate Runtime Infrastructure

From this directory:

```powershell
terraform apply
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

The application should be fully functional and connected to the existing Foundation-managed SQL Server database.

## Runtime Destroy Procedure

For automated runtime destruction through GitHub Actions, see Runtime Shutdown Automation below.

Destroy only runtime resources:

```powershell
terraform destroy
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

## Operational Notes

### Runtime Shutdown Automation

The Runtime layer can be destroyed either manually using Terraform or through the GitHub Actions **Runtime Shutdown** workflow.

### GitHub Actions Runtime Shutdown

Workflow:

```text
.github/workflows/runtime-shutdown.yml
```

The workflow performs the following sequence:

1. Configure AWS credentials using GitHub OIDC.
2. Configure kubectl access to the EKS cluster.
3. Delete the application ingress.
4. Wait for ingress deletion to complete.
5. Execute `terraform destroy`.

### Kubernetes Finalizer Considerations

During early testing of Runtime Shutdown automation, Terraform destroy occasionally failed with:

```text
Error: context deadline exceeded
```

Investigation revealed a Kubernetes resource lifecycle dependency involving the AWS Load Balancer Controller.

The application ingress creates AWS Application Load Balancer resources and associated `TargetGroupBinding` objects. These resources contain Kubernetes finalizers managed by the AWS Load Balancer Controller:

```text
ingress.k8s.aws/resources
elbv2.k8s.aws/resources
```

When Terraform destroyed Runtime infrastructure, the AWS Load Balancer Controller could be removed before it completed cleanup of ingress and TargetGroupBinding resources.

This resulted in:

```text
Ingress stuck in Terminating state
TargetGroupBindings stuck in Terminating state
Namespace stuck in Terminating state
Terraform destroy timeout
```

Because the controller responsible for removing the finalizers had already been deleted, Kubernetes could not complete namespace deletion.

### Solution

The Runtime Shutdown workflow now explicitly deletes the ingress and waits for deletion to complete before Terraform destroy begins.

This ensures:

```text
Ingress deleted
↓
ALB resources cleaned up
↓
TargetGroupBindings deleted
↓
Finalizers removed
↓
Terraform destroy executes
```

This approach was successfully validated on 2026-05-30 and eliminated the namespace termination race condition that previously caused Runtime Shutdown failures.

### Recovery Procedure

If Runtime Shutdown is interrupted during destruction and the `imagegallery` namespace becomes stuck in the `Terminating` state:

1. Recreate Runtime infrastructure using `terraform apply`.
2. Allow the AWS Load Balancer Controller to reconcile and remove remaining finalizers.
3. Run `terraform plan`.
4. Recreate any missing Kubernetes resources reported by Terraform.
5. Redeploy application workloads.
6. Re-run the Runtime Shutdown workflow.

This recovery procedure was successfully validated during troubleshooting of the finalizer issue.