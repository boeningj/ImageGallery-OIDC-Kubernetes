# ImageGallery-OIDC-Kubernetes

Cloud-native ASP.NET Core OAuth2/OIDC platform deployed with Kubernetes and AWS EKS.

## Overview

ImageGallery is a cloud-native ASP.NET Core platform demonstrating modern OAuth2/OpenID Connect authentication, containerization, Kubernetes orchestration, and AWS cloud deployment.

The platform consists of:
- An ASP.NET Core MVC client
- A protected ASP.NET Core Web API
- A Duende IdentityServer authentication provider

Supported deployment environments include:
- Local ASP.NET Core
- Docker Compose
- Local Kubernetes
- AWS EKS

## Tech Stack

- ASP.NET Core 8
- Duende IdentityServer
- OAuth2 / OpenID Connect
- EF Core Migrations
- SQL Server
- Docker & Docker Compose
- Kubernetes
- NGINX
- AWS EKS
- Amazon ECR
- Amazon RDS for SQL Server
- AWS ALB + ACM
- Cloudflare DNS
- Kubernetes ExternalDNS
- Terraform
- GitHub Actions
- GitHub OIDC Federation

## Key Features

### Security & Authentication

- OpenID Connect (OIDC) login flows
- OAuth2-protected APIs
- Reference token introspection
- Claims-based and role-based authorization
- Custom ASP.NET Core authorization handlers
- Resource ownership enforcement
- Persistent IdentityServer signing certificate management
- Deterministic JWKS publication across distributed environments
- Stable multi-replica IdentityServer signing identity
- Kubernetes Secret-based signing certificate management
- Rollout-safe authentication infrastructure

### Cloud & Infrastructure

- Docker containerization
- Kubernetes orchestration
- AWS EKS deployment
- HTTPS ingress configuration
- Terraform Infrastructure as Code
- Kubernetes ConfigMaps and Secrets
- Amazon RDS for SQL Server
- SQL Server
- EF Core Migrations
- Persistent distributed IdentityServer signing identity

### Application Features

- ASP.NET Core MVC client
- Protected ASP.NET Core Web API
- Persistent image metadata storage using Amazon RDS SQL Server

## Live Demo

The ImageGallery MVC client application is publicly hosted on AWS EKS and can be accessed here:

🔗 **Live Application:** https://imagegallery.boeninglabs.net/

> ⚠️ Note: The environment may occasionally be offline during infrastructure maintenance, redeployment, or cost optimization activities.

### Test Users

#### Emma

- Username: Emma
- Password: password

**Permissions**
- Can upload images
- Can edit/delete owned images
- Assigned `PayingUser` role
- Contains required authorization claims

#### David

- Username: David
- Password: password

**Permissions**
- Can browse images
- Demonstrates restricted authorization policies

## Architecture

The following diagram illustrates the high-level architecture of the ImageGallery platform running on AWS EKS, including managed SQL Server persistence using Amazon RDS.

![Architecture Diagram](docs/ImageGallery-Architecture-Diagram-RDS.png)

## Infrastructure

The repository is organized to support multiple deployment environments and infrastructure layers, ranging from local ASP.NET Core development to fully cloud-hosted Kubernetes workloads on AWS EKS.

### Repository Structure

```text
ImageGallery.API/
ImageGallery.Client/
ImageGallery.IDP/
ImageGallery.Authorization/
ImageGallery.Infrastructure/

certs/

infra/terraform/aws/

k8s/local/
k8s/aws/

nginx/

env/templates/

docker-compose.yaml
```

