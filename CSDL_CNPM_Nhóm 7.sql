-- =================================================================================
-- 1. KHỞI TẠO DATABASE & MÔI TRƯỜNG
-- =================================================================================
USE master;
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'NHA_THUOC_LTW')
BEGIN
    CREATE DATABASE NHA_THUOC_LTW;
END
GO
USE NHA_THUOC_LTW;
GO

-- Tắt kiểm tra khóa ngoại để drop bảng dễ dàng
EXEC sp_msforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT all';

-- Xóa bảng (Bao gồm cả bảng KHACH_HANG cũ nếu tồn tại)
DROP TABLE IF EXISTS CHI_TIET_HOA_DON;
DROP TABLE IF EXISTS LO_THUOC;
DROP TABLE IF EXISTS HOA_DON;
DROP TABLE IF EXISTS THUOC;
DROP TABLE IF EXISTS DANH_MUC_THUOC;
DROP TABLE IF EXISTS LOAI_THUOC;
DROP TABLE IF EXISTS KHACH_HANG; 
DROP TABLE IF EXISTS TAI_KHOAN; 
DROP TABLE IF EXISTS PHAN_QUYEN;
DROP TABLE IF EXISTS DIA_DIEM_NHA_THUOC;
DROP TABLE IF EXISTS PHUONG_XA;
DROP TABLE IF EXISTS QUAN_HUYEN;
DROP TABLE IF EXISTS TINH_THANH;

-- Bật lại kiểm tra khóa ngoại
EXEC sp_msforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all';
GO

-- =================================================================================
-- 2. TẠO BẢNG (SCHEMA DEFINITION)
-- =================================================================================

-- --- NHÓM 1: CÁC BẢNG DANH MỤC & CẤU HÌNH ---

-- 1. Bảng Phân quyền
CREATE TABLE PHAN_QUYEN (
    id_quyen INT IDENTITY(1,1) PRIMARY KEY,
    ten_quyen NVARCHAR(50) UNIQUE NOT NULL, 
    mo_ta NVARCHAR(255)
);

-- 2. Bảng Danh mục thuốc
CREATE TABLE DANH_MUC_THUOC (
    id_danh_muc INT IDENTITY(1,1) PRIMARY KEY,
    ten_danh_muc NVARCHAR(255) NOT NULL UNIQUE,
    mo_ta NVARCHAR(MAX)
);

-- 3. Bảng Loại thuốc
CREATE TABLE LOAI_THUOC (
    id_loai_thuoc INT PRIMARY KEY,
    ten_loai NVARCHAR(255)
);

-- --- NHÓM 2: ĐỊA ĐIỂM & HÀNH CHÍNH ---

-- 4. Tỉnh/Thành
CREATE TABLE TINH_THANH (
    id_tinh_thanh INT IDENTITY(1,1) PRIMARY KEY,
    ten NVARCHAR(100) NOT NULL
);

-- 5. Quận/Huyện
CREATE TABLE QUAN_HUYEN (
    id_quan_huyen INT IDENTITY(1,1) PRIMARY KEY,
    ten NVARCHAR(100) NOT NULL,
    id_tinh_thanh INT FOREIGN KEY REFERENCES TINH_THANH(id_tinh_thanh)
);

-- 6. Phường/Xã
CREATE TABLE PHUONG_XA (
    id_phuong_xa INT IDENTITY(1,1) PRIMARY KEY,
    ten NVARCHAR(100) NOT NULL,
    id_quan_huyen INT FOREIGN KEY REFERENCES QUAN_HUYEN(id_quan_huyen)
);

-- 7. Địa điểm Nhà thuốc
CREATE TABLE DIA_DIEM_NHA_THUOC (
    id_dia_diem INT IDENTITY(1,1) PRIMARY KEY,
    ten_nha_thuoc NVARCHAR(255),
    thanh_pho_tinh NVARCHAR(255),
    quan_huyen NVARCHAR(255),
    so_duong_ten_duong NVARCHAR(255)
);

-- --- NHÓM 3: TÀI KHOẢN ---

-- 8. Tài khoản 
CREATE TABLE TAI_KHOAN (
    id_tai_khoan INT IDENTITY(1,1) PRIMARY KEY,
    
    -- Thông tin đăng nhập
    ten_dang_nhap NVARCHAR(50) UNIQUE NOT NULL,
    mat_khau NVARCHAR(255) NOT NULL,
    
    -- Thông tin cá nhân cơ bản
    ho_ten NVARCHAR(255) NOT NULL,
    email NVARCHAR(255) NULL, 
    so_dien_thoai NVARCHAR(15) NULL,
    dia_chi NVARCHAR(255) NULL,

    -- Phân quyền
    id_quyen INT NOT NULL DEFAULT 4, 
    CONSTRAINT FK_TaiKhoan_PhanQuyen FOREIGN KEY (id_quyen) REFERENCES PHAN_QUYEN(id_quyen)
);
GO


-- --- NHÓM 4: SẢN PHẨM & KHO ---

-- 9. Thuốc
CREATE TABLE THUOC (
    id_thuoc INT IDENTITY(1,1) PRIMARY KEY,
    id_danh_muc INT NOT NULL,
    id_loai_thuoc INT,
    ten_thuoc NVARCHAR(255) NOT NULL UNIQUE,
    hang NVARCHAR(255),
    hoat_chat NVARCHAR(255),
    don_vi_tinh NVARCHAR(50),
    dang_bao_che NVARCHAR(100),
    ham_luong NVARCHAR(100),
    cong_dung NVARCHAR(MAX),
    lieu_dung NVARCHAR(MAX),
    chong_chi_dinh NVARCHAR(MAX),
    gia_ban INT, 
    anh_thuoc NVARCHAR(MAX),
    CONSTRAINT FK_Thuoc_DanhMucThuoc FOREIGN KEY (id_danh_muc) REFERENCES DANH_MUC_THUOC(id_danh_muc),
    CONSTRAINT FK_Thuoc_LoaiThuoc FOREIGN KEY (id_loai_thuoc) REFERENCES LOAI_THUOC(id_loai_thuoc)
);

-- 10. Lô thuốc
CREATE TABLE LO_THUOC (
    id_lo_thuoc INT IDENTITY(1,1) PRIMARY KEY,
    id_thuoc INT NOT NULL,
    so_lo VARCHAR(100) NOT NULL,
    han_su_dung DATE,
    ngay_san_xuat DATE,
    so_luong_nhap INT DEFAULT 0,
    so_luong_ton INT DEFAULT 0,
    gia_nhap INT DEFAULT 0,
    CONSTRAINT FK_LoThuoc_Thuoc FOREIGN KEY (id_thuoc) REFERENCES Thuoc(id_thuoc)
);

-- --- NHÓM 5: GIAO DỊCH ---

