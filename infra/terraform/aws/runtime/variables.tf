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

variable "aws_s3_bucket" {
    description = "AWS S3 Bucket"
    type        = string
}

variable "aws_s3_bucket_key" {
    description = "AWS S3 Bucket Key"
    type        = string
}