# AWS EKS Deployment

Additional AWS EKS infrastructure, ingress, Terraform, and Kubernetes deployment details for the ImageGallery platform.

## Infrastructure Provisioning

AWS cloud infrastructure is provisioned and managed using Terraform Infrastructure as Code (IaC).

Terraform resources include:

- Amazon EKS cluster
- EC2-based worker node groups
- VPC networking
- Security groups
- IAM roles and policies
- ALB ingress integration

Terraform configuration is located under:

```text
infra/terraform/aws/
```

## Kubernetes Deployment

AWS-specific Kubernetes manifests are located under:

```text
k8s/aws/
```

Deployment includes:

- MVC client deployment
- ImageGallery API deployment
- Duende IdentityServer deployment
- Kubernetes Services
- ConfigMaps
- Kubernetes Secrets
- ALB ingress configuration

## HTTPS & Ingress

The platform uses AWS Load Balancer Controller with Kubernetes ingress resources to provision an AWS Application Load Balancer dynamically.

HTTPS/TLS termination is handled by the AWS Application Load Balancer using AWS Certificate Manager (ACM) certificates. Traffic is then routed into the Kubernetes cluster through ingress resources managed by the AWS Load Balancer Controller.

Ingress routing supports:

- HTTPS termination
- Path-based routing
- Public internet access
- OIDC redirect compatibility