## Future Terraform root module for persistent shared infrastructure.

This layer will contain long-lived foundational infrastructure resources shared by application runtime environments including:

- VPC networking
- Public and private subnets
- NAT gateways
- Route tables
- Shared security groups
- Amazon RDS SQL Server
- Amazon ECR repositories
- Shared IAM resources
- ACM certificates (future)
- Remote Terraform backend resources (future)

Foundation resources are intended to persist independently from disposable runtime infrastructure such as Kubernetes clusters, node groups, ingress controllers, and application workloads.

This separation enables safer infrastructure lifecycle management, reduced operational blast radius, and independent runtime destroy/rebuild workflows without impacting persistent application data or shared networking resources.