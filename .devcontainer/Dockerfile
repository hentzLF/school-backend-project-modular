FROM mcr.microsoft.com/devcontainers/dotnet:10.0

RUN apt-get update && apt-get install -y --no-install-recommends \
    git \
    curl \
    wget \
    jq \
    less \
    nano \
    vim \
    procps \
    sudo \
    && apt-get clean && rm -rf /var/lib/apt/lists/*

# Playwright browser dependencies (for E2E tests)
RUN apt-get update && apt-get install -y --no-install-recommends \
    libnss3 \
    libnspr4 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libcups2 \
    libdrm2 \
    libxkbcommon0 \
    libatspi2.0-0 \
    libxcomposite1 \
    libxdamage1 \
    libxfixes3 \
    libxrandr2 \
    libgbm1 \
    libpango-1.0-0 \
    libcairo2 \
    libasound2 \
    && apt-get clean && rm -rf /var/lib/apt/lists/*

USER vscode

# Install EF Core CLI tool
RUN dotnet tool install --global dotnet-ef

ENV PATH="${PATH}:/home/vscode/.dotnet/tools"

WORKDIR /workspace