CREATE TABLE HOA_DON (
    id_hoa_don INT IDENTITY(1,1) PRIMARY KEY,
    id_nguoi_mua INT NULL,       
    ngay_ban DATETIME DEFAULT GETDATE(),
    tong_tien INT DEFAULT 0,

    -- THÔNG TIN GIAO HÀNG
    ten_nguoi_nhan NVARCHAR(255) NOT NULL, 
    sdt_nguoi_nhan NVARCHAR(15) NOT NULL,  
    dia_chi_giao_hang NVARCHAR(255) NOT NULL, 
    email_nguoi_mua NVARCHAR(255) NULL,        
    hinh_thuc_thanh_toan NVARCHAR(255) NULL,

    CONSTRAINT FK_HoaDon_NguoiMua FOREIGN KEY (id_nguoi_mua) REFERENCES TAI_KHOAN(id_tai_khoan)
);
GO

-- 12. Chi tiết hoá đơn 
CREATE TABLE CHI_TIET_HOA_DON (
    id_chi_tiet_hoa_don INT IDENTITY(1,1) PRIMARY KEY,
    id_hoa_don INT NOT NULL,
    id_lo_thuoc INT NOT NULL, 
    so_luong INT NOT NULL,
    don_gia INT DEFAULT 0, 
    thanh_tien INT DEFAULT 0,
    
    CONSTRAINT FK_ChiTiet_HoaDon FOREIGN KEY (id_hoa_don) REFERENCES HOA_DON(id_hoa_don),
    CONSTRAINT FK_ChiTiet_LoThuoc FOREIGN KEY (id_lo_thuoc) REFERENCES LO_THUOC(id_lo_thuoc)
);
GO

SET DATEFORMAT dmy;
GO

-- =================================================================================
-- 4. INSERT DỮ LIỆU
-- =================================================================================

PRINT N'=== 1. KHỞI TẠO DỮ LIỆU DANH MỤC & CẤU HÌNH ===';

INSERT INTO PHAN_QUYEN (ten_quyen, mo_ta) VALUES 
(N'QuanLy', N'Toàn quyền hệ thống'),
(N'KhachHang', N'Khách hàng mua hàng qua trang web');

INSERT INTO DANH_MUC_THUOC (ten_danh_muc, mo_ta) VALUES 
(N'Kháng sinh', N'Thuốc tiêu diệt hoặc ức chế vi khuẩn.'),
(N'Giảm đau - Hạ sốt', N'Thuốc giúp giảm đau, hạ sốt.'),
(N'Kháng viêm', N'Thuốc chống viêm, chống dị ứng.'),
(N'Vitamin và khoáng chất', N'Bổ sung dưỡng chất, hỗ trợ sức khỏe.');

INSERT INTO LOAI_THUOC (id_loai_thuoc, ten_loai) VALUES 
(0, N'Thuốc'), 
(1, N'Thực phẩm chức năng'),
(2, N'Mỹ phẩm'), 
(3, N'Đông Y - Thảo Dược');

PRINT N'=== 2. KHỞI TẠO DỮ LIỆU TỈNH THÀNH QUẬN HUYỆN PHƯỜNG XÃ ===';
-- 3. CHÈN TỈNH / THÀNH PHỐ (Sẽ có ID 1, 2, 3)
INSERT INTO TINH_THANH (ten) VALUES 
(N'Thành phố Hà Nội'),        -- ID = 1
(N'Thành phố Hồ Chí Minh'),   -- ID = 2
(N'Thành phố Đà Nẵng');       -- ID = 3
GO

-- 4. CHÈN QUẬN / HUYỆN (Liên kết với ID Tỉnh ở trên)

-- --- Hà Nội (ID_TINH = 1) ---
INSERT INTO QUAN_HUYEN (ten, id_tinh_thanh) VALUES 
(N'Quận Ba Đình', 1),    -- ID = 1
(N'Quận Hoàn Kiếm', 1),  -- ID = 2
(N'Quận Cầu Giấy', 1);   -- ID = 3

-- --- TP.HCM (ID_TINH = 2) ---
INSERT INTO QUAN_HUYEN (ten, id_tinh_thanh) VALUES 
(N'Quận 1', 2),          -- ID = 4
(N'Quận 3', 2),          -- ID = 5
(N'Quận Bình Thạnh', 2); -- ID = 6

-- --- Đà Nẵng (ID_TINH = 3) ---
INSERT INTO QUAN_HUYEN (ten, id_tinh_thanh) VALUES 
(N'Quận Hải Châu', 3),   -- ID = 7
(N'Quận Sơn Trà', 3);    -- ID = 8
GO

-- 5. CHÈN PHƯỜNG / XÃ (Liên kết với ID Quận ở trên)

-- --- Cho Quận Ba Đình (ID_QUAN = 1) ---
INSERT INTO PHUONG_XA (ten, id_quan_huyen) VALUES 
(N'Phường Phúc Xá', 1),
(N'Phường Trúc Bạch', 1),
(N'Phường Vĩnh Phúc', 1);

-- --- Cho Quận Hoàn Kiếm (ID_QUAN = 2) ---
INSERT INTO PHUONG_XA (ten, id_quan_huyen) VALUES 
(N'Phường Hàng Bạc', 2),
(N'Phường Hàng Gai', 2),
(N'Phường Tràng Tiền', 2);

-- --- Cho Quận Cầu Giấy (ID_QUAN = 3) ---
INSERT INTO PHUONG_XA (ten, id_quan_huyen) VALUES 
(N'Phường Dịch Vọng', 3),
(N'Phường Yên Hòa', 3);

-- --- Cho Quận 1 (ID_QUAN = 4) ---
INSERT INTO PHUONG_XA (ten, id_quan_huyen) VALUES 
(N'Phường Bến Nghé', 4),
(N'Phường Bến Thành', 4),
(N'Phường Đa Kao', 4);

-- --- Cho Quận 3 (ID_QUAN = 5) ---
INSERT INTO PHUONG_XA (ten, id_quan_huyen) VALUES 
(N'Phường Võ Thị Sáu', 5),
(N'Phường 1', 5);

-- --- Cho Quận Bình Thạnh (ID_QUAN = 6) ---
INSERT INTO PHUONG_XA (ten, id_quan_huyen) VALUES 
(N'Phường 25', 6),
(N'Phường 1', 6),
(N'Phường 3', 6);

-- --- Cho Quận Hải Châu - Đà Nẵng (ID_QUAN = 7) ---
INSERT INTO PHUONG_XA (ten, id_quan_huyen) VALUES 
(N'Phường Hải Châu I', 7),
(N'Phường Hải Châu II', 7);

-- --- Cho Quận Sơn Trà - Đà Nẵng (ID_QUAN = 8) ---
INSERT INTO PHUONG_XA (ten, id_quan_huyen) VALUES 
(N'Phường An Hải Bắc', 8),
(N'Phường An Hải Đông', 8);
GO


