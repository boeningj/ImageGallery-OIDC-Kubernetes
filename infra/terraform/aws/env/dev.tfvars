#project_name = "imagegallery"
environment = "dev"
db_username = "imagegalleryadm"
db_instance_class = "db.t3.micro"
db_allocated_storage = 20
aws_region = "us-east-2"
aws_s3_bucket = "imagegallery-terraform-state"
aws_s3_bucket_key = "dev/foundation/terraform.tfstate"
aws_vpc_cidr = "10.0.0.0/16"
aws_vpc_azs = [
  "us-east-2a",
  "us-east-2b"
]
aws_vpc_private_subnets = [
  "10.0.1.0/24",
  "10.0.2.0/24"
]
aws_vpc_public_subnets = [
  "10.0.101.0/24",
  "10.0.102.0/24"
]