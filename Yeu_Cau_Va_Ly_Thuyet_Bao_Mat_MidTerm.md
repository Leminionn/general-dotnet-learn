# TÀI LIỆU YÊU CẦU DỰ ÁN VÀ KIẾN THỨC BẢO MẬT DỮ LIỆU
*(Mid-term Assignment & Data Security Essentials Training)*

---

## PHẦN 1: TỔNG QUAN YÊU CẦU DỰ ÁN (MID-TERM ASSIGNMENT)

### 1. Phân hệ và Tính năng Chính (Project Requirements)
* **Student Portal (Cổng sinh viên):**
  * Đăng ký & Đăng nhập (Registration & Login).
  * Quản lý thông tin cá nhân (User Profile).
  * Xem thông báo (Announcement).
* **Admin Portal (Cổng quản trị):**
  * Quản lý người dùng (User Management): Thêm người dùng/nhóm, Import, Export, phân quyền/vai trò quản trị viên (Manage admin roles), trạng thái tài khoản (Active/Inactive), thời gian đăng nhập cuối (Last login time).
  * Quản lý thông báo (Announcement Management).

### 2. Nhiệm vụ Nâng cao / Điểm thưởng (Bonus Tasks)
* **Email Whitelist:** Giới hạn hoặc cấu hình danh sách domain/email được phép thông báo hoặc đăng ký vào hệ thống (Allow list/Whitelist).
* **Audit Log:** Ghi vết và theo dõi lịch sử hoạt động, kiểm toán thao tác người dùng/admin trong hệ thống.
* **MFA (Multi-Factor Authentication):** Xác thực đa yếu tố tăng cường bảo mật khi đăng nhập.
* **API & Integration:** Tích hợp với hệ thống bên thứ ba / Single Sign-On (SSO: Microsoft 365, Singpass,...).
* **AI Capable:** 
  * OCR (Nhận dạng ký tự quang học qua ảnh/tài liệu).
  * Facial Recognition (Nhận diện khuôn mặt).

---

## PHẦN 2: YÊU CẦU KỸ THUẬT (TECHNICAL REQUIREMENTS)

### 1. BackEnd (C# / .NET & SQL Server)
* **Cơ sở dữ liệu (Database):**
  * Sử dụng **SQL Server**.
  * Tự thiết kế Database Schema hoàn chỉnh, độc lập.
  * Đánh chỉ mục (**Index**) tối ưu hiệu năng truy vấn.
  * Cung cấp tập lệnh khởi tạo cơ sở dữ liệu hoàn chỉnh (**Database initialization script**).
* **Kiến trúc & Thiết kế API:**
  * Thiết kế RESTful API tuân thủ các chuẩn quy ước chuẩn (Standard conventions).
  * Áp dụng kiến trúc 3 tầng (**Three-layer Architecture**):
    * **API Layer** (Controllers)
    * **Service Layer** (Business Logic)
    * **Repository Layer** (Data Access)
* **Chất lượng mã & Tiêu chuẩn C#:**
  * Tuân thủ các coding standards và conventions của C#.
  * Xử lý ngoại lệ chuẩn hóa (**Proper Exception Handling**).
  * Tích hợp tài liệu hóa API thông qua **Swagger**.
  * Sử dụng **Fluent Validation** để kiểm tra tính hợp lệ của tham số đầu vào API (Input validation).
* **Giám sát & Ghi log:**
  * Triển khai ghi log phù hợp sử dụng **log4net**.
  * Cân nhắc sử dụng **Middleware** kết hợp hệ thống log monitoring để theo dõi toàn bộ các API operations.
* **Bảo mật BackEnd:**
  * Kiểm soát quyền truy cập API (**API Access Control / Authorization**).
  * Ngăn chặn triệt để lỗ hổng tấn công **SQL Injection**.

### 2. FrontEnd (React)
* **Công nghệ & Cấu trúc:**
  * Phát triển ứng dụng hoàn toàn bằng **React**.
  * Viết mã giao diện bằng **CSS** hoặc **SCSS**.
  * Định tuyến trang bằng **React Router**.
