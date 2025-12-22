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

            var model = new DashboardViewModel();

            // 1. THỐNG KÊ CƠ BẢN
            model.DoanhThuHomNay = db.HOA_DON
                .Where(h => DbFunctions.TruncateTime(h.ngay_ban) == today)
                .Sum(h => (decimal?)h.tong_tien) ?? 0;

            model.DonHangHomNay = db.HOA_DON
                .Count(h => DbFunctions.TruncateTime(h.ngay_ban) == today);

            // Thuốc sắp hết hạn
            DateTime warningDate = DateTime.Now.AddDays(90);
            model.ThuocSapHetHan = db.LO_THUOC
                .Count(l => l.han_su_dung > DateTime.Now && l.han_su_dung <= warningDate && l.so_luong_ton > 0);

            // Thuốc hết hàng (Logic: Gom nhóm theo thuốc -> Tính tổng tồn -> Đếm nhóm có tồn = 0)
            var tonKhoList = db.LO_THUOC.GroupBy(l => l.id_thuoc)
                .Select(g => new { Id = g.Key, Ton = g.Sum(x => x.so_luong_ton) })
                .ToList();
            model.ThuocHetHang = tonKhoList.Count(x => x.Ton == 0);


            // 2. BIỂU ĐỒ DOANH THU (7 NGÀY)
            model.ChartLabels = new List<string>();
            model.ChartValues = new List<decimal>();

            for (int i = 6; i >= 0; i--)
            {
                DateTime date = today.AddDays(-i);
                model.ChartLabels.Add(date.ToString("dd/MM"));

                decimal value = db.HOA_DON
                    .Where(h => DbFunctions.TruncateTime(h.ngay_ban) == date)
                    .Sum(h => (decimal?)h.tong_tien) ?? 0;

                model.ChartValues.Add(value);
            }


            // 3. TOP BÁN CHẠY
            // Lưu ý: Đảm bảo bảng LO_THUOC vẫn liên kết với THUOC
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


            // 4. ĐƠN HÀNG MỚI NHẤT (PHẦN QUAN TRỌNG ĐÃ SỬA)
            model.DonHangMoi = db.HOA_DON
                .OrderByDescending(h => h.ngay_ban)
                .Take(6)
                .Select(h => new RecentOrder
                {
                    IdHoaDon = h.id_hoa_don,

                    TenNguoiNhan = h.ten_nguoi_nhan,

                    TenNguoiBan = h.TAI_KHOAN != null ? h.TAI_KHOAN.ho_ten : "Đơn mới/Online",

                    TongTien = (decimal)(h.tong_tien ?? 0),
                    NgayBan = h.ngay_ban ?? DateTime.Now,

                    // Nếu chưa có cột trang_thai trong DB thì bạn có thể hardcode hoặc logic tạm
                    TrangThai = "Hoàn thành"
                })
                .ToList();

            return View(model);
        }
    }
}