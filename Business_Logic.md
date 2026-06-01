# Center Management System — Full Business Logic & Architecture Documentation

## 1. Project Overview

### Project Name
Center Management System

### System Type
Educational ERP / Center Management Platform

### Main Goal
Build a full educational center management system that manages:
- Students
- Instructors
- Courses
- Groups
- Sessions
- Attendance
- Payments
- Notifications
- QR Attendance
- Dashboards
- Role Permissions

The system is designed using:
- ASP.NET Core MVC
- Entity Framework Core
- SQL Server
- ASP.NET Identity
- Clean Architecture

## 2. Architecture Overview

### Clean Architecture Layers
The project uses Clean Architecture.

1. CenterManagement.Domain  
Contains:
- Entities
- Core business models
- Base entities
- Core relationships

This layer has:
- No database logic
- No UI logic
- No infrastructure logic

2. CenterManagement.Infrastructure  
Contains:
- Entity Framework Core
- DbContext
- Fluent API
- Identity configuration
- Database logic
- Migrations
- Seeders

Responsible for:
- SQL Server communication
- Database relationships
- Persistence

3. CenterManagement.Web  
Contains:
- MVC Controllers
- Views
- Authentication UI
- Dashboards
- ViewModels
- Routing

Responsible for:
- User interaction
- Authentication
- Authorization
- Dashboards

## 3. Roles & Permissions

The system uses ASP.NET Identity Roles.

### Roles

| Role | Description |
|---|---|
| Admin | Full system control |
| Instructor | Manages own groups/sessions |
| Student | Views attendance/payments/schedule |

## 4. Role Business Logic

### Admin Logic
The Admin is the highest authority in the system.

Admin Can:

#### User Management
- Create student accounts
- Create instructor accounts
- Activate/deactivate users
- Reset passwords
- Manage roles

#### Academic Management
- Create grade levels
- Create subjects
- Create courses
- Create groups
- Create sessions
- Cancel sessions
- Modify schedules

#### Group Management
- Add students to groups
- Remove students from groups
- Transfer students between groups
- Change instructors for groups

#### Attendance Management
- View student attendance
- View instructor attendance
- View attendance statistics
- Track late students

#### Payment Management
- Add course payments
- Add one-session payments
- Modify course price
- Track paid/unpaid students
- View payment dashboard
- View remaining balances

#### Dashboard Access
Admin dashboard includes:
- Total students
- Total instructors
- Revenue
- Attendance statistics
- Unpaid students
- Session reports
- Course reports

### Instructor Logic
The Instructor manages teaching activities.

Instructor Can:

#### Session Management
- View assigned groups
- View assigned sessions
- Cancel own sessions
- Add cancel reason

#### Attendance
- Scan QR attendance
- Take attendance
- View student attendance

#### Notifications
When instructor cancels session:
System automatically notifies:
- Admin
- Students in that group

#### Restrictions
Instructor CANNOT:
- Add students
- Remove students
- Change payments
- Modify courses
- Access admin dashboard

### Student Logic
Students are connected to:
- Grade level
- Courses
- Groups
- Sessions
- Payments

Student Can:

#### Attendance
- Scan QR code
- View own attendance
- View attendance history

#### Sessions
- View schedule
- View assigned sessions
- View instructor information

#### Payments
- View paid amount
- View remaining amount
- View payment history

#### Notifications
Receive notifications when:
- Session canceled
- Group changed
- New session added

#### Restrictions
Student CANNOT:
- Modify groups
- Modify attendance
- Change payments
- Access dashboards

## 5. Core Business Scenarios

### Scenario 1 — Multiple Sessions Same Day
Example
Ahmed is Grade 2 student.

Ahmed has:
- Math session at 3 PM
- History session at 8 PM

The system must identify the correct session based on:
- Current time
- Group schedule
- Student enrollment

When Ahmed scans at 3 PM:
System shows:
- Math Group 2
- Instructor Mr. Alaa
- Grade 2

