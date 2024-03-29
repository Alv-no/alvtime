#-------------------------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See https://go.microsoft.com/fwlink/?linkid=2090316 for license information.
#-------------------------------------------------------------------------------------------------------------
ARG VARIANT="5.0"
FROM mcr.microsoft.com/dotnet/sdk:${VARIANT}

# This Dockerfile adds a non-root user with sudo access. Use the "remoteUser"
# property in devcontainer.json to use it. On Linux, the container user's GID/UIDs
# will be updated to match your local UID/GID (when using the dockerFile property).
# See https://aka.ms/vscode-remote/containers/non-root-user for details.
ARG USERNAME=vscode
ARG USER_UID=1000
ARG USER_GID=$USER_UID

# Options for common package install script
ARG INSTALL_ZSH="true"
ARG UPGRADE_PACKAGES="true"
ARG COMMON_SCRIPT_SOURCE="https://raw.githubusercontent.com/microsoft/vscode-dev-containers/v0.128.0/script-library/common-debian.sh"
ARG COMMON_SCRIPT_SHA="a6bfacc5c9c6c3706adc8788bf70182729767955b7a5509598ac205ce6847e1e"

# [Optional] Settings for installing Node.js.
ARG INSTALL_NODE="true"
ARG NODE_SCRIPT_SOURCE="https://raw.githubusercontent.com/microsoft/vscode-dev-containers/master/script-library/node-debian.sh"
ARG NODE_SCRIPT_SHA="dev-mode"
ARG NODE_VERSION="lts/*"
ENV NVM_DIR=/usr/local/share/nvm
# Have nvm create a "current" symlink and add to path to work around https://github.com/microsoft/vscode-remote-release/issues/3224
ENV NVM_SYMLINK_CURRENT=true
ENV PATH=${NVM_DIR}/current/bin:${PATH}

# [Optional] Install the Azure CLI
ARG INSTALL_AZURE_CLI="false"

# Configure apt and install packages
RUN apt-get update \
  && export DEBIAN_FRONTEND=noninteractive \
  #
  # Verify git, common tools / libs installed, add/modify non-root user, optionally install zsh
  && apt-get -y install --no-install-recommends curl ca-certificates 2>&1 \
  && curl -sSL ${COMMON_SCRIPT_SOURCE} -o /tmp/common-setup.sh \
  && ([ "${COMMON_SCRIPT_SHA}" = "dev-mode" ] || (echo "${COMMON_SCRIPT_SHA} */tmp/common-setup.sh" | sha256sum -c -)) \
  && /bin/bash /tmp/common-setup.sh "${INSTALL_ZSH}" "${USERNAME}" "${USER_UID}" "${USER_GID}" "${UPGRADE_PACKAGES}" \
  #
  # Clean up
  && apt-get autoremove -y \
  && apt-get clean -y \
  && rm -f /tmp/common-setup.sh /tmp/node-setup.sh \
  && rm -rf /var/lib/apt/lists/*

WORKDIR /usr/src/app

COPY ["AlvTimeWebApi/AlvTimeWebApi.csproj", "AlvTimeWebApi/"]
COPY ["/AlvTime.Business/AlvTime.Business.csproj", "AlvTime.Business/"]
RUN dotnet restore "AlvTimeWebApi/AlvTimeWebApi.csproj"
