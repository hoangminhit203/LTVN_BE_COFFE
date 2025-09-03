# ☕ LVTN_BE_COFFE

## 📌 Giới thiệu
Dự án **Coffee Management System** xây dựng bằng **ASP.NET Core RESTful API**.  
Kiến trúc được tổ chức theo **N-Layer** để dễ dàng mở rộng, bảo trì và test.

---

## 📂 Cấu trúc thư mục hiện tại

```bash
LVTN_BE_COFFE/
│── .vs/                          # Thư mục tạm của Visual Studio (nên ignore)
│── bin/                          # File build (nên ignore)
│── obj/                          # File biên dịch tạm (nên ignore)
│
│── Controllers/                  # API Controllers
│   └── WeatherForecastController.cs
│
│── Domain/                       # Core business logic & common types
│   ├── Common/                   # Hằng số, cấu hình chung
│   ├── IServices/                # Interface cho Service Layer
│   ├── ModelShared/              # Model/Entity dùng chung
│   └── VModel/                   # ViewModels / DTOs
│
│── Infrastructures/Entities/     # Entity Framework Core (DbContext, Entities, Migrations…)
│
│── Middleware/                   # Custom Middleware (Logging, Exception Handling…)
│
│── Properties/                   # Cấu hình project (launchSettings.json)
│
│── Services/                     # Business logic layer
│   ├── Helpers/                  # Hàm tiện ích (format, convert, email…)
│   └── Services/                 # Triển khai các service (UserService, ProductService…)
│
│── appsettings.json              # Cấu hình chính (ConnectionString, Logging…)
│── appsettings.Development.json  # Cấu hình riêng cho môi trường dev
│── LVTN_BE_COFFE.csproj          # Project file
│── Program.cs                    # Entry point
│── WeatherForecast.cs            # Model sample (có thể xóa sau)
│── .gitignore                    # Ignore file (bin, obj, .vs…)


https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures?utm_source=chatgpt.com
