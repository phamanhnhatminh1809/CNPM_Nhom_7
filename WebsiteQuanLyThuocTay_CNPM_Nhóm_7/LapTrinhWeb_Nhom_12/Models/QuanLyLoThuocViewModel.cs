using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LapTrinhWeb_Nhom_12.Models
{
    public class QuanLyLoThuocViewModel
    {
        public int IdLoThuoc { get; set; }
        public string SoLo { get; set; }

        public int IdThuoc { get; set; }
        public string TenThuoc { get; set; }
        public string AnhThuoc { get; set; }
        public string DonViTinh { get; set; }

        public DateTime NgaySanXuat { get; set; }
        public DateTime HanSuDung { get; set; }
        public int SoLuongNhap { get; set; }
        public int SoLuongTon { get; set; }
        public int GiaNhap { get; set; }

        public bool DaHetHan => HanSuDung < DateTime.Now;
        public bool SapHetHan => !DaHetHan && (HanSuDung - DateTime.Now).TotalDays <= 90;

        public bool CoTheXoa { get; set; }
        public bool TieuHuy { get; set; }
    }
}