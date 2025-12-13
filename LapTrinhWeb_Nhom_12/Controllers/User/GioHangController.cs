using LapTrinhWeb_Nhom_12.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
namespace LapTrinhWeb_Nhom_12.Controllers
{

    // GET: GioHang
    public class GioHangController : Controller
    {
        private NHA_THUOCEntities db = new NHA_THUOCEntities();

        // Action Thêm vào giỏ hàng
        // Tham số: id (mã thuốc), soLuong (mặc định là 1 nếu không truyền)
        public ActionResult ThemGioHang(int id, int soLuong = 1)
        {
            // 1. Lấy giỏ hàng hiện tại từ Session
            // Ép kiểu về List<CartItem>
            var gioHang = Session["GioHang"] as List<CartItem>;

            // Nếu Session chưa có gì (giỏ hàng rỗng), thì khởi tạo List mới
            if (gioHang == null)
            {
                gioHang = new List<CartItem>();
                Session["GioHang"] = gioHang;
            }

            // 2. KIỂM TRA TRÙNG: Tìm xem thuốc này đã có trong giỏ chưa
            var item = gioHang.FirstOrDefault(t => t.IdThuoc == id);

            if (item != null)
            {
                // TRƯỜNG HỢP 1: Đã có trong giỏ -> Cộng thêm số lượng
                item.SoLuong += soLuong;
            }
            else
            {
                // TRƯỜNG HỢP 2: Chưa có -> Tìm thuốc trong DB và thêm mới
                var thuoc = db.THUOCs.Find(id);
                if (thuoc == null)
                {
                    // Xử lý lỗi nếu ID không tồn tại (ví dụ hacker sửa ID trên URL)
                    return RedirectToAction("TrangChu", "TrangChu");
                }

                // Tạo đối tượng CartItem mới
                item = new CartItem
                {
                    IdThuoc = thuoc.id_thuoc,
                    TenThuoc = thuoc.ten_thuoc,
                    AnhThuoc = thuoc.anh_thuoc,
                    DonGia = thuoc.gia_ban ?? 0, // Xử lý nếu giá null
                    SoLuong = soLuong
                };

                // Đẩy vào danh sách
                gioHang.Add(item);
            }

            Session["GioHang"] = gioHang;

            // Gán thông báo để hiển thị ra Layout (như tôi đã gợi ý ở câu trước)
            TempData["SuccessMessage"] = "Đã thêm thuốc vào giỏ hàng thành công!";

            // 4. Chuyển hướng người dùng
            // Cách hay nhất: Quay lại đúng trang họ vừa đứng (Trang chủ hoặc Chi tiết)
            if (Request.UrlReferrer != null)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }

