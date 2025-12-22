using LapTrinhWeb_Nhom_12.Helper;
using LapTrinhWeb_Nhom_12.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace LapTrinhWeb_Nhom_12.Controllers
{
    public class TaiKhoanController : Controller
    {
        private NHA_THUOCEntities db = new NHA_THUOCEntities();

        // ---------------- ĐĂNG KÝ ----------------
        public ActionResult DangKy()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangKy(DangKyViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Kiểm tra trùng tên đăng nhập & Email
                if (db.TAI_KHOAN.Any(s => s.ten_dang_nhap == model.TenDangNhap))
                {
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại.");
                    return View(model);
                }
                if (db.TAI_KHOAN.Any(s => s.email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                    return View(model);
                }

                try
                {
                    // 2. Tạo mã OTP ngẫu nhiên (6 số)
                    Random rand = new Random();
                    string otpCode = rand.Next(100000, 999999).ToString();

                    // 3. Gửi Email
                    string content = "Mã xác thực đăng ký tài khoản của bạn là: <b>" + otpCode + "</b>";
                    new MailHelper().SendMail(model.Email, "Mã xác thực OTP MedForAll", content);

                    // 4. LƯU TẠM VÀO SESSION (Chưa lưu vào DB)
                    Session["TempRegModel"] = model;
                    Session["TempOTP"] = otpCode;

                    return RedirectToAction("XacThucOTP");
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Lỗi gửi mail: " + ex.Message;
                    return View(model);
                }
            }
            return View(model);
        }

        // ---------------- TRANG NHẬP OTP (GET) ----------------
        [HttpGet]
        public ActionResult XacThucOTP()
        {
            // Nếu không có thông tin đăng ký tạm thì đá về trang đăng ký
            if (Session["TempRegModel"] == null)
            {
                return RedirectToAction("DangKy");
            }
            return View();
        }

        // ----------------  XỬ LÝ OTP (POST) ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XacThucOTP(string otpInput)
        {
            // Lấy mã OTP đúng từ Session
            string correctOtp = Session["TempOTP"] as string;

            // Lấy thông tin khách hàng từ Session
            var model = Session["TempRegModel"] as DangKyViewModel;

            // Kiểm tra Session có tồn tại không (tránh timeout)
            if (model == null || string.IsNullOrEmpty(correctOtp))
            {
                TempData["Error"] = "Phiên giao dịch hết hạn, vui lòng đăng ký lại.";
                return RedirectToAction("DangKy");
            }

            // KIỂM TRA MÃ OTP
            if (otpInput == correctOtp)
            {
                // --- OTP ĐÚNG -> TIẾN HÀNH LƯU VÀO DATABASE ---

                var taiKhoanMoi = new TAI_KHOAN();
                taiKhoanMoi.ten_dang_nhap = model.TenDangNhap;
                taiKhoanMoi.mat_khau = model.MatKhau;
                taiKhoanMoi.email = model.Email;

                var quyenKhach = db.PHAN_QUYEN.FirstOrDefault(q => q.ten_quyen == "KhachHang");
                taiKhoanMoi.id_quyen = (quyenKhach != null) ? quyenKhach.id_quyen : 4;

                taiKhoanMoi.ho_ten = model.TenDangNhap;

                taiKhoanMoi.so_dien_thoai = "";
                taiKhoanMoi.dia_chi = "";

                db.TAI_KHOAN.Add(taiKhoanMoi);
                db.SaveChanges();

                Session.Remove("TempRegModel");
                Session.Remove("TempOTP");

                TempData["Success"] = "Đăng ký thành công! Mời bạn đăng nhập.";
                return RedirectToAction("DangNhap");
            }
            else
            {
                // OTP SAI
                ViewBag.Error = "Mã xác thực không đúng, vui lòng kiểm tra lại Email.";
                return View();
            }
        }

        // ---------------- ĐĂNG NHẬP ----------------
        [HttpGet]
        public ActionResult DangNhap()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangNhap(DangNhapViewModel model)
        {
            if (ModelState.IsValid)
            {

                // 2. Tìm tài khoản
                var data = db.TAI_KHOAN.FirstOrDefault(s => s.ten_dang_nhap.Equals(model.TenDangNhap) && s.mat_khau.Equals(model.MatKhau));

                if (data != null)
                {
                    // 3. Đăng nhập thành công -> Lưu Session

                    // Lưu toàn bộ object để dùng cho trang Thanh toán (Họ tên, SĐT, Địa chỉ...)
                    Session["User"] = data;

                    // Lưu các thông tin rời để hiển thị Header
                    Session["TenDangNhap"] = data.ten_dang_nhap;
                    Session["TenHienThi"] = data.ho_ten; // Hiển thị "Xin chào, Nguyễn Văn A" đẹp hơn "user123"
                    Session["IdQuyen"] = data.id_quyen;
                    Session["TenQuyen"] = data.PHAN_QUYEN?.ten_quyen; // Dùng navigation property

                    return RedirectToAction("TrangChu", "TrangChu");
                }
                else
                {
                    ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng";
                    return View(model);
                }
            }
            return View(model);
        }

        // ---------------- ĐĂNG XUẤT ----------------
        public ActionResult DangXuat()
        {
            Session.Clear(); // Xóa sạch session
            Session.Abandon(); // Hủy phiên làm việc
            return RedirectToAction("TrangChu", "TrangChu");
        }

        // ---------------- QUẢN LÝ THÔNG TIN CÁ NHÂN ----------------

        [HttpGet]
        public ActionResult ThongTin()
        {
            // 1. Kiểm tra đăng nhập
            if (Session["User"] == null)
            {
                return RedirectToAction("DangNhap");
            }

            // 2. Lấy ID từ Session
            var userSession = Session["User"] as TAI_KHOAN;

            // 3. Truy vấn lại từ Database để lấy dữ liệu mới nhất
            // (Tránh trường hợp Session lưu dữ liệu cũ chưa cập nhật)
            var user = db.TAI_KHOAN.Find(userSession.id_tai_khoan);

            if (user == null) return RedirectToAction("DangXuat");

            return View(user);
        }

        // POST: Cập nhật thông tin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThongTin(TAI_KHOAN model)
        {
            // Lấy ID người dùng đang đăng nhập
            var userSession = Session["User"] as TAI_KHOAN;
            if (userSession == null) return RedirectToAction("DangNhap");

            // Tìm tài khoản trong DB
            var userInDb = db.TAI_KHOAN.Find(userSession.id_tai_khoan);

            if (userInDb != null)
            {
                // Cập nhật các trường cho phép sửa
                // Lưu ý: KHÔNG cập nhật ten_dang_nhap, mat_khau, id_quyen tại đây
                userInDb.ho_ten = model.ho_ten;
                userInDb.email = model.email;
                userInDb.so_dien_thoai = model.so_dien_thoai;
                userInDb.dia_chi = model.dia_chi;

                try
                {
                    db.SaveChanges();

                    // Cập nhật lại Session để hiển thị đúng trên Header ngay lập tức
                    Session["User"] = userInDb;
                    Session["TenHienThi"] = userInDb.ho_ten;

                    ViewBag.Success = "Cập nhật thông tin thành công!";
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Lỗi cập nhật: " + ex.Message;
                }

                return View(userInDb);
            }

            return View(model);
        }

        // ---------------- LỊCH SỬ ĐƠN HÀNG ----------------

        // 1. Danh sách đơn hàng
        public ActionResult DonHang()
        {
            // Kiểm tra đăng nhập
            if (Session["User"] == null) return RedirectToAction("DangNhap");

            var user = Session["User"] as TAI_KHOAN;

            // Lấy danh sách hóa đơn của người này (Sắp xếp mới nhất lên đầu)
            var listDonHang = db.HOA_DON
                .Where(h => h.id_nguoi_mua == user.id_tai_khoan)
                .OrderByDescending(h => h.ngay_ban)
                .Select(h => new LichSuHoaDonViewModel
                {
                    IdHoaDon = h.id_hoa_don,
                    NgayDat = h.ngay_ban ?? DateTime.Now,
                    TongTien = h.tong_tien ?? 0,
                    DiaChiNhan = h.dia_chi_giao_hang
                }).ToList();

            return View(listDonHang);
        }

        // 2. Chi tiết một đơn hàng cụ thể
        public ActionResult ChiTietDonHang(int id)
        {
            if (Session["User"] == null) return RedirectToAction("DangNhap");
            var user = Session["User"] as TAI_KHOAN;

            // Tìm hóa đơn
            var hd = db.HOA_DON.Find(id);

            if (hd == null || hd.id_nguoi_mua != user.id_tai_khoan)
            {
                return RedirectToAction("DonHang");
            }

            // Lấy chi tiết sản phẩm
            var model = new LichSuHoaDonViewModel
            {
                IdHoaDon = hd.id_hoa_don,
                NgayDat = hd.ngay_ban ?? DateTime.Now,
                TongTien = hd.tong_tien ?? 0,
                DiaChiNhan = hd.dia_chi_giao_hang,

                ChiTiet = db.CHI_TIET_HOA_DON
                    .Where(ct => ct.id_hoa_don == id)
                    .Select(ct => new LichSuHoaDonChiTiet
                    {
                        // Nav properties: ChiTiet -> LoThuoc -> Thuoc
                        TenThuoc = ct.LO_THUOC.THUOC.ten_thuoc,
                        AnhThuoc = ct.LO_THUOC.THUOC.anh_thuoc,
                        SoLuong = ct.so_luong,
                        DonGia = ct.don_gia ?? 0,
                        ThanhTien = ct.thanh_tien ?? 0
                    }).ToList()
            };

            return View(model);
        }
    }
}