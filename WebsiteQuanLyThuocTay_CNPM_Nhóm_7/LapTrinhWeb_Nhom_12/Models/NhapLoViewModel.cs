using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LapTrinhWeb_Nhom_12.Models
{
    public class NhapLoViewModel
    {
        public int IdThuoc { get; set; }
        public string TenThuoc { get; set; }
        public string DonViTinh { get; set; }
        public string AnhThuoc { get; set; }
        public decimal GiaBanHienTai { get; set; }
        public int TongTonKho { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lô")]
        [Display(Name = "Số lô sản xuất")]
        public string SoLo { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ngày sản xuất")]
        [DataType(DataType.Date)]
        public DateTime NgaySanXuat { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng nhập hạn sử dụng")]
        [DataType(DataType.Date)]
        public DateTime HanSuDung { get; set; } = DateTime.Now.AddYears(2);

        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SoLuongNhap { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá nhập")]
        [Range(0, int.MaxValue, ErrorMessage = "Giá nhập không hợp lệ")]
        public int GiaNhap { get; set; }
    }
}