#!/usr/bin/env bash

set -e

cd packages/frontend
source build.sh

cd ../adminpanel
source build.sh

cd ../api
source build.sh

cd ../slack-app
source build.sh

cd ../..
