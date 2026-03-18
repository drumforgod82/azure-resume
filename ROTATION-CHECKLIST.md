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

Goal: rotate storage keys used for direct blob access or deployment scripts.

### Safe sequence

1. Open Azure Portal.
2. Open the Storage Account.
3. Go to `Access keys`.
4. Regenerate `key2` first.
5. Update anything that uses `key2`.
6. Test uploads or access.
7. Regenerate the old `key1`.

### Verify

Check:

- static site still loads
- blob uploads still work
- frontend deployments still succeed

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
