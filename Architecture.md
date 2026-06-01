Center Management System
Clean Architecture + ASP.NET Core MVC Structure

2. Clean Architecture with MVC?
In this project, MVC is only the presentation layer. It handles the UI, controllers, and user interaction.
Clean Architecture is the full structure of the application. It separates the project into layers so each part has one
clear job.
That means:
 MVC manages pages and requests
 Application handles use cases and workflows
 Domain contains business entities and rules
 Infrastructure handles database and external services
So yes, MVC + Clean Architecture is a valid and professional architecture.

3. Layer Responsibilities
3.1 Domain Layer
This is the core of the system.
What it contains
Entities
Entities are the main business objects of the system. Each entity represents a real concept in the application and
usually maps to a database table.
Examples:
 User
 Admin
 Instructor
 Student
 Course
 Group
 Payment
 Attendance
 Exam
 Grade
Enums
Enums are fixed sets of values used across the system. They help make the code clearer, safer, and easier to
maintain.
Examples:
 PaymentStatus -&gt; Pending, Partial, Paid
 StudentGroupStatus -&gt; Active, Transferred, Removed
 UserRoles -&gt; Admin, Instructor, Student
Business Rules
Business rules are the conditions that control how the system behaves. They define what is allowed and what is not
allowed inside the domain.
Examples:
 a student cannot be added to a group if the group is full
 a grade cannot be greater than the exam total marks
 a payment status can only move through valid states
 an attendance record cannot be duplicated for the same student in the same session
Core Domain Logic
Core domain logic is the essential logic that belongs to the business itself, not to the UI or database. It keeps the
system independent from external frameworks and technical details.
Examples:
 validating entity state
 enforcing relationships between objects
 checking domain constraints
 handling important business decisions
Role of this layer
The Domain layer describes the business itself, without depending on database or UI details.
Important note

This layer should stay clean and simple. It should not know anything about:
 MVC
 Entity Framework
 SQL Server
 Views
 Controllers

3.2 Application Layer
This is the layer that controls the business workflows.
What it contains
Services
Services contain the business workflows and application operations.
Examples:
 StudentService
 PaymentService
 AttendanceService
 ExamService
Interfaces
Interfaces define contracts between layers and support Dependency Injection.
Examples:
 IStudentService
 IPaymentService
 IGroupService

Business Use Cases
Use cases describe the actions the system performs.
Examples:
 create group
 transfer student
 upload grades
 generate attendance session
 update payment status
Application Rules
Application rules coordinate workflows and validations between services and repositories.
ViewModels
ViewModels are objects used to transfer prepared data between controllers and views.
Examples:
 CreateStudentViewModel
 LoginViewModel
 PaymentDetailsViewModel
Role of this layer
This layer coordinates what the system should do.
Why it exists
The Application layer keeps the business process independent from the database and the UI.
Example responsibilities
 validating workflow rules
 calling repositories or services through interfaces
 managing application-level operations
 preparing data for the Web layer

3.3 Infrastructure Layer
This layer handles the technical implementation details.
What it contains
DbContext
The main connection point between the application and SQL Server using Entity Framework Core.
Entity Framework Configurations
Configurations define table mappings, relationships, foreign keys, constraints, and indexes.
Repository Implementations
Repositories handle data access operations.
Examples:
 StudentRepository
 PaymentRepository
 AttendanceRepository
Identity Setup
Handles authentication and authorization configuration.
QR Services
Responsible for QR code generation and attendance session support.
File Upload Services
Manage document and image uploads.
Email Sender Services
Handle sending emails and notifications.
Role of this layer
This layer connects the business logic to real-world tools like SQL Server, Identity, file system, email, and QR
generation.
Why it is separate
So the core application does not depend directly on EF Core or SQL Server.

3.4 Web Layer
This is the MVC presentation layer.
What it contains
Controllers
Controllers receive HTTP requests and return responses or views.
Examples:
 StudentController
 PaymentController
 AttendanceController
