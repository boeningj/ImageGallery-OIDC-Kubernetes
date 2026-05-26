## Future Terraform root module for disposable runtime infrastructure.

This layer will contain Kubernetes and application runtime resources including:

- Amazon EKS cluster
- EKS managed node groups
- AWS Load Balancer Controller
- Kubernetes ingress resources
- Helm deployments
- Kubernetes workloads and services

The runtime layer is intended to be independently destroyable and rebuildable without impacting persistent foundation infrastructure such as VPC networking, RDS, shared security groups, or container registries.