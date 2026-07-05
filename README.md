# 🎓 Center Management System

A comprehensive **Educational ERP** built with **ASP.NET Core MVC** following **Clean Architecture** principles. The system streamlines the management of educational centers by providing secure, scalable, and role-based solutions for managing students, instructors, courses, groups, attendance, payments, and academic operations.

---

# 📌 Overview

The Center Management System is designed to simplify daily administrative and academic workflows within educational institutions. It provides a centralized platform for administrators, instructors, and students while ensuring maintainability through a layered Clean Architecture.

---

# ✨ Features

## 👥 User & Role Management

* Secure authentication using ASP.NET Identity.
* Role-based authorization for **Admin**, **Instructor**, and **Student**.
* User profile and account management.

## 🎓 Academic Management

* Manage courses, subjects, grade levels, and student groups.
* Assign instructors to groups.
* Schedule and manage class sessions.
* Student enrollment and group transfers.

## 📅 Attendance Management

* QR Code attendance scanning.
* Manual attendance tracking.
* Attendance history and statistics.
* Session attendance reports.

## 💳 Payment Management

* Course and session payment tracking.
* Outstanding balance monitoring.
* Payment history and financial summaries.

## 📊 Dashboard & Analytics

* Administrative dashboard with KPIs.
* Revenue and payment analytics.
* Attendance insights.
* Student and instructor statistics.
* Smart reporting and performance metrics.

## 🔔 Notifications

* Automatic notifications for important events.
* Session cancellation alerts.
* Academic updates.

---

# 🏛 Architecture

The project follows **Clean Architecture**, separating responsibilities into independent layers:

* **Domain** – Business entities and core business rules.
* **Application** – Use cases, services, DTOs, and interfaces.
* **Infrastructure** – Entity Framework Core, SQL Server, Identity, repositories, and external services.
* **Web** – ASP.NET Core MVC presentation layer, controllers, views, authentication, and dashboards.

This architecture improves maintainability, scalability, testability, and separation of concerns.

---

# 🛠 Tech Stack

* ASP.NET Core MVC
* C#
* Entity Framework Core
* SQL Server
* ASP.NET Identity
* Clean Architecture
* Dependency Injection
* LINQ
* AutoMapper
* Bootstrap
* HTML5
* CSS3
* JavaScript

---

# 🔐 Authentication & Authorization

* ASP.NET Identity Authentication
* Role-Based Authorization
* Secure Login & Registration
* Password Management
* User Roles:

  * Admin
  * Instructor
  * Student

---

# 📂 Project Structure

```text
CenterManagementSystem
│
├── CenterManagement.Domain
├── CenterManagement.Application
├── CenterManagement.Infrastructure
├── CenterManagement.Web
├── Architecture.md
└── Business_Logic.md
```

---

# 🚀 Getting Started

## Prerequisites

* Visual Studio 2022
* .NET 8 SDK (or the version used in the project)
* SQL Server

## Installation

Clone the repository:

```bash
git clone https://github.com/Habeeba00/CenterManagementSystem.git
```

Navigate to the project directory:

```bash
cd CenterManagementSystem
```

Restore dependencies:

```bash
dotnet restore
```

Apply database migrations:

```bash
dotnet ef database update
```

Run the project:

```bash
dotnet run
```

---

# 📸 Screenshots

Include screenshots of:

* Login Page
* Admin Dashboard
* Student Management
* Instructor Dashboard
* Attendance Management
* Payment Dashboard
* Analytics Dashboard

---

# 📚 What I Learned

Through this project, I gained practical experience with:

* Clean Architecture
* ASP.NET Core MVC
* Entity Framework Core
* SQL Server
* ASP.NET Identity
* Dependency Injection
* Repository & Service Patterns
* Role-Based Authorization
* Dashboard Development
* Educational ERP System Design

---

# 🔮 Future Improvements

* Email notifications
* SMS integration
* Online payment gateway
* Parent portal
* Mobile application
* Report exporting (PDF/Excel)
* REST API integration
* Docker deployment

---

# 👩‍💻 Author

**Habeeba Mohamed**

* GitHub: https://github.com/Habeeba00
* LinkedIn:www.linkedin.com/in/habiba-mohamed-mahmoud



---

# 📄 License

This project was developed for educational and portfolio purposes.
