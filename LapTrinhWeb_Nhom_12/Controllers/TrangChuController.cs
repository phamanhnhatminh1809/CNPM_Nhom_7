using LapTrinhWeb_Nhom_12.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LapTrinhWeb_Nhom_12.Controllers
{
    public class TrangChuController : Controller
    {
        private NHA_THUOCEntities1 NhaThuocDatabase = new NHA_THUOCEntities1();

        public ActionResult TrangChu()
        {
            var danhSachThuoc = NhaThuocDatabase.THUOCs.Select(t => new TrangChuViewModel
            {
                IdThuoc = t.id_thuoc,
                TenThuoc = t.ten_thuoc,
                AnhThuoc = t.anh_thuoc,
                GiaBan = t.gia_ban ?? 0,
                TenDanhMuc = t.DANH_MUC_THUOC.ten_danh_muc,
                SoLuongTon = t.LO_THUOC.Sum(lt => lt.so_luong_ton) ?? 0
            });
            return View(danhSachThuoc);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                NhaThuocDatabase.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}