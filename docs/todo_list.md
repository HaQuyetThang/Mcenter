# 📋 Danh sách công việc (To-Do List) - MCenter RPA Orchestrator

Dựa trên bản thiết kế kiến trúc sử dụng **.NET 8 (Web API) + React (Frontend) + .NET Worker Service (Agent)** với cơ chế **Polling**. Dưới đây là danh sách các tác vụ cần thực hiện (được chia theo từng Phase/Layer):

## ✅ Phase 1: Môi trường & Database (Infrastructure)

- [x] **1. Setup Database**
  - [x] Cài đặt PostgreSQL (hoặc sử dụng instance có sẵn).
  - [x] Tạo schema ERD Diagram trên pgAdmin hoặc DBeaver.
- [x] **2. Khởi tạo Solution**
  - [x] Tạo Visual Studio Solution (`MCenter.sln`).
  - [x] Khởi tạo 3 Process Project chính:
    - [x] `MCenter.API` (ASP.NET Core Web API).
    - [x] `MCenter.Core` (Class Library chứa Models, Entities).
    - [x] `MCenter.Agent` (Worker Service).
- [x] **3. Setup Entity Framework Core**
  - [x] Tích hợp EF Core với Data Provider `Npgsql.EntityFrameworkCore.PostgreSQL`.
  - [x] Map đối tượng vào Database Models (Tables: `Server`, `Workflow`, `Schedule`, `ScheduleServer`, `Execution`, `User`).
  - [x] Chạy initial migration và update-database (`dotnet ef migrations add InitialCreate`).

---

## ⏳ Phase 2: Web API (Orchestrator Backend)

- [x] **1. Authentication & Security**
  - [x] Cài đặt ASP.NET Core Identity (dùng BCrypt & JWT).
  - [x] Thêm JWT Bearer Token Authentication (cho Admin Dashboard đăng nhập).
- [x] **2. RESTful APIs: CRUD Core Entities**
  - [x] Tạo **ServersController**: GET, POST, PUT, DELETE Server status/info.
  - [x] Tạo **WorkflowsController**: GET, POST cấu hình Upload/Package paths.
  - [x] Tạo **SchedulesController**: GET, POST gắn lịch chạy (Cron).
  - [x] Tạo **ExecutionsController**: GET xem lịch sử (History) và trạng thái. 
- [x] **3. Scheduler Integration (Hangfire / Quartz.NET)**
  - [x] Tích hợp `Hangfire` vào Backend ASP.NET Core API sử dụng PostgreSQL Storage.
  - [x] Viết chức năng định kì convert `Schedule` sang `Execution` jobs bằng mã Cron.
  - [x] Push Execution vào Database kèm theo Status = `Queued`.
- [x] **4. API Endpoint cho Agent (Worker Polling)**
  - [x] Tạo **AgentController** (Bao gồm cơ chế tự động đăng ký Server khi nhận Heartbeat).
    - [x] `GET /api/agent/pending-jobs/{serverId}` (Truy xuất `Execution` Queued).
    - [x] `POST /api/agent/heartbeat/{serverId}` (Cập nhật `LastHeartbeat` của Agent).
    - [x] `PUT /api/agent/executions/{id}/status` (Đổi `Status` = Running/Success/Failed và đẩy Log).

---

## ⏳ Phase 3: Agent Service (Worker trên các Node RPA)

- [x] **1. Khởi tạo Windows Service**
  - [x] Setup `IHostedService` / BackgroundService trong thư mục `MCenter.Agent`.
  - [x] Config `appsettings.json`: Cấu hình base URL của API Backend.
- [x] **2. Heartbeat Service (Monitoring)**
  - [x] Viết `HeartbeatWorker`: Định kì (VD: mỗi 30 giây) gửi call POST vể API `/api/agent/heartbeat` (kèm theo CPU / RAM / Hostname).
- [x] **3. Thực thi UiRobot.exe**
  - [x] Viết logic `UiPathExecutor`: Command Line Wrapper đóng gói lệnh (VD: `Process.Start("UiRobot.exe", "--execute ...")`).
  - [x] Bắt ExitCode và Output Logging.
- [x] **4. Polling Worker (Kéo Task)**
  - [x] Viết Task định kì GET về server qua `/api/agent/pending-jobs`.
  - [x] Xử lý logic khi kéo Request (PUT -> `Running`), pass qua thư viện `UiPathExecutor` để chạy Process, chờ đợi Timeout/Exit, PUT (`Success`/`Log`).
- [x] **5. Publish và cài đặt thử nghiệm cục bộ**
  - [x] Đã build thành công và sẵn sàng để chạy thử nghiệm console.

---

## ⏳ Phase 4: Frontend Web Dashboard (React)

- [x] **1. Khởi tạo web-app**
  - [x] Tạo thư mục `mcenter-web`: `npx create-vite mcenter-web --template react-ts`.
  - [x] Cài đặt công cụ giao diện: `Ant Design`, `Tailwind CSS`, `React-Router`.
- [x] **2. Giao diện Đăng nhập**
  - [x] Trang Login (Gọi JWT Token và lưu vào LocalStorage/Zustand).
- [x] **3. Giao diện Layout & Danh mục**
  - [x] Tạo layout có Sidebar theo dõi phân mục.
  - [x] **Trang Quản lý Máy chủ (Servers)**: Component Table realtime (Heartbeat status).
  - [x] **Trang Workflows**: Table hiển thị thông tin code package.
  - [x] **Trang Schedules (Lịch phát)**: Form Create (Workflow, Server chỉ định, mã Cron), Form chỉnh sửa.
  - [x] **Trang Lịch sử Thực thi (Executions)**: Form Logs, filter theo State (`Queued`, `Running`, `Success`, `Failed`). Trình diễn output logs của Bot.
- [x] **4. Tích hợp Axios**
  - [x] Đấu nối toàn bộ giao diện Web Dashboard với Backend API .NET để đổ dữ liệu render.

---

## ⏳ Phase 5: Testing & Release

- [ ] **1. End-to-End Testing (Luồng chạy toàn diện)**
  - [ ] Thử đặt 1 lịch cron 1 phút / lần bằng Admin Dashboard.
  - [ ] Verify `Hangfire` tạo `Execution` Queued ở Database.
  - [ ] Agent Polling và fetch vào chạy `UiRobot.exe`.
  - [ ] Agent Push log và status, xem trực tiếp ở log qua UI.
- [ ] **2. Refactoring**
  - [ ] Áp dụng Try-catch chặt chẽ, Handle Exception Connection / Db Fail.
- [ ] **3. Build Version & Release Production**
  - [ ] Đóng gói Web Frontend.
  - [ ] Host Backend API vào IIS/Linux (Kestrel).
  - [ ] Viết file Auto-Install (.bat/.ps1) tự động setup MCenter.Agent cho các máy trạm cuối.
