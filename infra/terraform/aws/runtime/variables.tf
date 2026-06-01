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