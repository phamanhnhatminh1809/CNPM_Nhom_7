using LapTrinhWeb_Nhom_12.Helper;
using LapTrinhWeb_Nhom_12.Models;
using System.Linq;
using System.Web.Mvc;

namespace LapTrinhWeb_Nhom_12.Controllers
{
    public class TaiKhoanController : Controller
    {
        private NHA_THUOCEntities db = new NHA_THUOCEntities();

        // ---------------- ĐĂNG KÝ ----------------
        [HttpGet]
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
                // 1. Kiểm tra tên đăng nhập đã tồn tại chưa
                var check = db.TAI_KHOAN.FirstOrDefault(s => s.ten_dang_nhap == model.TenDangNhap);
                if (check == null)
                {
                    // 2. Lấy ID quyền Khách Hàng (Mặc định khi đăng ký)
                    // Giả sử quyền Khách hàng có ten_quyen = 'KhachHang'
                    var quyenKhach = db.PHAN_QUYEN.FirstOrDefault(q => q.ten_quyen == "KhachHang");

                    if (quyenKhach == null)
                    {
                        ViewBag.Error = "Lỗi hệ thống: Chưa cấu hình quyền Khách Hàng.";
                        return View(model);
                    }

                    // 3. Tạo tài khoản mới
                    var taiKhoanMoi = new TAI_KHOAN();
                    taiKhoanMoi.ten_dang_nhap = model.TenDangNhap;
                    // Mã hóa mật khẩu trước khi lưu
                    taiKhoanMoi.mat_khau = MaHoa.ToMD5(model.MatKhau);
                    taiKhoanMoi.id_quyen = quyenKhach.id_quyen;
                    try
                    {
                        db.TAI_KHOAN.Add(taiKhoanMoi);
                        db.SaveChanges();
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                    {
                        // Đoạn code này sẽ giúp bạn xem lỗi cụ thể là gì
                        foreach (var entityValidationErrors in ex.EntityValidationErrors)
                        {
                            foreach (var validationError in entityValidationErrors.ValidationErrors)
                            {
                                // In lỗi ra cửa sổ Output của Visual Studio
                                System.Diagnostics.Debug.WriteLine("Lỗi tại trường: " + validationError.PropertyName + " - Nội dung: " + validationError.ErrorMessage);
                            }
                        }
                        // Ném lại lỗi để chương trình dừng lại cho bạn đọc Output
                        throw;
                    }

                    // Đăng ký thành công -> Chuyển qua đăng nhập
                    return RedirectToAction("DangNhap");
                }
                else
                {
                    ViewBag.Error = "Tên đăng nhập đã tồn tại";
                    return View(model);
                }
            }
            return View(model);
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
                // 1. Mã hóa mật khẩu người dùng nhập để so sánh với DB
                var f_password = MaHoa.ToMD5(model.MatKhau);

                // 2. Tìm tài khoản trong DB
                var data = db.TAI_KHOAN.FirstOrDefault(s => s.ten_dang_nhap.Equals(model.TenDangNhap) && s.mat_khau.Equals(f_password));
                if (model.TenDangNhap == "admin")
                {
                    data = db.TAI_KHOAN.FirstOrDefault(s => s.ten_dang_nhap.Equals(model.TenDangNhap) && s.mat_khau.Equals(model.MatKhau));
                }
                

                if (data != null)
                {
                    // 3. Đăng nhập thành công -> Lưu Session
                    // Lưu thông tin cơ bản
                    Session["User"] = data;
                    Session["TenDangNhap"] = data.ten_dang_nhap;
                    Session["IdQuyen"] = data.id_quyen;

                    // Lấy tên quyền để phân quyền sau này
                    var quyen = db.PHAN_QUYEN.Find(data.id_quyen);
                    Session["TenQuyen"] = quyen.ten_quyen;

                    return RedirectToAction("TrangChu", "TrangChu"); // Về trang chủ
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
            Session.Clear(); // Xóa hết session
            return RedirectToAction("TrangChu", "TrangChu");
        }
    }
}