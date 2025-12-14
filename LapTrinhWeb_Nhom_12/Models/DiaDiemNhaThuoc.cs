using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LapTrinhWeb_Nhom_12.Models
{
    public class DiaDiemNhaThuoc
    {
        public int id_dia_diem { get; set; }
        public string ten_nha_thuoc { get; set; }
        public string thanh_pho_tinh { get; set; } 
        public string phuong_xa { get; set; }      
        public string so_duong_ten_duong { get; set; }
    }
}