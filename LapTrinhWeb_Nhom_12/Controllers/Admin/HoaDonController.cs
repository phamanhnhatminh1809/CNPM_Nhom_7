using LapTrinhWeb_Nhom_12.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LapTrinhWeb_Nhom_12.Controllers
{
    public class HoaDonController : Controller
    {
        private NHA_THUOCEntities db = new NHA_THUOCEntities();

        // GET: Admin/HoaDon
        public ActionResult Index(string searchString, string fromDate, string toDate)
        {
            var query = db.HOA_DON.AsQueryable();

            // 1. Lọc theo tên khách hàng
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(h => h.KHACH_HANG.ten_khach_hang.Contains(searchString) ||
                                         h.KHACH_HANG.so_dien_thoai.Contains(searchString));
            }

            // 2. Lọc theo ngày (Mặc định lấy tháng hiện tại nếu không chọn)
            if (!string.IsNullOrEmpty(fromDate))
            {
                DateTime dtFrom = DateTime.Parse(fromDate);
                query = query.Where(h => h.ngay_ban >= dtFrom);
            }

            if (!string.IsNullOrEmpty(toDate))
            {
                DateTime dtTo = DateTime.Parse(toDate).AddDays(1);
                query = query.Where(h => h.ngay_ban < dtTo);
            }

            // 3. Sắp xếp mới nhất lên đầu và Map sang ViewModel
            var data = query.OrderByDescending(h => h.ngay_ban)
                            .Select(h => new HoaDonViewModel
                            {
                                IdHoaDon = h.id_hoa_don,
                                NgayBan = h.ngay_ban ?? DateTime.Now,
                                TenKhachHang = h.KHACH_HANG != null ? h.KHACH_HANG.ten_khach_hang : "Khách lẻ",
                                TenNhanVien = h.TAI_KHOAN.ten_dang_nhap, // Hoặc tên đầy đủ nếu có
                                TongTien = h.tong_tien ?? 0
                            }).ToList();

            // Giữ lại giá trị lọc để hiển thị trên View
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.SearchString = searchString;

            return View(data);
        }

        // GET: Admin/HoaDon/GetChiTiet/5
        public ActionResult GetChiTiet(int id)
        {
            var hd = db.HOA_DON.Find(id);
            if (hd == null) return HttpNotFound();

            // Lấy danh sách chi tiết
            var chiTietItems = db.CHI_TIET_HOA_DON
                .Where(ct => ct.id_hoa_don == id)
                .Select(ct => new ChiTietItem
                {
                    // Truy vấn ngược: ChiTiet -> LoThuoc -> Thuoc -> TenThuoc
                    TenThuoc = ct.LO_THUOC.THUOC.ten_thuoc,
                    SoLo = ct.LO_THUOC.so_lo,
                    DonViTinh = ct.LO_THUOC.THUOC.don_vi_tinh,
                    SoLuong = ct.so_luong,
                    DonGia = ct.don_gia ?? 0,
                    ThanhTien = ct.thanh_tien ?? 0
                }).ToList();

            var model = new ChiTietHoaDonViewModel
            {
                IdHoaDon = hd.id_hoa_don,
                NgayBan = hd.ngay_ban ?? DateTime.Now,
                TenKhachHang = hd.KHACH_HANG?.ten_khach_hang ?? "Khách lẻ",
                SoDienThoai = hd.KHACH_HANG?.so_dien_thoai ?? "-",
                TenNhanVien = hd.TAI_KHOAN.ten_dang_nhap,
                TongTien = hd.tong_tien ?? 0,
                DanhSachThuoc = chiTietItems
            };

            return PartialView("_ChiTietPartial", model);
        }
    }
}