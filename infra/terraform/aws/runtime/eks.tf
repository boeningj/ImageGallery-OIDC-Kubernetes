module "eks" {
  source  = "terraform-aws-modules/eks/aws"
  version = "20.8.5"

  cluster_name    = var.eks_cluster_name
  cluster_version = var.eks_cluster_version

  vpc_id     = data.terraform_remote_state.foundation.outputs.vpc_id
  subnet_ids = data.terraform_remote_state.foundation.outputs.private_subnet_ids

  enable_irsa = true

  # ============================================================
  # EKS ACCESS CONFIGURATION (IAM → Kubernetes RBAC)
  # ============================================================
  #
  # By default, creating an EKS cluster does NOT grant your IAM user access to the Kubernetes API.
  #
  # This block maps the IAM user used by Terraform (terraform-user) to a Kubernetes admin role.
  #
  # Without this:
  #   - kubectl will fail with "You must be logged in"
  #
  # This uses the newer EKS access entry system instead of the legacy aws-auth ConfigMap.
  #
  # PRODUCTION NOTE:
  #
  # This example maps both a local Terraform IAM user (terraform-user)
  # and a GitHub Actions IAM role (ImageGallery-GitHubActions-Dev) to
  # full cluster-admin access for simplicity in a local/dev environment.
  #
  # The GitHub Actions role is used by CI/CD automation workflows to
  # perform Terraform operations against the EKS cluster.
  #
  # In a real-world production environment:
  #
  # - DO NOT grant access to individual IAM users
  # - DO NOT grant cluster-admin access broadly
  # - INSTEAD, use dedicated IAM roles (often federated via SSO such as AWS IAM Identity Center, Okta, or Azure AD)
  #
  # Typical role patterns:
  #
  #   platform-admin-role
  #     - Full cluster administration (cluster-admin)
  #     - Restricted to a small number of trusted operators
  #
  #   dev-team-role
  #     - Deploy applications, view resources
  #     - Usually scoped to specific namespaces (not cluster-wide)
  #
  #   qa-team-role
  #     - Test deployments, view logs, limited write access
  #
  #   ci-cd-role
  #     - Used by pipelines (GitHub Actions, Azure DevOps, etc.)
  #     - Limited to applying manifests and updating workloads without interactive access
  #
  #   read-only-role
  #     - View-only access (get/list/watch)
  #     - Used by auditors, support teams, or monitoring tools
  #
  #   sre-role
  #     - Operational debugging (logs, exec into pods, restarts)
  #
  # Best practices:
  #
  # - Follow the principle of least privilege
  # - Avoid granting cluster-admin broadly
  # - Prefer namespace-scoped RBAC over cluster-wide permissions
  # - Manage access via Terraform to ensure consistency and auditability
  #
  # NOTE:
  # This project intentionally grants cluster-admin access to both
  # terraform-user and ImageGallery-GitHubActions-Dev for development
  # convenience. Production environments should use more restrictive,
  # role-based access controls.
  # Changing access_entries updates cluster access control without recreating the cluster.
  # ============================================================
  access_entries = {
    terraform_user = {
      principal_arn = "arn:aws:iam::323146836950:user/terraform-user"

      policy_associations = {
        admin = {
          policy_arn = "arn:aws:eks::aws:cluster-access-policy/AmazonEKSClusterAdminPolicy"
          access_scope = {
            type = "cluster"
          }
        }
      }
    }

    github_actions = {
      principal_arn = "arn:aws:iam::323146836950:role/ImageGallery-GitHubActions-Dev"

      policy_associations = {
        admin = {
          policy_arn = "arn:aws:eks::aws:cluster-access-policy/AmazonEKSClusterAdminPolicy"
          access_scope = {
            type = "cluster"
          }
        }
      }
    }
  }

  # ============================================================
  # EKS CLUSTER ENDPOINT ACCESS CONFIGURATION
  # ============================================================
  #
  # By default, the terraform-aws-eks module may create a cluster
  # with ONLY private endpoint access (no public access).
  #
  # That means:
  #   - kubectl from a local machine will NOT work
  #   - only resources inside the VPC can access the cluster
  #
  # For LOCAL DEVELOPMENT / TESTING:
  #   - Enable public access so kubectl from a developer machine works
  #   - Keep private access enabled so that cluster nodes can communicate internally
  #
  #   cluster_endpoint_public_access  = true
  #   cluster_endpoint_private_access = true
  #
  # ------------------------------------------------------------
  # PRODUCTION RECOMMENDATION:
  #
  #   Option 1 (most secure):
  #     public_access  = false
  #     private_access = true
  #     → Access via VPN / bastion host
  #
  #   Option 2 (restricted public access):
  #     public_access  = true
  #     private_access = true
  #     public_access_cidrs = ["<your-office-ip>/32"]
  #
  # ------------------------------------------------------------
  # IMPORTANT:
  # Changing these settings AFTER cluster creation may require
  # cluster updates and can temporarily disrupt access.
  # ============================================================
  cluster_endpoint_public_access  = true
  cluster_endpoint_private_access = true

  eks_managed_node_groups = {
    default = {
      instance_types = [var.eks_node_instance_type]

      vpc_security_group_ids = [data.terraform_remote_state.foundation.outputs.app_runtime_sg_id]
      
      # ============================================================
      # NODE GROUP AMI (Amazon Machine Image) CONFIGURATION
      # ============================================================
      #
      # Explicitly specify the AMI type for EKS worker nodes.
      #
      # WHY THIS IS NEEDED:
      # - The terraform-aws-eks module may default to an AMI that is not compatible with the selected Kubernetes version.
      # - This can cause node group creation to fail with errors like:  "Requested AMI for this version is not supported"
      #
      # SOLUTION:
      # - Force a known-compatible AMI type for the node group.
      #
      # CURRENT CHOICE:
      # - AL2_x86_64 (Amazon Linux 2)
      #   Stable and widely supported
      #   Compatible with EKS 1.29
      #
      # PRODUCTION CONSIDERATIONS:
      # - Consider upgrading to:
      #     AL2023_x86_64
      #   for newer OS support and security improvements.
      #
      # - Alternatively, use custom AMIs or Bottlerocket for:
      #     hardened security
      #     reduced attack surface
      #
      # IMPORTANT:
      # - Changing AMI type will recreate the node group.
      # ============================================================
      ami_type = "AL2_x86_64"

      min_size     = var.eks_node_min_size
      max_size     = var.eks_node_max_size
      desired_size = var.eks_node_desired_size
    }
  }

  tags = {
    Environment = var.environment
  }
}