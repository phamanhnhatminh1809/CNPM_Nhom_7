using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LapTrinhWeb_Nhom_12.Models
{
    public class ThuocAPI
    {
        public string BrandName { get; set; }        // Tên thương hiệu
        public string GenericName { get; set; }      // Tên hoạt chất
        public string Purpose { get; set; }          // Công dụng
        public string Manufacturer { get; set; }     // Nhà sản xuất
        public string ActiveIngredient { get; set; } // Thành phần hoạt tính
        public string Warnings { get; set; }         // Cảnh báo

        public string Dosage { get; set; }        // Liều dùng & Cách dùng
        public string StopUse { get; set; }       // Khi nào cần ngưng sử dụng
        public string Storage { get; set; }       // Hướng dẫn bảo quản
        public string InactiveIngredient { get; set; } // Tá dược (thành phần phụ)
    }

    // --- Các class bên dưới dùng để mapping đúng cấu trúc JSON của OpenFDA ---
    public class OpenFdaResponse
    {
        public List<OpenFdaResult> results { get; set; }
    }

    public class OpenFdaResult
    {
        public OpenFdaDetails openfda { get; set; }
        public List<string> purpose { get; set; }
        public List<string> active_ingredient { get; set; }
        public List<string> warnings { get; set; }

        public List<string> dosage_and_administration { get; set; } // JSON key: dosage_and_administration
        public List<string> stop_use { get; set; }                  // JSON key: stop_use
        public List<string> storage_and_handling { get; set; }      // JSON key: storage_and_handling
        public List<string> inactive_ingredient { get; set; }
    }

    public class OpenFdaDetails
    {
        public List<string> brand_name { get; set; }
        public List<string> generic_name { get; set; }
        public List<string> manufacturer_name { get; set; }
    }
}
