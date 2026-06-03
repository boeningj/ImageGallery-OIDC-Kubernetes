# Legacy Terraform Architecture

## Overview

This directory contains the original environment-centric Terraform architecture used by the ImageGallery AWS deployment.

The contents of this directory are retained for historical reference and to document the evolution of the infrastructure design.

## Status

**Deprecated**

This architecture has been superseded by the shared Foundation/Runtime Terraform architecture located under:

```text
infra/terraform/aws/
├── env
├── foundation
└── runtime
```

New infrastructure changes should be made in the active architecture and not in this directory.

## Original Design

The original implementation organized Terraform by environment:

```text
environments/
├── dev/
├── qa/
└── prod/
```

Each environment contained its own copies of:

- VPC configuration
- Runtime infrastructure
- Variable definitions
- Backend configuration
- Supporting Terraform resources

While functional, this approach introduced duplication and made it more difficult to separate persistent infrastructure from disposable runtime resources.

## Replacement Architecture

The active architecture separates infrastructure into two independent root modules:

### Foundation

Long-lived shared infrastructure:

- VPC
- Public and private subnets
- Route tables
- NAT Gateway
- Shared security groups
- Amazon RDS SQL Server
- Terraform state storage

Terraform state:

```text
dev/foundation/terraform.tfstate
```

### Runtime

Disposable application runtime infrastructure:

- Amazon EKS
- Managed node groups
- AWS Load Balancer Controller
- Kubernetes namespace
- Runtime IAM resources

Terraform state:

```text
dev/runtime/terraform.tfstate
```

Runtime consumes Foundation outputs through Terraform remote state.

## Benefits of the New Architecture

- Independent Foundation and Runtime lifecycles
- Reduced Terraform duplication
- Cleaner environment management
- Easier support for multiple environments
- Simplified GitHub Actions automation
- Safer runtime destroy/rebuild operations
- Improved infrastructure maintainability

## Historical Reference

This directory is retained to:

- Preserve the original implementation
- Document architectural evolution
- Provide a reference during future refactoring efforts
- Support comparison between infrastructure approaches

No active development should occur within this directory.