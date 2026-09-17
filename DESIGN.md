# DESIGN GUIDELINES: Đêm Hội Đèn Lồng

Dựa trên GDD và tiêu chuẩn thiết kế UI/UX (chuẩn *Impeccable*), dưới đây là Bản thiết kế tổng quát (Design System & Art Direction) cho toàn bộ dự án. Tài liệu này sẽ làm kim chỉ nam cho tất cả các thiết kế giao diện, hình ảnh, và luồng trải nghiệm người dùng trong game.

## 1. Tầm nhìn Thẩm mỹ (Art Vision)
- **Phong cách cốt lõi:** 2D Vector/Flat, nét vẽ dễ thương (cute, chibi). Thân thiện, phù hợp mọi lứa tuổi.
- **Cảm giác mang lại (Game Feel):** Lễ hội, rực rỡ, ấm áp nhưng không kém phần kịch tính của một game bắn súng cuộn dọc.
- **Tối ưu Mobile (Mobile-first):** Mọi thiết kế hình ảnh và UI phải đảm bảo khả năng đọc (readability) tức thì trên màn hình nhỏ khi đang di chuyển với tốc độ cao.

## 2. Hệ thống Màu sắc (Color Palette)

Hệ thống màu sắc được thiết kế để tạo độ tương phản cao, làm nổi bật các yếu tố tương tác.

### Màu UI Chung (Global UI)
- **Background (Nền):** Xanh chàm sâu thẳm / Deep Indigo (`#0a1029`) - Tạo cảm giác bầu trời đêm tĩnh lặng, làm nền hoàn hảo để tôn lên các màu rực chuyên biệt khác.
- **Action/Positive (Nút nâng cấp, Xác nhận):** Xanh Ngọc Bích / Jade Green (`#217d5e`) - Mát mắt, tích cực.
- **Highlight/Currency:** Vàng Sáng / Moon Gold (`#fac938`) - Dùng cho tiền tệ (Đèn ông sao), tên nhân vật, các điểm nhấn quan trọng.
- **Danger/Unlock/Premium:** Đỏ Lồng Đèn / Lantern Red (`#e34233`) - Thu hút sự chú ý mạnh nhất, dùng cho Nút Mở Khóa hoặc cảnh báo.

### Màu Bối Cảnh Theo Vùng (Environment Palettes)
- **Act 1 - Làng Quê:** Vàng ấm, xanh lá nhạt, cam đất (Cảm giác mộc mạc, bình yên).
- **Act 2 - Phố Cổ:** Đỏ lồng đèn, nâu gỗ, cam rực (Nhộn nhịp, dồn dập).
- **Act 3 - Cung Trăng:** Bạc, tím nhạt, xanh dương (Huyền ảo, cao trào, thiêng liêng).

## 3. Ngôn ngữ Hình khối & Giao diện (Shape & UI Layout)
- **Hình khối:** **Tuyệt đối không dùng góc nhọn.** Mọi panel, nút bấm, khung viền đều phải được bo tròn mềm mại (Border radius tỷ lệ ~28px trên độ phân giải 1080p). Điều này tạo sự nhất quán với hình ảnh "chiếc lồng đèn" và phong cách casual.
- **Layout (Bố cục màn hình dọc 9:16):**
  - **Top-heavy (Đỉnh):** Luôn dành cho thông tin trạng thái (Tiền tệ, Máu, Tiến độ màn chơi).
  - **Bottom-heavy (Đáy):** Nơi đặt mọi tương tác vật lý của người chơi (Nút Kỹ năng trong màn chơi, Các nút Nâng cấp/Mua sắm ở Hangar) để tối ưu thao tác 1 tay (Thumb-zone).
  - **Center (Trung tâm):** Dành trọn vẹn cho Gameplay (Lân & Địch) hoặc Trình diễn (Hình ảnh Lân lớn trong Hangar).

## 4. Yêu cầu chi tiết về Assets (Graphics Requirements)

Để triển khai thiết kế này vào Unity, dự án cần chuẩn bị các nhóm Asset sau (Tất cả lưu ở định dạng `.PNG` nền trong suốt):

