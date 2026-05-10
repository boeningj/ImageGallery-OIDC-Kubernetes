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

## Architecture Diagram

![Architecture Diagram](docs/ImageGallery-Architecture-Diagram.png)

## Authentication & Authorization

## Deployment Modes

## Local Development

## Docker Deployment

## Kubernetes Deployment

## AWS EKS Deployment

## Infrastructure

## Security Notes

## Screenshots

## Lessons Learned

Linux (Docker/K8s) is case-sensitive: "images" ≠ "Images"

## Future Improvements

## Author