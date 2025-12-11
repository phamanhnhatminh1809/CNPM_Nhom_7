using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LapTrinhWeb_Nhom_12.Models
{
    public class CartItem
    {
        public int IdThuoc { get; set; }
        public string TenThuoc { get; set; }
        public string AnhThuoc { get; set; }
        public int DonGia { get; set; }
        public int SoLuong { get; set; }

        // Thuộc tính tính toán thành tiền = đơn giá * số lượng
        public int ThanhTien
        {
            get { return SoLuong * DonGia; }
        }
    }
}