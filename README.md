# CI/CD with GitHub Actions — Todo App

A full-stack **Todo List** application built to learn and demonstrate CI/CD using GitHub Actions, with automated deployments to Azure.

---

## What the project does

- A simple Todo app where you can **add**, **complete**, and **delete** tasks
- Every push to `main` automatically builds, tests, and deploys the app to Azure
- Every Pull Request triggers a CI build and test run — preventing broken code from merging
- All Azure infrastructure is defined as code using **Bicep** and deployed via a GitHub Actions workflow

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend API | ASP.NET Core 8 Web API (.NET 8) |
| Frontend | Angular 19 (standalone components) |
| Database | In-memory EF Core (no external DB needed) |
| Unit Tests | xUnit |
| Infrastructure as Code | Azure Bicep |
| CI/CD | GitHub Actions |
| Cloud | Azure App Service (backend) + Azure Static Web Apps (frontend) |
| Auth (CI/CD→Azure) | OIDC / Federated Identity (no stored passwords) |

---

## Project Structure

```
/
├── backend/
│   ├── TodoApi/              .NET 8 Web API
│   │   ├── Controllers/      TodosController — CRUD endpoints
│   │   ├── Models/           TodoItem model
│   │   ├── Data/             EF Core DbContext
│   │   └── Program.cs        App setup — CORS, EF, Swagger
│   └── TodoApi.Tests/        xUnit unit tests
├── frontend/
│   └── src/
│       ├── app/
│       │   ├── app.component.*       Main Todo UI
│       │   ├── app.config.ts         Angular app config
│       │   └── services/
│       │       └── todo.service.ts   HttpClient calls to the API
│       └── environments/
│           ├── environment.ts        Dev — points to localhost:5140
│           └── environment.prod.ts   Prod — points to Azure App Service URL
├── infra/
│   ├── main.bicep            Entry point — subscription-scoped deployment
│   ├── main.bicepparam       Parameter values (app name, region, custom domain)
│   └── modules/
│       ├── appserviceplan.bicep   App Service Plan (free F1, Linux)
│       ├── webapp.bicep           .NET 8 Web App + CORS config
│       └── staticwebapp.bicep     Static Web App + optional custom domain
└── .github/
    └── workflows/
        ├── infra.yml      Deploys Bicep on infra changes
        ├── backend.yml    Builds, tests, and deploys the .NET API
        └── frontend.yml   Builds and deploys the Angular app
```

---

## API Endpoints

Base URL locally: `http://localhost:5140`

| Method | Path | Description |
|---|---|---|
| `GET` | `/api/todos` | Get all todos |
| `GET` | `/api/todos/{id}` | Get a single todo |
| `POST` | `/api/todos` | Create a new todo |
| `PUT` | `/api/todos/{id}` | Update a todo (title / completed state) |
| `DELETE` | `/api/todos/{id}` | Delete a todo |

Swagger UI is available at `http://localhost:5140/swagger` when running locally.

---

## Prerequisites

Make sure the following are installed before you begin:

| Tool | Version | Install |
|---|---|---|
| .NET SDK | 8.0+ | https://dotnet.microsoft.com/download |
| Node.js | 20+ | https://nodejs.org |
| Angular CLI | 19+ | `npm install -g @angular/cli` |
| Azure CLI | Latest | https://learn.microsoft.com/cli/azure/install-azure-cli |
| Git | Any | https://git-scm.com |

---

## Running the Backend Locally

```bash
# 1. Navigate to the API project
cd backend/TodoApi

# 2. Restore packages and run
dotnet run --launch-profile http
```

The API starts at **http://localhost:5140**  
Open **http://localhost:5140/swagger** to explore and test all endpoints interactively.

To run the unit tests:
```bash
cd backend
dotnet test
```

---

## Running the Frontend Locally

```bash
# 1. Navigate to the frontend folder
cd frontend

# 2. Install dependencies
npm install

# 3. Start the dev server
ng serve
```

Open **http://localhost:4200** in your browser.

> The Angular dev environment is configured to call the backend at `http://localhost:5140`.  
> Make sure the backend is running first (see above) before opening the frontend.

---

## Setting Up the Full CI/CD Pipeline (New Contributor Guide)

Follow these steps once to get everything running from scratch.

### Step 1 — Fork / clone the repository

```bash
git clone https://github.com/NishiketOrg/CICD-GithubActions-Azure.git
cd CICD-GithubActions-Azure
```

### Step 2 — Choose a unique app name

Open `infra/main.bicepparam` and update `appName` to something globally unique (used as the Azure App Service name — must be unique across all Azure):

```
param appName = 'todo-cicd-yourname'
```

Also update `frontend/src/environments/environment.prod.ts` with the same name:
```ts
apiBaseUrl: 'https://todo-cicd-yourname.azurewebsites.net'
```

### Step 3 — Login to Azure

```bash
az login
```

### Step 4 — Create an App Registration for OIDC

This is a one-time setup that lets GitHub Actions authenticate to Azure **without storing any passwords**.

