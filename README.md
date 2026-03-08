# MCenter — RPA Orchestrator

> Hệ thống điều phối RPA (Robotic Process Automation) tự xây dựng, cho phép quản lý máy chủ Agent, lập lịch chạy workflow UiPath và theo dõi lịch sử thực thi theo thời gian thực.

---

## 📐 Kiến trúc hệ thống

```
┌─────────────────────────────────────────────────────────┐
│                    mcenter-web (React)                  │
│         Quản lý Server / Workflow / Lịch trình          │
└──────────────────────┬──────────────────────────────────┘
                       │ HTTP / REST API
┌──────────────────────▼──────────────────────────────────┐
│                MCenter.API (ASP.NET Core)               │
│  Auth · CRUD · Hangfire Scheduler · Server Monitor      │
└──────────────────────┬──────────────────────────────────┘
                       │ PostgreSQL
┌──────────────────────▼──────────────────────────────────┐
│                    PostgreSQL DB                        │
└──────────────────────┬──────────────────────────────────┘
                       │ Polling
┌──────────────────────▼──────────────────────────────────┐
│              MCenter.Agent (.NET Worker)                │
│    Heartbeat · Job Polling · UiPath Execution · Log     │
└─────────────────────────────────────────────────────────┘
```

### Các thành phần

| Thành phần | Công nghệ | Mô tả |
|---|---|---|
| `MCenter.API` | ASP.NET Core 9, EF Core, Hangfire | Backend REST API, lập lịch, giám sát |
| `MCenter.Core` | .NET 9 Class Library | Entities, DbContext chia sẻ |
| `MCenter.Agent` | .NET 9 Worker Service | Chạy bot UiPath trên máy agent |
| `mcenter-web` | React + Vite + Ant Design | Giao diện quản trị |

---

## ✅ Yêu cầu cài đặt

| Phần mềm | Phiên bản tối thiểu |
|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 9.0+ |
| [Node.js](https://nodejs.org/) | 18+ |
| [PostgreSQL](https://www.postgresql.org/download/) | 14+ |
| [UiPath Robot](https://www.uipath.com/) | Bất kỳ (cần `UiRobot.exe`) |

---

## 🚀 Hướng dẫn cài đặt

### 1. Clone dự án

```bash
git clone https://github.com/<your-username>/MCenter.git
cd MCenter
```

### 2. Cấu hình Database (PostgreSQL)

Tạo database mới:

```sql
CREATE DATABASE "MCenter";
```

Cập nhật connection string trong `src/MCenter.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=MCenter;Username=postgres;Password=your_password"
  }
}
```

Chạy migration để tạo bảng:

```bash
dotnet ef database update --project src/MCenter.Core --startup-project src/MCenter.API
```

### 3. Chạy Backend API

```bash
dotnet run --project src/MCenter.API
```

API sẽ chạy tại: `http://localhost:5148`
Hangfire Dashboard: `http://localhost:5148/hangfire`

### 4. Cài đặt và chạy Frontend

```bash
cd src/mcenter-web
npm install
npm run dev
```

Giao diện sẽ chạy tại: `http://localhost:5173`

Cấu hình URL API trong `src/mcenter-web/.env`:

```env
VITE_API_URL=http://localhost:5148/api
```

### 5. Cấu hình và chạy Agent

Chỉnh sửa `src/MCenter.Agent/appsettings.json` theo máy cài:

```json
{
  "Agent": {
    "ServerId": 0,
    "ServerName": "TÊN_MÁY_CHỦ",
    "ApiBaseUrl": "http://localhost:5148",
    "PollingIntervalSeconds": 10
  },
  "UiPath": {
    "RobotPath": "C:\\Users\\<username>\\AppData\\Local\\Programs\\UiPath\\Studio\\UiRobot.exe",
    "LogDirectory": "C:\\Users\\<username>\\AppData\\Local\\UiPath\\Logs"
  }
}
```

Chạy Agent:

```bash
dotnet run --project src/MCenter.Agent
```

Agent sẽ tự động đăng ký với API khi khởi động lần đầu.

---

## 🔧 Cấu hình nâng cao

### Biểu thức Cron

Hệ thống sử dụng chuẩn Cron 5 trường:

| Biểu thức | Ý nghĩa |
|---|---|
| `*/5 * * * *` | Mỗi 5 phút |
| `*/15 * * * *` | Mỗi 15 phút |
| `0 * * * *` | Mỗi giờ |
| `0 0 * * *` | Hàng ngày lúc 00:00 |
| `0 0 * * 0` | Hàng tuần (Chủ nhật 00:00) |

### Timeout giám sát Server

Thời gian tối đa Agent không gửi heartbeat trước khi bị đánh dấu `Offline` được cấu hình trong `src/MCenter.API/Services/Monitoring/ServerMonitorService.cs`:

```csharp
private readonly TimeSpan _timeout = TimeSpan.FromMinutes(1);  // Mặc định: 1 phút
private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30); // Kiểm tra mỗi 30 giây
```

---

## 📋 Tính năng chính

- **🖥️ Quản lý máy chủ**: Thêm/sửa/xóa, giám sát trạng thái Online/Offline tự động.
- **⚙️ Quản lý Workflow**: Quản lý danh sách các gói UiPath `.nupkg`.
- **📅 Lập lịch thực thi**: Tạo lịch chạy bằng Cron Expression với giao diện trực quan.
- **📊 Lịch sử thực thi**: Xem log chi tiết từng lần chạy với màu sắc theo level (Info/Warning/Error).
- **🤖 Tự động đăng ký Agent**: Agent tự đăng ký khi khởi động, chống trùng lặp theo Tên + IP.
- **🔒 Xác thực JWT**: Đăng nhập bảo mật bằng Bearer Token.

---

## 📁 Cấu trúc thư mục

```
MCenter/
├── src/
│   ├── MCenter.API/          # Backend ASP.NET Core
│   │   ├── Controllers/      # API Endpoints
│   │   ├── Models/           # DTOs, Request/Response models
│   │   └── Services/         # Scheduler, Monitor services
│   ├── MCenter.Core/         # Shared library
│   │   ├── Data/             # AppDbContext, Migrations
│   │   └── Entities/         # Domain entities
│   ├── MCenter.Agent/        # Worker Service chạy UiPath
│   │   └── Services/         # AgentWorker, UiPathExecutor
│   └── mcenter-web/          # React Frontend
│       └── src/
│           ├── pages/        # ServersPage, WorkflowsPage, SchedulesPage, ExecutionsPage
│           ├── api/          # Axios config
│           └── store/        # Zustand auth store
└── docs/                     # Tài liệu kiến trúc
```

---

## 🛠️ Build Production

```bash
# Build Backend
dotnet publish src/MCenter.API -c Release -o ./publish/api

# Build Frontend
cd src/mcenter-web
npm run build
```

---

## 📝 Ghi chú

- Agent **không cần** cài UiPath Studio đầy đủ, chỉ cần `UiRobot.exe`.
- Mỗi máy agent chạy một instance `MCenter.Agent` riêng, tự đăng ký với API.
- Khi lịch trình bị **Tắt kích hoạt**, tất cả Job đang chờ (`Queued`) sẽ bị hủy ngay lập tức.
- Log UiPath được đọc tự động sau mỗi lần thực thi từ thư mục `%LOCALAPPDATA%\UiPath\Logs`.
