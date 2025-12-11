using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LapTrinhWeb_Nhom_12.Models
{
    public class TheThuocViewModel
    {
        public int IdThuoc { get; set; }
        public string TenThuoc { get; set; }
        public string AnhThuoc { get; set; }
        public decimal GiaBan { get; set; }
        public string TenDanhMuc { get; set; }
        public int SoLuongTon { get; set; }
        public string DonViTinh { get; set; }
        public string Hang { get; set; }
    }
}