# Personnel Records Management – Test Task

This project was implemented as part of a technical test task for a job application.  
It is an **ASP.NET Core MVC** application with SQL Server (locally) and Azure SQL Database (in production).  

## 🚀 Features
- Import personnel records from a CSV file.
- Display records in a searchable and paginated table (DataTables).
- Edit existing personnel records.
- Validation and error handling for invalid or duplicate records.
- Unit tests for key controller actions.

## 🛠️ Tech Stack
- **ASP.NET Core MVC**
- **Entity Framework Core**
- **SQL Server / Azure SQL Database**
- **Bootstrap 5**
- **jQuery DataTables**
- **xUnit** (for unit testing)

## ⚡ Additional Work (beyond the requirements)
- Configured **CI/CD pipeline** using **GitHub Actions**.
- Deployed the application to **Azure App Service**.
- Configured **Azure SQL Database** as the main data source.
- Managed secrets and connection strings via **Azure App Service Configuration** (instead of storing in `appsettings.json`).

## 📦 Getting Started (Local Development)
1. Clone the repository:
   ```bash
   git clone https://github.com/Sardoralgoritm/Integration.git
