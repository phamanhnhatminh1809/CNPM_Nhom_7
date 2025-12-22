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
        public int ThuocSapHetHan { get; set; }
        public int ThuocHetHang { get; set; }  
        public List<string> ChartLabels { get; set; } 
        public List<decimal> ChartValues { get; set; } 

        public List<TopProduct> TopBanChay { get; set; }

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
        public string TenNguoiNhan { get; set; }
        public string TenNguoiBan { get; set; }
        public decimal TongTien { get; set; }
        public DateTime NgayBan { get; set; }
        public string TrangThai { get; set; } // Ví dụ: "Hoàn thành"
    }
}