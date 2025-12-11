using LapTrinhWeb_Nhom_12.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace LapTrinhWeb_Nhom_12.Controllers
{
    public class DashboardController : Controller
    {
        private NHA_THUOCEntities db = new NHA_THUOCEntities();

        public ActionResult Index()
        {
            DateTime today = DateTime.Now.Date;
            DateTime startOfMonth = new DateTime(today.Year, today.Month, 1);

            var model = new DashboardViewModel();

            // 1. THỐNG KÊ CƠ BẢN (CARDS)
            // Doanh thu hôm nay
            model.DoanhThuHomNay = db.HOA_DON
                .Where(h => DbFunctions.TruncateTime(h.ngay_ban) == today)
                .Sum(h => (decimal?)h.tong_tien) ?? 0;

            // Số đơn hôm nay
            model.DonHangHomNay = db.HOA_DON
                .Count(h => DbFunctions.TruncateTime(h.ngay_ban) == today);

            // Thuốc sắp hết hạn (< 90 ngày) & Còn tồn
            DateTime warningDate = DateTime.Now.AddDays(90);
            model.ThuocSapHetHan = db.LO_THUOC
                .Count(l => l.han_su_dung > DateTime.Now && l.han_su_dung <= warningDate && l.so_luong_ton > 0);

            // Thuốc hết hàng (Tính theo loại thuốc có tổng tồn = 0)
            // Logic: Group by thuốc, sum tồn kho, đếm số thuốc có sum = 0
            var tonKhoList = db.LO_THUOC.GroupBy(l => l.id_thuoc)
                .Select(g => new { Id = g.Key, Ton = g.Sum(x => x.so_luong_ton) })
                .ToList();
            model.ThuocHetHang = tonKhoList.Count(x => x.Ton == 0);


            // 2. DỮ LIỆU BIỂU ĐỒ (7 ngày gần nhất)
            model.ChartLabels = new List<string>();
            model.ChartValues = new List<decimal>();

            for (int i = 6; i >= 0; i--)
            {
                DateTime date = today.AddDays(-i);
                string label = date.ToString("dd/MM");

                decimal value = db.HOA_DON
                    .Where(h => DbFunctions.TruncateTime(h.ngay_ban) == date)
                    .Sum(h => (decimal?)h.tong_tien) ?? 0;

                model.ChartLabels.Add(label);
                model.ChartValues.Add(value);
            }


            // 3. TOP 5 SẢN PHẨM BÁN CHẠY
            model.TopBanChay = db.CHI_TIET_HOA_DON
                .GroupBy(ct => ct.LO_THUOC.THUOC)
                .Select(g => new TopProduct
                {
                    TenThuoc = g.Key.ten_thuoc,
                    AnhThuoc = g.Key.anh_thuoc,
                    SoLuongBan = g.Sum(x => x.so_luong),
                    DoanhThu = g.Sum(x => (decimal?)x.thanh_tien) ?? 0
                })
                .OrderByDescending(x => x.SoLuongBan)
                .Take(5)
                .ToList();


            // 4. ĐƠN HÀNG MỚI NHẤT
            model.DonHangMoi = db.HOA_DON
                .OrderByDescending(h => h.ngay_ban)
                .Take(6)
                .Select(h => new RecentOrder
                {
                    IdHoaDon = h.id_hoa_don,
                    TenKhach = h.KHACH_HANG != null ? h.KHACH_HANG.ten_khach_hang : "Khách lẻ",
                    TongTien = (decimal)(h.tong_tien ?? 0),
                    NgayBan = h.ngay_ban ?? DateTime.Now
                })
                .ToList();

            return View(model);
        }
    }
}