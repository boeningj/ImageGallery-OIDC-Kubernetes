resource "kubernetes_namespace" "imagegallery" {
  metadata {
    name = "imagegallery"
  }
}