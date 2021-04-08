#!/usr/bin/env bash

set -e

echo "Cleaning up temporary terraform files"
cd packages/infrastructure
rm -rf \
  stage-1/.terraform \
  stage-1/.terraform.lock.hcl \
  stage-1/plan \
  stage-2/.terraform \
  stage-2/.terraform.lock.hcl \
  stage-2/plan \
