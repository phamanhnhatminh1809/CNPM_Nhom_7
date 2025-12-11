using LapTrinhWeb_Nhom_12.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LapTrinhWeb_Nhom_12.Controllers
{
    public class LoThuocController : Controller
    {
        private NHA_THUOCEntities db = new NHA_THUOCEntities();

        // GET: Admin/LoThuoc
        // GET: Admin/LoThuoc
        // Thêm tham số sortOrder
        public ActionResult Index(string search, string status, string sortOrder)
        {
            var query = db.LO_THUOC.AsQueryable();

            // --- 1. GIỮ NGUYÊN BỘ LỌC CŨ ---
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(l => l.THUOC.ten_thuoc.Contains(search) || l.so_lo.Contains(search));
            }

            DateTime today = DateTime.Now;
            if (status == "expired")
            {
                query = query.Where(l => l.han_su_dung < today);
            }
            else if (status == "warning")
            {
                DateTime warningDate = today.AddDays(90);
                query = query.Where(l => l.han_su_dung >= today && l.han_su_dung <= warningDate);
            }

            ViewBag.SoLoSortParm = String.IsNullOrEmpty(sortOrder) ? "solo_desc" : "";

            // Lưu lại trạng thái sắp xếp hiện tại để View hiển thị icon mũi tên
            ViewBag.CurrentSort = sortOrder;

            // Biến tạm để lưu kết quả đã sort
            IOrderedQueryable<LO_THUOC> sortedQuery;

            switch (sortOrder)
            {
                case "solo_desc": // Z -> A
                    sortedQuery = query.OrderByDescending(l => l.so_lo);
                    break;

                case "": // A -> Z (Mặc định khi bấm sort lần 1)
                case "solo_asc":
                    sortedQuery = query.OrderBy(l => l.so_lo);
                    break;

                default: 
                    sortedQuery = query.OrderByDescending(l => l.so_luong_ton > 0)
                                       .ThenBy(l => l.han_su_dung);
                    break;
            }

            if (sortOrder == "solo_asc")
            {
                query = query.OrderBy(l => l.so_lo);
            }
            else if (sortOrder == "solo_desc")
            {
                query = query.OrderByDescending(l => l.so_lo);
            }
            else
            {
                query = query.OrderByDescending(l => l.so_luong_ton > 0).ThenBy(l => l.han_su_dung);
            }


            var data = query.Select(l => new
            {
                Lo = l,
                Thuoc = l.THUOC,
                DaBan = db.CHI_TIET_HOA_DON.Any(ct => ct.id_lo_thuoc == l.id_lo_thuoc)
            }).ToList().Select(x => new QuanLyLoThuocViewModel
            {
                IdLoThuoc = x.Lo.id_lo_thuoc,
                SoLo = x.Lo.so_lo,
                IdThuoc = x.Thuoc.id_thuoc,
                TenThuoc = x.Thuoc.ten_thuoc,
                AnhThuoc = x.Thuoc.anh_thuoc,
                DonViTinh = x.Thuoc.don_vi_tinh,
                NgaySanXuat = x.Lo.ngay_san_xuat ?? DateTime.Now,
                HanSuDung = x.Lo.han_su_dung ?? DateTime.Now,
                SoLuongNhap = x.Lo.so_luong_nhap ?? 0,
                SoLuongTon = x.Lo.so_luong_ton ?? 0,
                GiaNhap = x.Lo.gia_nhap ?? 0,
                CoTheXoa = !x.DaBan
            }).ToList();

            return View(data);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                var lo = db.LO_THUOC.Find(id);
                if (lo == null) return Json(new { success = false, message = "Không tìm thấy lô." });

                bool daBan = db.CHI_TIET_HOA_DON.Any(ct => ct.id_lo_thuoc == id);
                if (daBan)
                {
                    return Json(new { success = false, message = "KHÔNG THỂ XÓA! Lô thuốc này đã phát sinh giao dịch bán hàng. Hãy điều chỉnh tồn kho về 0 thay vì xóa." });
                }

                db.LO_THUOC.Remove(lo);
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}