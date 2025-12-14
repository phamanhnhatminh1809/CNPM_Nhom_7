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
                // Giữ nguyên id_loai_thuoc như code cũ của bạn
                query = query.Where(t => t.id_loai_thuoc == id);
            }

            var allBrands = query.Select(t => t.hang).Distinct().ToList();

            // --- PHẦN ĐƯỢC SỬA: THÊM LOGIC LỌC GIÁ ---
            if (!string.IsNullOrEmpty(giaBanRange))
            {
                switch (giaBanRange)
                {
                    case "duoi-100":
                        query = query.Where(t => t.gia_ban < 100000);
                        break;
                    case "100-300":
                        query = query.Where(t => t.gia_ban >= 100000 && t.gia_ban <= 300000);
                        break;
                    case "300-500":
                        query = query.Where(t => t.gia_ban >= 300000 && t.gia_ban <= 500000);
                        break;
                    case "tren-500":
                        query = query.Where(t => t.gia_ban > 500000);
                        break;
                }
            }
            // ------------------------------------------

            if (!string.IsNullOrEmpty(hang))
            {
                query = query.Where(t => t.hang == hang);
            }

            var listThuoc = query.Select(t => new TheThuocViewModel
            {
                IdThuoc = t.id_thuoc,
                TenThuoc = t.ten_thuoc,
                AnhThuoc = t.anh_thuoc,
                GiaBan = t.gia_ban ?? 0,
                TenDanhMuc = t.DANH_MUC_THUOC != null ? t.DANH_MUC_THUOC.ten_danh_muc : "",
                DonViTinh = t.don_vi_tinh,
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