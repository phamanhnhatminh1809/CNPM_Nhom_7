using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LapTrinhWeb_Nhom_12.Models
{
    public class HoaDonViewModel
    {
        public int IdHoaDon { get; set; }
        public DateTime NgayBan { get; set; }
        public string TenKhachHang { get; set; }
        public string TenNhanVien { get; set; } // Lấy từ bảng TAI_KHOAN
        public int TongTien { get; set; }
    }
    public class ChiTietHoaDonViewModel
    {
        // Thông tin chung
        public int IdHoaDon { get; set; }
        public DateTime NgayBan { get; set; }
        public string TenKhachHang { get; set; }
        public string SoDienThoai { get; set; }
        public string TenNhanVien { get; set; }
        public int TongTien { get; set; }

        // Danh sách thuốc đã mua
        public List<ChiTietItem> DanhSachThuoc { get; set; }
    }

    public class ChiTietItem
    {
        public string TenThuoc { get; set; }
        public string SoLo { get; set; }
        public string DonViTinh { get; set; }
        public int SoLuong { get; set; }
        public int DonGia { get; set; }
        public int ThanhTien { get; set; }
    }
}