INSERT INTO DIA_DIEM_NHA_THUOC (ten_nha_thuoc, thanh_pho_tinh, quan_huyen, so_duong_ten_duong) VALUES 
(N'Nhà thuốc MedForAll Quận Hoàn Kiếm', N'Hà Nội', N'Quận Hoàn Kiếm', N'80 Phố Hàng Gai'),
(N'Nhà thuốc MedForAll Quận Ba Đình', N'Hà Nội', N'Quận Ba Đình', N'120 Đường Kim Mã'),
(N'Nhà thuốc MedForAll Quận Tân Phú', N'Hồ Chí Minh', N'Quận Tân Phú', N'123 Lê Sao');


PRINT N'=== 2. KHỞI TẠO TÀI KHOẢN (NHÂN VIÊN & KHÁCH HÀNG) ===';

-- 2.1 Tạo tài khoản Nhân viên
INSERT INTO TAI_KHOAN (ten_dang_nhap, mat_khau, ho_ten, id_quyen, dia_chi, so_dien_thoai, email) VALUES 
('admin', 'admin123', N'Phạm Giám Đốc', 1, NULL, NULL, NULL);

-- 2.2 Tạo tài khoản Khách hàng
INSERT INTO TAI_KHOAN (ten_dang_nhap, mat_khau, ho_ten, id_quyen, dia_chi, so_dien_thoai, email) VALUES
('An', '123456', N'Trần Văn An', 2, N'123 Lê Lợi, TP.HCM', '0918000111', NULL),
('Bich', '123456', N'Lê Thị Bích', 2, N'456 Nguyễn Trãi, TP.HCM', '0918000222', 'bich.le@email.com'),
('Cuong', '123456', N'Phạm Văn Cường', 2, N'789 CMT8, TP.HCM', '0918000333', NULL);


PRINT N'=== 3. KHỞI TẠO DỮ LIỆU THUỐC & LÔ ===';

----------------------------------------- THUỐC ----------------------------------------- 
SET DATEFORMAT DMY
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)

VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Kháng sinh'), 0, N'Amoxicillin', N'Imexpharm', N'Amoxicillin', N'Viên nang', N'500mg', 30000, N'Hộp', N'Trị nhiễm khuẩn', N'Uống 1v', N'Dị ứng Penicillin');

    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap)

    SELECT id_thuoc, 'AMOXI001', '01-01-2026', '01-01-2024', 250, 250, 22000 FROM THUOC WHERE ten_thuoc = N'Amoxicillin';


INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)

VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Giảm đau - Hạ sốt'), 0, N'Paracetamol', N'DHG', N'Paracetamol', N'Viên nén', N'500mg', 62000, N'Hộp', N'Hạ sốt', N'Uống 1v', N'Suy gan');

    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap)

    SELECT id_thuoc, 'PARA01082015', '10-09-2025', '01-08-2015', 100, 100, 48000 FROM THUOC WHERE ten_thuoc = N'Paracetamol';

INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)

VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Vitamin và khoáng chất'), 0, N'Vitamin C', N'OPC', N'Ascorbic Acid', N'Viên sủi', N'500mg', 20000, N'Tuýp', N'Tăng đề kháng', N'Uống 1v', N'Sỏi thận');

    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap)

    SELECT id_thuoc, 'VITC001', '12-12-2025', '12-12-2023', 143, 143, 14000 FROM THUOC WHERE ten_thuoc = N'Vitamin C';
----------------------------------------- THUỐC DỮ LIỆU BỔ SUNG ----------------------------------------- 
-- 1. Panadol Extra (Giảm đau - Hạ sốt)
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Giảm đau - Hạ sốt'), 0, N'Panadol Extra', N'GSK', N'Paracetamol, Caffeine', N'Viên nén', N'500mg/65mg', 25000, N'Vỉ', N'Giảm đau mạnh, hạ sốt', N'1-2 viên mỗi 4-6h', N'Suy gan, mẫn cảm');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'PAN001', '01-12-2026', '01-01-2024', 500, 500, 18000 FROM THUOC WHERE ten_thuoc = N'Panadol Extra'
    UNION ALL SELECT id_thuoc, 'PAN002', '15-02-2027', '15-02-2024', 300, 300, 18000 FROM THUOC WHERE ten_thuoc = N'Panadol Extra';

-- 2. Efferalgan 500mg (Giảm đau - Hạ sốt)
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Giảm đau - Hạ sốt'), 0, N'Efferalgan', N'UPSA', N'Paracetamol', N'Viên sủi', N'500mg', 55000, N'Tuýp', N'Hạ sốt nhanh, giảm đau', N'Hòa tan 1 viên vào nước', N'Bệnh phenylceton niệu');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'EFF001', '20-10-2025', '20-10-2023', 200, 200, 42000 FROM THUOC WHERE ten_thuoc = N'Efferalgan'
    UNION ALL SELECT id_thuoc, 'EFF002', '10-01-2026', '10-01-2024', 200, 200, 42000 FROM THUOC WHERE ten_thuoc = N'Efferalgan'
    UNION ALL SELECT id_thuoc, 'EFF003', '05-05-2026', '05-05-2024', 150, 150, 42000 FROM THUOC WHERE ten_thuoc = N'Efferalgan';

-- 3. Augmentin 625mg (Kháng sinh)
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Kháng sinh'), 0, N'Augmentin 625mg', N'GSK', N'Amoxicillin, Acid Clavulanic', N'Viên nén', N'625mg', 220000, N'Hộp', N'Trị nhiễm khuẩn nặng', N'1 viên x 2 lần/ngày', N'Dị ứng Beta-lactam');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'AUG001', '30-06-2025', '01-01-2023', 100, 100, 180000 FROM THUOC WHERE ten_thuoc = N'Augmentin 625mg'
    UNION ALL SELECT id_thuoc, 'AUG002', '31-12-2025', '01-06-2023', 100, 100, 180000 FROM THUOC WHERE ten_thuoc = N'Augmentin 625mg';

-- 4. Cefixime 200mg (Kháng sinh)
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Kháng sinh'), 0, N'Cefixime 200mg', N'Domesco', N'Cefixime', N'Viên nang', N'200mg', 80000, N'Hộp', N'Nhiễm khuẩn đường tiết niệu, hô hấp', N'1-2 viên/ngày', N'Mẫn cảm Cephalosporin');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'CEF001', '15-08-2026', '15-08-2024', 150, 150, 60000 FROM THUOC WHERE ten_thuoc = N'Cefixime 200mg'
    UNION ALL SELECT id_thuoc, 'CEF002', '01-09-2026', '01-09-2024', 200, 200, 60000 FROM THUOC WHERE ten_thuoc = N'Cefixime 200mg';

