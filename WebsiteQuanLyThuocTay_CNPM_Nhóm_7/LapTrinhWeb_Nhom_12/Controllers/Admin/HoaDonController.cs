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

        public ActionResult Index(string searchString, string fromDate, string toDate)
        {
            var query = db.HOA_DON.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(h => h.ten_nguoi_nhan.Contains(searchString) ||
                                         h.sdt_nguoi_nhan.Contains(searchString));
            }

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

            var data = query.OrderByDescending(h => h.ngay_ban)
                            .Select(h => new HoaDonViewModel
                            {
                                IdHoaDon = h.id_hoa_don,
                                NgayBan = h.ngay_ban ?? DateTime.Now,

                                TenKhachHang = h.ten_nguoi_nhan,

                                TenNhanVien = h.TAI_KHOAN != null ? h.TAI_KHOAN.ho_ten : "Đơn Online",

                                TongTien = h.tong_tien ?? 0
                            }).ToList();

            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.SearchString = searchString;

            return View(data);
        }

        public ActionResult GetChiTiet(int id)
        {
            var hd = db.HOA_DON.Find(id);
            if (hd == null) return HttpNotFound();

            var chiTietItems = db.CHI_TIET_HOA_DON
                .Where(ct => ct.id_hoa_don == id)
                .Select(ct => new ChiTietItem
                {
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
                TenKhachHang = hd.ten_nguoi_nhan,
                SoDienThoai = hd.sdt_nguoi_nhan,

                TenNhanVien = hd.TAI_KHOAN != null ? hd.TAI_KHOAN.ho_ten : "Đơn Online",

                TongTien = hd.tong_tien ?? 0,
                DanhSachThuoc = chiTietItems
            };

            return PartialView("_ChiTietPartial", model);
        }
        public JsonResult GetInvoiceDetails(int id)
        {
            var hoadon = db.HOA_DON.Find(id); // Lấy hóa đơn từ DB

            // Tạo object data đơn giản để trả về
            var result = new
            {
                idHoaDon = hoadon.id_hoa_don,
                ngayBan = hoadon.ngay_ban?.ToString("dd/MM/yyyy") ?? "",
                tenKhachHang = hoadon.TAI_KHOAN?.ten_dang_nhap ?? "Khách lẻ",
                tongTien = hoadon.tong_tien,
                chiTietHoaDon = hoadon.CHI_TIET_HOA_DON.Select(ct => new {
                    tenThuoc = ct.LO_THUOC.THUOC.ten_thuoc,
                    soLuong = ct.so_luong,
                    donGia = ct.don_gia,
                    thanhTien = ct.so_luong * ct.don_gia
                }).ToList()
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}