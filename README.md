# Veterinary Clinic Management System

An individual academic desktop application for managing a veterinary clinic's administrative, clinical, and billing workflows.

Built by **Ahmed Doghman**, a Business Information Systems student at ISG Tunis. The application uses a French-language Windows Forms interface and separate workspaces for administrators, secretaries, and veterinarians.

## Features

| Workspace | Features represented in the source |
| --- | --- |
| Administrator | User management, medicine inventory, clinic statistics, and consultation overview |
| Secretary | Owner and animal records, appointments, invoices, and payment tracking |
| Veterinarian | Appointments, consultations, diagnoses, treatments, vaccination records, and medicine lookup |

## Technology stack

- C# and Windows Forms.
- .NET Framework 4.8.
- SQL Server / SQL Server Express.
- Dapper over ADO.NET (`System.Data.SqlClient`) with parameterized queries.
- NuGet package management using `packages.config`.

**This version uses SQL Server, not MySQL.**

## My contribution

I developed this project individually, including the Windows Forms interfaces, relational database, database-access helper, and role-specific workflows.

## Run locally on Windows

### Requirements

- Windows with Visual Studio and the .NET desktop development workload.
- .NET Framework 4.8 Developer Pack / targeting pack.
- SQL Server Express (or another local SQL Server instance).
- SQL Server Management Studio or another client that understands `GO` batches.
- NuGet package restore access.

### 1. Create the database

Open `SQLQuery1.sql` in your SQL client and execute it against your local SQL Server instance. It creates the `CliniqueVeto` database and its tables. Run it only against a new installation; it is not an upgrade script.

Original seed rows have been removed from this package. If you want local demonstration accounts, execute `demo-accounts.sql` once after creating the database.

### 2. Configure the connection

In `Helpers/DatabaseHelper.cs`, check the connection string:

```text
Server=localhost\SQLEXPRESS;Database=CliniqueVeto;Integrated Security=True;
```

Change the server instance to match your setup. The current connection uses Windows authentication, so your Windows account needs access to the database. Do not commit real SQL credentials if you change the authentication mode.

### 3. Restore dependencies and build

Open `CliniqueVeterinaire.sln` in Visual Studio and restore the NuGet packages. The project references a `packages` directory beside its `.csproj`; ensure restored dependencies are placed there.

For a NuGet CLI workflow, run from the project directory:

```powershell
nuget restore .\packages.config -PackagesDirectory .\packages
```

Build the solution, select `CliniqueVeterinaire` as the startup project if necessary, and run it.

### 4. Sign in to the local demo

Only after executing the optional `demo-accounts.sql`:

| Role | Email |
| --- | --- |
| Administrator | `admin@example.invalid` |
| Secretary | `secretary@example.invalid` |
| Veterinarian | `vet@example.invalid` |

The shared fictional demo password is `DemoOnly-2026!`. These credentials are exclusively for a disposable local demonstration. Create fictional owner and animal records through the application to explore the workflows.

## Existing local databases

The updated login does not accept legacy plain-text passwords. Close the application, back up your existing `CliniqueVeto` database, and run the following once in Windows PowerShell 5.1 with .NET Framework 4.8:

```powershell
.\tools\Migrate-LegacyPasswords.ps1 -ServerInstance 'localhost\SQLEXPRESS'
```

This converts existing plain-text values to salted hashes in a transaction, preserves user IDs and roles, and skips valid hashes. It does not print passwords. The optional `demo-accounts.sql` already contains hashes; use it only on a fresh demo database. Backups taken before conversion still contain the old values and should remain private.

## Password tests

```powershell
.\tests\Test-PasswordHasher.ps1
```

The test script covers a valid password, a wrong password, random salts, malformed/legacy values, and an independently generated Unicode test vector. It requires Windows PowerShell 5.1 and .NET Framework 4.8. It was supplied for local verification and has not been executed in the preparation environment.

## Repository structure

| Path | Contents |
| --- | --- |
| `Forms/` | Login and role-specific Windows Forms |
| `Helpers/` | Database connection and current user session |
| `Models/` | Application data models |
| `Properties/` | Application resources and settings |
| `SQLQuery1.sql` | SQL Server database schema |
| `demo-accounts.sql` | Optional fictional local demo accounts |
| `packages.config` | NuGet dependencies |
| `CliniqueVeterinaire.sln` | Visual Studio solution |

## Current scope

This is an academic prototype for local demonstration. Passwords are stored using PBKDF2-HMAC-SHA256 (600,000 iterations, a random 16-byte salt, and a 32-byte derived key). Login verifies the stored hash; new users and password changes use the same helper. The SQL column already supports 255 characters. Consistent role checks at the data-access boundary and a broader security review are still needed before use with real accounts.

The project has been inspected statically for repository preparation. It has not been compiled or run against SQL Server in that environment. Original user records, customer records, and build outputs are excluded; only explicitly fictional demo accounts are supplied.

## Author

Ahmed Doghman — [GitHub](https://github.com/dogh05) · [LinkedIn](https://www.linkedin.com/in/ahmed-doghman-377034415/)