-- 5. Alpha Choay (Kháng viêm)
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Kháng viêm'), 0, N'Alpha Choay', N'Sanofi', N'Chymotrypsin', N'Viên nén', N'21 microkatal', 75000, N'Hộp', N'Chống phù nề, kháng viêm dạng men', N'Ngậm dưới lưỡi 2 viên', N'Dị ứng đạm');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'ALP001', '01-01-2026', '01-01-2024', 1000, 1000, 55000 FROM THUOC WHERE ten_thuoc = N'Alpha Choay'
    UNION ALL SELECT id_thuoc, 'ALP002', '05-05-2026', '05-05-2024', 500, 500, 55000 FROM THUOC WHERE ten_thuoc = N'Alpha Choay'
    UNION ALL SELECT id_thuoc, 'ALP003', '10-10-2026', '10-10-2024', 500, 500, 55000 FROM THUOC WHERE ten_thuoc = N'Alpha Choay';

-- 6. Medrol 16mg (Kháng viêm)
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Kháng viêm'), 0, N'Medrol', N'Pfizer', N'Methylprednisolone', N'Viên nén', N'16mg', 120000, N'Hộp', N'Chống viêm steroid, ức chế miễn dịch', N'Theo chỉ định bác sĩ', N'Nhiễm nấm toàn thân');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'MED001', '20-11-2026', '20-11-2023', 80, 80, 95000 FROM THUOC WHERE ten_thuoc = N'Medrol'
    UNION ALL SELECT id_thuoc, 'MED002', '01-02-2027', '01-02-2024', 100, 100, 95000 FROM THUOC WHERE ten_thuoc = N'Medrol';

-- 7. Vitamin 3B (Vitamin)
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Vitamin và khoáng chất'), 0, N'Vitamin 3B', N'Pharbaco', N'B1, B6, B12', N'Viên nang', N'125mg', 40000, N'Hộp', N'Bổ thần kinh, trị đau dây thần kinh', N'2 viên/ngày', N'Mẫn cảm thành phần thuốc');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, '3B001', '12-12-2026', '12-12-2023', 400, 400, 25000 FROM THUOC WHERE ten_thuoc = N'Vitamin 3B'
    UNION ALL SELECT id_thuoc, '3B002', '01-06-2027', '01-06-2024', 300, 300, 25000 FROM THUOC WHERE ten_thuoc = N'Vitamin 3B';

-- 8. Calcium Corbiere (Vitamin & Khoáng chất)
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Vitamin và khoáng chất'), 0, N'Calcium Corbiere', N'Sanofi', N'Canxi, Vitamin C, PP', N'Ống uống', N'10ml', 160000, N'Hộp', N'Bổ sung canxi cho trẻ em, bà bầu', N'1-2 ống/ngày buổi sáng', N'Sỏi canxi thận');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'CAL001', '15-03-2025', '15-03-2023', 200, 200, 130000 FROM THUOC WHERE ten_thuoc = N'Calcium Corbiere'
    UNION ALL SELECT id_thuoc, 'CAL002', '15-03-2026', '15-03-2024', 200, 200, 130000 FROM THUOC WHERE ten_thuoc = N'Calcium Corbiere';

-- 9. Ibuprofen 400mg (Kháng viêm/Giảm đau)
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Kháng viêm'), 0, N'Ibuprofen Gonsa', N'Gonsa', N'Ibuprofen', N'Viên nén', N'400mg', 60000, N'Hộp', N'Giảm đau, kháng viêm cơ xương khớp', N'1 viên sau ăn', N'Loét dạ dày tá tràng');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'IBU001', '01-01-2027', '01-01-2024', 120, 120, 40000 FROM THUOC WHERE ten_thuoc = N'Ibuprofen Gonsa'
    UNION ALL SELECT id_thuoc, 'IBU002', '05-06-2027', '05-06-2024', 120, 120, 40000 FROM THUOC WHERE ten_thuoc = N'Ibuprofen Gonsa';

-- 10. Berberin (Xếp vào Kháng sinh/Diệt khuẩn đường ruột)
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Kháng sinh'), 0, N'Berberin', N'Mộc Hoa Tràm', N'Berberin Clorid', N'Viên nang', N'100mg', 15000, N'Lọ', N'Trị tiêu chảy, kiết lỵ', N'4-6 viên chia 2 lần', N'Phụ nữ có thai');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'BER001', '10-10-2027', '10-10-2024', 500, 500, 8000 FROM THUOC WHERE ten_thuoc = N'Berberin'
    UNION ALL SELECT id_thuoc, 'BER002', '11-11-2027', '11-11-2024', 300, 300, 8000 FROM THUOC WHERE ten_thuoc = N'Berberin';

----------------------------------------- THỰC PHẨM CHỨC NĂNG ----------------------------------------- 
-- 1. Sâm Alipas (Tăng cường sinh lực)
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Vitamin và khoáng chất'), 1, N'Sâm Alipas', N'Eco', N'Eurycoma Longifolia', N'Viên nang', N'Lọ 30 viên', 750000, N'Hộp', N'Tăng cường sinh lực phái mạnh', N'1 viên/ngày', N'Người dưới 18 tuổi');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'ALI001', '01-01-2027', '01-01-2024', 50, 50, 600000 FROM THUOC WHERE ten_thuoc = N'Sâm Alipas'
    UNION ALL SELECT id_thuoc, 'ALI002', '05-05-2027', '05-05-2024', 60, 60, 600000 FROM THUOC WHERE ten_thuoc = N'Sâm Alipas';

-- 2. Boganic (Bổ gan)
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Vitamin và khoáng chất'), 1, N'Boganic', N'Traphaco', N'Actiso, Rau đắng đất', N'Viên nang mềm', N'Hộp 5 vỉ', 90000, N'Hộp', N'Giải độc gan, mát gan', N'2 viên x 3 lần/ngày', N'Người bị tắc mật');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'BOG001', '12-12-2026', '12-12-2023', 200, 200, 65000 FROM THUOC WHERE ten_thuoc = N'Boganic'
    UNION ALL SELECT id_thuoc, 'BOG002', '01-06-2027', '01-06-2024', 150, 150, 65000 FROM THUOC WHERE ten_thuoc = N'Boganic';

-- 3. Glucosamine Orihiro (Khớp)
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Vitamin và khoáng chất'), 1, N'Glucosamine Orihiro', N'Orihiro Japan', N'Glucosamine', N'Viên nén', N'1500mg', 600000, N'Lọ', N'Tái tạo sụn khớp, giảm đau khớp', N'10 viên/ngày chia 2 lần', N'Dị ứng hải sản');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'GLU001', '20-10-2027', '20-10-2024', 40, 40, 450000 FROM THUOC WHERE ten_thuoc = N'Glucosamine Orihiro'
    UNION ALL SELECT id_thuoc, 'GLU002', '01-01-2028', '01-01-2025', 40, 40, 450000 FROM THUOC WHERE ten_thuoc = N'Glucosamine Orihiro';

