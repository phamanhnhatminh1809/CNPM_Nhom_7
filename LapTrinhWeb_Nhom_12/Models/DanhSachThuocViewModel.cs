using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LapTrinhWeb_Nhom_12.Models
{
    public class DanhSachThuocViewModel
    {
        // Danh sách thuốc đã lọc để hiển thị
        public List<TheThuocViewModel> DanhSachThuoc { get; set; }

        public List<string> DanhSachHang { get; set; }
        public List<string> DanhSachDanhMuc { get; set; }

        public string GiaBanFilter { get; set; }
        public string HangFilter { get; set; }
        public int? LoaiThuocId { get; set; } // ID loại thuốc hiện tại
    }
}