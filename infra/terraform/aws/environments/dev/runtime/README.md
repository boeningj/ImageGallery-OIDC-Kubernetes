# Runtime Infrastructure

Terraform root module for disposable runtime infrastructure.

This layer contains Kubernetes and application runtime resources that can be independently destroyed and recreated without impacting persistent foundation infrastructure.

## Resources Managed

- Amazon EKS cluster
- EKS managed node groups
- AWS Load Balancer Controller
- Kubernetes ingress resources
- Helm deployments
- Kubernetes workloads and services

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
- Kubernetes application workloads

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

### Step 3 - Recreate Application Namespace

The Kubernetes namespace is not currently managed by Terraform and must be recreated after rebuilding the cluster.

```powershell
kubectl create namespace imagegallery
```

### Step 4 - Redeploy Application Workloads

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

### Step 5 - Verify Runtime Health

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

### Step 6 - Validate Application

Browse to:

```text
https://imagegallery.boeninglabs.net
```

The application should be fully functional and connected to the existing Foundation-managed SQL Server database.

## Runtime Destroy Procedure

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

This does NOT remove:

- VPC
- Subnets
- NAT Gateway
- Route tables
- Shared security groups
- Amazon RDS SQL Server

These resources remain managed by the Foundation stack.