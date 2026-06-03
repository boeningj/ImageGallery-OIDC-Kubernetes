# ============================================================
# AWS LOAD BALANCER CONTROLLER - IAM POLICY
# ============================================================
#
# Defines the AWS permissions required by the Kubernetes ALB Controller.
#
# This controller dynamically creates and manages:
#   - Application Load Balancers (ALB)
#   - Target Groups
#   - Listeners / Rules
#
# The policy JSON is sourced from the official AWS repository to ensure compatibility and completeness.
# NOTE:  This defines WHAT actions are allowed, but not WHO can use them.
# ============================================================

resource "aws_iam_policy" "alb_controller" {
  name        = "AWSLoadBalancerControllerIAMPolicy"
  description = "Permissions for AWS Load Balancer Controller"
  policy      = file("${path.module}/iam-policy-alb.json")
}

# ============================================================
# AWS LOAD BALANCER CONTROLLER - IAM ROLE (IRSA)
# ============================================================
#
# IRSA (IAM Roles for Service Accounts) allows Kubernetes pods
# to assume AWS IAM roles without storing AWS credentials.
#
# This works via:
#   Kubernetes ServiceAccount → OIDC identity → IAM Role
#
# The EKS cluster exposes an OIDC provider which AWS trusts.
# The role below defines WHICH Kubernetes identity is allowed
# to assume this role.
# ============================================================

data "aws_iam_policy_document" "alb_assume_role" {
  statement {
    actions = ["sts:AssumeRoleWithWebIdentity"]

    principals {
      type        = "Federated"
      
      # Trust identities issued by the EKS OIDC provider
      # (i.e., identities coming from this Kubernetes cluster)
      identifiers = [module.eks.oidc_provider_arn]
    }

    condition {
      test     = "StringEquals"

      # The OIDC "sub" claim must match the exact service account
      # format: system:serviceaccount:<namespace>:<name>
      variable = "${replace(module.eks.oidc_provider, "https://", "")}:sub"
      
      # Restrict access to ONLY this service account:  kube-system/aws-load-balancer-controller
      # This enforces least privilege — no other pod or service account in the cluster can assume this role.
      values   = ["system:serviceaccount:kube-system:aws-load-balancer-controller"]
    }
  }
}

# ============================================================
# IAM ROLE FOR AWS LOAD BALANCER CONTROLLER
# ============================================================
#
# This role is assumed by the Kubernetes ServiceAccount used by the AWS Load Balancer Controller.
# It acts as the bridge between Kubernetes and AWS APIs.
# ============================================================
resource "aws_iam_role" "alb_controller" {
  name               = "alb-controller-role"
  assume_role_policy = data.aws_iam_policy_document.alb_assume_role.json
}

# ============================================================
# ATTACH POLICY TO ROLE
# ============================================================
#
# Connects the IAM role to the ALB permissions defined above.
# Result:
#   Pod → assumes role → gains permissions → calls AWS APIs
# ============================================================
resource "aws_iam_role_policy_attachment" "alb_controller" {
  role       = aws_iam_role.alb_controller.name
  policy_arn = aws_iam_policy.alb_controller.arn
}

# ============================================================
# AWS LOAD BALANCER CONTROLLER - HELM INSTALL
# ============================================================
#
# Installs the AWS Load Balancer Controller into the cluster.
#
# This will:
#   - Create the ServiceAccount
#   - Annotate it with the IAM role (IRSA)
#   - Deploy the controller pod
#
# Once running, the controller watches for Ingress resources and creates ALBs automatically in AWS.
# ============================================================

resource "helm_release" "alb_controller" {
  name       = "aws-load-balancer-controller"
  repository = "https://aws.github.io/eks-charts"
  chart      = "aws-load-balancer-controller"
  namespace  = "kube-system"

  depends_on = [
    aws_iam_role_policy_attachment.alb_controller
  ]

  set {
    name  = "clusterName"
    value = module.eks.cluster_name
  }

  set {
    name  = "region"
    value = "us-east-2"
  }

  set {
    name  = "vpcId"
    value = data.terraform_remote_state.foundation.outputs.vpc_id
  }

  set {
    name  = "serviceAccount.create"
    value = "true"
  }

  # ============================================================
  # IRSA ROLE ANNOTATION (CRITICAL)
  # ============================================================
  #
  # This annotation links the Kubernetes ServiceAccount used by the AWS Load Balancer Controller to an AWS IAM role.
  #
  # HOW IT WORKS:
  # - Helm creates a ServiceAccount in Kubernetes
  # - This annotation tells EKS:
  #     "When a pod uses this ServiceAccount, it may assume this IAM role"
  # - The pod receives an OIDC token from the cluster
  # - AWS validates the token and allows the role to be assumed
  #
  # RESULT:
  #   Controller Pod → assumes IAM Role → calls AWS APIs (ALB, etc.)
  #
  # SECURITY:
  # - Only this specific ServiceAccount can assume the role
  # - No static AWS credentials are stored in the pod
  # - Enables least-privilege, pod-level access control (IRSA)
  # ============================================================
  set {
    name  = "serviceAccount.annotations.eks\\.amazonaws\\.com/role-arn"
    value = aws_iam_role.alb_controller.arn
  }
}