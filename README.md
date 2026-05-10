# ImageGallery-OIDC-Kubernetes

## Overview

ImageGallery is an ASP.NET Core application that allows authenticated users to browse, upload, and manage image content through a secure cloud-native platform.

Originally designed as a local ASP.NET Core security sample application, the project was expanded into a fully containerized multi-environment platform demonstrating OAuth2/OpenID Connect authentication, claims-based authorization, Docker containerization, Kubernetes deployment, AWS EKS hosting, HTTPS ingress with AWS ALB + ACM, and Infrastructure as Code using Terraform.

Supported deployment environments include:

- Local ASP.NET Core development
- Docker Compose deployment
- Local Kubernetes deployment
- AWS EKS cloud deployment

The platform demonstrates modern authentication and authorization patterns using Duende IdentityServer, including OpenID Connect login flows, OAuth2-protected APIs, reference token introspection, role-based authorization, and claims-based authorization policies.

## Features

### Authentication & Authorization

- Duende IdentityServer authentication provider
- OpenID Connect (OIDC) login flow
- OAuth2-protected APIs
- Reference token introspection
- Claims-based authorization
- Role-based authorization policies
- Custom resource-based authorization handlers
- Image ownership enforcement using custom authorization requirements

### Application Features

- ASP.NET Core MVC client application
- Protected ASP.NET Core Web API
- Secure image upload and management

### Deployment & Infrastructure

- Docker containerization
- Docker Compose orchestration
- Local Kubernetes deployment
- AWS EKS deployment
- AWS ALB ingress controller
- HTTPS with AWS Certificate Manager (ACM)
- Cloudflare DNS integration
- Terraform infrastructure provisioning
- Kubernetes ConfigMaps and Secrets

## Live Demo

The ImageGallery MVC client application is publicly hosted on AWS EKS and can be accessed here:

🔗 **Live Application:** https://imagegallery.boeninglabs.net/

> ⚠️ Note: The environment may occasionally be offline during infrastructure maintenance, redeployment, or cost optimization activities.

The MVC client communicates with protected backend API services secured using OAuth2 and OpenID Connect.

Authentication and token issuance are handled using Duende IdentityServer.

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

![Architecture Diagram](docs/ImageGallery-Architecture-Diagram.png)

The platform consists of an ASP.NET Core MVC client, a protected ASP.NET Core Web API, and a Duende IdentityServer authentication provider running as containerized workloads inside an AWS EKS cluster. Traffic is routed through an AWS Application Load Balancer (ALB) with HTTPS termination provided by AWS Certificate Manager (ACM).

## Authentication & Authorization

Authentication and authorization are implemented using Duende IdentityServer with OpenID Connect (OIDC) and OAuth2 flows.

The ASP.NET Core MVC client authenticates users through the Duende IdentityServer IDP using the Authorization Code Flow.

The protected API supports multiple OAuth2 token validation modes, configurable through environment-based settings:

- Reference tokens validated through OAuth2 introspection
- Self-contained JWT access tokens validated locally
- Local development JWTs using `dotnet user-jwts`

The platform defaults to OAuth2 reference tokens to demonstrate introspection-based validation patterns commonly used in distributed systems and microservices environments.

Authorization is enforced at multiple levels throughout the platform:

- Authenticated user requirements
- Role-based authorization
- Claims-based authorization
- OAuth2 scope-based authorization
- Resource ownership enforcement using custom authorization handlers

Example authorization scenarios include:

- Only authenticated users can access protected API endpoints
- Only users with the `PayingUser` role may upload images
- Upload permissions additionally require specific user claims
- Users may only modify or delete images they own
- APIs validate OAuth2 scopes before allowing write operations

The platform includes custom ASP.NET Core authorization handlers and requirements to demonstrate resource-based authorization patterns using token claims and route-based ownership validation.

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

The environment configuration includes:

- OIDC authority settings
- Client application configuration
- API configuration
- Database connection strings
- Token validation settings

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
| Duende IdentityServer | https://localhost:5001 |

## Docker Deployment

The platform supports fully containerized local deployment using Docker Compose.

Containerized deployment includes:

- ASP.NET Core MVC client
- ASP.NET Core Web API
- Duende IdentityServer IDP
- NGINX reverse proxy for HTTPS/TLS termination

### Docker Architecture

The Docker environment simulates a production-style reverse proxy architecture where:

- NGINX handles HTTPS/TLS termination
- IdentityServer runs behind a reverse proxy using the `/idp` path base
- Containers communicate internally over a dedicated Docker network
- Browser traffic is routed through HTTPS endpoints exposed by NGINX

### Environment Configuration

Docker environment configuration templates are provided in:

```text
env/templates/docker.template.env
```

The template includes detailed documentation covering:

- Internal vs external container networking
- OIDC authority configuration
- Reverse proxy forwarding behavior
- HTTPS/TLS termination
- OAuth2 token introspection
- Docker container communication patterns

Create a local Docker environment file:

```powershell
Copy-Item env/templates/docker.template.env env/docker.env
```

Replace placeholder values with local development settings before starting the environment.

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

### Notes

- Docker Compose automatically provisions a dedicated Docker network for inter-container communication.
- NGINX is used to simulate ingress-style HTTPS routing before Kubernetes deployment.
- The Docker environment uses self-signed development certificates for HTTPS.
- Internal container communication intentionally uses HTTP within the isolated Docker network.
- The platform separates browser-facing URLs from internal container URLs to support OpenID Connect flows correctly inside Docker containers.

## Kubernetes Deployment

The platform supports Kubernetes-based deployment using Docker Desktop Kubernetes for local orchestration and testing.

