using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LapTrinhWeb_Nhom_12.Models
{
    public class LichSuHoaDonViewModel
    {
        public int IdHoaDon { get; set; }
        public DateTime NgayDat { get; set; }
        public int TongTien { get; set; }
        public string DiaChiNhan { get; set; }

        // Dùng cho trang chi tiết
        public List<LichSuHoaDonChiTiet> ChiTiet { get; set; }
    }
    public class LichSuHoaDonChiTiet
    {
        public string TenThuoc { get; set; }
        public string AnhThuoc { get; set; }
        public int SoLuong { get; set; }
        public int DonGia { get; set; }
        public int ThanhTien { get; set; }
    }
}