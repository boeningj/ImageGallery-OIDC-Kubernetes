# ============================================================
# IMAGE STORAGE IAM POLICY
# ============================================================
#
# Defines the AWS permissions required by the ImageGallery API
# pods to interact with the S3 image bucket.
#
# This policy grants:
#   - ListBucket
#   - GetObject
#   - PutObject
#   - DeleteObject
#
# IMPORTANT:
# - Bucket access is scoped ONLY to the environment-specific
#   image bucket defined via:
#
#     var.aws_s3_image_bucket
#
# - This supports clean multi-environment separation:
#
#     dev  -> imagegallery-dev-images
#     qa   -> imagegallery-qa-images
#     prod -> imagegallery-prod-images
#
# SECURITY:
# - Least privilege
# - No wildcard S3 admin permissions
# - No account-wide bucket access
# ============================================================

data "aws_iam_policy_document" "image_storage" {

  # ==========================================================
  # BUCKET-LEVEL PERMISSIONS
  # ==========================================================
  #
  # Required for listing bucket contents.
  # Some SDK operations and future enhancements may require this.
  # ==========================================================
  statement {
    sid = "ListBucket"

    actions = [
      "s3:ListBucket"
    ]

    resources = [
      "arn:aws:s3:::${var.aws_s3_image_bucket}"
    ]
  }

  # ==========================================================
  # OBJECT-LEVEL PERMISSIONS
  # ==========================================================
  #
  # Required for:
  #   - Uploading images
  #   - Downloading images
  #   - Deleting images
  #
  # IMPORTANT:
  # - Object operations target the bucket objects path:
  #
  #     arn:aws:s3:::bucket-name/*
  #
  #   NOT just the bucket ARN itself.
  # ==========================================================
  statement {
    sid = "ObjectOperations"

    actions = [
      "s3:GetObject",
      "s3:PutObject",
      "s3:DeleteObject"
    ]

    resources = [
      "arn:aws:s3:::${var.aws_s3_image_bucket}/*"
    ]
  }
}

# ============================================================
# IMAGE STORAGE IAM POLICY RESOURCE
# ============================================================
#
# Creates the reusable IAM policy that will later be attached
# to the ImageGallery API IRSA role.
#
# ENVIRONMENT-AWARE NAMING:
# - Prevents collisions across environments
# - Supports parallel dev / qa / prod deployments
# ============================================================

resource "aws_iam_policy" "image_storage" {
  name        = var.image_storage_policy_name
  description = "S3 access policy for ImageGallery API image storage"

  policy = data.aws_iam_policy_document.image_storage.json
}

# ============================================================
# IMAGE STORAGE IRSA ASSUME ROLE POLICY
# ============================================================
#
# IRSA (IAM Roles for Service Accounts) allows a Kubernetes
# pod to assume an AWS IAM role WITHOUT storing static AWS
# credentials inside the container.
#
# FLOW:
#
#   Kubernetes ServiceAccount
#       ↓
#   EKS OIDC identity token
#       ↓
#   AWS IAM Role
#       ↓
#   Temporary AWS credentials
#       ↓
#   S3 access
#
# SECURITY BENEFITS:
# - No hardcoded AWS credentials
# - Least privilege
# - Pod-level IAM isolation
# - Only the ImageGallery API pod receives S3 access
#
# IMPORTANT:
# - This trust policy restricts role assumption to ONLY:
#
#     namespace:      imagegallery
#     serviceAccount: imagegallery-api
#
# - No other pod/service account in the cluster can assume
#   this role.
# ============================================================

data "aws_iam_policy_document" "image_storage_assume_role" {

  statement {

    # ========================================================
    # IRSA ASSUME ROLE ACTION
    # ========================================================
    #
    # Allows the Kubernetes-issued OIDC token to exchange for
    # temporary AWS credentials.
    # ========================================================
    actions = [
      "sts:AssumeRoleWithWebIdentity"
    ]

    # ========================================================
    # TRUST THE EKS OIDC PROVIDER
    # ========================================================
    #
    # This tells AWS to trust identities issued by THIS EKS
    # cluster's OIDC provider.
    # ========================================================
    principals {
      type = "Federated"

      identifiers = [
        module.eks.oidc_provider_arn
      ]
    }

    # ========================================================
    # RESTRICT TO SPECIFIC SERVICE ACCOUNT
    # ========================================================
    #
    # The OIDC "sub" claim must EXACTLY match:
    #
    #   system:serviceaccount:<namespace>:<serviceaccount>
    #
    # This ensures ONLY the ImageGallery API pod can assume
    # this role.
    # ========================================================
    condition {
      test = "StringEquals"

      variable = "${replace(module.eks.oidc_provider, "https://", "")}:sub"

      values = [
        "system:serviceaccount:imagegallery:sa-imagegallery-api"
      ]
    }
  }
}

# ============================================================
# IMAGE STORAGE IRSA IAM ROLE
# ============================================================
#
# This IAM role will be assumed by the Kubernetes ServiceAccount
# used by the ImageGallery API deployment.
#
# Result:
#
#   API Pod
#      ↓
#   ServiceAccount
#      ↓
#   IAM Role
#      ↓
#   Temporary AWS Credentials
#      ↓
#   S3 Access
#
# ENVIRONMENT-AWARE NAMING:
# - Prevents naming collisions
# - Supports parallel dev / qa / prod environments
# ============================================================

resource "aws_iam_role" "image_storage" {

  name = var.image_storage_role_name

  assume_role_policy = data.aws_iam_policy_document.image_storage_assume_role.json
}

# ============================================================
# ATTACH IMAGE STORAGE POLICY TO ROLE
# ============================================================
#
# Grants the ImageGallery API pod the S3 permissions defined
# earlier in this file:
#
#   - ListBucket
#   - GetObject
#   - PutObject
#   - DeleteObject
#
# scoped ONLY to the environment-specific image bucket.
# ============================================================

resource "aws_iam_role_policy_attachment" "image_storage" {

  role = aws_iam_role.image_storage.name

  policy_arn = aws_iam_policy.image_storage.arn
}