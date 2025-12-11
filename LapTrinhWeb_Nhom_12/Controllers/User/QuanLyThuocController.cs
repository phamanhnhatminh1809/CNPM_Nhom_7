using LapTrinhWeb_Nhom_12.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LapTrinhWeb_Nhom_12.Controllers
{
    public class QuanLyThuocController : Controller
    {
        private NHA_THUOCEntities db = new NHA_THUOCEntities();

        // 1. Xem danh sách thuốc (Admin)
        public ActionResult Index()
        {
            // Lấy danh sách sắp xếp theo tên mới nhất
            var listThuoc = db.THUOCs.OrderByDescending(t => t.id_thuoc).ToList();
            return View(listThuoc);
        }

        // ---------------------------------------------------------
        // 2. THÊM MỚI (CREATE) - CÓ KIỂM TRA TRÙNG
        // ---------------------------------------------------------

        // GET: Hiển thị form thêm mới
        public ActionResult Themmoi()
        {
            // Load danh sách danh mục để chọn (DropdownList)
            ViewBag.id_danh_muc = new SelectList(db.DANH_MUC_THUOC, "id_danh_muc", "ten_danh_muc");
            return View();
        }

        // POST: Xử lý dữ liệu thêm mới
        [HttpPost]
        [ValidateInput(false)] // Cho phép nhập HTML trong mô tả
        public ActionResult Themmoi(THUOC thuoc, HttpPostedFileBase fileAnh)
        {
            // A. KIỂM TRA TRÙNG TÊN THUỐC
            // (Kiểm tra xem trong DB đã có thuốc nào tên y hệt chưa)
            bool biTrung = db.THUOCs.Any(t => t.ten_thuoc.ToLower() == thuoc.ten_thuoc.ToLower());
            if (biTrung)
            {
                ModelState.AddModelError("ten_thuoc", "Tên thuốc này đã tồn tại! Vui lòng chọn tên khác.");
                // Load lại danh mục để không bị lỗi view
                ViewBag.id_danh_muc = new SelectList(db.DANH_MUC_THUOC, "id_danh_muc", "ten_danh_muc", thuoc.id_danh_muc);
                return View(thuoc);
            }

            if (ModelState.IsValid)
            {
                // B. Xử lý Upload ảnh (Nếu có chọn ảnh)
                if (fileAnh != null && fileAnh.ContentLength > 0)
                {
                    // Lấy tên file
                    string _FileName = Path.GetFileName(fileAnh.FileName);
                    // Lưu vào thư mục Images/AnhThuoc
                    string _path = Path.Combine(Server.MapPath("~/Images/AnhThuoc"), _FileName);
                    fileAnh.SaveAs(_path);
                    // Gán tên file vào đối tượng
                    thuoc.anh_thuoc = _FileName;
                }

                // C. Lưu vào Database
                db.THUOCs.Add(thuoc);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.id_danh_muc = new SelectList(db.DANH_MUC_THUOC, "id_danh_muc", "ten_danh_muc", thuoc.id_danh_muc);
            return View(thuoc);
        }

        // ---------------------------------------------------------
        // 3. CHỈNH SỬA (EDIT)
        // ---------------------------------------------------------
        public ActionResult ChinhSua(int id)
        {
            var thuoc = db.THUOCs.Find(id);
            if (thuoc == null) return HttpNotFound();

            ViewBag.id_danh_muc = new SelectList(db.DANH_MUC_THUOC, "id_danh_muc", "ten_danh_muc", thuoc.id_danh_muc);
            return View(thuoc);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ChinhSua(THUOC thuoc, HttpPostedFileBase fileAnh)
        {
            if (ModelState.IsValid)
            {
                // Tìm lại đối tượng cũ trong DB để sửa
                var thuocInDb = db.THUOCs.FirstOrDefault(t => t.id_thuoc == thuoc.id_thuoc);
                if (thuocInDb != null)
                {
                    thuocInDb.ten_thuoc = thuoc.ten_thuoc;
                    thuocInDb.gia_ban = thuoc.gia_ban;
                    thuocInDb.id_danh_muc = thuoc.id_danh_muc;

                    // Nếu có chọn ảnh mới thì thay ảnh, không thì giữ nguyên
                    if (fileAnh != null && fileAnh.ContentLength > 0)
                    {
                        string _FileName = Path.GetFileName(fileAnh.FileName);
                        string _path = Path.Combine(Server.MapPath("~/Images/AnhThuoc"), _FileName);
                        fileAnh.SaveAs(_path);
                        thuocInDb.anh_thuoc = _FileName;
                    }

                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            ViewBag.id_danh_muc = new SelectList(db.DANH_MUC_THUOC, "id_danh_muc", "ten_danh_muc", thuoc.id_danh_muc);
            return View(thuoc);
        }

        // ---------------------------------------------------------
        // 4. XÓA (DELETE)
        // ---------------------------------------------------------
        public ActionResult Xoa(int id)
        {
            var thuoc = db.THUOCs.Find(id);
            if (thuoc != null)
            {
                db.THUOCs.Remove(thuoc);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}