* **Xử lý Logic & Component:**
  * Sử dụng hợp lý các **React Hooks** và tự xây dựng **Custom Hooks** khi cần thiết.
  * Phân tách component hợp lý (**Properly split components**) để tối ưu hóa khả năng tái sử dụng (**Reusability**).
  * Đảm bảo các biểu mẫu nhập liệu có cơ chế validation chặt chẽ (**Form Validation**).
* **Trải nghiệm người dùng (UX) & Giao diện:**
  * Xử lý trạng thái tải (**Loading states**) và xử lý thông báo lỗi (**Error handling**) rõ ràng khi gọi API từ Server.
  * Hỗ trợ thiết kế thích ứng (**Responsive Design**), hiển thị tối ưu trên cả giao diện Máy tính (**Web view**) lẫn Điện thoại di động (**Mobile view**).

---

## PHẦN 3: LÝ THUYẾT BẢO MẬT DỮ LIỆU CỐT LÕI (DATA SECURITY ESSENTIALS)

Mô hình bảo mật đa lớp: **Secure Data = Multiple Layers, not one magic feature**.

### 1. Identity & Authentication (Danh tính & Xác thực)
* **Câu hỏi then chốt:** *"Who are you?"* (Bạn là ai?).
* **Khái niệm:** Đăng nhập chứng minh danh tính của người dùng (Login proves who you are), nhưng **không đồng nghĩa** với việc xác định bạn được phép làm gì.

### 2. Access Control & Authorization (Kiểm soát truy cập & Phân quyền)
* **Câu hỏi then chốt:** *"What may you do?"* (Bạn được phép thao tác những gì?).
* **Mô hình RBAC (Role-Based Access Control):**
  * Kiểm soát quyền hạn dựa trên vai trò của đối tượng.
  * Ví dụ:
    * **Teacher:** Chỉnh sửa điểm số (Edit scores).
    * **Student:** Chỉ xem điểm của chính mình (View own score).
    * **Attacker:** Không được phép xem dữ liệu của toàn bộ người dùng (Prevent "View everyone").

### 3. Input Validation & Ngăn chặn SQL Injection
* **Câu hỏi then chốt:** *"Can input become a command?"* (Dữ liệu đầu vào có thể biến thành câu lệnh hay không?).
* **Nguyên tắc cốt lõi:**
  * **"Student records stay DATA — not commands."** (Dữ liệu luôn chỉ là dữ liệu, không thể bị hiểu thành mã thực thi).
  * **Cách làm sai (BAD):** Nối chuỗi trực tiếp (`sql = "SELECT * FROM students WHERE name = '" + userInput + "'"`), tạo điều kiện cho SQL Injection.
  * **Cách làm chuẩn (GOOD - Prepared Statement):**
    1. Sử dụng khuôn mẫu câu lệnh (**SQL template / Parameterized queries**).
    2. Ràng buộc các tham số riêng biệt (**Parameters bound separately**).
    3. Server thực hiện kiểm tra tính hợp lệ dữ liệu đầu vào (**Server validates input**).
    4. Cấp quyền tài khoản cơ sở dữ liệu theo nguyên tắc tối thiểu quyền hạn (**DB account gets least privilege**).

### 4. Bảo vệ Dữ liệu Truyền tải & Lưu trữ (Data in Transit & At Rest)
* **Dữ liệu truyền tải (In Transit):**
  * Luôn sử dụng **HTTPS / TLS** giữa Browser và Server.
  * Tuyệt đối tránh giao thức truyền tin thuần **HTTP** không mã hóa.
* **Dữ liệu lưu trữ (At Rest):**
  * Bảo vệ cơ sở dữ liệu và bản sao lưu (**Database / Backup**).
  * Sử dụng **Mã hóa (Encryption) + Kiểm soát truy cập (Access Control)** để đảm bảo an toàn ngay cả khi ổ cứng hoặc file backup bị đánh cắp.

