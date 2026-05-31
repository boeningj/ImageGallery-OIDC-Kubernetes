data "terraform_remote_state" "foundation" {
  backend = "s3"

  config = {
    bucket = "imagegallery-terraform-state"
    key    = "dev/foundation/terraform.tfstate"
    region = "us-east-2"
  }
}