| Path | Description |
|---|---|
| `ImageGallery.API/` | Secured ASP.NET Core Web API |
| `ImageGallery.Client/` | ASP.NET Core MVC client application |
| `ImageGallery.IDP/` | Duende IdentityServer OIDC/OAuth2 provider |
| `ImageGallery.Authorization/` | Shared authorization policies and requirements |
| `ImageGallery.Infrastructure/` | Shared infrastructure services including ASP.NET Core Data Protection persistence and infrastructure EF Core migrations |
| `certs/` | Development signing certificate documentation and local signing certificate storage for distributed IdentityServer deployments
| `infra/terraform/aws/` | AWS Terraform Infrastructure as Code |
| `k8s/local/` | Local Kubernetes manifests |
| `k8s/aws/` | AWS EKS Kubernetes manifests |
| `nginx/` | Local reverse proxy configuration |
| `env/templates/` | Sanitized environment configuration templates |
| `docker-compose.yaml` | Docker Compose orchestration |

### Terraform Architecture

AWS infrastructure is organized using a shared Foundation/Runtime Terraform architecture.

| Layer | Purpose |
|---------|---------|
| Foundation | Long-lived shared infrastructure such as VPC networking, Amazon RDS SQL Server, security groups, and Terraform state storage |
| Runtime | Disposable application infrastructure such as Amazon EKS, managed node groups, ingress controllers, and Kubernetes workloads |

This separation enables independent Runtime destroy/rebuild operations without impacting persistent infrastructure or application data.

Additional details:

- [Foundation Infrastructure](infra/terraform/aws/foundation/README.md)
- [Runtime Infrastructure](infra/terraform/aws/runtime/README.md)

### Infrastructure Design Goals

The platform infrastructure was designed to demonstrate:

- Environment-specific deployment strategies
- Infrastructure as Code (IaC)
- Secure configuration management
- Cloud-native application hosting
- Kubernetes orchestration patterns
- HTTPS ingress and reverse proxy architecture
- Environment-driven application configuration

### Configuration Management

Configuration is separated by deployment environment using:

- Local environment files
- Docker environment files
- Kubernetes ConfigMaps & Secrets
- Terraform-managed infrastructure resources

Sanitized configuration templates are included to simplify onboarding and local development setup without exposing secrets.

## Authentication & Authorization

Authentication and authorization are implemented using Duende IdentityServer with OAuth2 and OpenID Connect (OIDC).

The platform demonstrates:
- Authorization Code Flow authentication
- OAuth2-protected APIs
- Claims-based and role-based authorization
- OAuth2 scope validation
- Reference token introspection
- Resource-based authorization using custom ASP.NET Core authorization handlers
- JWT access token validation (`at+jwt`)
- Persistent distributed IdentityServer signing certificate management
- Deterministic JWKS publication across distributed environments
- HTTPS/TLS ingress configuration
- Kubernetes Secret-based configuration management
- Persistent distributed IdentityServer signing certificate management
- Deterministic JWKS publication across distributed environments

The API supports both OAuth2 reference tokens and self-contained JWT access tokens through environment-based configuration.

## Deployment Modes

The platform supports multiple deployment environments to demonstrate the evolution from local ASP.NET Core development to fully containerized cloud-native deployment.

| Environment | Description |
|---|---|
| Local ASP.NET Core | Traditional multi-project local development |
| Docker Compose | Fully containerized local deployment |
| Local Kubernetes | Kubernetes deployment using Docker Desktop |
| AWS EKS | Cloud-native deployment on Amazon EKS using Terraform Infrastructure as Code |

## Local Development

The platform can be run locally using standard ASP.NET Core hosting without Docker or Kubernetes.

### Prerequisites

- .NET 8 SDK
- PowerShell
- ASP.NET Core HTTPS development certificates
- SQL Server

### Environment Configuration

Local environment configuration templates are provided in:

```text
env/templates/local.template.env
```

Create a local development environment file:

```powershell
Copy-Item env/templates/local.template.env env/local.env
```

Replace placeholder values with local development settings before starting the platform.

### Local Database

Local development currently uses a local SQL Server instance for persistent storage.

The default development configuration expects SQL Server to be available at:

```text
localhost,1433
```

