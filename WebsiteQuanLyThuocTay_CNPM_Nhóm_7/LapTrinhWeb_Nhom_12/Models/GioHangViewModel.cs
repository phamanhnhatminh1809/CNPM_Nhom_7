using System.Collections.Generic;

namespace LapTrinhWeb_Nhom_12.Models
{
    public class GioHangViewModel
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string ProductName { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
        public List<string> Units { get; set; } = new List<string> { "Hộp", "Vỉ", "Viên" }; // Danh sách đơn vị
        public string SelectedUnit { get; set; }
    }
}