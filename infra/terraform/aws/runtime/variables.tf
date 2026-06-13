variable "project_name" {
    description = "Project Name"
    type        = string
    default     = "imagegallery"
}

variable "environment" {
    description = "Environment"
    type        = string
}

variable "aws_region" {
    description = "AWS Region"
    type        = string
}

variable "foundation_state_key" {
  description = "Foundation remote state key"
  type        = string
}

variable "eks_cluster_name" {
  description = "EKS cluster name"
  type        = string
}

variable "eks_cluster_version" {
  description = "EKS Kubernetes version"
  type        = string
}

variable "eks_node_instance_type" {
  description = "EKS worker node instance type"
  type        = string
}

variable "eks_node_desired_size" {
  description = "Desired EKS node count"
  type        = number
}

variable "eks_node_min_size" {
  description = "Minimum EKS node count"
  type        = number
}

variable "eks_node_max_size" {
  description = "Maximum EKS node count"
  type        = number
}

variable "github_actions_role_arn" {
  description = "GitHub Actions IAM role ARN"
  type        = string
}

variable "aws_s3_image_bucket" {
  description = "S3 bucket used for ImageGallery runtime image storage"
  type        = string
}

variable "image_storage_policy_name" {
  description = "IAM policy name for ImageGallery S3 image storage access"
  type        = string
}

variable "image_storage_role_name" {
  description = "IAM role name for ImageGallery API IRSA image storage access"
  type        = string
}