            // Mặc định thì về trang chủ
            return RedirectToAction("TrangChu", "TrangChu");
        }
        public ActionResult XoaGioHang(int id)
        {
            var gioHang = Session["GioHang"] as List<CartItem>;
            if (gioHang != null)
            {
                var itemXoa = gioHang.FirstOrDefault(m => m.IdThuoc == id);
                if (itemXoa != null)
                {
                    gioHang.Remove(itemXoa);
                }
                Session["GioHang"] = gioHang;
            }
            return RedirectToAction("XemGioHang");
        }
        public ActionResult CapNhatGioHang(int id, int soLuong)
        {
            var gioHang = Session["GioHang"] as List<CartItem>;
            if (gioHang != null)
            {
                var itemSua = gioHang.FirstOrDefault(m => m.IdThuoc == id);
                if (itemSua != null)
                {
                    itemSua.SoLuong = soLuong;
                }
                Session["GioHang"] = gioHang;
            }
            return RedirectToAction("XemGioHang");
        }
        public ActionResult XemGioHang()
        {
            // 1. Lấy giỏ hàng từ Session
            var gioHang = Session["GioHang"] as List<CartItem>;

            // 2. Nếu giỏ hàng chưa có hoặc rỗng, khởi tạo list rỗng để tránh lỗi null ở View
            if (gioHang == null || gioHang.Count == 0)
            {
                ViewBag.ThongBao = "Giỏ hàng đang trống";
                return View(new List<CartItem>());
            }

            // 3. Tính tổng tiền để hiển thị (hoặc tính trực tiếp bên View cũng được)
            ViewBag.TongTien = gioHang.Sum(s => s.ThanhTien);

            // Breadcrumb
            ViewBag.CurrentPage = "Giỏ hàng"; // Tên trang hiện tại

            return View(gioHang);
        }
        [HttpPost]
        public ActionResult CapNhatSoLuongAjax(int id, int soLuong)
        {
            // 1. Lấy giỏ hàng từ Session
            List<CartItem> gioHang = Session["GioHang"] as List<CartItem>;

            if (gioHang != null)
            {
                // 2. Tìm sản phẩm và cập nhật
                var item = gioHang.FirstOrDefault(x => x.IdThuoc == id);
                if (item != null)
                {
                    item.SoLuong = soLuong;
                }

                // 3. Tính toán lại tổng tiền giỏ hàng
                decimal tongTien = gioHang.Sum(x => x.ThanhTien);

                // 4. Trả về JSON cho JavaScript
                return Json(new
                {
                    success = true,
                    itemTotal = item.ThanhTien.ToString("N0") + " đ", // Format tiền Việt
                    cartTotal = tongTien.ToString("N0") + " đ"
                });
            }

            return Json(new { success = false, message = "Lỗi giỏ hàng" });
        }
        // Action này dùng để load nội dung Mini Cart
        public ActionResult GetMiniCart()
        {
            List<CartItem> gioHang = Session["GioHang"] as List<CartItem>;
            if (gioHang == null) gioHang = new List<CartItem>();

            return PartialView("_MiniCartPartial", gioHang);
        }

        // Action Thêm giỏ hàng trả về JSON (để dùng Ajax)
        [HttpPost]
        public ActionResult ThemGioHangAjax(int id)
        {
            // 1. Lấy giỏ hàng từ Session
            List<CartItem> gioHang = Session["GioHang"] as List<CartItem>;

            // 2. QUAN TRỌNG: Nếu chưa có thì tạo mới ngay (Khắc phục lỗi Null)
            if (gioHang == null)
            {
                gioHang = new List<CartItem>();
                Session["GioHang"] = gioHang;
            }

            // 3. Logic thêm sản phẩm (Bạn cần đoạn này để hàng thực sự vào giỏ)
            var sanPham = gioHang.FirstOrDefault(s => s.IdThuoc == id);
            if (sanPham != null)
            {
                sanPham.SoLuong++;
            }
            else
            {
                // Chưa có thì tìm trong DB và thêm mới
                var thuoc = db.THUOCs.Find(id); // Đảm bảo 'db' đã được khai báo ở trên
                if (thuoc != null)
                {
                    var newItem = new CartItem
                    {
                        IdThuoc = thuoc.id_thuoc,
                        TenThuoc = thuoc.ten_thuoc,
                        AnhThuoc = thuoc.anh_thuoc,
                        DonGia = (int)(thuoc.gia_ban ?? 0),
                        SoLuong = 1,
                        DonViTinh = thuoc.don_vi_tinh
                    };
                    gioHang.Add(newItem);
                }
            }

            // 4. Tính tổng và trả về JSON
            int tongSoLuong = gioHang.Sum(x => x.SoLuong);
            return Json(new { success = true, totalQty = tongSoLuong });
        }

        // Action Xóa item từ Mini Cart
        [HttpPost]
        public ActionResult XoaKhoiMiniCart(int id)
        {
            List<CartItem> gioHang = Session["GioHang"] as List<CartItem>;
            var item = gioHang.FirstOrDefault(x => x.IdThuoc == id);
            if (item != null)
            {
                gioHang.Remove(item);
            }
            return Json(new { success = true });
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
