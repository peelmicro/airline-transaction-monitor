FROM jenkins/jenkins:lts

# Switch to root to install tools
USER root

# Install prerequisites + ICU library (required by .NET for globalization)
RUN apt-get update && apt-get install -y \
    curl \
    wget \
    gnupg \
    apt-transport-https \
    libicu-dev \
    && rm -rf /var/lib/apt/lists/*

# Install .NET SDK 10.0 via install script (avoids GPG/apt issues on Debian Trixie)
RUN curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin \
    --channel 10.0 \
    --install-dir /usr/share/dotnet
ENV PATH="$PATH:/usr/share/dotnet"
ENV DOTNET_ROOT=/usr/share/dotnet

# Install dotnet-sonarscanner global tool
RUN dotnet tool install --global dotnet-sonarscanner
ENV PATH="$PATH:/root/.dotnet/tools"

# Install Node.js 24
RUN curl -fsSL https://deb.nodesource.com/setup_24.x | bash - \
    && apt-get install -y nodejs \
    && rm -rf /var/lib/apt/lists/*

# Install Docker CLI (uses host Docker daemon via socket mount)
RUN curl -fsSL https://get.docker.com | sh

# Switch back to jenkins user
USER jenkins

# Skip setup wizard
ENV JAVA_OPTS="-Djenkins.install.runSetupWizard=false"

# Install Jenkins plugins
RUN jenkins-plugin-cli --plugins \
    configuration-as-code \
    job-dsl \
    workflow-aggregator \
    pipeline-stage-view \
    git \
    docker-workflow \
    ws-cleanup \
    pipeline-utility-steps \
    htmlpublisher \
    sonar

# Copy Jenkins Configuration as Code
COPY infra/jenkins/casc.yaml /var/jenkins_home/casc.yaml
ENV CASC_JENKINS_CONFIG=/var/jenkins_home/casc.yaml
