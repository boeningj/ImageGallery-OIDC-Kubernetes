variable "project_name" {
    description = "Project Name"
    type        = string
    default     = "imagegallery"
}

variable "environment" {
    description = "Environment"
    type        = string
}

variable "db_password" {
    description = "SQL Server master password"
    type        = string
    sensitive   = true
}

variable "db_username" {
    description = "SQL Server Username"
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

variable "aws_vpc_cidr" {
    description = "AWS VPC CIDR"
    type        = string
}

variable "aws_vpc_azs" {
  description = "Availability Zones for the VPC"
  type        = list(string)
}

variable "aws_vpc_private_subnets" {
  description = "Private subnet CIDR blocks"
  type        = list(string)
}

variable "aws_vpc_public_subnets" {
  description = "Public subnet CIDR blocks"
  type        = list(string)
}