```bash
# Create the App Registration — copy the "appId" from the output
az ad app create --display-name "cicd-demo-github"

# Create a Service Principal for it
az ad sp create --id <appId>
```

Add a federated credential so Azure trusts tokens from your GitHub repo's `main` branch:
```bash
az ad app federated-credential create --id <appId> --parameters '{
  "name": "github-main",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:YOUR_ORG_OR_USERNAME/YOUR_REPO_NAME:ref:refs/heads/main",
  "audiences": ["api://AzureADTokenExchange"]
}'
```

Add a second credential for Pull Requests:
```bash
az ad app federated-credential create --id <appId> --parameters '{
  "name": "github-pr",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:YOUR_ORG_OR_USERNAME/YOUR_REPO_NAME:pull_request",
  "audiences": ["api://AzureADTokenExchange"]
}'
```

Get your subscription and tenant IDs:
```bash
az account show --query "{subscriptionId:id, tenantId:tenantId}" -o table
```

Grant the App Registration Contributor access so it can create resources:
```bash
az role assignment create --assignee <appId> --role Contributor \
  --scope /subscriptions/<subscriptionId>
```

### Step 5 — Add GitHub Secrets

Go to your GitHub repo → **Settings → Secrets and variables → Actions → New repository secret**

Add these 5 secrets:

| Secret Name | Value |
|---|---|
| `AZURE_CLIENT_ID` | `appId` from Step 4 |
| `AZURE_TENANT_ID` | `tenantId` from Step 4 |
| `AZURE_SUBSCRIPTION_ID` | `subscriptionId` from Step 4 |
| `AZURE_WEBAPP_NAME` | Your app name (e.g. `todo-cicd-yourname`) |
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | Get this after running the infra workflow (see Step 6) |

### Step 6 — Trigger the Infrastructure workflow

Commit and push your changes:
```bash
git add .
git commit -m "config: set my app name and Azure URL"
git push
```

Go to **GitHub → Actions → Infrastructure** workflow. It will run automatically and create all Azure resources (resource group, App Service Plan, App Service, Static Web App).

When it finishes, get the Static Web App deployment token:
```bash
az staticwebapp secrets list \
  --name todo-cicd-yourname-swa \
  --resource-group rg-cicd-demo \
  --query "properties.apiKey" -o tsv
```

Add the output as the `AZURE_STATIC_WEB_APPS_API_TOKEN` GitHub secret (Step 5).

### Step 7 — Trigger the app workflows

Make any small change to `backend/` or `frontend/` and push — this triggers the backend and frontend CI/CD workflows which build, test, and deploy to Azure.

After the workflows complete:
- **Backend API**: `https://todo-cicd-yourname.azurewebsites.net/swagger`
- **Frontend app**: the Static Web App URL shown in the infra workflow output

---

## CI/CD Workflow Diagram

```
Push to feature branch
        │
        ▼
  CI runs (build + test)
        │
    Pass? ──No──▶ ❌ PR blocked from merging
        │
       Yes
        │
        ▼
   Open Pull Request
        │
        ▼
  CI runs again on PR
        │
        ▼
   Merge to main
        │
        ├──▶ backend/**  changed  ──▶ Build → Test → Deploy to App Service
        ├──▶ frontend/** changed  ──▶ Build → Deploy to Static Web Apps
        └──▶ infra/**    changed  ──▶ Deploy Bicep to Azure
```

---

## Custom Domain (Optional)

To connect a custom domain (e.g. `www.mytodoapp.com`) to the frontend:

1. Purchase a domain from any registrar (Namecheap, Cloudflare, GoDaddy, etc.)
2. Add a `CNAME` DNS record at your registrar: `www` → `<your-swa>.azurestaticapps.net`
3. Add a `TXT` validation record (get the value from Azure CLI — see below)
4. Update `infra/main.bicepparam`:
   ```
   param customDomain = 'www.mytodoapp.com'
   ```
5. Push — the Infrastructure workflow registers the domain and Azure provisions a free SSL certificate automatically

To get the DNS validation token:
```bash
az staticwebapp hostname show \
  --name todo-cicd-yourname-swa \
  --resource-group rg-cicd-demo \
  --hostname www.mytodoapp.com \
  --query "validationToken" -o tsv
```

---

## OIDC — Why No Passwords?

GitHub Actions authenticates to Azure using **OpenID Connect (OIDC)** — no passwords, tokens, or certificates are stored as GitHub Secrets. Here's what happens at runtime:

1. GitHub mints a short-lived JWT: *"I am workflow X in repo Y on branch main"*
2. Azure AD checks the federated credential: *"Is this repo/branch trusted?"* → Yes
3. Azure issues a real access token (valid ~1 hour)
4. The workflow uses that token to deploy
5. Workflow ends → token expires automatically

The only things stored as GitHub Secrets are three **non-sensitive IDs** (`CLIENT_ID`, `TENANT_ID`, `SUBSCRIPTION_ID`) — not passwords.