Views
Views render HTML pages using Razor syntax.
Razor Pages
Used for dynamic UI rendering and page interaction.
Program.cs
Responsible for application startup, service registration, middleware configuration, and dependency injection setup.
appsettings.json
Contains connection strings, application settings, and logging configuration.
Authentication Configuration
Handles login setup, cookies, and authorization rules.
Role of this layer
This layer receives requests from the browser and returns pages or redirects.
What it should do
 call application services
 pass data to views
 handle form submissions
 manage routing
 manage authorization at the UI level
What it should not do
 contain business logic
 query the database directly
 hold complex rules

4. How the Layers Work Together
The flow is:
Browser
-&gt; Web Layer (Controllers / Views)
-&gt; Application Layer (Services / Use Cases)
-&gt; Domain Layer (Entities / Rules)
-&gt; Infrastructure Layer (EF Core / SQL Server)

Example flow
1. The request comes to the MVC controller
2. The controller calls the application service
3. The application service checks rules
4. The infrastructure layer saves data in the database
5. The result goes back to the controller
6. The controller redirects or returns a view
5. Project Structure
src/
├── CenterManagement.Web
├── CenterManagement.Application
├── CenterManagement.Domain
└── CenterManagement.Infrastructure

6. Detailed Structure of Each Project
6.1 CenterManagement.Domain
This project contains the business core.
CenterManagement.Domain
├── Entities
│ ├── Role.cs
│ ├── User.cs
│ ├── Admin.cs
│ ├── Instructor.cs
│ ├── Student.cs
│ ├── Course.cs
│ ├── Group.cs
│ ├── StudentGroup.cs
│ ├── GroupTransfer.cs
│ ├── AttendanceSession.cs
│ ├── Attendance.cs
│ ├── Exam.cs
│ ├── Grade.cs
│ ├── Payment.cs
│ ├── PaymentHistory.cs
│ ├── Notification.cs
│ ├── Announcement.cs
│ ├── Document.cs
│ └── AuditLog.cs
└── Enums
├── PaymentStatus.cs
├── StudentGroupStatus.cs
└── UserRoles.cs

Role of the files
 Entities: represent the main business objects
 Enums: store fixed values used across the system

6.2 CenterManagement.Application
This project contains the business workflows.
CenterManagement.Application
├── Interfaces
│ ├── IAdminService.cs
│ ├── IInstructorService.cs
│ ├── IStudentService.cs
│ ├── ICourseService.cs
│ ├── IGroupService.cs
│ ├── IAttendanceService.cs
│ ├── IExamService.cs
│ ├── IGradeService.cs
│ ├── IPaymentService.cs
│ ├── INotificationService.cs
│ ├── IAnnouncementService.cs
│ ├── IDocumentService.cs
│ ├── IAuditLogService.cs
│ └── IAuthService.cs
├── Services
│ ├── AdminService.cs
│ ├── InstructorService.cs
│ ├── StudentService.cs
│ ├── CourseService.cs
│ ├── GroupService.cs
│ ├── AttendanceService.cs
│ ├── ExamService.cs
│ ├── GradeService.cs
│ ├── PaymentService.cs
│ ├── NotificationService.cs
│ ├── AnnouncementService.cs
│ ├── DocumentService.cs
│ ├── AuditLogService.cs
│ └── AuthService.cs
└── DependencyInjection
└── ApplicationServiceRegistration.cs

Role of the files
 Interfaces: define what the application needs
 Services: contain the actual use-case implementation
 DependencyInjection: registers services in the DI container

