using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LapTrinhWeb_Nhom_12.Models
{
    public class ChiTietThuocViewModel
    {
        public int IdThuoc { get; set; }
        public string TenThuoc { get; set; }
        public decimal GiaBan { get; set; }
        public string DonViTinh { get; set; }
        public string TenDanhMuc { get; set; }
        public string DangBaoChe { get; set; }
        public string HoatChat { get; set; }
        public string Hang { get; set; } // Hãng sản xuất
        public int HanSuDungThang { get; set; } // Hạn sử dụng (tháng)
        public string AnhThuoc { get; set; }
        public string CongDung { get; set; }
        public string LieuDung { get; set; }
        public string ChongChiDinh { get; set; }
    }
}