### 5. Cơ chế Mã hóa (Cryptography)
* **Mã hóa đối xứng (Symmetric Encryption - ví dụ: AES):**
  * Sử dụng **một khóa bí mật duy nhất (One secret key)** cho cả mã hóa và giải mã.
  * *Ưu điểm:* Tốc độ xử lý nhanh, phù hợp cho khối lượng dữ liệu lớn (**Large data, databases, backups**).
  * *Thách thức:* Phân phối khóa bí mật an toàn (**Key distribution = the tricky part**).
* **Mã hóa bất đối xứng (Asymmetric Encryption - ví dụ: RSA):**
  * Sử dụng một cặp khóa gồm: **Public Key** (Khóa công khai, ai cũng có thể dùng để mã hóa) và **Private Key** (Khóa riêng tư, chỉ chủ sở hữu nắm giữ để giải mã/ký).
  * *Ứng dụng:* Thiết lập kết nối bảo mật (**Secure setup**), trao đổi khóa (**Key exchange**), và chữ ký số (**Signatures**).

### 6. Tính toàn vẹn Dữ liệu (Integrity)
* **Câu hỏi then chốt:** *"Can we detect unwanted change?"* (Hệ thống có thể phát hiện các thay đổi ngoài ý muốn không?).
* **Hashing (Băm dữ liệu - ví dụ: SHA-256):**
  * Hoạt động như dấu vân tay số (**Fingerprint, not a lock**).
  * Cùng file/nội dung cho ra cùng mã hash; một thay đổi dù nhỏ nhất cũng sinh ra mã hash hoàn toàn khác biệt. Dùng để phát hiện can thiệp dữ liệu.
* **Digital Signature (Chữ ký số - ví dụ: SHA256withRSA):**
  * Chứng minh danh tính người gửi (**Proves who sent it**).
  * Kết hợp mã băm (Hash) với khóa riêng (Private Key). Phía nhận dùng khóa công khai (Public Key) để giải mã và xác minh tính toàn vẹn lẫn nguồn gốc.

### 7. Xử lý Lỗi an toàn (Failure Mode / Safe Error Handling)
* **Câu hỏi then chốt:** *"What does the attacker learn?"* (Kẻ tấn công có thể học được gì từ thông báo lỗi?).
* **Nguyên tắc:** **Errors should fail safely — not teach attackers.**
* **Cách làm sai (BAD):** Làm lộ chi tiết triển khai hệ thống (Leaks implementation details, ví dụ: `"SQL error in table student_score at line 42"`).
* **Cách làm đúng (GOOD):**
  * Trả về thông điệp chung, thân thiện cho người dùng cuối: *"Something went wrong. Please try again later."*
  * Ghi lại toàn bộ chi tiết ngăn xếp lỗi (stack trace, câu lệnh lỗi) vào hệ thống log nội bộ trên server (**Log details on the server**).

---

## PHẦN 4: THÔNG TIN TỔ CHỨC & DEADLINE (GROUPS & DEADLINE)
* **Hạn chót:** Nộp mã nguồn / thông tin nhóm (**Team code**) trước ngày **26 tháng 9**.
* **Danh sách nhóm (Groups):**
  * **Group 1:** Lucas Nguyen, Kevin Duong, Hugh Phan, Hector Nguyen, Elio Vo, Prome Dang.
  * **Group 2:** Harris Truong, Ryan Phan, Vanditee Tran, Joseph Quang, Chris Bui, Johnny Nguyen, Larry Ly.
  * **Group 3:** Gumball Ho, Andrew Nguyen, Benjamin Nguyen, Lucian Nguyen, Stanley Nguyen, Neil Vo, Antoine Phan.
  * **Group 4:** Nito Hoang, Christian Nguyen, Luna Tu, Arthur Hoang, Tyson Nguyen, Argus Nguyen, Dan Le.
  * **Group 5:** Nolan Vu, Victor Nguyen, Clark Tang, Peter Lam, Ethan Pham, Leo Pham.
  * **Group 6:** Beck Nguyen, Duke Nguyen, Leon Phan, Nathan Ho, Toretto Phan, Kyle Nguyen.
