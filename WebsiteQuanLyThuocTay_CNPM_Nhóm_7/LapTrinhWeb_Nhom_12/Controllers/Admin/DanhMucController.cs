using LapTrinhWeb_Nhom_12.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LapTrinhWeb_Nhom_12.Controllers
{
    public class DanhMucController : Controller
    {
        private NHA_THUOCEntities db = new NHA_THUOCEntities();

        // 1. Xem danh sách danh mục
        public ActionResult Index()
        {
            var listDanhMuc = db.DANH_MUC_THUOC.OrderByDescending(d => d.id_danh_muc).ToList();
            return View(listDanhMuc);
        }

        // 2. Giao diện thêm mới (GET)
        public ActionResult Create()
        {
            return View();
        }

        // 3. Xử lý thêm mới (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DANH_MUC_THUOC model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng tên danh mục
                var checkExist = db.DANH_MUC_THUOC.Any(x => x.ten_danh_muc == model.ten_danh_muc);
                if (checkExist)
                {
                    ModelState.AddModelError("ten_danh_muc", "Tên danh mục này đã tồn tại!");
                    return View(model);
                }

                try
                {
                    db.DANH_MUC_THUOC.Add(model);
                    db.SaveChanges();

                    // Thêm xong thì quay về danh sách
                    TempData["Success"] = "Thêm danh mục thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Lỗi hệ thống: " + ex.Message;
                }
            }
            return View(model);
        }
    }
}