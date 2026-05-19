# Docker Deployment

Additional Docker deployment, networking, and reverse proxy architecture details for the ImageGallery platform.

## Docker Architecture

The Docker environment simulates a production-style reverse proxy architecture where:

- NGINX handles HTTPS/TLS termination
- IdentityServer runs behind a reverse proxy using the `/idp` path base
- Containers communicate internally over a dedicated Docker network
- Browser traffic is routed through HTTPS endpoints exposed by NGINX

## Environment Configuration

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

## Notes

- Docker Compose automatically provisions a dedicated Docker network for inter-container communication.
- NGINX is used to simulate ingress-style HTTPS routing before Kubernetes deployment.
- The Docker environment uses self-signed development certificates for HTTPS.
- Internal container communication intentionally uses HTTP within the isolated Docker network.
- The platform separates browser-facing URLs from internal container URLs to support OpenID Connect flows correctly inside Docker containers.