# Secret Rotation Checklist

Use this checklist to rotate secrets safely without exposing values in Git, GitHub, logs, or chat.

## Core Rule

Never paste any of these into source control, workflow files, browser JavaScript, or chat:

- connection strings
- account keys
- function keys
- SAS URLs
- client secrets

## Recommended Order

Rotate secrets in this order:

1. Cosmos DB keys
2. Function keys
3. Storage account keys
4. GitHub `AZURE_CREDENTIALS`

This order keeps the resume site working while you cut services over to new credentials.

## 1. Cosmos DB Key Rotation

Goal: rotate the Cosmos DB key used by the Azure Function without downtime.

### Safe sequence

1. Open Azure Portal.
2. Go to your Cosmos DB account.
3. Open `Keys`.
4. Regenerate the `Secondary Key`.
5. Build a new connection string using the new secondary key.
6. Open the Azure Function App.
7. Go to `Configuration` or `Environment variables`.
8. Update `AzureResumeConnectionString` to use the new secondary-based connection string.
9. Save the configuration.
10. Wait for the Function App to restart.
11. Test the live function endpoint.
12. If the endpoint works, go back to Cosmos DB.
13. Regenerate the old `Primary Key`.

### Verify

Open the API endpoint in a browser:

```text
https://getresumecounterjamesdean.azurewebsites.net/api/GetResumeCounter
```

You should get JSON like:

```json
{
  "id": "1",
  "count": 123
}
```

### Azure CLI

Run this yourself and do not paste the output:

```bash
az cosmosdb keys regenerate \
  -g azureresume-rg \
  -n <COSMOS_ACCOUNT_NAME> \
  --key-kind secondary
```

## 2. Function Key Rotation

Goal: remove old keys and prevent any leaked function keys from being reused.

### If your function is `Anonymous`

The safest setup for this project is to keep the public counter endpoint anonymous and not use function keys in the frontend at all.

### Steps

1. Open Azure Portal.
2. Open the Function App.
3. Go to `Functions`.
4. Open the HTTP-triggered function.
5. Open `Function Keys`.
6. Open `Host Keys`.
7. Regenerate or delete any key that may have been exposed.
8. Confirm the frontend does not use a `?code=` query string anymore.

### Verify

Check [`frontend/main.js`](/Users/JamesDean/Git/azure-resume/frontend/main.js) and confirm the API URL does not include a function key.

## 3. Storage Account Key Rotation

Goal: rotate storage keys used by the Function App runtime and the deployed package without breaking the counter.

### Important for this project

For this repo, rotating the storage account keys affects more than one setting.

If you only update `AzureWebJobsStorage`, the Function App can still fail.

After rotating a storage key, you must review all of these in the Function App:

- `AzureWebJobsStorage`
- `WEBSITE_CONTENTAZUREFILECONNECTIONSTRING`
- `WEBSITE_RUN_FROM_PACKAGE`

Why this matters:

- `AzureWebJobsStorage` is used by the Azure Functions runtime
- `WEBSITE_CONTENTAZUREFILECONNECTIONSTRING` is used by the Function App content storage
- `WEBSITE_RUN_FROM_PACKAGE` points to the deployed zip package in blob storage, and its SAS URL can break after key rotation

### Beginner-safe sequence

Follow these steps in order. Do not skip the verification step before rotating the other key.

1. Open Azure Portal.
2. Go to the storage account `azureresumestoragejames`.
3. Open `Access keys`.
4. Choose the key you want to rotate first.
5. Copy the full `Connection string` for the other key that is still valid.
6. Open the Function App `GetResumeCounterJamesDean`.
7. Open `Configuration` or `Environment variables`.
8. Find `AzureWebJobsStorage`.
9. Replace it with the full storage connection string from the still-valid key.
10. Find `WEBSITE_CONTENTAZUREFILECONNECTIONSTRING`.
11. Replace it with the same full storage connection string.
12. Save the configuration changes.
13. Restart the Function App.
14. Test the counter API endpoint.
15. If the API returns `503` or `ServiceUnavailable`, check `WEBSITE_RUN_FROM_PACKAGE`.
16. If `WEBSITE_RUN_FROM_PACKAGE` contains a blob URL with a SAS token, redeploy the backend or generate a fresh SAS URL for the package blob and update the setting.
17. Restart the Function App again.
18. Test the counter API endpoint again.
19. Only after the API is working, go back to the storage account.
20. Regenerate the old key.

