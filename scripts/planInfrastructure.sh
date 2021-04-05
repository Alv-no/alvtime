#!/usr/bin/env bash

set -e

cd packages/infrastructure/stage-1
source plan.sh

cd ../stage-2
source plan.sh

cd ../../..