### Starting The Platform

The repository includes a PowerShell orchestration script that automatically launches all platform services in the correct order.

Run:

```powershell
.\Start-All-ImageGalleryV2.ps1
```

The startup script performs the following tasks:

- Loads environment variables from `env/local.env`
- Starts the Duende IdentityServer IDP
- Waits for the IDP to become available
- Starts the ImageGallery API
- Waits for the API to become available
- Starts the MVC client application

Each service is launched in a separate PowerShell window to simplify local debugging and development.

### Local Service Endpoints

| Service | URL |
|---|---|
| MVC Client | https://localhost:7184 |
| ImageGallery API | https://localhost:7075 |
| Duende IdentityServer | https://localhost:5001/idp |

### Identity Provider Path Base

The Duende IdentityServer host is externally exposed under the `/idp` path segment in all environments:

- Local ASP.NET Core
- Docker Compose
- Local Kubernetes
- AWS EKS

Example:

```text
https://localhost:5001/idp
```

## Docker Deployment

The platform supports fully containerized local deployment using Docker Compose.

Containerized services include:

- ASP.NET Core MVC client
- ASP.NET Core Web API
- Duende IdentityServer IDP
- NGINX reverse proxy for HTTPS/TLS termination

### Starting The Docker Environment

Run:

```powershell
docker compose up --build
```

### Docker Service Endpoints

| Service | URL |
|---|---|
| MVC Client (via NGINX) | https://localhost:5000 |
| Duende IdentityServer | https://localhost:5000/idp |
| ImageGallery API | https://localhost:7075 |

Additional Docker architecture, networking, and environment configuration details are available in [`docs/docker.md`](docs/docker.md).

## Kubernetes Deployment

The platform supports Kubernetes-based deployment using Docker Desktop Kubernetes for local orchestration and testing.

The Kubernetes environment demonstrates:

- Container orchestration
- Service discovery
- Kubernetes ConfigMaps and Secrets
- Ingress-based routing
- HTTPS/TLS termination
- Multi-service workload deployment

### Local Kubernetes Deployment

Deploy the local environment:

```powershell
kubectl apply -f k8s/local/
```

Verify workloads:

```powershell
kubectl get pods -n imagegallery
```

Verify services:

```powershell
kubectl get svc -n imagegallery
```

Additional Kubernetes architecture, manifest organization, and configuration details are available in [`docs/kubernetes.md`](docs/kubernetes.md).

## AWS EKS Deployment

The platform is publicly hosted on Amazon Web Services (AWS) using Amazon Elastic Kubernetes Service (EKS).

The AWS deployment demonstrates a production-style cloud-native architecture including:

- Amazon EKS Kubernetes orchestration
- AWS Application Load Balancer (ALB) ingress
- HTTPS/TLS termination using AWS Certificate Manager (ACM)
- Public DNS routing using Cloudflare
- Automated DNS reconciliation using Kubernetes ExternalDNS and Cloudflare
- Infrastructure as Code using Terraform
- Kubernetes ConfigMaps and Secrets
- Containerized ASP.NET Core workloads
- Amazon RDS for SQL Server managed persistence
- GitHub Actions automation for Runtime startup and shutdown workflows
- GitHub OIDC federation for passwordless AWS authentication

The platform uses Kubernetes ExternalDNS with Cloudflare to automatically reconcile public DNS records whenever AWS Application Load Balancer hostnames change during runtime rebuild operations.

### AWS Traffic Flow Architecture

The following diagram illustrates the high-level public traffic flow for the AWS EKS deployment.

![AWS EKS Public Traffic Flow](docs/AWS-EKS-PublicTrafficFlow.png)

Additional AWS infrastructure, ingress, Terraform, and Kubernetes deployment details are available in [`docs/aws-eks.md`](docs/aws-eks.md).

## Screenshots

### OpenID Connect Login

Duende IdentityServer authentication flow used by the MVC client application.

