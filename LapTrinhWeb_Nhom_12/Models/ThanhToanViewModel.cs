using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LapTrinhWeb_Nhom_12.Models
{
    public class ThanhToanViewModel
    {
        [Required]
        public string TenNguoiDat { get; set; }
        [Required]
        public string SdtNguoiDat { get; set; }
        public string EmailNguoiDat { get; set; }

        // 2. Hình thức nhận hàng (GiaoHang / NhanTaiQuay)
        public string hinhThucNhan { get; set; }

        // -- Nếu Giao Hàng --
        public int? id_tinh_thanh_ship { get; set; }
        public int? id_quan_huyen_ship { get; set; }
        public int? id_phuong_xa_ship { get; set; }
        public string DiaChiCuThe { get; set; }

        // -- Nếu Nhận Tại Quầy --
        public int? id_dia_diem_nha_thuoc { get; set; }

        // 3. Thanh toán (COD / BANK)
        public string PhuongThucThanhToan { get; set; }
    }
}