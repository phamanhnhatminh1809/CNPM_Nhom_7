using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace LapTrinhWeb_Nhom_12.Models
{
    public class TinhThanh
    {
        public int id_tinh_thanh { get; set; }
        public string ten { get; set; }
    }
    public class QuanHuyen
    {
        public int id_quan_huyen{ get; set; }
        public string ten { get; set; }
        public int id_tinh_thanh { get; set; }

    }
    public class PhuongXa
    {
        public int id_phuong_xa { get; set; }
        public string ten { get; set; }
        public int id_quan_huyen { get; set; }
    }
}