# Letsencrypt terraform module

terraform {
  required_providers {
    helm = {
      source  = "hashicorp/helm"
      version = ">= 2.0.2"
    }
  }
}

resource "helm_release" "nginx_ingress" {
  name       = "nginx-ingress"
  repository = "https://kubernetes.github.io/ingress-nginx"
  chart      = "ingress-nginx"

  set {
    name  = "controller.replicaCount"
    value = "1"
    type  = "string"
  }

  set {
    name  = "controller.nodeSelector.beta\\.kubernetes\\.io/os"
    value = "linux"
    type  = "string"
  }

  set {
    name  = "defaultBackend.nodeSelector.beta\\.kubernetes\\.io/os"
    value = "linux"
    type  = "string"
  }

  set {
    name  = "controller.admissionWebhooks.patch.nodeSelector.beta\\.kubernetes\\.io/os"
    value = "linux"
    type  = "string"
  }
}

resource "helm_release" "cert-manager" {
  name       = "cert-manager"
  repository = "https://charts.jetstack.io"
  chart      = "cert-manager"
  version    = "v0.16.1"

  set {
    name  = "installCRDs"
    value = "true"
    type  = "string"
  }

  set {
    name  = "nodeSelector.kubernetes\\.io/os"
    value = "linux"
    type  = "string"
  }

  set {
    name  = "webhook.nodeSelector.kubernetes\\.io/os"
    value = "linux"
    type  = "string"
  }

  set {
    name  = "cainjector.nodeSelector.kubernetes\\.io/os"
    value = "linux"
    type  = "string"
  }
}
