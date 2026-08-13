# ABC Retail — Azure Storage Solution

A web application built for ABC Retail's Cloud Development module project, demonstrating the use of four Azure Storage services to modernize the company's order processing, media handling, event messaging, and logging infrastructure.

**Live app:** [https://[yourstudentnumber].azurewebsites.net](https://st10099281ts-dtfdhrbmdqgbfdat.austriaeast-01.azurewebsites.net/)]
**Module:** Cloud Development
**Project:** Project 1 — Azure Storage Solution

---

## Overview

ABC Retail's legacy on-premises system struggled with peak-season transaction volumes, inefficient image storage on network drives, and unreliable message queuing. This application addresses each of those problems using purpose-built Azure Storage services:

| Business Problem | Azure Service Used |
|---|---|
| Customer/product data outgrowing the relational DB | **Azure Table Storage** |
| Product images stored inefficiently on network drives | **Azure Blob Storage** |
| Unreliable, non-scalable message queuing | **Azure Queue Storage** |
| No centralised, accessible logging | **Azure File Storage** |

## Features

- **Customers** — Create, view, edit, and delete customer profiles (Azure Table Storage)
- **Products** — Create, view, edit, and delete product records, with optional linked product image (Azure Table Storage + Blob Storage)
- **Media** — Upload, view, and delete product images/multimedia (Azure Blob Storage)
- **Orders** — View queued order/inventory processing events; manually push new messages (Azure Queue Storage)
- **Logs** — View system log files, automatically generated whenever a queue event occurs (Azure File Storage)

## Tech Stack

- **ASP.NET Core MVC** (.NET 8)
- **Azure.Data.Tables** — Table Storage SDK
- **Azure.Storage.Blobs** — Blob Storage SDK
- **Azure.Storage.Queues** — Queue Storage SDK
- **Azure.Storage.Files.Shares** — Azure Files SDK
- **Bootstrap 5** — UI styling
- Deployed to **Azure App Service**

## Project Structure

```
AbcRetailApp/
├── Controllers/
│   ├── HomeController.cs
│   ├── CustomersController.cs      # Table Storage CRUD
│   ├── ProductsController.cs       # Table Storage CRUD + Blob link
│   ├── MediaController.cs          # Blob Storage upload/gallery
│   ├── OrdersController.cs         # Queue Storage messages
│   └── LogsController.cs           # File Storage logs
├── Models/
│   ├── CustomerProfileEntity.cs
│   └── ProductEntity.cs
├── Services/
│   ├── TableStorageService.cs
│   ├── BlobStorageService.cs
│   ├── QueueStorageService.cs
│   └── FileStorageService.cs
├── Views/
│   ├── Home/, Customers/, Products/, Media/, Orders/, Logs/, Shared/
├── Program.cs
├── appsettings.json
└── AbcRetailApp.csproj
```

## Architecture Notes

- Each Azure Storage service is wrapped in its own service class (`Services/`) registered as a singleton via dependency injection, so controllers never talk to the Azure SDK directly.
- **Table Storage**: `CustomerProfileEntity` and `ProductEntity` implement `ITableEntity`, stored in separate tables (`CustomerProfiles`, `Products`).
- **Blob Storage**: uploaded files are stored in a `product-images` container with unique GUID-prefixed names to avoid collisions.
- **Queue Storage**: order/inventory events (`Processing Order`, `Stock Update`, `Image Uploaded`, etc.) are sent as JSON messages to the `order-processing` queue. Sending a message automatically triggers a log write.
- **File Storage**: log files are written to a `logs-share` file share, timestamped per file, and are created automatically whenever a queue message is sent (from Products, Media, or manual Orders actions).

## Getting Started (Local Development)

### Prerequisites
- Visual Studio 2022 (or later) with the ASP.NET and web development workload
- .NET 8 SDK
- An Azure account with an active Storage Account

### Setup

1. Clone the repository:
   ```bash
  
   ```
2. Open `AbcRetailApp.sln` in Visual Studio.
3. Add your Azure Storage connection string. **Do not commit real credentials** — use User Secrets:
   - Right-click the project → **Manage User Secrets**
   - Add:
     ```json
     {
       "ConnectionStrings": {
         "AzureStorage": "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net"
       }
     }
     ```
4. Press **F5** to build and run locally.

The app will automatically create the required tables, blob container, queue, and file share on first run if they don't already exist.

## Deployment

Deployed to **Azure App Service** (.NET 8, Standard/LRS Storage Account for cost-effectiveness). The connection string is set via the App Service's **Environment variables / App settings**:

- **Name:** `ConnectionStrings__AzureStorage`
- **Value:** your Azure Storage connection string

Publish directly from Visual Studio via **Right-click project → Publish → Azure → Azure App Service**.

## Design Considerations

- **Scalability**: Table and Blob Storage scale automatically with data volume; Queue Storage decouples order processing from the web app so traffic spikes (e.g. peak shopping seasons) don't bottleneck the system.
- **Reliability**: Azure Queue Storage provides durable, at-least-once message delivery, replacing the unreliable legacy middleware described in the original business case.
- **Cost-effectiveness**: Standard performance tier with Locally Redundant Storage (LRS) was chosen over Premium/geo-redundant options, appropriate for this workload's scale and budget.

## Author
Thembalethu Ndlovu
Student Number: ST10099281
Module: Cloud Development

## Acknowledgements

Portions of this codebase were developed with the assistance of Anthropic's Claude (AI language model) for boilerplate generation, debugging support, and documentation drafting. All code was reviewed, tested, and deployed by the author.
