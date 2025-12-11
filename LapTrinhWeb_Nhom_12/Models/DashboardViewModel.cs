using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LapTrinhWeb_Nhom_12.Models
{
    public class DashboardViewModel
    {
        public decimal DoanhThuHomNay { get; set; }
        public int DonHangHomNay { get; set; }
        public int ThuocSapHetHan { get; set; } // Còn < 90 ngày
        public int ThuocHetHang { get; set; }   // Tồn kho = 0

        // 2. Biểu đồ doanh thu (7 ngày gần nhất)
        public List<string> ChartLabels { get; set; } // ["01/12", "02/12"...]
        public List<decimal> ChartValues { get; set; } // [1tr, 2tr, 500k...]

        // 3. Top bán chạy
        public List<TopProduct> TopBanChay { get; set; }

        // 4. Đơn hàng mới nhất
        public List<RecentOrder> DonHangMoi { get; set; }
    }
    public class TopProduct
    {
        public string TenThuoc { get; set; }
        public string AnhThuoc { get; set; }
        public int SoLuongBan { get; set; }
        public decimal DoanhThu { get; set; }
    }

    public class RecentOrder
    {
        public int IdHoaDon { get; set; }
        public string TenKhach { get; set; }
        public decimal TongTien { get; set; }
        public DateTime NgayBan { get; set; }
        public string TrangThai { get; set; } // Ví dụ: "Hoàn thành"
    }
}