-- 4. Collagen Youtheory (Làm đẹp)
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Vitamin và khoáng chất'), 1, N'Collagen Youtheory', N'Youtheory', N'Collagen Type 1,2,3', N'Viên nén', N'6000mg', 580000, N'Lọ', N'Đẹp da, chống lão hóa, tốt cho tóc móng', N'6 viên/ngày', N'Suy thận nặng');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'COL001', '15-05-2026', '15-05-2023', 60, 60, 420000 FROM THUOC WHERE ten_thuoc = N'Collagen Youtheory'
    UNION ALL SELECT id_thuoc, 'COL002', '15-05-2027', '15-05-2024', 80, 80, 420000 FROM THUOC WHERE ten_thuoc = N'Collagen Youtheory';

-- 5. Men vi sinh Bio-acimin (Tiêu hóa)
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Vitamin và khoáng chất'), 1, N'Bio-acimin Gold', N'Việt Đức', N'Lợi khuẩn, Vitamin', N'Cốm', N'Gói 3g', 150000, N'Hộp', N'Hỗ trợ tiêu hóa, ăn ngon miệng', N'1-2 gói/ngày', N'Mẫn cảm thành phần');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'BIO001', '01-01-2026', '01-01-2024', 100, 100, 110000 FROM THUOC WHERE ten_thuoc = N'Bio-acimin Gold'
    UNION ALL SELECT id_thuoc, 'BIO002', '01-06-2026', '01-06-2024', 100, 100, 110000 FROM THUOC WHERE ten_thuoc = N'Bio-acimin Gold';

-- 6. Hoạt Huyết Dưỡng Não (Tuần hoàn)
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Vitamin và khoáng chất'), 1, N'Hoạt Huyết Dưỡng Não', N'Traphaco', N'Đinh lăng, Bạch quả', N'Viên bao đường', N'Hộp 100 viên', 95000, N'Hộp', N'Tăng cường trí nhớ, giảm đau đầu', N'2 viên x 2 lần/ngày', N'Phụ nữ có thai');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'HHD001', '01-08-2026', '01-08-2024', 200, 200, 70000 FROM THUOC WHERE ten_thuoc = N'Hoạt Huyết Dưỡng Não'
    UNION ALL SELECT id_thuoc, 'HHD002', '10-10-2026', '10-10-2024', 200, 200, 70000 FROM THUOC WHERE ten_thuoc = N'Hoạt Huyết Dưỡng Não';

-- 7. DHC Zinc (Kẽm)
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Vitamin và khoáng chất'), 1, N'DHC Zinc', N'DHC Japan', N'Kẽm, Selen', N'Viên nang', N'Gói 60 ngày', 180000, N'Gói', N'Giảm mụn, tăng đề kháng, khỏe tóc', N'1 viên/ngày', N'Không dùng quá liều');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'ZIN001', '01-01-2027', '01-01-2024', 50, 50, 130000 FROM THUOC WHERE ten_thuoc = N'DHC Zinc'
    UNION ALL SELECT id_thuoc, 'ZIN002', '01-02-2027', '01-02-2024', 50, 50, 130000 FROM THUOC WHERE ten_thuoc = N'DHC Zinc';

-- 8. Blackmores Evening Primrose Oil (Hoa anh thảo)
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Vitamin và khoáng chất'), 1, N'Tinh dầu Hoa Anh Thảo', N'Blackmores', N'Evening Primrose Oil', N'Viên nang mềm', N'1000mg', 450000, N'Lọ', N'Cân bằng nội tiết tố nữ, đẹp da', N'2 viên/ngày', N'Người dùng thuốc chống đông');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'EPO001', '15-07-2026', '15-07-2023', 40, 40, 320000 FROM THUOC WHERE ten_thuoc = N'Tinh dầu Hoa Anh Thảo'
    UNION ALL SELECT id_thuoc, 'EPO002', '15-07-2027', '15-07-2024', 60, 60, 320000 FROM THUOC WHERE ten_thuoc = N'Tinh dầu Hoa Anh Thảo';

-- 9. Viên uống rau củ DHC
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Vitamin và khoáng chất'), 1, N'Viên uống Rau củ DHC', N'DHC Japan', N'32 loại rau củ', N'Viên nén', N'Gói 60 ngày', 250000, N'Gói', N'Bổ sung chất xơ, giảm táo bón', N'4 viên/ngày', N'Dị ứng thành phần');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'VEG001', '01-01-2026', '01-01-2024', 100, 100, 180000 FROM THUOC WHERE ten_thuoc = N'Viên uống Rau củ DHC'
    UNION ALL SELECT id_thuoc, 'VEG002', '01-02-2026', '01-02-2024', 100, 100, 180000 FROM THUOC WHERE ten_thuoc = N'Viên uống Rau củ DHC';

-- 10. Omega 3-6-9
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Vitamin và khoáng chất'), 1, N'Omega 3-6-9', N'Healthy Care', N'Dầu cá, dầu hạt lanh', N'Viên nang mềm', N'1000mg', 420000, N'Lọ', N'Tốt cho tim mạch, mắt và não', N'2 viên/ngày', N'Dị ứng cá biển');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'OMG369', '10-10-2026', '10-10-2023', 50, 50, 300000 FROM THUOC WHERE ten_thuoc = N'Omega 3-6-9'
    UNION ALL SELECT id_thuoc, 'OMG369_2', '11-11-2026', '11-11-2023', 50, 50, 300000 FROM THUOC WHERE ten_thuoc = N'Omega 3-6-9';

----------------------------------------- MỸ PHẨM / DƯỢC MỸ PHẨM ----------------------------------------- 
-- 1. La Roche-Posay Effaclar Duo+ (Trị mụn)
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Kháng viêm'), 2, N'La Roche-Posay Duo+', N'La Roche-Posay', N'Niacinamide, Salicylic Acid', N'Kem bôi', N'Tuýp 40ml', 420000, N'Tuýp', N'Giảm mụn sưng viêm, ngừa thâm', N'Thoa 2 lần/ngày', N'Da quá mẫn cảm');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'DUO001', '01-01-2027', '01-01-2024', 80, 80, 320000 FROM THUOC WHERE ten_thuoc = N'La Roche-Posay Duo+'
    UNION ALL SELECT id_thuoc, 'DUO002', '01-06-2027', '01-06-2024', 100, 100, 320000 FROM THUOC WHERE ten_thuoc = N'La Roche-Posay Duo+';

-- 2. Klenzit MS (Trị mụn)
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Kháng viêm'), 2, N'Klenzit MS', N'Glenmark', N'Adapalene', N'Gel', N'Tuýp 15g', 120000, N'Tuýp', N'Trị mụn trứng cá, mụn ẩn', N'Thoa buổi tối', N'Phụ nữ có thai');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'KLE001', '12-12-2026', '12-12-2023', 200, 200, 85000 FROM THUOC WHERE ten_thuoc = N'Klenzit MS'
    UNION ALL SELECT id_thuoc, 'KLE002', '01-01-2027', '01-01-2024', 200, 200, 85000 FROM THUOC WHERE ten_thuoc = N'Klenzit MS';

