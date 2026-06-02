data "terraform_remote_state" "foundation" {
  backend = "s3"

  config = {
    bucket = "imagegallery-terraform-state"
    key    = var.foundation_state_key
    region = var.aws_region
  }
}