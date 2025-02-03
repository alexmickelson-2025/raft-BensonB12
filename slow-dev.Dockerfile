FROM mcr.microsoft.com/dotnet/sdk:9.0

RUN apt-get update && \
    apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/*

RUN groupadd -g 1000 developer && \
    useradd -r -u 1000 -g developer -m -s /usr/bin/bash developer 

WORKDIR /app

# Copy files before changing ownership
COPY --chown=developer:developer . /app

# Ensure the user has full access
RUN chmod -R 755 /app

USER developer:developer

CMD dotnet run --project website