-- 3. Sữa rửa mặt Cetaphil
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Kháng viêm'), 2, N'Sữa rửa mặt Cetaphil', N'Galderma', N'Gentle Skin Cleanser', N'Dung dịch', N'Chai 125ml', 130000, N'Chai', N'Làm sạch dịu nhẹ cho da nhạy cảm', N'Dùng 2 lần/ngày', N'Không');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'CET001', '05-05-2027', '05-05-2024', 150, 150, 95000 FROM THUOC WHERE ten_thuoc = N'Sữa rửa mặt Cetaphil'
    UNION ALL SELECT id_thuoc, 'CET002', '10-10-2027', '10-10-2024', 150, 150, 95000 FROM THUOC WHERE ten_thuoc = N'Sữa rửa mặt Cetaphil';

-- 4. Kem chống nắng Anessa
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Vitamin và khoáng chất'), 2, N'KCN Anessa Milk', N'Shiseido', N'Zinc Oxide, Titanium Dioxide', N'Sữa', N'Chai 60ml', 550000, N'Chai', N'Chống nắng bảo vệ da', N'Thoa trước khi ra nắng 20p', N'Kích ứng thành phần');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'ANE001', '01-03-2027', '01-03-2024', 60, 60, 400000 FROM THUOC WHERE ten_thuoc = N'KCN Anessa Milk'
    UNION ALL SELECT id_thuoc, 'ANE002', '01-04-2027', '01-04-2024', 60, 60, 400000 FROM THUOC WHERE ten_thuoc = N'KCN Anessa Milk';

-- 5. Tẩy trang Bioderma Hồng
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Kháng viêm'), 2, N'Tẩy trang Bioderma Hồng', N'Bioderma', N'Micellar Water', N'Dung dịch', N'Chai 500ml', 380000, N'Chai', N'Làm sạch bụi bẩn, lớp trang điểm', N'Dùng bông tẩy trang lau sạch', N'Không');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'BIOH001', '01-01-2027', '01-01-2024', 100, 100, 280000 FROM THUOC WHERE ten_thuoc = N'Tẩy trang Bioderma Hồng'
    UNION ALL SELECT id_thuoc, 'BIOH002', '02-02-2027', '02-02-2024', 100, 100, 280000 FROM THUOC WHERE ten_thuoc = N'Tẩy trang Bioderma Hồng';

-- 6. Serum Vitamin C Melano CC
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Vitamin và khoáng chất'), 2, N'Melano CC Essence', N'Rohto', N'Vitamin C, Vitamin E', N'Tinh chất', N'Tuýp 20ml', 250000, N'Tuýp', N'Mờ thâm nám, sáng da', N'Thoa 4-5 giọt/lần', N'Da mụn viêm nặng');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'CC001', '01-09-2027', '01-09-2024', 120, 120, 180000 FROM THUOC WHERE ten_thuoc = N'Melano CC Essence'
    UNION ALL SELECT id_thuoc, 'CC002', '10-10-2027', '10-10-2024', 120, 120, 180000 FROM THUOC WHERE ten_thuoc = N'Melano CC Essence';

-- 7. Kem dưỡng ẩm Obagi
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Vitamin và khoáng chất'), 2, N'Obagi Kinetin+', N'Obagi Medical', N'Kinetin, Zeatin', N'Kem', N'Chai 50ml', 1200000, N'Chai', N'Phục hồi da, cấp ẩm sâu', N'Thoa 2 lần/ngày', N'Kích ứng');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'OBA001', '01-01-2026', '01-01-2023', 20, 20, 950000 FROM THUOC WHERE ten_thuoc = N'Obagi Kinetin+'
    UNION ALL SELECT id_thuoc, 'OBA002', '05-05-2026', '05-05-2023', 20, 20, 950000 FROM THUOC WHERE ten_thuoc = N'Obagi Kinetin+';

-- 8. Derma Forte (Trị thâm mụn)
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Kháng viêm'), 2, N'Derma Forte', N'Gamma', N'Azelaic Acid', N'Gel', N'Tuýp 15g', 105000, N'Tuýp', N'Giảm thâm mụn, ngừa mụn', N'Thoa sáng tối', N'Mẫn cảm');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'DER001', '01-01-2027', '01-01-2024', 300, 300, 75000 FROM THUOC WHERE ten_thuoc = N'Derma Forte'
    UNION ALL SELECT id_thuoc, 'DER002', '05-01-2027', '05-01-2024', 300, 300, 75000 FROM THUOC WHERE ten_thuoc = N'Derma Forte';

-- 9. Xịt khoáng Vichy
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Vitamin và khoáng chất'), 2, N'Xịt khoáng Vichy', N'Vichy', N'Nước khoáng núi lửa', N'Dung dịch nén', N'Chai 150ml', 260000, N'Chai', N'Cấp nước tức thì, làm dịu da', N'Xịt khi cần thiết', N'Không');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'VIC001', '01-06-2027', '01-06-2024', 100, 100, 190000 FROM THUOC WHERE ten_thuoc = N'Xịt khoáng Vichy'
    UNION ALL SELECT id_thuoc, 'VIC002', '01-07-2027', '01-07-2024', 100, 100, 190000 FROM THUOC WHERE ten_thuoc = N'Xịt khoáng Vichy';

-- 10. Sữa tắm Eucerin
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Kháng viêm'), 2, N'Sữa tắm Eucerin pH5', N'Eucerin', N'pH5 Citrate Buffer', N'Sữa', N'Chai 400ml', 220000, N'Chai', N'Làm sạch, duy trì độ ẩm da', N'Dùng hàng ngày', N'Không');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'EUC001', '01-01-2027', '01-01-2024', 80, 80, 160000 FROM THUOC WHERE ten_thuoc = N'Sữa tắm Eucerin pH5'
    UNION ALL SELECT id_thuoc, 'EUC002', '02-01-2027', '02-01-2024', 80, 80, 160000 FROM THUOC WHERE ten_thuoc = N'Sữa tắm Eucerin pH5';

----------------------------------------- ĐÔNG Y - THẢO DƯỢC ----------------------------------------- 
-- 1. Thuốc ho Bảo Thanh
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Kháng viêm'), 3, N'Thuốc ho Bảo Thanh', N'Hoa Linh', N'Mật ong, Ô mai', N'Siro', N'Chai 125ml', 45000, N'Chai', N'Trị ho khan, ho có đờm', N'3 lần/ngày', N'Tiểu đường thận trọng');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'BT001', '10-10-2026', '10-10-2023', 300, 300, 32000 FROM THUOC WHERE ten_thuoc = N'Thuốc ho Bảo Thanh'
    UNION ALL SELECT id_thuoc, 'BT002', '11-11-2026', '11-11-2023', 300, 300, 32000 FROM THUOC WHERE ten_thuoc = N'Thuốc ho Bảo Thanh';

