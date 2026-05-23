# Kubernetes Deployment

Additional Kubernetes deployment, orchestration, ingress, and configuration details for the ImageGallery platform.

## Kubernetes Architecture

The Kubernetes deployment consists of:

- ASP.NET Core MVC client deployment
- ASP.NET Core Web API deployment
- Duende IdentityServer deployment
- ClusterIP internal services
- NGINX ingress routing
- Shared configuration using ConfigMaps
- Sensitive configuration using Kubernetes Secrets

## Kubernetes Manifests

Kubernetes manifests are organized under:

```text
k8s/
```

Environment-specific manifests include:

```text
k8s/local/
k8s/aws/
```

## Kubernetes Features Demonstrated

- Environment variable injection using ConfigMaps and Secrets
- Internal service-to-service communication
- HTTPS ingress routing
- Containerized ASP.NET Core workloads
- Kubernetes namespace isolation
- Persistent signing key handling for IdentityServer
- Kubernetes readiness and liveness health probes