The Kubernetes environment demonstrates:

- Container orchestration
- Service discovery
- Environment-based configuration
- Kubernetes ConfigMaps
- Kubernetes Secrets
- Ingress-based routing
- HTTPS/TLS termination
- Multi-service workload deployment

### Kubernetes Architecture

The Kubernetes deployment consists of:

- ASP.NET Core MVC client deployment
- ASP.NET Core Web API deployment
- Duende IdentityServer deployment
- ClusterIP internal services
- NGINX ingress routing
- Shared configuration using ConfigMaps
- Sensitive configuration using Kubernetes Secrets

### Kubernetes Manifests

Kubernetes manifests are organized under:

```text
k8s/
```

Environment-specific manifests include:

```text
k8s/local/
k8s/aws/
```

### Local Kubernetes Deployment

Local Kubernetes deployment uses Docker Desktop Kubernetes.

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

### Kubernetes Features Demonstrated

- Environment variable injection using ConfigMaps and Secrets
- Internal service-to-service communication
- HTTPS ingress routing
- Containerized ASP.NET Core workloads
- Kubernetes namespace isolation
- Persistent signing key handling for IdentityServer
- Health probe configuration for containerized services

## AWS EKS Deployment

The platform is publicly hosted on Amazon Web Services (AWS) using Amazon Elastic Kubernetes Service (EKS).

The AWS deployment demonstrates a production-style cloud-native architecture including:

- Amazon EKS Kubernetes orchestration
- AWS Application Load Balancer (ALB) ingress
- HTTPS/TLS termination using AWS Certificate Manager (ACM)
- Public DNS routing using Cloudflare
- Infrastructure as Code using Terraform
- Kubernetes ConfigMaps and Secrets
- Containerized ASP.NET Core workloads

### AWS Traffic Flow Architecture

The following diagram illustrates how public internet traffic is routed through Cloudflare, AWS infrastructure, and Kubernetes ingress resources before reaching the containerized ASP.NET Core workloads running inside Amazon EKS.

![AWS EKS Public Traffic Flow](docs/AWS-EKS-PublicTrafficFlow.png)

HTTPS/TLS termination is handled by the AWS Application Load Balancer using AWS Certificate Manager (ACM) certificates. Traffic is then routed into the Kubernetes cluster through ingress resources managed by the AWS Load Balancer Controller.

### Infrastructure Provisioning

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

### Kubernetes Deployment

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

### HTTPS & Ingress

The platform uses AWS Load Balancer Controller with Kubernetes ingress resources to provision an AWS Application Load Balancer dynamically.

HTTPS certificates are managed through AWS Certificate Manager (ACM).

Ingress routing supports:

- HTTPS termination
- Path-based routing
- Public internet access
- OIDC redirect compatibility

### Operational Lessons Learned

The AWS deployment process exposed several real-world cloud-native engineering challenges including:

- Kubernetes ingress configuration
- OIDC redirect URI handling behind reverse proxies
- HTTPS/TLS forwarding behavior
- Internal vs external service addressing
- Container networking differences between Windows and Linux
- Kubernetes Secret and ConfigMap injection
- Persistent IdentityServer signing key management
- Linux container filesystem case sensitivity

## Infrastructure

The repository is organized to support multiple deployment environments and infrastructure layers, ranging from local ASP.NET Core development to fully cloud-hosted Kubernetes workloads on AWS EKS.

### Repository Structure

```text
ImageGallery.API/
ImageGallery.Client/
ImageGallery.IDP/
ImageGallery.Authorization/

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
| `infra/terraform/aws/` | AWS Terraform Infrastructure as Code |
| `k8s/local/` | Local Kubernetes manifests |
| `k8s/aws/` | AWS EKS Kubernetes manifests |
| `nginx/` | Local reverse proxy configuration |
| `env/templates/` | Sanitized environment configuration templates |
| `docker-compose.yaml` | Docker Compose orchestration |

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
- Kubernetes ConfigMaps
- Kubernetes Secrets
- Terraform-managed infrastructure resources

Sanitized configuration templates are included to simplify onboarding and local development setup without exposing secrets.

## Security Notes

The platform was designed to demonstrate modern authentication and authorization patterns commonly used in a cloud-native ASP.NET Core application.

Security-focused implementation details include:

- OpenID Connect (OIDC) authentication using Duende IdentityServer
- OAuth2-protected APIs
- Support for both reference tokens and JWT access tokens
- OAuth2 token introspection support
- Claims-based and role-based authorization
- Resource ownership validation using custom authorization handlers
- HTTPS/TLS termination using AWS Certificate Manager (ACM)
- Kubernetes Secret-based sensitive configuration management
- Environment-specific configuration isolation
- Fail-fast startup validation for invalid authentication configuration

### JWT Validation

When operating in JWT validation mode, the API validates the token type header (`at+jwt`) to reduce the risk of JWT confusion attacks.

### Development Environment Notes

Development environments use self-signed certificates and simplified secrets management intended for local development and educational purposes.

Production-grade deployments should additionally consider:

- Managed database services
- External secret providers
- Centralized logging and monitoring
- IdentityServer persistent storage
- Automated certificate rotation
- Vulnerability scanning and image hardening

## Screenshots

## Lessons Learned

- Linux containers and Kubernetes deployments are case-sensitive, which exposed path inconsistencies that worked on Windows development environments but failed inside Docker and Kubernetes.

## Future Improvements

## Author

Jonathan Boening

- GitHub: [boeningj](https://github.com/boeningj)
- LinkedIn: [Jonathan Boening](https://www.linkedin.com/in/jonathan-boening/)