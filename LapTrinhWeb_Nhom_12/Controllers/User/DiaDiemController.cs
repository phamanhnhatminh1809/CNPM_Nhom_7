using LapTrinhWeb_Nhom_12.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls.WebParts;

namespace LapTrinhWeb_Nhom_12.Controllers
{
    public class DiaDiemController : Controller
    {
        private NHA_THUOCEntities db = new NHA_THUOCEntities();

        // 1. Lấy danh sách Tỉnh
        public ActionResult GetTinhThanh()
        {
            var data = db.TINH_THANH.Select(p => new { p.id_tinh_thanh, p.ten }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // 2. Lấy danh sách Quận theo ID Tỉnh
        public ActionResult GetQuanHuyen(int id_tinh_thanh)
        {
            var data = db.QUAN_HUYEN.Where(d => d.id_tinh_thanh == id_tinh_thanh)
                                   .Select(d => new { d.id_quan_huyen, d.ten }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // 3. Lấy danh sách Phường theo ID Quận
        public ActionResult GetPhuongXa(int id_quan_huyen)
        {
            var data = db.PHUONG_XA.Where(w => w.id_quan_huyen == id_quan_huyen)
                               .Select(w => new { w.id_phuong_xa, w.ten }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        // 1. Lấy danh sách Tỉnh có chứa nhà thuốc (Dùng Distinct để loại bỏ trùng lặp)
        public ActionResult GetTinhThanhCoNhaThuoc()
        {
            // Chỉ lấy tên tỉnh, ví dụ: ["Hà Nội", "Hồ Chí Minh"]
            var data = db.DIA_DIEM_NHA_THUOC
                         .Select(x => x.thanh_pho_tinh)
                         .Distinct()
                         .ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // 2. Lấy danh sách Quận thuộc Tỉnh đó có chứa nhà thuốc
        public ActionResult GetQuanHuyenCoNhaThuoc(string tenTinh)
        {
            var data = db.DIA_DIEM_NHA_THUOC
                         .Where(x => x.thanh_pho_tinh == tenTinh)
                         .Select(x => x.quan_huyen)
                         .Distinct()
                         .ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetDanhSachNhaThuoc(string tenTinh, string tenQuan)
        {
            // Tìm gần đúng hoặc chính xác. Vì dropdown trả về chuẩn nên dùng Contains là ổn.
            var data = db.DIA_DIEM_NHA_THUOC
                         .Where(x => x.thanh_pho_tinh.Contains(tenTinh) && x.quan_huyen.Contains(tenQuan))
                         .Select(x => new {
                             x.id_dia_diem,
                             x.ten_nha_thuoc,
                             x.thanh_pho_tinh,
                             x.quan_huyen,
                             x.so_duong_ten_duong
                         }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}