6.3 CenterManagement.Infrastructure
This project connects the application to the database and external tools.
CenterManagement.Infrastructure
├── Persistence
│ ├── CenterManagementDbContext.cs
│ ├── Configurations
│ │ ├── RoleConfiguration.cs
│ │ ├── UserConfiguration.cs
│ │ ├── AdminConfiguration.cs
│ │ ├── InstructorConfiguration.cs
│ │ ├── StudentConfiguration.cs
│ │ ├── CourseConfiguration.cs
│ │ ├── GroupConfiguration.cs
│ │ ├── StudentGroupConfiguration.cs
│ │ ├── GroupTransferConfiguration.cs
│ │ ├── AttendanceSessionConfiguration.cs
│ │ ├── AttendanceConfiguration.cs
│ │ ├── ExamConfiguration.cs
│ │ ├── GradeConfiguration.cs
│ │ ├── PaymentConfiguration.cs
│ │ ├── PaymentHistoryConfiguration.cs
│ │ ├── NotificationConfiguration.cs
│ │ ├── AnnouncementConfiguration.cs
│ │ ├── DocumentConfiguration.cs
│ │ └── AuditLogConfiguration.cs
│ └── Migrations
├── Repositories
│ ├── Interfaces
│ └── Implementations
├── Services
│ ├── QrCodeService.cs
│ ├── FileUploadService.cs
│ └── EmailSenderService.cs
└── DependencyInjection
└── InfrastructureServiceRegistration.cs

Role of the files
 DbContext: connects the app to SQL Server
 Configurations: define tables, keys, relationships, and constraints
 Repositories: implement data access
 Services: implement technical helpers
 DependencyInjection: registers infrastructure services

6.4 CenterManagement.Web
This is the MVC front-end layer.
CenterManagement.Web
├── Controllers
├── Views
├── ViewModels
├── wwwroot
├── appsettings.json
└── Program.cs

Role of the files
 Controllers: receive requests and return responses
 Views: render HTML pages
 ViewModels: prepare data for the views
 wwwroot: static files like CSS, JS, images
 Program.cs: app startup and dependency injection
 appsettings.json: configuration and connection strings
7. Entity and Table Responsibility
Each entity maps to one table in the database.
Core entities
 Role -&gt; stores role information
 User -&gt; stores login and account data
 Admin -&gt; stores admin profile data
 Instructor -&gt; stores instructor profile data
 Student -&gt; stores student profile data
 Course -&gt; stores course data
 Group -&gt; stores course group data
 StudentGroup -&gt; stores student enrollment in groups
 GroupTransfer -&gt; stores group movement history
 AttendanceSession -&gt; stores QR attendance session data
 Attendance -&gt; stores attendance records
 Exam -&gt; stores exam data
 Grade -&gt; stores student grades
 Payment -&gt; stores student payment data
 PaymentHistory -&gt; stores payment change history
 Notification -&gt; stores user notifications
 Announcement -&gt; stores center announcements
 Document -&gt; stores uploaded documents
 AuditLog -&gt; stores system activity history

8. Principles Followed
SOLID principles
 Single Responsibility: each class has one job
 Open/Closed: extend without changing core logic
 Liskov Substitution: implementations should be replaceable
 Interface Segregation: small, focused interfaces
 Dependency Inversion: depend on abstractions, not concrete classes
DI principles
 services are injected through constructors
 the Web layer does not create dependencies manually
 the Infrastructure layer is registered in the startup file
9. Why This Structure Is Good for Your Project
This architecture is suitable because the system is:
 a real business product
 large and multi-module
 role-based
 data-heavy
 likely to grow later
 built by a team
It gives you:
 cleaner code
 better separation
 easier maintenance
 easier testing
 easier future expansion
10. Final Summary
This project uses ASP.NET Core MVC as the presentation layer and Clean Architecture as the main structure.
The responsibility of each layer is:
 Domain: core entities and business rules
 Application: services and workflows
 Infrastructure: database and technical integrations
 Web: controllers, views, and UI interaction

The final structure is:
CenterManagement.Web
CenterManagement.Application
CenterManagement.Domain
CenterManagement.Infrastructure

This is a strong and professional structure for a real-world center management system.
