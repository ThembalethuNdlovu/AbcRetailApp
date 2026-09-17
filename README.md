# ABC Retail — Azure Cloud Retail Platform

A full-stack cloud-native retail web application built for the Cloud Development module, demonstrating Azure Storage integration, Azure Functions, and role-based e-commerce functionality on Azure App Service.

**Live Web App:** https://[yourstudentnumber].azurewebsites.net](https://st10099281ts-dtfdhrbmdqgbfdat.austriaeast-01.azurewebsites.net/
**Live Function App:** [https://abcretail-functions-10099281-[suffix].azurewebsites.net](https://abcretail-functions-10099281-drdfczawgsb7a2cp.austriaeast-01.azurewebsites.net/)
**Module:** Cloud Development
**Projects:** Project 1 — Azure Storage Solution · Project 2 — Integrating Azure Services into a Web Application

---

## Overview

ABC Retail's legacy on-premises system struggled with peak-season transaction volumes, inefficient image storage, and unreliable message queuing. This application addresses those problems using Azure Storage, then expands into a full role-based e-commerce platform using Azure Functions and ASP.NET Core Identity.

| Business Problem | Solution |
|---|---|
| Customer/product data outgrowing the relational DB | **Azure Table Storage** |
| Product images stored inefficiently on network drives | **Azure Blob Storage** |
| Unreliable, non-scalable message queuing | **Azure Queue Storage** |
| No centralised, accessible logging | **Azure File Storage** |
| Discrete operations needing independent scaling | **Azure Functions** (serverless, pay-per-execution) |
| No secure customer accounts or shopping experience | **ASP.NET Core Identity** + Cart/Checkout |

## Features

### Project 1 — Azure Storage Solution
- **Customers** — CRUD for customer profiles (Azure Table Storage)
- **Products** — CRUD for product records, linked to product images (Azure Table Storage + Blob Storage)
- **Media** — Upload, view, and delete product images/multimedia (Azure Blob Storage)
- **Orders** — View and send order/inventory processing messages (Azure Queue Storage)
- **Logs** — View system log files, auto-generated on every queue event (Azure File Storage)

### Project 2 — Integrating Azure Services + Role-Based Web App

**Azure Functions** (separate `.NET 8` isolated-worker Functions project, same repo, same Storage Account):
- `StoreTransaction` — HTTP-triggered, writes to Azure Table Storage
- `UploadBlob` — HTTP-triggered, writes to Azure Blob Storage
- `QueueTransaction` — HTTP-triggered, writes **and** reads (peeks) Azure Queue Storage
- `WriteFile` — HTTP-triggered, writes a log file to Azure Files

**Role-Based Access Control** (ASP.NET Core Identity + SQL Server/Azure SQL):
- Customer registration and login
- Two roles: **Admin** (full management access — Customers/Products/Media/Orders/Logs/Dashboard) and **Customer** (storefront access only)
- Admin and Customer areas are fully separated — each role is blocked from the other's functionality via `[Authorize(Roles = "...")]`

**Customer Storefront:**
- **Catalog** — browse/search products (read-only view of Products, separate from Admin's management controller)
- **Cart** — session-based shopping cart (add/update/remove items)
- **Checkout** — places an order (writes to a new `Orders` Azure Table + sends a message to the existing Queue), then shows an order confirmation
- **Order History** — customers can view their own past orders

**Admin Dashboard:**
- Live stats: customer count, product count, order count, total revenue, media/log file counts
- Low-stock alerts (products with ≤3 units)
- Recent orders feed
- Quick links to all management areas

**Visual Redesign:**
- Custom navy/orange theme, Google Fonts (Poppins/Inter), Bootstrap Icons
- Hero banners, card hover effects, redesigned navbar split by role
- Storefront-style product grid for customers, stat-card dashboard for admins

## Tech Stack

- **ASP.NET Core MVC** (.NET 8) — main web application
- **Azure Functions** (.NET 8, isolated worker model) — integrated serverless functions
- **ASP.NET Core Identity** + **Entity Framework Core** — authentication, roles
- **SQL Server (LocalDB)** locally / **Azure SQL Database** in production — Identity data store
- **Azure.Data.Tables**, **Azure.Storage.Blobs**, **Azure.Storage.Queues**, **Azure.Storage.Files.Shares** — Azure Storage SDKs
- **Bootstrap 5** + **Bootstrap Icons** — UI
- Deployed to **Azure App Service** (web app) and **Azure Functions Flex Consumption** (functions)

## Project Structure
AbcRetailApp/ # Main web application
├── Controllers/
│ ├── HomeController.cs
│ ├── DashboardController.cs # Admin-only stats dashboard
│ ├── CustomersController.cs # Admin-only — Table Storage CRUD
│ ├── ProductsController.cs # Admin-only — Table Storage CRUD + Blob link
│ ├── MediaController.cs # Admin-only — Blob Storage upload/gallery
│ ├── OrdersController.cs # Admin-only — Queue Storage messages
│ ├── LogsController.cs # Admin-only — File Storage logs
│ ├── CatalogController.cs # Customer-only — read-only product browsing
│ ├── CartController.cs # Customer-only — session-based cart
│ └── CheckoutController.cs # Customer-only — order placement + history
├── Models/
│ ├── CustomerProfileEntity.cs
│ ├── ProductEntity.cs
│ ├── OrderEntity.cs
│ └── CartItem.cs
├── Data/
│ ├── ApplicationDbContext.cs # Identity DbContext (SQL Server)
│ └── DbSeeder.cs # Seeds Admin/Customer roles + default admin
├── Services/
│ ├── TableStorageService.cs
│ ├── BlobStorageService.cs
│ ├── QueueStorageService.cs
│ ├── FileStorageService.cs
│ ├── SessionExtensions.cs # Cart session serialization helper
│ └── NoOpEmailSender.cs # Placeholder IEmailSender for Identity
├── Areas/Identity/Pages/Account/ # Scaffolded Register/Login/Logout pages
├── Views/
├── Migrations/ # EF Core Identity schema migrations
├── Program.cs
└── appsettings.json

AbcRetailApp.Functions/ # Azure Functions project
├── TableFunction.cs # StoreTransaction
├── BlobFunction.cs # UploadBlob
├── QueueFunction.cs # QueueTransaction (write + read)
├── FileFunction.cs # WriteFile
├── TableEntity.cs # TransactionEntity model
└── local.settings.json # (gitignored — local config only)


## Architecture Notes

- Each Azure Storage service is wrapped in its own service class, registered as a singleton via dependency injection — both the web app and the Functions project call the **same underlying Storage Account**, demonstrating genuine integration rather than isolated silos.
- **Role separation**: Admin and Customer functionality is split into entirely separate controllers (e.g. `ProductsController` for Admin CRUD vs. `CatalogController` for Customer browsing) rather than branching logic inside shared controllers — this keeps authorization boundaries clean and auditable.
- **Cart** is held in ASP.NET Core Session (server-side, per-browser-session) rather than Azure Storage, since cart contents are transient until checkout.
- **Checkout** ties the new `Orders` Azure Table directly into the existing `order-processing` Azure Queue, so a customer order flows through the same messaging pipeline used elsewhere in the app.
- **Identity** uses its own SQL Server/Azure SQL database, kept deliberately separate from the Azure Storage account — authentication data and business/retail data are different concerns.
- **Azure Functions** use the isolated worker model (.NET 8) with `AuthorizationLevel.Anonymous` for straightforward testing; all four read their Storage connection string from a flat `AzureStorage` configuration key.

## Getting Started (Local Development)

### Prerequisites
- Visual Studio 2022+ with ASP.NET/web development and Azure development workloads
- .NET 8 SDK
- SQL Server LocalDB (ships with Visual Studio)
- Azure Functions Core Tools (installed automatically with the Azure development workload)
- An Azure account with an active Storage Account

### Setup — Web App

1. Clone the repository and open `AbcRetailApp.sln`.
2. Add your Azure Storage connection string via **User Secrets** (right-click `AbcRetailApp` → Manage User Secrets):
```json
   {
     "ConnectionStrings": {
       "AzureStorage": "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net",
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AbcRetailAppIdentity;Trusted_Connection=True;MultipleActiveResultSets=true"
     }
   }
```
3. In Package Manager Console: `Update-Database` (creates the Identity schema locally).
4. Press **F5**. Roles and a default admin account (`admin@abcretail.com` / `Admin@123` — **change this before any public deployment**) are seeded automatically on first run.

### Setup — Azure Functions

1. In `AbcRetailApp.Functions/local.settings.json` (gitignored, create if missing):
```json
   {
     "IsEncrypted": false,
     "Values": {
       "AzureWebJobsStorage": "<your real Azure Storage connection string>",
       "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
       "AzureStorage": "<your real Azure Storage connection string>"
     }
   }
```
2. Set both `AbcRetailApp` and `AbcRetailApp.Functions` as startup projects to run them together, or run the Functions project independently for isolated testing.

## Deployment

- **Web App**: Azure App Service (.NET 8), Standard/LRS Storage Account, Azure SQL Database (Basic tier) for Identity. Connection strings set via App Service **Environment variables** (`AzureStorage` as an App setting, `DefaultConnection` as a Connection string).
- **Function App**: Azure Functions on a **Flex Consumption** plan (pay-per-execution, scales to zero) for cost-effectiveness. `AzureStorage` set as an App setting; `AzureWebJobsStorage` configured automatically from the linked Storage Account.
- Both are published directly from Visual Studio via **Publish → Azure**.

## Design Considerations

- **Scalability**: Table, Blob, and File Storage scale automatically with data volume; Queue Storage decouples order processing from web traffic spikes; Azure Functions on a Consumption/Flex plan scale independently of the main web app.
- **Reliability**: Queue Storage provides durable message delivery; role-based access control prevents unauthorized actions between Admin and Customer users.
- **Cost-effectiveness**: Standard/LRS Storage, Basic-tier Azure SQL, and a Flex Consumption Functions plan (scale-to-zero billing) were chosen throughout to minimise cost for a workload of this scale.

## Author

Thembalethu Ndlovu
Module: Cloud Development
