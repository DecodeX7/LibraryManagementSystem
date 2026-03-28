<div align="center">

# 📚 Library Management System

### A full-featured, role-based library management web application built with ASP.NET Core MVC

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-48.2%25-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-EF_Core_8.0-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/en-us/sql-server)
[![HTML](https://img.shields.io/badge/HTML-51.3%25-E34F26?style=for-the-badge&logo=html5&logoColor=white)](https://developer.mozilla.org/en-US/docs/Web/HTML)
[![License](https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge)](LICENSE)

<br/>

**🎬 [Watch Demo on YouTube](https://youtu.be/RZDayx180Gg) &nbsp;|&nbsp; ⭐ [Star this Repo](https://github.com/DecodeX7/LibraryManagementSystem) &nbsp;|&nbsp; 🐛 [Report a Bug](https://github.com/DecodeX7/LibraryManagementSystem/issues)**

<br/>

[![YouTube Demo](https://img.shields.io/badge/▶_Watch_Full_Demo-FF0000?style=for-the-badge&logo=youtube&logoColor=white)](https://youtu.be/RZDayx180Gg)

</div>

---

## 🌟 Overview

The **Library Management System** is a robust, production-ready web application designed to streamline all core operations of a modern library. From book cataloging and student management to automated fine calculation and email notifications — this system handles it all with an elegant, intuitive interface.

Built on **ASP.NET Core MVC (.NET 8)** with a clean layered architecture, this project demonstrates real-world software engineering practices including service-layer separation, secure authentication, database migrations, OTP verification, and automated email delivery.

---

## ✨ Features

### 👨‍💼 Admin Panel
- 📖 **Book Management** — Add, edit, delete, and search books with full catalog control
- 🎓 **Student Management** — Register and manage student accounts with role-based access
- 📋 **Issue & Return Tracking** — Issue books to students and track return dates
- 💰 **Fine Management** — Automatically calculate overdue fines per day
- 📊 **Dashboard Overview** — At-a-glance stats for books, members, and dues
- 🔐 **Secure Admin Access** — Session-based authentication with BCrypt-hashed passwords

### 🎓 Student Portal
- 🔍 **Book Search & Browse** — Explore the entire catalog with ease
- 📚 **My Issued Books** — View currently borrowed books and due dates
- 💳 **Fine Tracking** — Check outstanding fines with due history
- 🔑 **OTP-based Password Reset** — Secure account recovery via email OTP
- 📧 **Email Notifications** — Automated alerts for issue confirmations and reminders

### 🔒 Security & Auth
- Session-based login system with role separation (Admin / Student)
- BCrypt password hashing — no plain text passwords ever stored
- OTP verification for account recovery
- HTTP-only, essential session cookies

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| **Framework** | ASP.NET Core MVC (.NET 8) |
| **Language** | C# |
| **Frontend** | HTML5, CSS3, Razor Views |
| **ORM** | Entity Framework Core 8.0 |
| **Database** | Microsoft SQL Server |
| **Authentication** | Session-based + BCrypt.Net-Next 4.1.0 |
| **Email Service** | MailKit 4.15.1 + MimeKit 4.15.1 |
| **Serialization** | Newtonsoft.Json 13.0.4 |
| **Architecture** | MVC + Service Layer Pattern |

---

## 📁 Project Structure

```
LibraryManagementSystem/
│
├── Controllers/          # MVC Controllers (Auth, Book, Issue, Fine, Student, Admin)
├── Data/                 # AppDbContext + DbSeeder
├── Migrations/           # EF Core database migrations
├── Models/               # Entity models (Book, Student, Issue, Fine, etc.)
├── Services/             # Business logic layer
│   ├── AuthService.cs
│   ├── BookService.cs
│   ├── IssueService.cs
│   ├── FineService.cs
│   ├── EmailService.cs
│   ├── StudentService.cs
│   ├── OtpService.cs
│   └── AdminService.cs
├── Views/                # Razor view templates
├── wwwroot/              # Static assets (CSS, JS, images)
├── Program.cs            # App entry point & DI configuration
├── appsettings.json      # App configuration
└── LibraryManagementSystem.csproj
```

---

## 🚀 Getting Started

### Prerequisites

Make sure you have the following installed:

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or SQL Server Express)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Installation

**1. Clone the repository**
```bash
git clone https://github.com/DecodeX7/LibraryManagementSystem.git
cd LibraryManagementSystem
```

**2. Configure the database connection**

Open `appsettings.json` and update your SQL Server connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=LibraryDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**3. Configure Email Settings**

Add your SMTP credentials in `appsettings.json`:
```json
{
  "EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

**4. Apply database migrations**
```bash
dotnet ef database update
```

**5. Run the application**
```bash
dotnet run
```

**6. Open in browser**
```
https://localhost:5001
```

> 🌱 The database is automatically seeded with a default admin account on first launch via `DbSeeder`.

---

## 🎬 Demo

> Watch the complete walkthrough on YouTube 👇

[![Watch the Demo](https://img.shields.io/badge/▶_Watch_Demo-FF0000?style=for-the-badge&logo=youtube&logoColor=white)](https://youtu.be/RZDayx180Gg)

**https://youtu.be/RZDayx180Gg**

---

## 📦 NuGet Packages

| Package | Version | Purpose |
|---|---|---|
| `BCrypt.Net-Next` | 4.1.0 | Secure password hashing |
| `MailKit` | 4.15.1 | Email sending (SMTP) |
| `MimeKit` | 4.15.1 | MIME message construction |
| `Microsoft.EntityFrameworkCore.SqlServer` | 8.0.0 | SQL Server ORM |
| `Microsoft.EntityFrameworkCore.Tools` | 8.0.0 | EF Core CLI migrations |
| `Newtonsoft.Json` | 13.0.4 | JSON serialization |

---

## 🤝 Contributing

Contributions are welcome and appreciated! Here's how you can help:

1. **Fork** the repository
2. **Create** a new branch (`git checkout -b feature/YourFeature`)
3. **Commit** your changes (`git commit -m 'Add some feature'`)
4. **Push** to the branch (`git push origin feature/YourFeature`)
5. **Open** a Pull Request

Please make sure your code follows the existing conventions and is well-tested before submitting a PR.

---

## 🐛 Bug Reports & Feature Requests

Found a bug or have a feature idea? Please open an issue on the [GitHub Issues](https://github.com/DecodeX7/LibraryManagementSystem/issues) page with clear details and steps to reproduce.

---

## 📄 License

This project is licensed under the **MIT License** — feel free to use, modify, and distribute it for personal or commercial use.

---

## 👨‍💻 Author

<div align="center">

**DecodeX7**

[![GitHub](https://img.shields.io/badge/GitHub-DecodeX7-181717?style=for-the-badge&logo=github&logoColor=white)](https://github.com/DecodeX7)

*If you found this project helpful, please consider giving it a ⭐ — it means a lot!*

</div>

---

<div align="center">

Made with ❤️ using ASP.NET Core · C# · SQL Server

</div>
