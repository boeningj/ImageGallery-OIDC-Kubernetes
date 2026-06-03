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
- Amazon RDS for SQL Server
- RDS subnet groups
- Database security groups

Terraform configuration is located under:

```text
infra/terraform/aws/
```

## Infrastructure Automation

The AWS Runtime environment can be independently started and stopped using GitHub Actions workflows.

Automation is implemented using:

- GitHub Actions
- GitHub OIDC Federation
- Terraform
- Amazon EKS
- Kubernetes

The platform uses OpenID Connect (OIDC) federation between GitHub Actions and AWS IAM, eliminating the need for long-lived AWS access keys in CI/CD workflows.

Runtime automation includes:

- Automated Terraform Runtime deployment
- Automated Terraform Runtime destruction
- Automated Kubernetes cluster configuration
- Automated Kubernetes secret creation from GitHub repository secrets
- Automated application deployment to Amazon EKS

The Runtime Startup and Runtime Shutdown workflows were validated through multiple full destroy/rebuild cycles while preserving Foundation-managed infrastructure and application data.

Foundation infrastructure remains persistent and includes:

- VPC networking
- Security groups
- Amazon RDS SQL Server
- Terraform remote state storage

This separation enables the Kubernetes runtime environment to be recreated on demand without impacting persistent application data or shared infrastructure resources.

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
- Managed relational persistence using Amazon RDS for SQL Server

## HTTPS & Ingress

The platform uses AWS Load Balancer Controller with Kubernetes ingress resources to provision an AWS Application Load Balancer dynamically.

HTTPS/TLS termination is handled by the AWS Application Load Balancer using AWS Certificate Manager (ACM) certificates. Traffic is then routed into the Kubernetes cluster through ingress resources managed by the AWS Load Balancer Controller.

Ingress routing supports:

- HTTPS termination
- Path-based routing
- Public internet access
- OIDC redirect compatibility