When Ahmed scans at 8 PM:
System shows:
- History Group 1
- Instructor Mr. Hossam

### Scenario 2 — Instructor Multiple Sessions
Instructor may have:
- Multiple groups
- Multiple sessions same day
- Same grade repeated

The system determines the correct session using:
- Session date
- Start time
- End time
- Group relation

### Scenario 3 — Student Transfer
Admin can:
- Remove student from group
- Add student to another group

The system:
- Keeps old attendance history
- Keeps payment history
- Creates new enrollment

### Scenario 4 — Course Payments
When Admin adds course to student:
System automatically:
- Creates StudentCoursePayment record
- Adds course cost to required amount

When course removed:
System:
- Reduces required amount
- Preserves transaction history

### Scenario 5 — One Session Payment
Student may attend one session only.

Admin can create:
- SessionPayment

This payment is independent from full course subscription.

### Scenario 6 — QR Attendance

#### Student Flow
- Student arrives
- Scans QR

System checks:
- Current time
- Group enrollment
- Active session

Attendance recorded
Session shown to student

#### Instructor Flow
Instructor scans QR

System checks active session
Instructor attendance recorded

## 6. Database Design Overview

The database is fully relational.

The system supports:
- Soft delete
- Identity authentication
- Full navigation properties
- Foreign keys
- Indexes
- Reporting queries

## 7. BaseEntity Design

All entities inherit from BaseEntity.

### BaseEntity Fields

| Field | Purpose |
|---|---|
| Id | Primary key |
| CreatedAt | Creation timestamp |
| UpdatedAt | Update timestamp |
| IsDeleted | Soft delete |

## 8. Entity Documentation

### ApplicationUser
Represents authenticated system user.

#### Fields

| Field | Type |
|---|---|
| FullName | string |
| Email | string |
| ImagePath | string? |
| IsActive | bool |

#### Navigation Properties

| Navigation | Relation |
|---|---|
| StudentProfile | One-to-One |
| InstructorProfile | One-to-One |
| Notifications | One-to-Many |
| AuditLogs | One-to-Many |
| PaymentTransactions | One-to-Many |
| SessionPayments | One-to-Many |
| QrCodeLogs | One-to-Many |

### GradeLevel
Represents school grade.

Examples:
- Grade 1
- Grade 2
- Grade 3

#### Navigation

| Navigation | Relation |
|---|---|
| Students | One-to-Many |
| Courses | One-to-Many |

### Subject
Represents academic subject.

Examples:
- Math
- History
- Physics

#### Navigation

| Navigation | Relation |
|---|---|
| Courses | One-to-Many |
| Instructors | One-to-Many |

### Course
Represents academic course.

Examples:
- Grade 2 Math
- Grade 3 Physics

#### Fields

| Field | Purpose |
|---|---|
| Name | Course name |
| Price | Subscription price |
| SubjectId | Subject relation |
| GradeLevelId | Grade relation |

#### Navigation

| Navigation | Relation |
|---|---|
| Subject | Many-to-One |
| GradeLevel | Many-to-One |
| Groups | One-to-Many |
| StudentPayments | One-to-Many |

### InstructorProfile
Stores instructor-specific data.

#### Navigation

| Navigation | Relation |
|---|---|
| User | One-to-One |
| Subject | Many-to-One |
| Groups | One-to-Many |
| Attendances | One-to-Many |

### StudentProfile
Stores student-specific data.

#### Navigation

| Navigation | Relation |
|---|---|
| User | One-to-One |
| GradeLevel | Many-to-One |
| Enrollments | One-to-Many |
| Attendances | One-to-Many |
| CoursePayments | One-to-Many |
| SessionPayments | One-to-Many |

### Group
Represents teaching group.

Examples:
- Math Group 1
- History Group 2

#### Navigation

| Navigation | Relation |
|---|---|
| Course | Many-to-One |
| InstructorProfile | Many-to-One |
| Sessions | One-to-Many |
| Enrollments | One-to-Many |

