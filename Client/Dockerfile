# syntax=docker/dockerfile:1

################################################################################
# 1. BUILD STAGE
################################################################################
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /source

# Copy your source code into the container
COPY . .

# Build and publish the application. 
# This outputs the final compiled static files into /app/publish
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish "Client/Client.csproj" -c Release -o /app/publish

################################################################################
# 2. RUNTIME STAGE (Serve static files with Nginx)
################################################################################
# We use Alpine Linux with Nginx because it is incredibly small and fast
FROM nginx:alpine AS final

# Set the working directory to Nginx's default public HTML folder
WORKDIR /usr/share/nginx/html

# Copy ONLY the 'wwwroot' folder from the build stage into Nginx.
# This contains your index.html, css, and the _framework folder with your WASM files.
COPY --from=build /app/publish/wwwroot .

# COPY YOUR CUSTOM NGINX CONFIGURATION
COPY Client/nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80 so the outside world can access the web server
EXPOSE 80

# (Optional but recommended) Nginx runs by default, so we don't even need an ENTRYPOINT!