### 4.1. Backgrounds (Phông nền)
- **Background Menu/Chung:** Bầu trời đêm sâu thẳm, nhiều sao.
- **Background Act 1 (Làng Quê):** Cánh đồng, mái đình, ao làng dưới trăng rằm.
- **Background Act 2 (Phố Cổ):** Phố đèn lồng, chợ đêm, mái ngói cổ kính.
- **Background Act 3 (Cung Trăng):** Cung điện trên mây, ánh bạc, hoa quế.

### 4.2. UI Icons & Tiền tệ (UI & Currency)
- **Tiền tệ:** Đèn Ông Sao (Soft Currency), Kim Cương / Ngọc Bích (Hard Currency).
- **UI Nút bấm:** Nút Upgrade, Nút Max Upgrade (Các nút cơ bản khác có thể dùng code bo góc màu trơn).
- **UI In-game:** Hình Trái Tim hoặc Lọ Thuốc nhỏ (đại diện cho số mạng/HP còn lại của người chơi).

### 4.3. Lân & Kỹ Năng (Player Characters & Skills)
- **Nhân vật Lân (4 hệ):** Kim Lân, Hỏa Lân, Thủy Lân, Lân Vàng (Bao gồm hình avatar hiển thị UI và Sprite bay trong game).
- **Icon Kỹ năng Lân:** Pháo hoa vàng (Golden Firework), Khiên ngọc bích (Jade Protective Shield), Lốc xoáy (Swirling Wind Gust).
- **Đạn của Lân (Player Projectiles):** Hình ảnh tia đạn, pháo sáng, hoặc viên đạn ma thuật do Lân bắn ra.

### 4.4. Kẻ địch & Boss (Enemies & Bosses)
- **Kẻ địch thường (Mobs):**
  - *Đèn giấy (Thường):* Bay thẳng, tốc độ trung bình.
  - *Đèn gai:* Có gai nhọn xung quanh (báo hiệu phát nổ).
  - *Đèn cá chép:* Bay lượn sóng (sin wave).
  - *Đèn quỷ/ma:* Mặt quỷ mờ ảo, có khả năng bắn đạn ngược lại.
- **Đạn của Kẻ địch (Enemy Projectiles):** Hình ảnh đạn hoặc cầu lửa do quái bắn ra.
- **Boss (3 loại):** Kích thước lớn, nhiều chi tiết, có hitbox rõ ràng.
  - *Đèn Lồng Khổng Lồ* (Boss Vùng 1)
  - *Rồng Đèn Phố Cổ* (Boss Vùng 2)
  - *Thỏ Ngọc hóa quái* (Boss Vùng 3)

### 4.5. Vật phẩm & Sự kiện (Power-ups & Events)
- **Lọ thuốc Chú Cuội / Trái tim:** Hồi mạng cho người chơi.
- **Bánh Trung Thu:** Cung cấp lớp khiên chắn.
- **Nam châm:** Hút tiền tệ (đèn ông sao) tự động.
- **Trống Lân:** Gây choáng toàn màn hình.

### 4.6. Hiệu ứng Kỹ xảo (VFX)
- **Hiệu ứng va chạm / Nổ (Hit/Explosion):** Tia chớp, đốm sáng nhỏ hoặc vụ nổ pháo hoa khi tiêu diệt địch. (Có thể làm hình ảnh PNG hoặc dùng Particle System của Unity).

### 4.7. Typography (Nghệ thuật chữ)
- **Font UI chính (Thông số, Nút bấm, Tiền tệ):** Sử dụng các font Sans-serif hiện đại, rõ ràng, hỗ trợ đầy đủ Unicode Tiếng Việt (VD: *Roboto, Montserrat, Be Vietnam Pro*).
- **Font Tiêu đề (Tên game, Tên Lân, Chuyển màn):** Sử dụng font mang hơi hướng thư pháp cách điệu hoặc Serif cổ điển để nhấn mạnh tính truyền thống (VD: *TUV Montserrat, font thư pháp việt hóa*).