### Session
Represents actual lecture/session.

#### Fields

| Field | Purpose |
|---|---|
| SessionDate | Session day |
| StartTime | Start time |
| EndTime | End time |
| IsCanceled | Session status |

#### Navigation

| Navigation | Relation |
|---|---|
| Group | Many-to-One |
| Attendances | One-to-Many |
| SessionPayments | One-to-Many |

### Enrollment
Connects student to group.

#### Navigation

| Navigation | Relation |
|---|---|
| StudentProfile | Many-to-One |
| Group | Many-to-One |

### StudentAttendance
Tracks student attendance.

#### Navigation

| Navigation | Relation |
|---|---|
| StudentProfile | Many-to-One |
| Session | Many-to-One |

### InstructorAttendance
Tracks instructor attendance.

#### Navigation

| Navigation | Relation |
|---|---|
| InstructorProfile | Many-to-One |

### StudentCoursePayment
Represents course subscription payment.

#### Fields

| Field | Purpose |
|---|---|
| RequiredAmount | Total required |
| PaidAmount | Paid |
| RemainingAmount | Remaining |
| IsPaid | Fully paid |

#### Navigation

| Navigation | Relation |
|---|---|
| StudentProfile | Many-to-One |
| Course | Many-to-One |
| Transactions | One-to-Many |

### PaymentTransaction
Represents payment operation.

#### Navigation

| Navigation | Relation |
|---|---|
| StudentCoursePayment | Many-to-One |
| Admin | Many-to-One |

### SessionPayment
Represents single-session payment.

#### Navigation

| Navigation | Relation |
|---|---|
| StudentProfile | Many-to-One |
| Session | Many-to-One |
| Admin | Many-to-One |

### Notification
Stores notifications.

Examples:
- Session canceled
- New session
- Group changed

#### Navigation

| Navigation | Relation |
|---|---|
| User | Many-to-One |

### AuditLog
Tracks important system operations.

Examples:
- Student transfer
- Payment modification
- Session cancellation

#### Navigation

| Navigation | Relation |
|---|---|
| User | Many-to-One |

### QrCodeLog
Tracks QR scans.

#### Navigation

| Navigation | Relation |
|---|---|
| User | Many-to-One |

## 9. Soft Delete Logic

The system uses global soft delete.

Deleted records are NOT physically removed.
Instead:
- IsDeleted = true

Benefits:
- Restore data later
- Keep history
- Prevent data loss
- Preserve reports

## 10. Security Design

### Authentication
Using:
- ASP.NET Identity
- Cookie Authentication

### Authorization
Using:
- Role-based authorization

Example:
```csharp
[Authorize(Roles = "Admin")]
```

## 11. Dashboard Logic

### Admin Dashboard
Contains:
- Student count
- Instructor count
- Revenue
- Unpaid students
- Session statistics
- Attendance analytics

### Instructor Dashboard
Contains:
- Assigned groups
- Today's sessions
- Attendance reports
- Session cancellation

### Student Dashboard
Contains:
- Schedule
- Attendance history
- Payments
- Notifications

## 12. Future Expansion

The architecture supports future additions:
- Mobile app
- API layer
- Real-time notifications
- Online payments
- Parent accounts
- QR generation
- Reports export
- AI analytics
- Multi-branch centers
- Multi-language support

## 13. Current Technical Stack

| Technology | Purpose |
|---|---|
| ASP.NET Core MVC | Web framework |
| Entity Framework Core | ORM |
| SQL Server | Database |
| ASP.NET Identity | Authentication |
| Clean Architecture | Structure |
| Bootstrap | UI |

## 14. Final System Summary

The system is designed as a scalable educational ERP.

It supports:
- Full role management
- Attendance tracking
- Payment management
- QR attendance
- Notifications
- Reporting
- Dashboard analytics
- Group/session management
- Multi-session scheduling
- Student transfer logic
- Soft delete
- Enterprise architecture

The architecture is production-ready and scalable for future enterprise development.
