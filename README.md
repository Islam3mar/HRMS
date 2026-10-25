<div align="center">

# 👥 HRMS: Human Resources Management System

**An Arabic (RTL) HR platform for managing employees, attendance, official holidays, and payroll, with granular role-based permissions, Excel import/export, and printable reports.**

[![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![EF Core](https://img.shields.io/badge/EF%20Core-10-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![Bootstrap](https://img.shields.io/badge/Bootstrap%20RTL-7952B3?logo=bootstrap&logoColor=white)](https://getbootstrap.com/)

[**🔗 Live Demo**](https://hrmsapp.runasp.net) · [Features](#-features) · [Architecture](#-architecture) · [Getting Started](#-getting-started) · [Configuration](#%EF%B8%8F-configuration)

</div>

---

## 📖 Overview

**HRMS** is a full-stack ASP.NET Core MVC application built from a written **SRS** (Software Requirements Specification). It covers the complete HR cycle: employee records, daily attendance, official holidays, and monthly payroll, with an emphasis on **strict data integrity** and **fine-grained access control**.

It was developed during my internship at **Pioneers Solutions**, with a focus on **clean architecture**, **business-rule validation**, and **protecting sensitive employee data**.

## ✨ Features

### 🧑‍💼 Employee Management
- Full CRUD for employees with personal and job data (contract date, salary, working hours, department).
- **Egyptian National ID validation** that decodes the 14 digits and checks that:
  - the century digit is valid (`2` or `3`),
  - the embedded **birth date** is a real date and is **not in the future**,
  - the **governorate code** is a known Egyptian governorate,
  - the entered birth date **matches** the one encoded in the National ID,
  - the National ID is **unique** across employees.
- **Egyptian mobile validation** (11 digits, valid prefixes `010`, `011`, `012`, `015`).
- Business rules: unique full name, minimum age of **20 at contract date**, contract date not before the company foundation date or in the future, departure time after attendance time.
- **Smart deletion**: an employee is deleted outright when possible; if they have payroll or attendance history, they are **deactivated (soft delete)** so their financial history stays intact. Deactivated employees can be **reactivated**.
- Departments management and paged, searchable employee lists.

### 🕒 Attendance
- Daily check-in / check-out records per employee, with **one record per employee per day**.
- Validation: check-out must be after check-in, and records can only be added or edited for **today or future dates**.
- **Excel import** (ClosedXML) with a downloadable template that includes an employee-code lookup sheet. Every row is validated and a detailed **row-by-row error report** is returned (invalid code, unknown employee, invalid date or time, duplicate rows in the file, and more).
- **Excel export** and a **print-friendly report** of the filtered results.
- Filters by employee, department, and date range, plus a grouped-by-employee view.

### 💰 Payroll
- Monthly payroll report for all employees or a filtered employee, with live calculation:
  - **Absence days** = expected working days (excluding the **two weekly holidays** and **official holidays**, and counting only from the contract date) minus days attended.
  - **Late hours** (check-in after the scheduled time) are deducted at the configured deduction rate.
  - **Overtime hours** (check-out after the scheduled time) are paid at the configured addition rate.
  - Salary is computed on a fixed **30-day month**, so paid holidays are never deducted.
- **Approve payroll** to store it as a **fixed snapshot** (`PayrollRecord`), so later changes to attendance or base salary never alter an already approved payslip.
- Guardrails: payroll can't be approved for a **future month** or for the **current, unfinished month**.
- **Manual adjustment** of overtime/deduction rates on an approved payslip, with the previous values tracked and a check that the resulting net salary stays positive.
- Printable payslips.

### 🗓 Holidays & General Settings
- Official holidays management (name and date).
- Global settings: addition and deduction percentages, and the two weekly holiday days (the same day can't be chosen twice).

### 🔐 Role-Based Access Control (RBAC)
- Custom **roles** with per-page permissions: **View / Add / Edit / Delete** across 8 system pages (Employees, Departments, Attendance, Payroll, Holidays, General Settings, Users, Roles).
- Enforced by a **custom authorization filter** (`[PermissionAuthorize(page, action)]`) that checks the user's role permissions on every request, so permission changes take effect immediately.
- Users management: create system users, assign roles, login by **username or email**.
- Access Denied page for unauthorized actions.

### 📊 Dashboard
- Total employees and departments, **present / absent today**, whether today is a holiday (and why), and upcoming holidays.
- A prompt when General Settings haven't been configured yet.

### 🌍 Arabic-first UI
- Fully **RTL Arabic interface** built on Bootstrap RTL and Bootstrap Icons, with Arabic validation messages throughout.

## 🏗 Architecture

The solution follows **Clean Architecture** with strict dependency direction:

```
┌──────────────────────────────────────────────┐
│  HRMS.Web                                    │
│  MVC Controllers · Views · ViewModels ·      │
│  Permission authorization filter             │
└──────────────────────┬───────────────────────┘
                       │ depends on
┌──────────────────────▼───────────────────────┐
│  HRMS.Infrastructure                         │
│  EF Core · Repositories · Unit of Work ·     │
│  Specification evaluator · Data seeding      │
└──────────────────────┬───────────────────────┘
                       │ depends on
┌──────────────────────▼───────────────────────┐
│  HRMS.Application                            │
│  Services · DTOs · FluentValidation ·        │
│  AutoMapper profiles · Encryption · Excel    │
└──────────────────────┬───────────────────────┘
                       │ depends on
┌──────────────────────▼───────────────────────┐
│  HRMS.Domain                                 │
│  Entities · Enums · Specifications ·         │
│  Repository contracts                        │
└──────────────────────────────────────────────┘
```

**Patterns & practices**

| Pattern | Where |
|---|---|
| Repository + Unit of Work | `Domain/Interfaces`, `Infrastructure/Repositories` |
| Specification | `Domain/Specifications` (paging, search by name, by department, attendance search, ...) + `SpecificationEvaluator` |
| Dependency Injection | `AddApplicationServices()` / `AddInfrastructureServices()` |
| FluentValidation | Async validators with DB-backed rules (uniqueness, cross-field checks) |
| AutoMapper | Profiles per aggregate in `Application/Profiles` |
| Custom authorization filter | `PermissionAuthorizeAttribute` + `PermissionAuthorizeFilter` |
| Keyed services & data seeding | `IDataSeeder` / `IdentitySeeder`, run at startup after migrations |
| Snapshot pattern | `PayrollRecord` stores the calculated payroll at approval time |

## 🛠 Tech Stack

| Area | Technology |
|---|---|
| Framework | ASP.NET Core MVC on **.NET 10** |
| Data | Entity Framework Core 10, SQL Server |
| Validation | FluentValidation 12 |
| Mapping | AutoMapper |
| Excel | ClosedXML |
| Security | Cookie authentication, BCrypt, AES encryption |
| Frontend | Razor Views, Bootstrap (RTL), Bootstrap Icons, jQuery validation |

## 🔒 Security

- **Passwords** are hashed with **BCrypt**.
- **National IDs are encrypted at rest** with **AES (CBC)**. The encryption is deterministic by design, so the database can still enforce National ID uniqueness. IDs are decrypted only when loaded for display.
- **Granular permissions** are checked server-side on every request, not just hidden in the UI.
- **Anti-forgery tokens** are validated on all `POST` actions.
- Cookie authentication with a **sliding 10-minute expiration**.
- Login errors are intentionally generic, so they don't reveal whether a username exists.
- Secrets (connection string, encryption key, seed admin) are read from configuration, so they can live in **User Secrets** or environment variables instead of source control.

## 📁 Project Structure

```
HRMS/
├── HRMS.Domain/
│   ├── Common/              # BaseEntity, PagedResult, search filters
│   ├── Entities/            # Employee, AttendanceRecord, PayrollRecord, Role, User, ...
│   ├── Enums/               # SystemPage, PermissionAction, EgyptianGovernorate, Gender, ...
│   ├── Interfaces/          # Repository & UnitOfWork contracts
│   └── Specifications/      # Query specifications per aggregate
├── HRMS.Application/
│   ├── Common/              # AES encryption, National ID decoder, password hasher
│   ├── DTOs/                # Inputs, results, filters
│   ├── Interfaces/          # Service contracts
│   ├── Profiles/            # AutoMapper profiles
│   ├── Services/            # Employee, Attendance, Payroll, Auth, Roles, Users, ...
│   └── Validators/          # FluentValidation rules
├── HRMS.Infrastructure/
│   ├── Data/                # DbContext + entity configurations
│   ├── Migrations/
│   ├── Repositories/
│   ├── Seeding/             # IdentitySeeder (default role + admin user)
│   └── Specifications/      # SpecificationEvaluator
└── HRMS.Web/
    ├── Authorization/       # Permission attribute + filter
    ├── Controllers/
    ├── Views/               # Razor views incl. print layouts
    ├── ViewModels/
    └── wwwroot/
```

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Express, or full)

### Run locally

```bash
# 1. Clone
git clone https://github.com/Islam3mar/HRMS.git
cd HRMS/HRMS.Web

# 2. Configure secrets (never commit these)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.;Database=HRMSDb;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet user-secrets set "Encryption:Key" "<base64-32-byte-key>"
dotnet user-secrets set "Encryption:IV"  "<base64-16-byte-iv>"
dotnet user-secrets set "SeedAdmin:FullName" "HR Admin"
dotnet user-secrets set "SeedAdmin:Username" "admin"
dotnet user-secrets set "SeedAdmin:Email"    "admin@example.com"
dotnet user-secrets set "SeedAdmin:Password" "<a-strong-password>"

# 3. Run (migrations and seeding are applied automatically on startup)
dotnet run
```

Generate the AES key and IV:

```bash
# Linux / macOS / Git Bash
openssl rand -base64 32   # → Encryption:Key
openssl rand -base64 16   # → Encryption:IV
```

On first run the app applies all migrations and seeds an **"HR Admin"** role with full permissions on every page, plus the admin user from `SeedAdmin`. Log in with it, then create more roles and users from the **Roles** and **Users** pages.

> ⚠️ Keep the `Encryption` key and IV safe and **never change them** once data exists, otherwise already-encrypted National IDs can no longer be decrypted.

## ⚙️ Configuration

| Key | Description |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `Encryption:Key` | Base64-encoded **32-byte** AES key |
| `Encryption:IV` | Base64-encoded **16-byte** AES initialization vector |
| `SeedAdmin:FullName` / `Username` / `Email` / `Password` | Initial admin account created on first run |

## 🧪 Core Business Rules

- An employee's National ID must be valid, unique, and consistent with the entered birth date.
- Employees must be at least 20 years old at contract date.
- One attendance record per employee per day; check-out must come after check-in.
- Payroll uses a fixed 30-day month; weekly and official holidays are paid and never counted as absences.
- Payroll cannot be approved for the current or a future month.
- Approved payslips are stored as snapshots and are unaffected by later attendance or salary changes.
- Employees with historical records are deactivated instead of deleted.

## 🗺 Screenshots

> _Add screenshots here: dashboard, employees list, attendance report, payroll report, payslip print, roles & permissions._

```md
![Dashboard](docs/screenshots/dashboard.png)
```

## 👤 Author

**Islam Omar**, Back-End Developer (.NET)

[![LinkedIn](https://img.shields.io/badge/LinkedIn-0A66C2?logo=linkedin&logoColor=white)](https://linkedin.com/in/islam-omar-3ba867266)
[![GitHub](https://img.shields.io/badge/GitHub-181717?logo=github&logoColor=white)](https://github.com/Islam3mar)
[![Portfolio](https://img.shields.io/badge/Portfolio-1F5F7A?logoColor=white)](https://islam3mar.github.io/portfolio/)
[![Email](https://img.shields.io/badge/Email-D14836?logo=gmail&logoColor=white)](mailto:islam3marahmed@gmail.com)

Built during my internship at **Pioneers Solutions**.
