# Employee Shift Management System

A comprehensive employee shift scheduling and management solution built with **.NET 10** and **Blazor Server**, designed to streamline workforce planning, time tracking, and reporting.

## 🚀 Features

### Core Functionality
- **Dashboard & Analytics**: Real-time KPIs, drill-down charts, and interactive visualizations
- **Employee Management**: Complete CRUD operations with validation and role management
- **Shift Management**: Define shift templates with time constraints and business rules
- **Shift Planner**: Intuitive drag-and-drop scheduling interface with conflict detection
- **Reports & Exports**: Generate detailed reports with Excel/PDF export capabilities
- **Authentication & Authorization**: Secure role-based access control (Admin/User roles)

### Business Features
- Automated shift assignment validation
- Working hours calculation and tracking
- Conflict detection and prevention
- Seed data initialization for quick start
- Bilingual support (English + Hindi) - _In Progress_

## 🏗️ Architecture

This solution follows **Clean Architecture** principles with clear separation of concerns:

## 🛠️ Technology Stack

- **Framework**: .NET 10
- **UI**: Blazor Server
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Frontend**: Bootstrap 5, Chart.js for visualizations

## 📋 Prerequisites

- .NET 10 SDK or later
- SQL Server 2019+ or SQL Server Express
- Visual Studio 2026 or Visual Studio Code
- PowerShell (for setup scripts)

## ⚙️ Getting Started

### 1. Clone the Repository

### 2. Configure Database Connection
Update `appsettings.json` in `EmployeeShiftManagement.Web`:

### 3. Run Database Migrations

### 4. Run the Application

The application will be available at `https://localhost:5001`

### 5. Default Login Credentials
The system seeds default users on first run:
- **Admin**: `admin@company.com` / `Admin@123`
- **User**: `user@company.com` / `User@123`

## 📖 Usage

### For End Users
1. **Login** with your credentials
2. **View Dashboard** for shift overview and quick stats
3. **Browse Shifts** assigned to you
4. **View Reports** of your work hours

### For Administrators
1. **Manage Employees**: Add, edit, or remove employee records
2. **Define Shifts**: Create shift templates with start/end times
3. **Plan Schedule**: Assign shifts to employees via the interactive planner
4. **Generate Reports**: Export detailed reports for analysis
5. **Monitor KPIs**: Track key metrics on the dashboard

## 🔒 Security Features

- Password hashing with ASP.NET Core Identity
- Role-based authorization
- Secure session management
- Input validation and sanitization
- SQL injection protection via EF Core

## 📊 Key Business Rules

- Shift times must not overlap for the same employee
- Start time must be before end time
- Minimum shift duration validation
- Employee availability checks
- Working hours calculation per week/month

## 🐛 Known Limitations

- Single-timezone support (requires configuration for multi-timezone)
- In-memory caching only (no distributed cache)
- PDF export requires additional configuration
- Limited mobile responsiveness (optimized for desktop)

## 📚 Documentation

- [User Manual (EN + HI)](docs/manual/User_Manual_EN_HI.md) - _Coming Soon_
- [API Documentation](docs/api/) - _Coming Soon_
- [Architecture Decision Records](docs/adr/) - _Coming Soon_

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 Version History

- **v1.0.0** (2026-07) - Initial release
  - Core shift management features
  - Blazor Server UI
  - Authentication & authorization
  - Reports and exports

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Authors

- **Arindam Singha Ray** - Initial work

## 🙏 Acknowledgments

- Built with .NET 10 and Blazor Server
- UI components powered by Bootstrap 5
- Charts rendered with Chart.js

## 📞 Support

For issues, questions, or feature requests:
- Open an issue on GitHub
- Contact: support@yourcompany.com
- Documentation: [Wiki](../../wiki)

---

**Built with ❤️ using .NET 10 + Blazor**