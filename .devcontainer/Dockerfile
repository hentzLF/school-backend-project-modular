FROM mcr.microsoft.com/devcontainers/dotnet:10.0

RUN apt-get update && apt-get install -y --no-install-recommends \
    git \
    curl \
    wget \
    jq \
    less \
    nano \
    vim \
    tmux \
    procps \
    sudo \
    && apt-get clean && rm -rf /var/lib/apt/lists/*

USER vscode

# Install EF Core CLI tool
RUN dotnet tool install --global dotnet-ef

ENV PATH="${PATH}:/home/vscode/.dotnet/tools"

WORKDIR /workspace