-- 2. Bổ Phế Nam Hà
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Kháng viêm'), 3, N'Bổ Phế Nam Hà', N'Nam Hà', N'Tỳ bà diệp, Bách bộ', N'Viên ngậm', N'Hộp 2 vỉ', 25000, N'Hộp', N'Tiêu đờm, bổ phổi, sát trùng họng', N'Ngậm 4-6 viên/ngày', N'Trẻ em dưới 2 tuổi');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'NH001', '01-01-2026', '01-01-2024', 500, 500, 18000 FROM THUOC WHERE ten_thuoc = N'Bổ Phế Nam Hà'
    UNION ALL SELECT id_thuoc, 'NH002', '01-02-2026', '01-02-2024', 500, 500, 18000 FROM THUOC WHERE ten_thuoc = N'Bổ Phế Nam Hà';

-- 3. Cao Sao Vàng
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Giảm đau - Hạ sốt'), 3, N'Cao Sao Vàng', N'TW3', N'Tinh dầu bạc hà, long não', N'Cao bôi', N'Hộp 3g', 5000, N'Hộp', N'Trị đau đầu, chóng mặt, côn trùng cắn', N'Bôi ngoài da', N'Bôi lên mắt');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'SV001', '01-01-2028', '01-01-2024', 1000, 1000, 2000 FROM THUOC WHERE ten_thuoc = N'Cao Sao Vàng'
    UNION ALL SELECT id_thuoc, 'SV002', '01-01-2028', '01-01-2024', 1000, 1000, 2000 FROM THUOC WHERE ten_thuoc = N'Cao Sao Vàng';

-- 4. Dầu Phật Linh
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Giảm đau - Hạ sốt'), 3, N'Dầu Phật Linh', N'Trường Sơn', N'Tinh dầu tràm, bạc hà', N'Dầu nước', N'Chai 5ml', 10000, N'Chai', N'Trị cảm cúm, sổ mũi, đau bụng', N'Bôi hoặc xông', N'Trẻ sơ sinh');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'PL001', '05-05-2027', '05-05-2024', 800, 800, 6000 FROM THUOC WHERE ten_thuoc = N'Dầu Phật Linh'
    UNION ALL SELECT id_thuoc, 'PL002', '06-06-2027', '06-06-2024', 800, 800, 6000 FROM THUOC WHERE ten_thuoc = N'Dầu Phật Linh';

-- 5. Trà Gừng
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Vitamin và khoáng chất'), 3, N'Trà Gừng', N'Traphaco', N'Gừng tươi', N'Cốm hòa tan', N'Hộp 10 gói', 30000, N'Hộp', N'Làm ấm cơ thể, trị đầy hơi', N'Pha nước nóng uống', N'Người đang sốt cao');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'TG001', '01-01-2026', '01-01-2024', 200, 200, 22000 FROM THUOC WHERE ten_thuoc = N'Trà Gừng'
    UNION ALL SELECT id_thuoc, 'TG002', '01-02-2026', '01-02-2024', 200, 200, 22000 FROM THUOC WHERE ten_thuoc = N'Trà Gừng';

-- 6. Phong Tê Thấp
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Giảm đau - Hạ sốt'), 3, N'Phong Tê Thấp Bà Giằng', N'Bà Giằng', N'Mã tiền chế', N'Viên hoàn', N'Lọ 250 viên', 60000, N'Lọ', N'Trị đau nhức xương khớp, tê mỏi', N'10-12 viên/lần', N'Phụ nữ có thai, trẻ em');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'BG001', '01-09-2026', '01-09-2024', 150, 150, 45000 FROM THUOC WHERE ten_thuoc = N'Phong Tê Thấp Bà Giằng'
    UNION ALL SELECT id_thuoc, 'BG002', '01-10-2026', '01-10-2024', 150, 150, 45000 FROM THUOC WHERE ten_thuoc = N'Phong Tê Thấp Bà Giằng';

-- 7. Diệp Hạ Châu
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Kháng viêm'), 3, N'Diệp Hạ Châu', N'Vạn Xuân', N'Diệp hạ châu đắng', N'Viên nang', N'Lọ 100 viên', 45000, N'Lọ', N'Giải độc gan, hỗ trợ viêm gan B', N'3 viên x 3 lần/ngày', N'Phụ nữ có thai');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'DHC001', '01-01-2027', '01-01-2024', 200, 200, 30000 FROM THUOC WHERE ten_thuoc = N'Diệp Hạ Châu'
    UNION ALL SELECT id_thuoc, 'DHC002', '01-02-2027', '01-02-2024', 200, 200, 30000 FROM THUOC WHERE ten_thuoc = N'Diệp Hạ Châu';

-- 8. Tam Thất Bắc
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Vitamin và khoáng chất'), 3, N'Bột Tam Thất Bắc', N'Dược liệu TW', N'Củ Tam thất', N'Bột', N'Hũ 100g', 350000, N'Hũ', N'Bồi bổ sức khỏe, cầm máu, tiêu u', N'1-2 thìa cafe/ngày', N'Phụ nữ có thai');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'TT001', '01-12-2025', '01-12-2023', 50, 50, 280000 FROM THUOC WHERE ten_thuoc = N'Bột Tam Thất Bắc'
    UNION ALL SELECT id_thuoc, 'TT002', '01-01-2026', '01-01-2024', 50, 50, 280000 FROM THUOC WHERE ten_thuoc = N'Bột Tam Thất Bắc';

-- 9. Cao dán Salonpas
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Giảm đau - Hạ sốt'), 3, N'Salonpas', N'Hisamitsu', N'Methyl Salicylate', N'Miếng dán', N'Hộp 20 miếng', 18000, N'Hộp', N'Giảm đau cơ, đau lưng', N'Dán lên vùng đau', N'Vết thương hở');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'SAL001', '01-01-2028', '01-01-2024', 1000, 1000, 12000 FROM THUOC WHERE ten_thuoc = N'Salonpas'
    UNION ALL SELECT id_thuoc, 'SAL002', '01-02-2028', '01-02-2024', 1000, 1000, 12000 FROM THUOC WHERE ten_thuoc = N'Salonpas';

-- 10. Viên ngậm Kha Tử
INSERT INTO THUOC (id_danh_muc, id_loai_thuoc, ten_thuoc, hang, hoat_chat, dang_bao_che, ham_luong, gia_ban, don_vi_tinh, cong_dung, lieu_dung, chong_chi_dinh)
VALUES ((SELECT id_danh_muc FROM DANH_MUC_THUOC WHERE ten_danh_muc = N'Kháng viêm'), 3, N'Viên ngậm Kha Tử', N'Vạn Xuân', N'Kha tử, Cam thảo', N'Viên ngậm', N'Vỉ 10 viên', 12000, N'Vỉ', N'Trị khản tiếng, mất tiếng', N'Ngậm khi đau họng', N'Không');
    INSERT INTO LO_THUOC (id_thuoc, so_lo, han_su_dung, ngay_san_xuat, so_luong_nhap, so_luong_ton, gia_nhap) SELECT id_thuoc, 'KT001', '01-06-2026', '01-06-2024', 300, 300, 8000 FROM THUOC WHERE ten_thuoc = N'Viên ngậm Kha Tử'
    UNION ALL SELECT id_thuoc, 'KT002', '01-07-2026', '01-07-2024', 300, 300, 8000 FROM THUOC WHERE ten_thuoc = N'Viên ngậm Kha Tử';