<p align="center">
  <img src="docs/screenshots/login.png" width="700" />
</p>

### OAuth2 Consent & Claims

Duende IdentityServer consent flow demonstrating OAuth2 scopes, claims-based identity information, and delegated API permissions.

<p align="center">
  <img src="docs/screenshots/emma-consent.png" width="700" />
</p>

### Authorized User Experience (Emma)

Emma is assigned the `PayingUser` role and required authorization claims, allowing image upload and management operations.

<p align="center">
  <img src="docs/screenshots/emma-authorized.png" width="700" />
</p>

### Restricted Authorization Experience (David)

David can browse image content but does not satisfy the authorization requirements needed for image upload operations.

<p align="center">
  <img src="docs/screenshots/david-restricted.png" width="700" />
</p>

## Lessons Learned

Building and deploying the platform across local development, Docker, Kubernetes, and AWS EKS environments exposed several real-world engineering and operational challenges.

Key lessons learned included:

- Linux containers and Kubernetes deployments are case-sensitive, which exposed path inconsistencies that worked on Windows development environments but failed inside Docker and Kubernetes.
- OAuth2/OIDC applications deployed behind reverse proxies require careful handling of external vs internal URLs, especially for redirect URIs and token validation endpoints.
- Docker container networking differs significantly from local host networking, requiring explicit handling of internal service discovery and hostname resolution.
- Kubernetes ingress and AWS ALB routing introduce additional complexity around HTTPS forwarding, path handling, and TLS termination behavior.
- Introducing Kubernetes ExternalDNS into an environment with pre-existing manually managed Cloudflare DNS records exposed DNS ownership reconciliation behavior using TXT registry records and highlighted the importance of controller-managed declarative infrastructure.
- OAuth2 reference tokens require runtime token introspection and introduce different operational considerations compared to self-contained JWT access tokens.
- Kubernetes ConfigMaps and Secrets greatly simplify environment-specific configuration management compared to local environment variable handling.
- Migrating from container-local SQLite storage to Amazon RDS SQL Server highlighted the importance of shared durable persistence for Kubernetes workloads and multi-replica cloud-native deployments.
- Persistent IdentityServer signing identity becomes critical in containerized and distributed environments to prevent token validation failures during redeployments, pod restarts, horizontal scaling operations, and cluster recreation events. Stable signing identity management using mounted certificates and Kubernetes Secrets enables deterministic JWKS publication and rollout-safe authentication behavior across distributed environments.
- Infrastructure as Code using Terraform improves repeatability and consistency for cloud infrastructure provisioning.
- Cloud-native deployments require stronger observability and troubleshooting practices compared to traditional local development workflows.
- Running applications successfully in Docker does not guarantee successful Kubernetes deployment due to differences in networking, ingress, configuration injection, and orchestration behavior.

## Future Improvements

Potential future enhancements for the platform include:

- Migrating Kubernetes Secret-based signing certificate and application secret management to external secret providers such as AWS Secrets Manager, HashiCorp Vault, or Kubernetes CSI Secret Store drivers.
- Implementing centralized logging and observability using tools such as CloudWatch, Prometheus, or Grafana.
- Implementing automated container vulnerability scanning and image hardening practices.
- Expanding ASP.NET Core Data Protection infrastructure to support dedicated distributed cache providers such as Redis for improved scalability and reduced database dependency.
- Implementing GitHub Actions CI/CD pipelines for automated container builds, ECR publishing, and Kubernetes deployments.
- Further expanding cloud-native automation workflows using Terraform, Kubernetes controllers, and GitHub Actions CI/CD pipelines.
- Supporting horizontal pod autoscaling (HPA) for Kubernetes workloads.

## Author

Jonathan Boening

- GitHub: [boeningj](https://github.com/boeningj)
- LinkedIn: [Jonathan Boening](https://www.linkedin.com/in/jonathan-boening/)