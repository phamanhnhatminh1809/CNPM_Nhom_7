using LapTrinhWeb_Nhom_12.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
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
            var gioHang = Session["GioHang"] as List<SanPhamTrongGioHang>;

            // Nếu Session chưa có gì (giỏ hàng rỗng), thì khởi tạo List mới
            if (gioHang == null)
            {
                gioHang = new List<SanPhamTrongGioHang>();
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

                // Tạo đối tượng SanPhamTrongGioHang mới
                item = new SanPhamTrongGioHang
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
            var gioHang = Session["GioHang"] as List<SanPhamTrongGioHang>;
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
            var gioHang = Session["GioHang"] as List<SanPhamTrongGioHang>;
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
            var gioHang = Session["GioHang"] as List<SanPhamTrongGioHang>;

            // 2. Nếu giỏ hàng chưa có hoặc rỗng, khởi tạo list rỗng để tránh lỗi null ở View
            if (gioHang == null || gioHang.Count == 0)
            {
                ViewBag.ThongBao = "Giỏ hàng đang trống";
                return View(new List<SanPhamTrongGioHang>());
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
            List<SanPhamTrongGioHang> gioHang = Session["GioHang"] as List<SanPhamTrongGioHang>;

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
            List<SanPhamTrongGioHang> gioHang = Session["GioHang"] as List<SanPhamTrongGioHang>;
            if (gioHang == null) gioHang = new List<SanPhamTrongGioHang>();

            return PartialView("_MiniCartPartial", gioHang);
        }

        // Action Thêm giỏ hàng trả về JSON (để dùng Ajax)
        [HttpPost]
        public ActionResult ThemGioHangAjax(int id)
        {
            // 1. Lấy giỏ hàng từ Session
            List<SanPhamTrongGioHang> gioHang = Session["GioHang"] as List<SanPhamTrongGioHang>;

            // 2. [QUAN TRỌNG] Kiểm tra NULL và khởi tạo nếu chưa có
            if (gioHang == null)
            {
                gioHang = new List<SanPhamTrongGioHang>();
                Session["GioHang"] = gioHang; // Lưu danh sách mới vào Session ngay
            }

            // 3. Logic thêm sản phẩm (Bạn đang thiếu đoạn này trong ảnh chụp lỗi)
            var sanPham = gioHang.FirstOrDefault(s => s.IdThuoc == id);
            if (sanPham != null)
            {
                sanPham.SoLuong++;
            }
            else
            {
                // Truy vấn DB để lấy thông tin thuốc
                var thuoc = db.THUOCs.Find(id);
                if (thuoc != null)
                {
                    var newItem = new SanPhamTrongGioHang
                    {
                        IdThuoc = thuoc.id_thuoc,
                        TenThuoc = thuoc.ten_thuoc,
                        AnhThuoc = thuoc.anh_thuoc,
                        // Xử lý null cho giá bán để tránh lỗi crash khác
                        DonGia = (int)(thuoc.gia_ban ?? 0),
                        SoLuong = 1,
                        DonViTinh = thuoc.don_vi_tinh
                    };
                    gioHang.Add(newItem);
                }
            }

            // 4. Tính tổng (Lúc này gioHang chắc chắn không null nên .Sum() sẽ an toàn)
            return Json(new { success = true, totalQty = gioHang.Sum(x => x.SoLuong) });
        }

        // Action Xóa item từ Mini Cart
        [HttpPost]
        public ActionResult XoaKhoiMiniCart(int id)
        {
            List<SanPhamTrongGioHang> gioHang = Session["GioHang"] as List<SanPhamTrongGioHang>;
            var item = gioHang.FirstOrDefault(x => x.IdThuoc == id);
            if (item != null)
            {
                gioHang.Remove(item);
            }
            return Json(new { success = true });
        }
        [HttpPost]
        public ActionResult DatHang(ThanhToanViewModel model)
        {
            // 1. Kiểm tra giỏ hàng
            List<SanPhamTrongGioHang> cart = Session["GioHang"] as List<SanPhamTrongGioHang>;
            if (cart == null || cart.Count == 0) return RedirectToAction("Index", "TrangChu");

            if (ModelState.IsValid)
            {
                try
                {
                    // 2. XỬ LÝ ĐỊA CHỈ GIAO HÀNG
                    string diaChiFinal = "";

                    if (model.hinhThucNhan == "GiaoHang")
                    {
                        var tinh = db.TINH_THANH.Find(model.id_tinh_thanh_ship)?.ten ?? "";
                        var quan = db.QUAN_HUYEN.Find(model.id_quan_huyen_ship)?.ten ?? "";
                        var phuong = db.PHUONG_XA.Find(model.id_phuong_xa_ship)?.ten ?? "";

                        diaChiFinal = $"{model.DiaChiCuThe}, {phuong}, {quan}, {tinh}";
                    }
                    else if (model.hinhThucNhan == "NhanTaiQuay")
                    {
                        var store = db.DIA_DIEM_NHA_THUOC.Find(model.id_dia_diem_nha_thuoc);
                        if (store != null)
                        {
                            diaChiFinal = $"NHẬN TẠI CỬA HÀNG: {store.ten_nha_thuoc} - {store.so_duong_ten_duong}, {store.quan_huyen}, {store.thanh_pho_tinh}";
                        }
                        else
                        {
                            diaChiFinal = "Khách nhận tại cửa hàng (Chưa chọn địa điểm cụ thể)";
                        }
                    }

                    // 3. TẠO HÓA ĐƠN (HOA_DON)
                    HOA_DON hd = new HOA_DON();
                    hd.ngay_ban = DateTime.Now;
                    hd.tong_tien = cart.Sum(x => x.ThanhTien);

                    hd.ten_nguoi_nhan = model.TenNguoiDat;
                    hd.sdt_nguoi_nhan = model.SdtNguoiDat;
                    hd.email_nguoi_mua = model.EmailNguoiDat;
                    hd.dia_chi_giao_hang = diaChiFinal;

                    // Xử lý thanh toán
                    hd.hinh_thuc_thanh_toan = model.PhuongThucThanhToan;


                    // Liên kết tài khoản nếu đã đăng nhập
                    if (Session["User"] != null)
                    {
                        var user = Session["User"] as TAI_KHOAN;
                        hd.id_nguoi_mua = user.id_tai_khoan;
                    }
                    else
                    {
                        hd.id_nguoi_mua = null; // Khách vãng lai
                    }

                    db.HOA_DON.Add(hd);
                    db.SaveChanges(); // Lưu để lấy ID Hóa Đơn

                    // 4. LƯU CHI TIẾT & TRỪ KHO (FEFO - Hết hạn trước xuất trước)
                    foreach (var item in cart)
                    {
                        // Lấy các lô thuốc còn hạn và còn hàng, sắp xếp hạn sử dụng tăng dần
                        var loThuocs = db.LO_THUOC
                            .Where(l => l.id_thuoc == item.IdThuoc && l.so_luong_ton > 0 && l.han_su_dung > DateTime.Now)
                            .OrderBy(l => l.han_su_dung)
                            .ToList();

                        int soLuongCanMua = item.SoLuong;

                        foreach (var lo in loThuocs)
                        {
                            if (soLuongCanMua <= 0) break;

                            int soLuongLay = 0;
                            if (lo.so_luong_ton >= soLuongCanMua)
                            {
                                // Lô này đủ hàng
                                soLuongLay = soLuongCanMua;
                                lo.so_luong_ton -= soLuongCanMua;
                                soLuongCanMua = 0;
                            }
                            else
                            {
                                // Lô này không đủ, lấy hết lô này rồi sang lô khác
                                soLuongLay = (int)lo.so_luong_ton;
                                soLuongCanMua -= (int)lo.so_luong_ton;
                                lo.so_luong_ton = 0;
                            }

                            // Tạo chi tiết hóa đơn
                            CHI_TIET_HOA_DON cthd = new CHI_TIET_HOA_DON();
                            cthd.id_hoa_don = hd.id_hoa_don;
                            cthd.id_lo_thuoc = lo.id_lo_thuoc;
                            cthd.so_luong = soLuongLay;
                            cthd.don_gia = item.DonGia;
                            cthd.thanh_tien = soLuongLay * item.DonGia;

                            db.CHI_TIET_HOA_DON.Add(cthd);
                        }
                    }

                    db.SaveChanges(); // Lưu tất cả chi tiết và cập nhật kho
                    try
                    {
                        // Kiểm tra nếu có email thì mới gửi
                        if (!string.IsNullOrEmpty(model.EmailNguoiDat))
                        {
                            GuiEmailHoaDon(hd, cart);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Ghi log lỗi gửi mail nếu cần, nhưng KHÔNG được throw lỗi
                        // để tránh việc đơn hàng đã lưu thành công nhưng khách lại thấy báo lỗi trang web
                        System.Diagnostics.Debug.WriteLine("Lỗi gửi mail: " + ex.Message);
                    }

                    // 5. Xóa giỏ hàng và chuyển hướng
                    Session["GioHang"] = null;
                    return RedirectToAction("DatHangThanhCong");

                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Có lỗi xảy ra: " + ex.Message;
                    // Trả về view giỏ hàng với dữ liệu cũ để khách nhập lại
                    return View("Index", cart);
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult DatHangThanhCong()
        {
            return View();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        [HttpGet]
        public async Task<ActionResult> KiemTraThanhToan(string noiDungChuyenKhoan, int soTien)
        {
            string apiToken = "M6SS8QZIE7EIHMYOURVPXJHQOWMYEIXAFOGGAWU7F81Q5T5B0SVBSI21W3FFKDPU";

            string accountNumber = "6513796242";

            try
            {
                using (var client = new HttpClient())
                {
                    string url = $"https://my.sepay.vn/userapi/transactions/list?account_number={accountNumber}&limit=20";
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiToken);

                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        var data = JObject.Parse(jsonString);
                        var transactions = data["transactions"];

                        foreach (var trans in transactions)
                        {
                            string content = trans["transaction_content"].ToString();
                            decimal amount = (decimal)trans["amount_in"];

                            if (content.Contains(noiDungChuyenKhoan) && amount >= soTien)
                            {
                                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        // Hàm hỗ trợ gửi Email
        private void GuiEmailHoaDon(HOA_DON hd, List<SanPhamTrongGioHang> cart)
        {
            var fromEmail = "phamanhnhatminh1809@gmail.com"; 
            var fromPassword = "pqzl sevx yzlp fnyn";

            var toEmail = hd.email_nguoi_mua;
            string subject = $"Xác nhận đơn hàng #{hd.id_hoa_don} - Nhà Thuốc MedForAll";

            // Tạo nội dung Email (HTML)
            StringBuilder body = new StringBuilder();
            body.Append($"<h3>Cảm ơn {hd.ten_nguoi_nhan} đã đặt hàng!</h3>");
            body.Append($"<p>Mã đơn hàng: <b>#{hd.id_hoa_don}</b></p>");
            body.Append($"<p>Ngày đặt: {hd.ngay_ban:dd/MM/yyyy HH:mm}</p>");
            body.Append($"<p>Địa chỉ giao: {hd.dia_chi_giao_hang}</p>");
            body.Append("<hr/>");
            body.Append("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse: collapse; width: 100%;'>");
            body.Append("<thead><tr style='background-color: #f2f2f2;'><th>Tên thuốc</th><th>SL</th><th>Đơn giá</th><th>Thành tiền</th></tr></thead>");
            body.Append("<tbody>");

            foreach (var item in cart)
            {
                body.Append("<tr>");
                body.Append($"<td>{item.TenThuoc}</td>");
                body.Append($"<td style='text-align: center;'>{item.SoLuong}</td>");
                body.Append($"<td style='text-align: right;'>{item.DonGia:N0} đ</td>");
                body.Append($"<td style='text-align: right;'>{item.ThanhTien:N0} đ</td>");
                body.Append("</tr>");
            }

            body.Append("</tbody>");
            body.Append($"<tfoot><tr><td colspan='3' style='text-align: right; font-weight: bold;'>TỔNG TIỀN:</td><td style='text-align: right; font-weight: bold; color: red;'>{hd.tong_tien:N0} đ</td></tr></tfoot>");
            body.Append("</table>");
            body.Append("<p>Cảm ơn quý khách đã tin tưởng sử dụng dịch vụ.</p>");

            // Cấu hình gửi mail (Google SMTP)
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail, fromPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body.ToString(),
                IsBodyHtml = true
            })
            {
                smtp.Send(message);
            }
        }
    }
}
