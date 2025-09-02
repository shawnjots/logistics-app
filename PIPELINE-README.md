# Azure Pipelines Configuration for Logistics Application

This repository includes an `azure-pipelines.yml` file that provides comprehensive CI/CD pipeline support for the Logistics Application.

## Pipeline Overview

The pipeline is structured into multiple stages to handle the diverse technology stack:

### Build Stage
- **BuildBackend**: Builds .NET backend services (API, Identity Server, DB Migrator)
- **BuildDotNetClients**: Builds .NET client applications (Admin App, HTTP Client)  
- **BuildOfficeApp**: Builds Angular frontend application using Bun
- **BuildMauiApp**: Builds cross-platform mobile application using .NET MAUI
- **BuildDockerImages**: Creates Docker images for containerized deployment

### Test Stage
- **RunTests**: Executes unit tests with coverage reporting

### Package Stage
- **CreatePackages**: Combines all artifacts into deployment packages

## Artifacts Generated

The pipeline creates the following artifacts:

1. **backend-services**: Web API, Identity Server, and DB Migrator deployables
2. **client-apps**: Admin app and client libraries
3. **office-app**: Angular frontend application build
4. **mobile-app**: MAUI mobile application packages
5. **docker-images**: Docker container images (optional)
6. **deployment-package**: Complete deployment package

## Configuration

### Prerequisites
- Azure DevOps project with pipeline service
- .NET 9.0 SDK support in build agents
- Container registry service connection (for Docker builds)

### Pipeline Variables
- `buildConfiguration`: Build configuration (default: Release)
- `dotNetVersion`: .NET SDK version (default: 9.0.x)
- `buildDockerImages`: Enable Docker image builds (default: true)

### Trigger Configuration
The pipeline triggers on:
- Push to `prime` or `main` branches
- Pull requests to `prime` or `main` branches
- Excludes documentation-only changes

### Agent Requirements
- **Linux agents**: Backend services, client apps, office app, tests
- **Windows agents**: MAUI mobile applications (requires Windows for iOS/Android workloads)

## Usage

1. **Setup**: Import the pipeline in Azure DevOps pointing to `azure-pipelines.yml`
2. **Configure**: Set up any required service connections (container registry, etc.)
3. **Variables**: Adjust pipeline variables as needed for your environment
4. **Run**: The pipeline will automatically trigger on code changes

## Customization

### Docker Registry
Update the `containerRegistry` field in Docker tasks to point to your container registry service connection.

### Build Agents
Modify the `vmImage` values to use your preferred build agents or self-hosted agents.

### Additional Projects
Add new projects by:
1. Including build/test steps in the appropriate job
2. Adding artifact publishing steps
3. Including in the final packaging stage

## Troubleshooting

### Common Issues
- **.NET version mismatch**: Ensure build agents support the configured .NET version
- **Bun installation**: The pipeline installs Bun dynamically; check agent internet connectivity
- **MAUI workloads**: Windows agents need MAUI workloads; these are installed automatically
- **Docker builds**: Require appropriate service connections and registry permissions

### Support
For pipeline issues, check:
1. Agent capabilities match requirements
2. Service connections are properly configured
3. Variable values are correct for your environment
4. Repository structure matches expected paths