### What to replace

Use the full connection string, not just the raw key.

Correct format:

```text
DefaultEndpointsProtocol=https;AccountName=azureresumestoragejames;AccountKey=...;EndpointSuffix=core.windows.net
```

Do not paste only this:

```text
AccountKey=...
```

### How to tell what broke

Use these symptoms as a shortcut:

- `ServiceUnavailable` or `503 Site Unavailable`: usually `AzureWebJobsStorage`, `WEBSITE_CONTENTAZUREFILECONNECTIONSTRING`, or `WEBSITE_RUN_FROM_PACKAGE`
- `403 AuthenticationFailed` on the package blob URL: `WEBSITE_RUN_FROM_PACKAGE` has an expired or invalid SAS URL
- Function App starts but the API still returns `500`: the host is running, but another app setting may still be stale

### Verify

Check the API directly first:

```text
https://getresumecounterjamesdean.azurewebsites.net/api/GetResumeCounter
```

Expected result:

```json
{
  "id": "1",
  "count": 123
}
```

Then check:

- static site still loads
- counter API returns JSON instead of `503` or `500`
- blob package URL is still valid if you use `WEBSITE_RUN_FROM_PACKAGE`
- frontend deployments still succeed

### If the API still fails after storage key rotation

This project also depends on Cosmos DB.

If the Function App now starts but the API returns `500 Internal Server Error`, check:

- `AzureResumeConnectionString`

That setting uses the Cosmos DB connection string, not the storage connection string. If the Cosmos key was rotated earlier and not updated, the counter will still fail even though the storage fix is correct.

### Recommendation

Prefer Azure RBAC where possible instead of long-lived storage account keys.

## 4. GitHub `AZURE_CREDENTIALS` Rotation

Goal: rotate the service principal secret used by GitHub Actions.

### Steps

1. Open Azure Portal.
2. Go to `Microsoft Entra ID`.
3. Open `App registrations`.
4. Find the app used by GitHub Actions.
5. Open `Certificates & secrets`.
6. Create a new client secret.
7. Copy the secret once and store it privately.
8. In GitHub, open the repo.
9. Go to `Settings` -> `Secrets and variables` -> `Actions`.
10. Replace `AZURE_CREDENTIALS` with a new JSON payload that uses the new client secret.
11. Run a workflow to confirm Azure login still works.
12. Delete the old client secret from Azure.

### Verify

Run or rerun:

- frontend deployment workflow
- backend deployment workflow

Both should log into Azure successfully.

## 5. Post-Rotation Verification

After all rotations:

1. Open the live website.
2. Confirm the profile image, JavaScript, and styles load normally.
3. Confirm the counter API returns JSON.
4. Confirm the counter appears on the page.
5. Confirm GitHub Actions frontend deploy succeeds.
6. Confirm GitHub Actions backend deploy succeeds.
7. Confirm there are no secrets in repo files or frontend code.

## 6. Safe Habits

Use these habits going forward:

- rotate the secondary credential first
- cut the app over to the new secondary
- verify service health
- rotate the old primary
- avoid embedding secrets in frontend code
- avoid pasting secrets into terminal screenshots, chat, or tickets
- prefer managed identity or RBAC over account keys when possible

## 7. Project-Specific Secrets To Review

For this repo, review and rotate any of these if they were exposed:

- Cosmos DB connection string
- old Function keys
- long-lived SAS URLs
- Storage account keys
- GitHub service principal client secret

## 8. When To Ask For Help

You can safely ask for help without exposing values.

Good example:

```text
I rotated the Cosmos secondary key and updated AzureResumeConnectionString. What should I test next?
```

Bad example:

```text
Here is my new Cosmos connection string: ...
```
