provider "aws" {
  region = "us-east-2"
}

terraform {
  required_version = ">= 1.3"

  backend "s3" {
    bucket = "imagegallery-terraform-state"
    key    = "dev/foundation/terraform.tfstate"
    region = "us-east-2"
  }

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}