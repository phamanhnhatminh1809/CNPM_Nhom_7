using LapTrinhWeb_Nhom_12.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LapTrinhWeb_Nhom_12.Controllers
{
    public class DanhSachController : Controller
    {
        private NHA_THUOCEntities NhaThuocDatabase = new NHA_THUOCEntities();

        public ActionResult DanhSachThuoc(int id, string giaBanRange, string hang)
        {
            var query = NhaThuocDatabase.THUOCs.AsQueryable();

            if (id > -1)
            {
                query = query.Where(t => t.id_loai_thuoc == id);
            }

            var allBrands = query.Select(t => t.hang).Distinct().ToList();

            if (!string.IsNullOrEmpty(giaBanRange))
            {
            }

            if (!string.IsNullOrEmpty(hang))
            {
                query = query.Where(t => t.hang == hang);
            }

            var listThuoc = query.Select(t => new TheThuocViewModel
            {
                IdThuoc = t.id_thuoc,
                TenThuoc = t.ten_thuoc,

                // Gán dữ liệu ảnh (Nguyên nhân mất ảnh)
                AnhThuoc = t.anh_thuoc,

                // Gán dữ liệu giá (Nguyên nhân giá = 0)
                // Dùng ?? 0 để xử lý nếu trong DB giá là NULL
                GiaBan = t.gia_ban ?? 0,

                // Các thông tin khác
                TenDanhMuc = t.DANH_MUC_THUOC != null ? t.DANH_MUC_THUOC.ten_danh_muc : "",
                DonViTinh = t.don_vi_tinh,

                // Tính tổng tồn kho từ các lô
                SoLuongTon = t.LO_THUOC.Sum(l => l.so_luong_ton) ?? 0,

                Hang = t.hang
            }).ToList();

            var allCategories = NhaThuocDatabase.DANH_MUC_THUOC.Select(d => d.ten_danh_muc).ToList();

            var model = new DanhSachThuocViewModel
            {
                DanhSachThuoc = listThuoc,
                DanhSachHang = allBrands, 
                DanhSachDanhMuc = allCategories,
                LoaiThuocId = id,
                GiaBanFilter = giaBanRange,
                HangFilter = hang
            };

            return View(model);
        }
    }
}