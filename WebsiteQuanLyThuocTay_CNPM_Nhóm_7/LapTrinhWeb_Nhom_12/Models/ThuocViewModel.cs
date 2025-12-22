using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LapTrinhWeb_Nhom_12.Models
{
    public class ThuocAdminViewModel
    {
        public int IdThuoc { get; set; }
        public int IdLoaiThuoc { get; set; }
        public string TenThuoc { get; set; }
        public string AnhThuoc { get; set; }
        public string TenDanhMuc { get; set; }
        public string TenLoaiThuoc { get; set; }
        public string DonViTinh { get; set; }
        public decimal GiaBan { get; set; }
        public bool ChoPhepBanOnline { get; set; }

        // Thông tin tổng hợp từ bảng LO_THUOC
        public int TongTonKho { get; set; }
        public int SoLoSapHetHan { get; set; } // Cảnh báo số lượng lô sắp hết date
    }

    // ViewModel cho chi tiết lô thuốc (khi bấm vào xem chi tiết)
    public class LoThuocViewModel
    {
        public string SoLo { get; set; }
        public DateTime? NgaySanXuat { get; set; }
        public DateTime? HanSuDung { get; set; }
        public int SoLuongTon { get; set; }
        public int GiaNhap { get; set; }
        public string TrangThai { get; set; }
    }
}