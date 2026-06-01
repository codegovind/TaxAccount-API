# TaxAccount - Multi-Tenant SaaS Accounting Application
## Complete Project Documentation

---

## 📋 PROJECT OVERVIEW

**Name**: TaxAccount  
**Type**: Multi-Tenant SaaS Accounting Web Application  
**Inspired by**: Tally Prime, Zoho Books, QuickBooks  
**Status**: Active Development (v0.1)

**Developer**: Govind R. Tekale  
**GitHub**: [@codegovind](https://github.com/codegovind)  
**Email**: govindsuryawanshi001@gmail.com

---

## 🏗️ ARCHITECTURE OVERVIEW

### Repositories
- **Backend API**: [TaxAccount-API](https://github.com/codegovind/TaxAccount-API) (ID: 1158715213)
- **Frontend**: [TaxAccount-Frontend](https://github.com/codegovind/TaxAccount-Frontend) (ID: 1207870124)
- **CI/CD**: GitHub Actions (Auto-deploy on push to `main`)

### Multi-Tenancy Strategy
- **Model**: Single Database, Multiple Tenants
- **Isolation**: TenantId column on all business tables
- **EF Core**: Global Query Filters for automatic data isolation
- **Service**: TenantService reads TenantId from JWT claims
- **Registration**: Atomic transaction (Tenant → User → Seed Defaults)

---

## 💻 DEVELOPMENT ENVIRONMENT

### Host Machine (Windows 11)
```
OS: Windows 11
RAM: 8GB | CPU: 4 Core | SSD: 256GB
Editor: VS Code
Tools: SSMS, Postman, Angular CLI 21.1.0
Node.js: 20.19.0
```

### Virtual Machine (Ubuntu Server 24.04 - VirtualBox)
```
RAM: 3GB | CPU: 2 Core | Storage: 30GB
Connected via VS Code Remote SSH
Port Forwarding: 2222 → 22 (SSH), 8080 → 8080 (API)
Installed:
  - .NET SDK 8.0.125
  - Docker 29.1.3
  - Nginx 1.24.0
```

### AWS Cloud (ap-south-1 - Mumbai Region)
```
EC2: t3.micro Ubuntu 24.04
Elastic IP: 13.206.23.136

RDS: SQL Server Express (db.t3.micro)
Endpoint: taxaccount-db.cr4oqika4x7j.ap-south-1.rds.amazonaws.com

S3: taxaccount-frontend (static hosting)
CloudFront Frontend: https://d2lrr9q3q8iecd.cloudfront.net
CloudFront API: https://d3dpmdc1qjwvnh.cloudfront.net

SSH Key: taxaccount-key.pem
```

---

## 🔧 BACKEND STACK (.NET 8 Web API)

### Core Technologies
| Component | Technology | Version |
|-----------|-----------|---------|
| Framework | .NET Web API | 8.0 |
| ORM | Entity Framework Core | 8 (Code First) |
| Database | MS SQL Server | Local Dev / AWS RDS Express |
| Authentication | JWT Bearer (Custom) | — |
| Password Hashing | BCrypt.Net-Next | 4.1.0 |
| Logging | Serilog | 10.0.0 |
| Caching | IMemoryCache | In-memory |
| Validation | FluentValidation | 11.3.1 |
| API Documentation | Swagger/OpenAPI | 6.6.2 |

### Project Location
```
Local Dev: ~/dotnetapp/TaxAccount (Ubuntu VM)
Project File: TaxAccount.csproj
Build Output: /out (Docker image)
```

### NuGet Packages
```xml
<PackageReference Include="BCrypt.Net-Next" Version="4.1.0" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.1" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.23" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.6" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.6" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.6" />
<PackageReference Include="Serilog.AspNetCore" Version="10.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.1.1" />
<PackageReference Include="Serilog.Sinks.File" Version="7.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.17.0" />
```

### Folder Structure (Backend)
```
TaxAccount/
├── Authorization/
│   ├── HasPermissionAttribute.cs
│   ├── PermissionHandler.cs
│   └── PermissionRequirement.cs
├── Controllers/
│   ├── AuthController.cs
│   ├── AccountingController.cs
│   ├── CashFlowController.cs
│   ├── ComplianceController.cs
│   ├── ContactController.cs
│   ├── HomeController.cs
│   ├── InvoiceController.cs
│   ├── ProductController.cs
│   ├── PurchaseController.cs
│   └── StockController.cs
├── Data/
│   ├── AppDbContext.cs
│   └── DbSeeder.cs
├── DTOs/
│   ├── Auth/
│   │   ├── RegisterDto.cs
│   │   ├── LoginDto.cs
│   │   └── AuthResponseDto.cs
│   ├── Contact/
│   │   ├── CreateContactDto.cs
│   │   ├── UpdateContactDto.cs
│   │   ├── ContactResponseDto.cs
│   │   └── ContactListDto.cs
│   ├── Invoice/
│   │   ├── CreateInvoiceDto.cs
│   │   ├── UpdateInvoiceDto.cs
│   │   └── InvoiceResponseDto.cs
│   ├── Product/
│   │   ├── CreateProductDto.cs
│   │   ├── UpdateProductDto.cs
│   │   └── ProductDto.cs
│   ├── Purchase/
│   │   ├── CreatePurchaseBillDto.cs
│   │   └── CreatePurchaseOrderDto.cs
│   ├── Stock/
│   │   ├── CreateStockAdjustmentDto.cs
│   │   └── StockAdjustmentResponseDto.cs
│   └── Compliance/
│       └── EWayBillRequestDto.cs
├── Exceptions/
│   ├── AppException.cs
│   ├── NotFoundException.cs
│   ├── UnauthorizedException.cs
│   └── ForbiddenException.cs
├── Helpers/
│   ├── CacheKeys.cs
│   └── GstCalculator.cs
├── Middleware/
│   └── ExceptionMiddleware.cs
├── Migrations/
│   └── (Auto-generated EF Core migrations)
├── Models/
│   ├── Core/
│   │   ├── Tenant.cs
│   │   ├── User.cs
│   │   ├── Role.cs
│   │   ├── Permission.cs
│   │   └── RolePermission.cs
│   ├── Accounting/
│   │   ├── AccountHead.cs
│   │   ├── LedgerEntry.cs
│   │   ├── Payment.cs
│   │   └── (Trial Balance, P&L DTOs)
│   ├── Business/
│   │   ├── Contact.cs
│   │   ├── Product.cs
│   │   ├── Invoice.cs
│   │   ├── InvoiceItem.cs
│   │   ├── PurchaseBill.cs
│   │   ├── PurchaseBillItem.cs
│   │   ├── PurchaseOrder.cs
│   │   ├── PurchaseOrderItem.cs
│   │   ├── StockAdjustment.cs
│   │   ├── TransportDetail.cs
│   │   └── CashFlowEntry.cs
│   └── Compliance/
│       ├── EWayBill.cs
│       └── TenantSetting.cs
├── Services/
│   ├── Core/
│   │   ├── IAuthService.cs / AuthService.cs
│   │   ├── ITenantService.cs / TenantService.cs
│   │   └── IContactService.cs / ContactService.cs
│   ├── Business/
│   │   ├── IProductService.cs / ProductService.cs
│   │   ├── IInvoiceService.cs / InvoiceService.cs
│   │   ├── IPurchaseService.cs / PurchaseService.cs
│   │   └── IStockService.cs / StockService.cs
│   ├── Accounting/
│   │   ├── IAccountingService.cs / AccountingService.cs
│   │   └── ICashFlowService.cs / CashFlowService.cs
│   ├── Compliance/
│   │   ├── IEWayBillService.cs / EWayBillService.cs
│   │   └── ITenantSettingService.cs / TenantSettingService.cs
│   └── Database/
│       └── DbSeeder.cs
├── Validators/
│   ├── CreateProductValidator.cs
│   └── UpdateProductValidator.cs
├── appsettings.json (gitignored)
├── appsettings.Example.json
├── Dockerfile
├── TaxAccount.csproj
├── TaxAccount.sln
└── Program.cs
```

---

## 🗄️ DATABASE SCHEMA

### Core Entities

#### Tenants
```
Id (PK), CompanyName, Email, State, Gstin, 
IsActive, CreatedAt, UpdatedAt
```

#### Users
```
Id (PK), TenantId (FK), FirstName, LastName, Email, 
PasswordHash, RoleId (FK), IsActive, CreatedAt, UpdatedAt
```

#### Roles (Seeded)
```
Id (PK), Name, Description
Values: 1=Owner, 2=Manager, 3=Staff, 4=Auditor
```

#### Permissions (Seeded)
```
Id (PK), Name, Description
Examples:
- products.view, products.create, products.edit, products.delete
- invoices.view, invoices.create, invoices.approve
- reports.view, users.manage, accounts.manage
- contacts.manage, stock.manage
```

#### RolePermissions (Junction)
```
RoleId (FK), PermissionId (FK) [Composite PK]
```

#### Contacts (Multi-tenant)
```
Id (PK), TenantId (FK), Name, Gstin, GstType (enum), 
ContactType (enum: Customer/Vendor), Phone, Address, 
City, State, PinCode, OpeningBalance, IsDefault, 
IsActive, CreatedAt, UpdatedAt
```

#### Products (Multi-tenant)
```
Id (PK), TenantId (FK), Name, SKU, HsnCode, Description, 
PurchasePrice, MarketValue, Price, Stock (Quantity), Unit, 
GSTPercent, IsActive, CreatedAt, UpdatedAt

Computed: ClosingStockValue = Stock * Min(PurchasePrice, MarketValue)
```

#### Invoices (Multi-tenant, Polymorphic)
```
Id (PK), TenantId (FK), InvoiceNumber, InvoiceType (enum: Sale/Purchase), 
InvoiceDate, DueDate, Status (enum), PaymentMethod, EntrySource, 
ContactId (FK, nullable), CreatedByUserId (FK), Notes, 
SubTotal, DiscountAmount, TaxAmount, TotalAmount, 
CreatedAt, UpdatedAt
```

#### InvoiceItems (Multi-tenant)
```
Id (PK), TenantId (FK), InvoiceId (FK), ProductId (FK), 
Description, HsnCode, Quantity, Unit, UnitPrice, 
DiscountPercent, DiscountAmount, TaxPercent, TaxAmount, 
CgstPercent, CgstAmount, SgstPercent, SgstAmount, 
IgstPercent, IgstAmount, TotalAmount
```

#### PurchaseBills (Multi-tenant)
```
Similar to Invoices but specific for purchases
Includes automatic stock updates on creation
```

#### PurchaseOrders (Multi-tenant)
```
Id (PK), TenantId (FK), OrderNumber, OrderDate, ExpectedDate, 
Status (enum), ContactId (FK), CreatedByUserId (FK), 
Notes, SubTotal, DiscountAmount, TaxAmount, TotalAmount, 
CreatedAt, UpdatedAt
Feature: Convert to Bill functionality
```

#### StockAdjustments (Multi-tenant)
```
Id (PK), TenantId (FK), ProductId (FK), Quantity, 
AdjustmentType (enum: Audit/Damage/Devaluation), 
Reason, AdjustmentDate, AdjustedByUserId (FK), CreatedAt
```

#### TransportDetails (Multi-tenant)
```
Id (PK), TenantId (FK), InvoiceId (FK, 1-to-1), 
TransporterId, TransporterName, VehicleNumber, Distance, 
TransportDocNo, Mode (enum)
Ready for E-Way Bill integration
```

#### AccountHeads (Multi-tenant, for Accounting Module)
```
Id (PK), TenantId (FK), Code, Name, Type (enum: Asset/Liability/Equity/Income/Expense), 
ParentId (FK, nullable, for hierarchical structure), 
OpeningBalance, IsActive, CreatedAt, UpdatedAt
```

#### LedgerEntries (Multi-tenant, for Accounting Module)
```
Id (PK), TenantId (FK), AccountHeadId (FK), 
Date, VoucherType (string), VoucherId (int), VoucherNumber, 
Narration, Debit, Credit, CreatedByUserId (FK), CreatedAt
```

#### EWayBills (Multi-tenant)
```
Id (PK), TenantId (FK), InvoiceId (FK), EWayBillNumber, 
GeneratedDate, ValidUntil, Irn, JsonData, CreatedAt
```

#### TenantSettings (Multi-tenant)
```
Id (PK), TenantId (FK), IsEWayBillEnabled, 
IsAccountingEnabled, CreatedAt, UpdatedAt
```

---

## 🔐 AUTHENTICATION & AUTHORIZATION

### JWT Token Configuration
```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key-min-32-chars",
    "Issuer": "TaxAccountAPI",
    "Audience": "TaxAccountApp",
    "ExpiryInMinutes": 480
  }
}
```

### JWT Claims Structure
```
ClaimTypes.NameIdentifier → UserId
ClaimTypes.Email → Email
ClaimTypes.GivenName → FirstName
ClaimTypes.Surname → LastName
ClaimTypes.Role → Role name (Owner, Manager, Staff, Auditor)
Custom Claims:
  - "tenantId" → TenantId
  - "companyName" → CompanyName
  - "permission" → Each permission as separate claim (repeating)
```

### Registration Flow (Atomic Transaction)
```
1. Validate email uniqueness
2. Begin DB transaction
3. Create Tenant record
4. Create User with Owner role (RoleId = 1)
5. DbSeeder.SeedTenantDefaultsAsync() → Creates:
   - Default Cash Customer contact
   - Default account heads (Assets, Liabilities, etc.)
6. Commit transaction (rollback all if any step fails)
7. Return JWT token with tenantId claim
```

### CORS Configuration
```
Allowed Origins:
- http://localhost:4200 (Local Angular dev)
- http://localhost:8080 (Local API)
- http://taxaccount-frontend.s3-website.ap-south-1.amazonaws.com
- https://d2lrr9q3q8iecd.cloudfront.net (Frontend CloudFront)
- https://d3dpmdc1qjwvnh.cloudfront.net (API CloudFront)

Methods: GET, POST, PUT, DELETE, PATCH, OPTIONS
Headers: *
Credentials: Allowed
```

---

## 📡 API ENDPOINTS

### Authentication
```
POST   /api/auth/register           Register new tenant + owner user
POST   /api/auth/login              Login user (returns JWT token)
```

### Home/Dashboard
```
GET    /api/home/dashboard          Get dashboard summary data
```

### Products
```
GET    /api/products                List all products (paginated)
POST   /api/products                Create new product
GET    /api/products/{id}           Get product details
PUT    /api/products/{id}           Update product
DELETE /api/products/{id}           Delete product (soft delete)
```

### Contacts (Customers/Vendors)
```
GET    /api/contacts                List all contacts
GET    /api/contacts/vendors        List vendors only
GET    /api/contacts/customers      List customers only
POST   /api/contacts                Create new contact
GET    /api/contacts/{id}           Get contact details
PUT    /api/contacts/{id}           Update contact
DELETE /api/contacts/{id}           Delete contact (soft delete)
```

### Invoices (Sales)
```
GET    /api/invoice                 List all invoices
POST   /api/invoice                 Create new invoice
GET    /api/invoice/{id}            Get invoice details
PATCH  /api/invoice/{id}/status     Update invoice status
DELETE /api/invoice/{id}            Delete invoice
```

### Purchase Orders
```
GET    /api/purchase/orders         List all purchase orders
POST   /api/purchase/orders         Create new purchase order
GET    /api/purchase/orders/{id}    Get order details
PATCH  /api/purchase/orders/{id}/status  Update order status
POST   /api/purchase/orders/{id}/convert-to-bill  Convert to purchase bill
```

### Purchase Bills
```
GET    /api/purchase/bills          List all purchase bills
POST   /api/purchase/bills          Create new purchase bill
GET    /api/purchase/bills/{id}     Get bill details
DELETE /api/purchase/bills/{id}     Delete bill
```

### Stock Management
```
POST   /api/stock/adjust            Create stock adjustment
GET    /api/stock/{productId}/adjustments  Get adjustment history
GET    /api/stock/{productId}/current      Get current stock
```

### Accounting (New)
```
GET    /api/accounting/accounts              List chart of accounts
POST   /api/accounting/accounts              Create account head
GET    /api/accounting/ledger                Get general ledger
GET    /api/accounting/trial-balance        Get trial balance
GET    /api/accounting/balance-sheet        Get balance sheet
GET    /api/accounting/profit-loss          Get P&L statement
POST   /api/accounting/post-payment         Post payment entry
```

### Compliance (New)
```
POST   /api/compliance/eway-bill            Generate E-Way Bill
GET    /api/compliance/eway-bill/{invoiceId}  Get E-Way Bill details
```

---

## 🎨 FRONTEND STACK (Angular 21)

### Core Technologies
| Component | Technology | Version |
|-----------|-----------|---------|
| Framework | Angular | 21.1.0 |
| Style | Standalone Components + Signals | — |
| Styling | SCSS | — |
| State Management | Angular Signals | — |
| Routing | Lazy-loaded modules | — |
| HTTP | HttpClient + Auth Interceptor | — |
| UI Components | Angular Material | 21.2.6 |
| Package Manager | npm | 10.8.2 |
| Node.js | 20.19.0 | — |

### Project Location
```
Windows: C:\New Start\TaxAccountApp\taxaccount-frontend
Dev Server: http://localhost:4200
Build Output: dist/
```

### Key Angular Patterns
- **Components**: All standalone: true
- **Control Flow**: @if / @for (NOT *ngIf/*ngFor)
- **State**: Signals (signal(), .set(), .update())
- **Auth**: localStorage stores token + user data
- **HTTP**: HttpClient with JWT interceptor
- **Routing**: Lazy-loaded feature modules + standalone components

### Frontend Folder Structure
```
src/app/
├── auth/
│   ├── login/ (standalone)
│   ├── register/ (standalone)
│   └── auth.module.ts
├── core/
│   ├── guards/
│   │   └── auth-guard.ts
│   ├── interceptors/
│   │   └── auth-interceptor.ts
│   ├── models/
│   │   ├── auth.model.ts
│   │   ├── contact.model.ts
│   │   ├── invoice.model.ts
│   │   ├── product.model.ts
│   │   ├── purchase.model.ts
│   │   ├── stock.model.ts
│   │   ├── accounting.model.ts
│   │   ├── compliance.model.ts
│   │   └── user.model.ts
│   └── services/
│       ├── auth.service.ts
│       ├── contact.service.ts
│       ├── home.service.ts
│       ├── invoice.service.ts
│       ├── product.service.ts
│       ├── purchase.service.ts
│       ├── stock.service.ts
│       ├── accounting.service.ts
│       ├── compliance.service.ts
│       └── tenant.service.ts
├── dashboard/
│   ├── dashboard/ (standalone)
│   └── dashboard.module.ts
├── invoices/
│   ├── invoice-list/ (standalone)
│   ├── invoice-create/ (standalone)
│   ├── invoice-detail/ (standalone)
│   └── invoices.module.ts
├── products/
│   ├── product-list/ (standalone)
│   ├── product-create/ (standalone)
│   └── products.module.ts
├── contacts/
│   ├── contact-list/ (standalone)
│   ├── contact-create/ (standalone)
│   └── contacts.module.ts
├── stock/
│   ├── stock-list/ (standalone)
│   ├── stock-adjust/ (standalone)
│   └── stock.module.ts
├── purchase/
│   ├── purchase-list/ (standalone)
│   ├── purchase-create/ (standalone)
│   ├── purchase-detail/ (standalone)
│   ├── order-list/ (standalone)
│   ├── order-create/ (standalone)
│   └── purchase.module.ts
├── accounting/ (New)
│   ├── chart-of-accounts/ (standalone)
│   ├── general-ledger/ (standalone)
│   ├── trial-balance/ (standalone)
│   ├── reports/ (standalone)
│   └── accounting.module.ts
├── compliance/ (New)
│   ├── eway-bill/ (standalone)
│   └── compliance.module.ts
├── settings/ (New)
│   ├── tenant-settings/ (standalone)
│   └── settings.module.ts
├── shared/
│   ├── layout/ (standalone - wraps sidebar + router-outlet)
│   ├── sidebar/ (standalone - @Input isOpen, @Output closeSidebar)
│   ├── quick-add-vendor/ (standalone modal)
│   ├── quick-add-product/ (standalone modal)
│   └── shared.module.ts
├── app.routes.ts
├── app.config.ts
├── app.ts (Main component)
└── app.html (Just <router-outlet />)
```

### App Routes Configuration
```typescript
Routes: [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'auth', loadChildren: () => import('./auth/auth.module') },
  {
    path: '',
    component: AppLayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', loadComponent: () => import('./dashboard/dashboard') },
      { path: 'products', loadChildren: () => import('./products/products-module') },
      { path: 'contacts', loadChildren: () => import('./contacts/contacts-module') },
      { path: 'stock', loadChildren: () => import('./stock/stock-module') },
      { path: 'purchase', loadChildren: () => import('./purchase/purchase-module') },
      { path: 'invoices', loadChildren: () => import('./invoices/invoices.module') },
      { path: 'accounting', loadChildren: () => import('./accounting/accounting-module') },
      { path: 'compliance', loadChildren: () => import('./compliance/compliance-module') },
      { path: 'settings', loadChildren: () => import('./settings/settings-module') }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
]
```

### LocalStorage Format (AuthResponse)
```json
{
  "token": "eyJhbGc...",
  "email": "user@example.com",
  "fullName": "John Doe",
  "role": "Owner",
  "companyName": "ABC Pvt Ltd",
  "tenantId": 1,
  "permissions": ["products.view", "invoices.create", ...],
  "expiresAt": "2026-06-02T10:30:00Z"
}
```

### Important Frontend Rules
- **TenantId**: Never sent from Angular in request body. Always read from JWT on backend.
- **GstType & ContactType**: Must be sent as numbers (not strings) - use `+value` in template.
- **API URLs**: Use plural routes (`/api/products` NOT `/api/product`)
- **Form Submission**: Validate types before sending to API
- **Error Handling**: Use HttpErrorResponse in interceptor for standardized errors

---

## 🐳 DOCKER SETUP

### Dockerfile (Multi-stage Build)
```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 8080
ENTRYPOINT ["dotnet", "TaxAccount.dll"]
```

### Docker Commands
```bash
# Build image
docker build -t taxaccount-api .

# Run container
docker run -d -p 8080:8080 \
  --name taxaccount-api \
  --restart always \
  -e ConnectionStrings__DefaultConnection="Server=..." \
  -e JwtSettings__SecretKey="..." \
  taxaccount-api

# Stop container
docker stop taxaccount-api
docker rm taxaccount-api
```

### Nginx Configuration (Ubuntu VM)
```nginx
location /api {
  proxy_pass http://localhost:8080;
  proxy_set_header Host $host;
  proxy_set_header X-Real-IP $remote_addr;
  proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
  proxy_set_header X-Forwarded-Proto $scheme;
}
```

---

## 🚀 CI/CD PIPELINES (GitHub Actions)

### Backend Deployment (.github/workflows/deploy-api.yml)
```yaml
Trigger: push to main branch
Steps:
  1. Checkout code
  2. Setup .NET 8
  3. dotnet restore
  4. dotnet build
  5. SSH to EC2 (taxaccount-key.pem)
  6. git pull origin main
  7. docker stop taxaccount-api
  8. docker rm taxaccount-api
  9. docker build -t taxaccount-api .
  10. docker run -d -p 8080:8080 --name taxaccount-api --restart always taxaccount-api
```

### Frontend Deployment (.github/workflows/deploy-frontend.yml)
```yaml
Trigger: push to main branch
Steps:
  1. Checkout code
  2. Setup Node 20
  3. npm install
  4. ng build --configuration production
  5. Configure AWS credentials
  6. aws s3 sync dist/ s3://taxaccount-frontend --delete
  7. cloudfront create-invalidation --distribution-id <ID> --paths "/*"
```

---

## ✅ COMPLETED FEATURES

### Authentication & Multi-tenancy
- ✅ JWT-based authentication with custom implementation
- ✅ Multi-tenant architecture with Global Query Filters
- ✅ Role-based access control (4 roles, 12 permissions)
- ✅ Atomic transaction registration (Tenant → User → Defaults)
- ✅ Secure password hashing with BCrypt

### Core Modules
- ✅ Products CRUD with inventory tracking
- ✅ Contacts (Customers/Vendors) with GST type classification
- ✅ Sales Invoices with CGST/SGST/IGST calculations
- ✅ Purchase Bills with automatic stock updates
- ✅ Purchase Orders with convert-to-bill functionality
- ✅ Stock Adjustments (Audit, Damage, Devaluation)
- ✅ Dashboard with summary data

### Advanced Features
- ✅ **Accounting Module**: Chart of Accounts, General Ledger, Trial Balance, P&L, Balance Sheet
- ✅ **Compliance Module**: E-Way Bill generation (ready for GSTN API integration)
- ✅ **Settings Module**: Tenant configuration (E-Way Bill toggle, accounting settings)
- ✅ **Cash Flow Analysis**: Monthly cash flow tracking
- ✅ Inline vendor/product creation in purchase forms
- ✅ Serilog logging with rolling file output (30 days retention)
- ✅ In-memory caching for performance
- ✅ FluentValidation for DTO validation
- ✅ Global exception middleware for error handling
- ✅ Swagger API documentation

### Infrastructure
- ✅ Docker containerization (multi-stage build)
- ✅ AWS RDS SQL Server database
- ✅ S3 + CloudFront CDN for static frontend
- ✅ GitHub Actions CI/CD pipelines
- ✅ Nginx reverse proxy on EC2
- ✅ Entity Framework Core migrations (auto-run on startup)

### UI/UX
- ✅ Angular 21 with standalone components
- ✅ Angular Signals for state management
- ✅ Lazy-loaded feature modules
- ✅ Mobile-responsive layout
- ✅ Hamburger menu for mobile navigation
- ✅ Modern SCSS styling with Angular Material

---

## 🔄 IN PROGRESS / KNOWN ISSUES

### Recent Fixes
- 🔧 Product create URL: `/api/product` → `/api/products` (plural)
- 🔧 Contact GstType: Fixed to send as number (not string)
- 🔧 Contact form field names: Fixed `gstNumber`, `email` → correct naming

### Current Work
- 🔄 Sales Invoice create form (with inline contact/product)
- 🔄 User management page (Owner manages users)
- 🔄 Comprehensive reports (Advanced filtering, exports)
- 🔄 E-Way Bill GSTN API integration

---

## ⏳ PENDING FEATURES

### High Priority
- 📋 Expenses module
- 👥 User management page (create, edit, delete, assign roles)
- ⚙️ Company Settings page (update tenant state/GSTIN)
- 📊 Advanced Reports (P&L, GST summary, stock report, aging)
- 📦 Batch operations (bulk delete, bulk status update)

### Medium Priority
- 💰 Payment entry & receipt tracking
- 🏷️ Tax return filing module
- 📧 Email notifications
- 📱 Mobile app (React Native)
- 🔐 Two-factor authentication (2FA)

### Lower Priority
- 📄 Export to Excel/PDF
- 📊 Data visualization (Charts.js)
- 🔔 Real-time notifications (SignalR)
- 🌍 Multi-language support (i18n)
- 🎨 Customizable themes

---

## 📌 IMPORTANT NOTES FOR DEVELOPERS

### Backend (.NET)
1. **TenantId Filtering**: All queries automatically filtered by Global Query Filter
2. **Transactions**: Use `await using var transaction = await _context.Database.BeginTransactionAsync()`
3. **Logging**: Use `_logger.LogInformation()`, `_logger.LogError()` - Serilog handles routing
4. **Validation**: FluentValidation validators auto-run on DTO binding
5. **Exceptions**: Use custom exception classes from `Exceptions/` folder
6. **Cache Keys**: Use CacheKeys.cs constants for consistency
7. **Migrations**: Run automatically on app startup via `context.Database.Migrate()`
8. **CORS**: Always included in requests - check allowed origins in Program.cs

### Frontend (Angular)
1. **Components**: Use `standalone: true` for all new components
2. **Control Flow**: Use `@if`, `@for`, `@switch` (NOT `*ngIf`, `*ngFor`)
3. **Forms**: Import `ReactiveFormsModule` or `FormsModule` in feature modules
4. **Signals**: Use `signal()`, `computed()`, `effect()` for reactive state
5. **HTTP Requests**: HttpClient auto-adds JWT token via interceptor
6. **Type Safety**: Always define interfaces/models in `/core/models/`
7. **Routing**: Lazy load modules with `loadChildren`, standalone components with `loadComponent`
8. **Error Handling**: Catch `HttpErrorResponse` in services, display in component
9. **Forms**: Use `FormBuilder` with `FormGroup` + `FormControl`
10. **CSS**: Use SCSS variables for colors, breakpoints (responsive design)

### Deployment & DevOps
1. **appsettings.json**: Gitignored - manually create on each environment
2. **EC2 IP**: 13.206.23.136 is Elastic IP (permanent)
3. **RDS Endpoint**: Keep in environment variables, never hardcode
4. **S3 Bucket**: Requires versioning disabled for CloudFront to work
5. **CloudFront Invalidation**: Required after each frontend build
6. **Docker**: Container has `--restart always` so auto-starts with Docker daemon
7. **SSH Key**: Protect taxaccount-key.pem, add to .env in GitHub Actions
8. **Cost Management**: Stop EC2 & RDS when not in use (free tier: 750 hours/month)
9. **Logs**: Check Serilog logs in `/Logs/` directory on EC2
10. **Database Backups**: Enable RDS automated backups (7-day retention)

### Security Best Practices
1. **JWT Secret**: Min 32 characters, change per environment
2. **CORS**: Restrict to known origins only
3. **Passwords**: Hash with BCrypt (never plain text)
4. **Sensitive Data**: Never log passwords, tokens, SSNs
5. **SQL Injection**: Use EF Core parameterized queries (not raw SQL)
6. **HTTPS**: Always use in production (CloudFront enforces this)
7. **API Keys**: Use environment variables, not hardcoded
8. **Secrets**: GitHub Secrets for CI/CD pipeline
9. **Validation**: Validate on both client and server
10. **Rate Limiting**: Consider adding later for login/API endpoints

---

## 📊 PROJECT STATISTICS

### Code Composition
**Backend (TaxAccount-API)**
- C#: 99.9%
- Dockerfile: 0.1%

**Frontend (TaxAccount-Frontend)**
- TypeScript: 53.4%
- HTML: 29.6%
- SCSS: 13.4%
- CSS: 3.6%

### Repository Info
- **Backend Created**: 15 February 2026
- **Frontend Created**: ~51 days ago
- **Last Updated**: 28 May 2026
- **License**: MIT (Frontend)
- **Visibility**: Public

---

## 🔗 USEFUL LINKS

### GitHub
- API Repo: https://github.com/codegovind/TaxAccount-API
- Frontend Repo: https://github.com/codegovind/TaxAccount-Frontend
- Developer: https://github.com/codegovind

### AWS
- EC2 Console: https://console.aws.amazon.com/ec2/
- RDS Console: https://console.aws.amazon.com/rds/
- S3 Console: https://console.aws.amazon.com/s3/
- CloudFront: https://console.aws.amazon.com/cloudfront/

### Documentation
- .NET Docs: https://learn.microsoft.com/dotnet/
- Angular Docs: https://angular.io/docs
- Entity Framework Core: https://learn.microsoft.com/ef/core/
- Serilog: https://serilog.net/

---

## 📝 DEVELOPMENT WORKFLOW

### Local Development
```bash
# Backend (Ubuntu VM)
cd ~/dotnetapp/TaxAccount
dotnet restore
dotnet build
dotnet run
# Swagger: http://localhost:8080/swagger

# Frontend (Windows)
cd taxaccount-frontend
npm install
ng serve
# App: http://localhost:4200
```

### Database Operations
```bash
# Add migration (from Ubuntu VM)
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Drop database (dev only)
dotnet ef database drop
```

### Docker Build & Deploy
```bash
# Build
docker build -t taxaccount-api:latest .

# Tag for registry (if using Docker Hub)
docker tag taxaccount-api:latest username/taxaccount-api:latest

# Run locally
docker run -d -p 8080:8080 --name taxaccount-api taxaccount-api
```

### Git Workflow
```bash
# Create feature branch
git checkout -b feature/feature-name

# Make changes and commit
git add .
git commit -m "feat: add feature description"

# Push to GitHub
git push origin feature/feature-name

# Create Pull Request on GitHub
# Merge to main triggers CI/CD pipeline
```

---

## 🎯 NEXT IMMEDIATE STEPS

1. **Refine Accounting Module**: Integrate with more invoice types
2. **Implement E-Way Bill API**: Connect to GSTN for real bills
3. **User Management UI**: Create pages for user CRUD
4. **Company Settings UI**: Allow tenants to update preferences
5. **Advanced Reporting**: Add filtering, exports, charts
6. **Error Handling**: Standardize error messages across app
7. **Testing**: Unit tests for services, integration tests for APIs
8. **Performance**: Add caching strategy, optimize queries
9. **Security Audit**: Review CORS, authentication, validation
10. **Documentation**: Add inline code comments, update README files

---

## 👤 DEVELOPER CONTACT

**Name**: Govind R. Tekale  
**Experience**: 2+ years as .NET Developer  
**Skills**: ASP.NET MVC, C#, SQL Server, jQuery, .NET 8, Angular 21, Docker, AWS  
**Currently Learning**: Full-stack cloud architecture, DevOps, advanced Angular patterns  
**Location**: Pune/Latur, Maharashtra, India  
**GitHub**: https://github.com/codegovind  
**Email**: govindsuryawanshi001@gmail.com

---

**Last Updated**: 1 June 2026  
**Version**: 0.1 (Active Development)  
**Status**: ✨ Production-Ready (with ongoing feature development)

---
