# Azure Resume

This repo hosts a resume website on Azure.

It has 2 parts:

- `frontend/`: static website files deployed to Azure Blob Storage static website hosting
- `backend/`: a `.NET 8` Azure Function that reads and updates a visitor counter in Cosmos DB

## What This Project Does

When someone opens the resume site:

1. `frontend/index.html` loads the page
2. `frontend/main.js` calls the Azure Function endpoint
3. the Azure Function reads a document from Cosmos DB
4. the function increments the `count`
5. the updated count is returned to the browser and shown on the page

## Architecture

- Azure Blob Storage static website: hosts the frontend
- Azure CDN or Front Door custom domain: serves the public website
- Azure Function App on Linux: runs the API
- Azure Cosmos DB: stores the counter document
- GitHub Actions: deploys frontend and backend on pushes to `main`

## Repo Layout

- `frontend/`: HTML, CSS, JavaScript, images
- `frontend/main.js`: calls the live function API
- `backend/api/`: Azure Function project
- `backend/tests/`: unit tests for backend logic
- `.github/workflows/frontend.main.yml`: frontend deployment workflow
- `.github/workflows/backend.main.yml`: backend deployment workflow

## Backend Details

The backend uses:

- `.NET 8`
- Azure Functions v4
- isolated worker model
- HTTP trigger
- Cosmos DB input/output bindings

Important files:

- [`backend/api/GetResumeCounter.cs`](/Users/JamesDean/Git/azure-resume/backend/api/GetResumeCounter.cs)
- [`backend/api/api.csproj`](/Users/JamesDean/Git/azure-resume/backend/api/api.csproj)

The function currently:

- route: `/api/GetResumeCounter`
- auth level: `Anonymous`
- Cosmos DB setting name: `AzureResumeConnectionString`
- database: `AzureResume`
- container: `Counter`
- document id: `1`
- partition key: `1`

## Frontend Details

The frontend currently calls:

```js
https://getresumecounterjamesdean.azurewebsites.net/api/GetResumeCounter
```

If you create your own site, this URL must be changed in [`frontend/main.js`](/Users/JamesDean/Git/azure-resume/frontend/main.js).

## Required Azure Resources

To build your own version, create:

1. A resource group
2. A storage account with static website enabled
3. A CDN profile and endpoint, or another public frontend layer
4. A Function App on Linux
5. A Cosmos DB account and database/container for the counter
6. A service principal for GitHub Actions deployment

## Cosmos DB Setup

Create:

- database: `AzureResume`
- container: `Counter`
- partition key: `/id`

Create one initial document:

```json
{
  "id": "1",
  "count": 0
}
```

This project expects the counter document to use `id = "1"`.

## Function App Setup

Use these settings on the Function App:

- runtime stack: `DOTNET-ISOLATED|8.0`
- `FUNCTIONS_WORKER_RUNTIME = dotnet-isolated`
- `FUNCTIONS_EXTENSION_VERSION = ~4`
- app setting `AzureResumeConnectionString` = your Cosmos DB connection string

## Local Development

Install:

- `.NET 8 SDK`
- Azure Functions Core Tools
- Azure CLI

Helpful commands:

```bash
dotnet test backend/tests/tests.csproj
```

```bash
dotnet publish backend/api/api.csproj --configuration Release --output backend/api/publish
```

To run locally, create `backend/api/local.settings.json` with your local settings.

Example:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "AzureResumeConnectionString": "<your-cosmos-connection-string>"
  }
}
```

Then run the function locally with Azure Functions Core Tools.

## GitHub Actions Setup

This repo uses one secret:

- `AZURE_CREDENTIALS`

That secret should contain the JSON credentials for an Azure service principal with permission to deploy the Function App and update the storage/CDN resources used by the workflows.

## Deployment Flow

### Frontend

[`frontend.main.yml`](/Users/JamesDean/Git/azure-resume/.github/workflows/frontend.main.yml) runs when files in `frontend/` change.

It:

1. logs into Azure
2. uploads the `frontend/` folder to the `$web` container
3. purges the CDN endpoint

### Backend

[`backend.main.yml`](/Users/JamesDean/Git/azure-resume/.github/workflows/backend.main.yml) runs when backend files change.

It:

1. logs into Azure
2. restores dependencies
3. runs unit tests
4. publishes the Azure Function
5. deploys the published package to the Function App

## What To Rename For Your Own Site

If you fork this project, update these values:

- Function App name in [`backend.main.yml`](/Users/JamesDean/Git/azure-resume/.github/workflows/backend.main.yml)
- storage account, CDN profile, CDN endpoint, and resource group in [`frontend.main.yml`](/Users/JamesDean/Git/azure-resume/.github/workflows/frontend.main.yml)
- function URL in [`frontend/main.js`](/Users/JamesDean/Git/azure-resume/frontend/main.js)
- any personal text, images, social links, and resume content in `frontend/`

## Recommended Setup Order

1. Fork or clone the repo
2. Update the resume content in `frontend/`
3. Create the Azure resources
4. Create the Cosmos DB counter document
5. Update the workflow file names/settings for your Azure resource names
6. Update the function URL in `frontend/main.js`
7. Add `AZURE_CREDENTIALS` to GitHub Secrets
8. Push to `main`
9. Verify the frontend site and backend counter endpoint

## Verification Checklist

- `main.js` loads from your site
- the function endpoint returns JSON like:

```json
{
  "id": "1",
  "count": 1
}
```

- the counter number appears on the page
- pushes to `main` trigger both GitHub Actions workflows when relevant files change

## Notes

- This project now uses the Azure Functions isolated worker model on `.NET 8`
- The counter API is anonymous, which is acceptable for a simple public resume counter
- Do not commit secrets, connection strings, account keys, or function keys into the repo