PRINT N'=== 4. KHỞI TẠO HOÁ ĐƠN MẪU (ĐÃ BỔ SUNG TRỪ KHO) ===';


-- Khai báo biến để lưu thông tin khách hàng cho Hóa đơn 1
DECLARE @IdKhach1 INT, @TenKhach1 NVARCHAR(255), @SDTKhach1 NVARCHAR(15), @DiaChiKhach1 NVARCHAR(255), @EmailKhach1 NVARCHAR(255);
SELECT @IdKhach1 = id_tai_khoan, @TenKhach1 = ho_ten, @SDTKhach1 = so_dien_thoai, @DiaChiKhach1 = dia_chi, @EmailKhach1 = email
FROM TAI_KHOAN WHERE so_dien_thoai = '0918000111';

-- Khai báo biến để lưu thông tin khách hàng cho Hóa đơn 2
DECLARE @IdKhach2 INT, @TenKhach2 NVARCHAR(255), @SDTKhach2 NVARCHAR(15), @DiaChiKhach2 NVARCHAR(255), @EmailKhach2 NVARCHAR(255);
SELECT @IdKhach2 = id_tai_khoan, @TenKhach2 = ho_ten, @SDTKhach2 = so_dien_thoai, @DiaChiKhach2 = dia_chi, @EmailKhach2 = email
FROM TAI_KHOAN WHERE so_dien_thoai = '0918000222';

DECLARE @NewHoaDonID INT;
DECLARE @SoLuongBan INT;
DECLARE @IdLoThuocBan INT;
DECLARE @GiaBanHienTai INT;
DECLARE @SoLoThuoc NVARCHAR(100);


-- >>> Hoá đơn 1
SET @SoLoThuoc = 'PARA01082015';
SET @SoLuongBan = 3;

-- Lấy thông tin Lô thuốc
SELECT @IdLoThuocBan = id_lo_thuoc, @GiaBanHienTai = gia_ban
FROM LO_THUOC L JOIN THUOC T ON L.id_thuoc = T.id_thuoc 
WHERE so_lo = @SoLoThuoc;


INSERT INTO HOA_DON (id_nguoi_mua, ngay_ban, tong_tien, ten_nguoi_nhan, sdt_nguoi_nhan, dia_chi_giao_hang, email_nguoi_mua, hinh_thuc_thanh_toan)
VALUES (@IdKhach1, GETDATE(), 0, @TenKhach1, @SDTKhach1, @DiaChiKhach1, @EmailKhach1, N'Tiền mặt');

SET @NewHoaDonID = SCOPE_IDENTITY();

    -- Thêm chi tiết cho PARACETAMOL
    INSERT INTO CHI_TIET_HOA_DON (id_hoa_don, id_lo_thuoc, so_luong, don_gia, thanh_tien)
    VALUES (@NewHoaDonID, @IdLoThuocBan, @SoLuongBan, @GiaBanHienTai, @SoLuongBan * @GiaBanHienTai);
    
    -- ** BỔ SUNG: TRỪ KHO **
    UPDATE LO_THUOC
    SET so_luong_ton = so_luong_ton - @SoLuongBan
    WHERE id_lo_thuoc = @IdLoThuocBan;

    -- Thêm chi tiết cho VITAMIN C
    SET @SoLoThuoc = 'VITC001';
    SET @SoLuongBan = 4;
    SELECT @IdLoThuocBan = id_lo_thuoc, @GiaBanHienTai = gia_ban
    FROM LO_THUOC L JOIN THUOC T ON L.id_thuoc = T.id_thuoc 
    WHERE so_lo = @SoLoThuoc;

    INSERT INTO CHI_TIET_HOA_DON (id_hoa_don, id_lo_thuoc, so_luong, don_gia, thanh_tien)
    VALUES (@NewHoaDonID, @IdLoThuocBan, @SoLuongBan, @GiaBanHienTai, @SoLuongBan * @GiaBanHienTai);
    
    -- ** BỔ SUNG: TRỪ KHO **
    UPDATE LO_THUOC
    SET so_luong_ton = so_luong_ton - @SoLuongBan
    WHERE id_lo_thuoc = @IdLoThuocBan;


-- >>> Hoá đơn 2
SET @SoLoThuoc = 'AMOXI001';
SET @SoLuongBan = 2;

-- Lấy thông tin Lô thuốc
SELECT @IdLoThuocBan = id_lo_thuoc, @GiaBanHienTai = gia_ban
FROM LO_THUOC L JOIN THUOC T ON L.id_thuoc = T.id_thuoc 
WHERE so_lo = @SoLoThuoc;

INSERT INTO HOA_DON (id_nguoi_mua, ngay_ban, tong_tien, ten_nguoi_nhan, sdt_nguoi_nhan, dia_chi_giao_hang, email_nguoi_mua, hinh_thuc_thanh_toan)
VALUES (@IdKhach2, GETDATE(), 0, @TenKhach2, @SDTKhach2, @DiaChiKhach2, @EmailKhach2, N'Chuyển khoản QR');

SET @NewHoaDonID = SCOPE_IDENTITY();

    -- Thêm chi tiết cho AMOXI
    INSERT INTO CHI_TIET_HOA_DON (id_hoa_don, id_lo_thuoc, so_luong, don_gia, thanh_tien)
    VALUES (@NewHoaDonID, @IdLoThuocBan, @SoLuongBan, @GiaBanHienTai, @SoLuongBan * @GiaBanHienTai);

    -- ** BỔ SUNG: TRỪ KHO **
    UPDATE LO_THUOC
    SET so_luong_ton = so_luong_ton - @SoLuongBan
    WHERE id_lo_thuoc = @IdLoThuocBan;

-- Cập nhật tổng tiền
UPDATE HOA_DON
SET tong_tien = (SELECT SUM(thanh_tien) FROM CHI_TIET_HOA_DON WHERE CHI_TIET_HOA_DON.id_hoa_don = HOA_DON.id_hoa_don);

-- Cập nhật ảnh thuốc
UPDATE THUOC
SET anh_thuoc = LOWER(CONCAT(LEFT(REPLACE(ten_thuoc, ' ', '_'), 6), '_image.jpg'));

--UPDATE THUOC
--SET anh_thuoc = NULL;


PRINT N'=== HOÀN TẤT ===';
GO

select * from THUOC