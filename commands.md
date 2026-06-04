# Center Management System - Commands & Manual Testing Guide

## Git Commands

```Branch 
git checkout -b PhaseN
git add .
git commit -m "PhaseN "
git push origin PhaseN 
```
```Merge
git checkout main
git pull
git merge PhaseN
git push origin main
``` 
## Prerequisites
Open your terminal and navigate to the project directory before running any of the following commands:
```powershell
cd d:\ITI\BACKEND\MVC\Project\CenterManagementSystem
```

**Common Credentials:**
- **Admin Email:** `admin@center.com` (or `admin`)
- **Admin Password:** `Admin@123` (or `admin123`)

---

## Phase 1: Authentication & Application Scaffolding

### Development Commands
```powershell
# Navigate to project directory
cd d:\ITI\BACKEND\MVC\Project\

# Build the project to ensure there are no compilation errors
cd CenterManagementSystem
dotnet build

# (Optional) Apply EF Migrations if DbContext changes are made
dotnet ef migrations add Phase1_Auth -p CenterManagement.Infrastructure -s CenterManagement.Web
dotnet ef database update -p CenterManagement.Infrastructure -s CenterManagement.Web

# Run the application
dotnet run --project CenterManagement.Web
```

### Manual Testing Steps
1. **Launch the application**: Open a browser and go to `https://localhost:7007` (or the port specified in `Properties/launchSettings.json`).
2. **Unauthenticated Access**: Try to access `/Dashboard`. You should be redirected to `/Auth/Login`.
3. **Login Form Validation**: Submit the login form without entering any credentials to verify client-side validation triggers.
4. **Invalid Login**: Attempt login with incorrect credentials. Verify that a server-side error message is displayed.
5. **Successful Admin Login**: Login using the admin credentials provided above.
6. **Dashboard & Sidebar**: Verify redirection to `/Dashboard` and that the sidebar displays all navigation links with active state on the current page.
7. **Logout**: Click the logout button and verify redirection back to the login page.

---

## Phase 2: Student Management

### Development Commands
```powershell
# Navigate to project directory
cd d:\ITI\BACKEND\MVC\Project\CenterManagementSystem

# Build the project
dotnet build

# Apply any new EF migrations for student entities
dotnet ef migrations add Phase2_Students -p CenterManagement.Infrastructure -s CenterManagement.Web
dotnet ef database update -p CenterManagement.Infrastructure -s CenterManagement.Web

# Run the application
dotnet run --project CenterManagement.Web
```

### Manual Testing Steps
1. **Navigate to Students Module**: Go to `/Student/Index` from the sidebar.
2. **List Students**: Ensure the student grid loads data correctly (with pagination and search functionality).
3. **Create Student**: Add a new student, upload a photo, and fill in required fields. Verify that the file saves to `wwwroot/uploads/` locally.
4. **Edit Student**: Update an existing student's details and confirm changes are saved successfully.
5. **Delete/Soft Delete**: Delete a student and verify they are soft-deleted from the database (no longer visible in the main list).

---

## Phase 3: Academic Core

### Development Commands
```powershell
# Navigate to project directory
cd d:\ITI\BACKEND\MVC\Project\CenterManagementSystem

# Build the project
dotnet build

# Apply any new EF migrations for academic entities
dotnet ef migrations add Phase3_Academic -p CenterManagement.Infrastructure -s CenterManagement.Web
dotnet ef database update -p CenterManagement.Infrastructure -s CenterManagement.Web

# Run the application
dotnet run --project CenterManagement.Web
```

### Manual Testing Steps
1. **Groups & Levels**: Navigate to the Academic section and verify CRUD operations for academic levels and subjects.
2. **Assign Instructors**: Assign an instructor to a subject or group.
3. **Enroll Students**: Enroll previously created students into specific groups.
4. **Session Scheduling**: Create a new session for a group and verify the time and date are properly scheduled and displayed.

---

## Phase 4: Attendance QR

### Development Commands
```powershell
# Navigate to project directory
cd d:\ITI\BACKEND\MVC\Project\CenterManagementSystem

# Build the project
dotnet build

# Add migrations for attendance records
dotnet ef migrations add Phase4_Attendance -p CenterManagement.Infrastructure -s CenterManagement.Web
dotnet ef database update -p CenterManagement.Infrastructure -s CenterManagement.Web

# Run the application
dotnet run --project CenterManagement.Web
```

### Manual Testing Steps
1. **Generate QR Code**: Go to a specific session and verify that a QR code can be generated for attendance.
2. **Scan/Mark Attendance**: Simulate scanning the QR code or manually mark a student as "Present".
3. **Late Grace Period**: Verify that students marked after the "Late Grace Minutes" (e.g., 15 mins) are marked as "Late".
4. **Attendance Report**: Check the session attendance summary to ensure counts (Present, Absent, Late) are accurate.

---

## Phase 5: Payments

### Development Commands
```powershell
# Navigate to project directory
cd d:\ITI\BACKEND\MVC\Project\CenterManagementSystem

# Build the project
dotnet build

# Add migrations for payment transactions
dotnet ef migrations add Phase5_Payments -p CenterManagement.Infrastructure -s CenterManagement.Web
dotnet ef database update -p CenterManagement.Infrastructure -s CenterManagement.Web

# Run the application
dotnet run --project CenterManagement.Web
```

### Manual Testing Steps
1. **Generate Invoice/Payment**: Navigate to a student's profile and generate a payment request for an enrolled group.
2. **Process Payment**: Submit a payment for the student (full or partial, based on business rules).
3. **Payment Receipt**: Verify that a payment receipt is generated and the student's balance is updated.
4. **Transaction Logs**: Check the global payments list to verify the transaction appears with the correct timestamp and amount.

---

## Phase 6: Dashboards Analytics

### Development Commands
```powershell
# Navigate to project directory
cd d:\ITI\BACKEND\MVC\Project\CenterManagementSystem

# Build the project
dotnet build

# Apply any final migrations (e.g., cached analytics tables or notification settings)
dotnet ef migrations add Phase6_Analytics -p CenterManagement.Infrastructure -s CenterManagement.Web
dotnet ef database update -p CenterManagement.Infrastructure -s CenterManagement.Web

# Run the application
dotnet run --project CenterManagement.Web
```

### Manual Testing Steps
1. **Admin Dashboard**: Log in as an Admin and view the dashboard (`/Dashboard`). Verify widgets display correct counts (total students, active sessions, recent payments).
2. **Charts & Graphs**: Check that revenue and attendance charts render correctly and reflect seeded data.
3. **Notifications**: Verify that the notification bell dynamically updates its unread count and displays recent alerts without a page reload.
4. **Instructor Dashboard**: Log in as an Instructor and verify the dashboard is scoped only to their assigned groups and sessions.
