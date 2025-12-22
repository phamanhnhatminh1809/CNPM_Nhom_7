using LapTrinhWeb_Nhom_12.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LapTrinhWeb_Nhom_12.Controllers
{
    public class ThuocAdminController : Controller
    {
        private NHA_THUOCEntities db = new NHA_THUOCEntities();

        public ActionResult Index(string searchString, int? categoryId)
        {
            var query = db.THUOCs.AsQueryable();

            // 1. Lọc theo tên thuốc
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(t => t.ten_thuoc.Contains(searchString));
            }

            // 2. Lọc theo danh mục
            if (categoryId.HasValue)
            {
                query = query.Where(t => t.id_danh_muc == categoryId);
            }

            // 3. Select ra ViewModel
            var data = query.Select(t => new
            {
                Thuoc = t,
                LoThuocs = t.LO_THUOC // Lấy danh sách lô để tính toán
            }).ToList().Select(item => new ThuocAdminViewModel
            {
                IdThuoc = item.Thuoc.id_thuoc,
                TenThuoc = item.Thuoc.ten_thuoc,
                AnhThuoc = item.Thuoc.anh_thuoc,
                TenDanhMuc = item.Thuoc.DANH_MUC_THUOC?.ten_danh_muc,
                TenLoaiThuoc = item.Thuoc.LOAI_THUOC?.ten_loai, 
                DonViTinh = item.Thuoc.don_vi_tinh,
                GiaBan = item.Thuoc.gia_ban ?? 0,

                // Tính tổng tồn kho từ các lô
                TongTonKho = item.LoThuocs.Sum(l => l.so_luong_ton) ?? 0,

                // Đếm số lô sắp hết hạn (ví dụ: còn dưới 90 ngày)
                SoLoSapHetHan = item.LoThuocs.Count(l => l.han_su_dung.HasValue &&
                               (l.han_su_dung.Value - DateTime.Now).TotalDays <= 90 &&
                                l.so_luong_ton > 0)
            }).ToList();

            // Chuẩn bị dữ liệu cho Dropdown lọc
            ViewBag.DanhMuc = new SelectList(db.DANH_MUC_THUOC, "id_danh_muc", "ten_danh_muc");

            return View(data);
        }
        public ActionResult GetLoThuoc(int id)
        {
            var loThuocs = db.LO_THUOC.Where(l => l.id_thuoc == id)
                             .OrderBy(l => l.han_su_dung) // Lô nào sắp hết hạn hiện lên đầu
                             .Select(l => new LoThuocViewModel
                             {
                                 SoLo = l.so_lo,
                                 NgaySanXuat = l.ngay_san_xuat,
                                 HanSuDung = l.han_su_dung,
                                 SoLuongTon = l.so_luong_ton ?? 0,
                                 GiaNhap = l.gia_nhap ?? 0
                             }).ToList();

            return PartialView("_LoThuocPartial", loThuocs);
        }
        public ActionResult Edit(int id)
        {
            var thuoc = db.THUOCs.Find(id);
            if (thuoc == null) return HttpNotFound();

            // Truyền danh sách Danh mục và Loại thuốc để hiển thị Dropdown
            ViewBag.IdDanhMuc = new SelectList(db.DANH_MUC_THUOC, "id_danh_muc", "ten_danh_muc", thuoc.id_danh_muc);
            ViewBag.IdLoaiThuoc = new SelectList(db.LOAI_THUOC, "id_loai_thuoc", "ten_loai", thuoc.id_loai_thuoc);

            return View(thuoc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(THUOC model, HttpPostedFileBase uploadAnh)
        {
            if (ModelState.IsValid)
            {
                var thuocInDb = db.THUOCs.Find(model.id_thuoc);
                if (thuocInDb == null) return HttpNotFound();

                // Cập nhật thông tin
                thuocInDb.ten_thuoc = model.ten_thuoc;
                thuocInDb.id_danh_muc = model.id_danh_muc;
                thuocInDb.id_loai_thuoc = model.id_loai_thuoc;
                thuocInDb.don_vi_tinh = model.don_vi_tinh;
                thuocInDb.gia_ban = model.gia_ban;
                thuocInDb.hang = model.hang;
                thuocInDb.hoat_chat = model.hoat_chat;
                thuocInDb.dang_bao_che = model.dang_bao_che;
                thuocInDb.ham_luong = model.ham_luong;
                thuocInDb.cong_dung = model.cong_dung;
                thuocInDb.lieu_dung = model.lieu_dung;
                thuocInDb.chong_chi_dinh = model.chong_chi_dinh;
                thuocInDb.cho_phep_ban_online = model.cho_phep_ban_online;

                // Xử lý Upload ảnh mới (nếu có chọn)
                if (uploadAnh != null && uploadAnh.ContentLength > 0)
                {
                    // Xóa ảnh cũ nếu cần (tùy chọn)

                    // Lưu ảnh mới
                    string _FileName = System.IO.Path.GetFileName(uploadAnh.FileName);
                    string _path = System.IO.Path.Combine(Server.MapPath("~/Images/AnhThuoc"), _FileName);
                    uploadAnh.SaveAs(_path);
                    thuocInDb.anh_thuoc = _FileName;
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Nếu lỗi thì load lại Dropdown
            ViewBag.IdDanhMuc = new SelectList(db.DANH_MUC_THUOC, "id_danh_muc", "ten_danh_muc", model.id_danh_muc);
            ViewBag.IdLoaiThuoc = new SelectList(db.LOAI_THUOC, "id_loai_thuoc", "ten_loai", model.id_loai_thuoc);
            return View(model);
        }
        [HttpPost] 
        public ActionResult Delete(int id)
        {
            try
            {
                var thuoc = db.THUOCs.Find(id);
                if (thuoc == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thuốc." });
                }

                // KIỂM TRA RÀNG BUỘC DỮ LIỆU
                // 1. Kiểm tra xem thuốc này đã có lô hàng nào nhập chưa
                bool coLoThuoc = db.LO_THUOC.Any(l => l.id_thuoc == id);
                if (coLoThuoc)
                {
                    return Json(new { success = false, message = "Không thể xóa! Thuốc này đang có lô hàng trong kho." });
                }

                // 2. Kiểm tra xem thuốc này đã từng được bán chưa (Chi tiết hóa đơn)

                db.THUOCs.Remove(thuoc);
                db.SaveChanges();

                return Json(new { success = true, message = "Đã xóa thuốc thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
        // 1. GET: Hiển thị form thêm mới
        public ActionResult Create()
        {
            // Chuẩn bị dữ liệu cho Dropdown
            ViewBag.IdDanhMuc = new SelectList(db.DANH_MUC_THUOC, "id_danh_muc", "ten_danh_muc");
            ViewBag.IdLoaiThuoc = new SelectList(db.LOAI_THUOC, "id_loai_thuoc", "ten_loai");

            return View();
        }

        // 2. POST: Xử lý lưu dữ liệu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(THUOC model, HttpPostedFileBase uploadAnh)
        {
            // Kiểm tra trùng tên thuốc (Validation thủ công)
            if (db.THUOCs.Any(x => x.ten_thuoc == model.ten_thuoc))
            {
                ModelState.AddModelError("ten_thuoc", "Tên thuốc này đã tồn tại trong hệ thống!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý Upload ảnh
                    if (uploadAnh != null && uploadAnh.ContentLength > 0)
                    {
                        string _FileName = System.IO.Path.GetFileName(uploadAnh.FileName);
                        // Để tránh trùng tên file ảnh, có thể thêm DateTime hoặc Guid vào tên file
                        string _path = System.IO.Path.Combine(Server.MapPath("~/Images/AnhThuoc"), _FileName);
                        uploadAnh.SaveAs(_path);
                        model.anh_thuoc = _FileName;
                    }
                    else
                    {
                        model.anh_thuoc = "default.png"; // Gán ảnh mặc định nếu không upload
                    }

                    // Lưu vào CSDL
                    db.THUOCs.Add(model);
                    db.SaveChanges();

                    // Thông báo thành công (Có thể dùng TempData để hiện alert bên view Index)
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi lưu dữ liệu: " + ex.Message);
                }
            }

            // Nếu dữ liệu lỗi, load lại Dropdown để không bị mất
            ViewBag.IdDanhMuc = new SelectList(db.DANH_MUC_THUOC, "id_danh_muc", "ten_danh_muc", model.id_danh_muc);
            ViewBag.IdLoaiThuoc = new SelectList(db.LOAI_THUOC, "id_loai_thuoc", "ten_loai", model.id_loai_thuoc);

            return View(model);
        }

        // 1. GET: Hiển thị form nhập hàng cho thuốc có ID cụ thể
        public ActionResult NhapHang(int id)
        {
            var thuoc = db.THUOCs.Find(id);
            if (thuoc == null) return HttpNotFound();

            // Tính tồn kho hiện tại để hiển thị cho Admin biết
            int tonKho = db.LO_THUOC.Where(l => l.id_thuoc == id).Sum(l => l.so_luong_ton) ?? 0;

            var model = new NhapLoViewModel
            {
                IdThuoc = thuoc.id_thuoc,
                TenThuoc = thuoc.ten_thuoc,
                AnhThuoc = thuoc.anh_thuoc,
                DonViTinh = thuoc.don_vi_tinh,
                GiaBanHienTai = thuoc.gia_ban ?? 0,
                TongTonKho = tonKho,

                // Giá trị mặc định cho form
                NgaySanXuat = DateTime.Now,
                HanSuDung = DateTime.Now.AddYears(2)
            };

            return View(model);
        }

        // 2. POST: Lưu lô thuốc vào CSDL
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NhapHang(NhapLoViewModel model)
        {
            // Validate logic nghiệp vụ
            if (model.HanSuDung <= model.NgaySanXuat)
            {
                ModelState.AddModelError("HanSuDung", "Hạn sử dụng phải lớn hơn ngày sản xuất!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Map từ ViewModel sang Entity
                    var loThuoc = new LO_THUOC
                    {
                        id_thuoc = model.IdThuoc,
                        so_lo = model.SoLo,
                        ngay_san_xuat = model.NgaySanXuat,
                        han_su_dung = model.HanSuDung,
                        so_luong_nhap = model.SoLuongNhap,
                        so_luong_ton = model.SoLuongNhap, // Mới nhập thì Tồn = Nhập
                        gia_nhap = model.GiaNhap
                    };

                    db.LO_THUOC.Add(loThuoc);
                    db.SaveChanges();

                    // Nhập xong quay về danh sách thuốc
                    TempData["SuccessMessage"] = "Đã nhập kho thành công cho thuốc: " + model.TenThuoc;
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                }
            }

            // Nếu lỗi, giữ nguyên dữ liệu hiển thị để Admin sửa
            return View(model);
        }
    
        // Quản lý thuốc được bán online
        // GET: Hiển thị giao diện danh sách các thuốc được bán online
        [HttpGet]
        // POST: Cập nhật dữ liệu cho phép bán online
        public ActionResult QuanLyThuocBanOnline(string searchString, int? categoryId)
        {
            var query = db.THUOCs.AsQueryable();

            // 1. Lọc theo tên thuốc
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(t => t.ten_thuoc.Contains(searchString));
            }

            // 2. Lọc theo danh mục
            if (categoryId.HasValue)
            {
                query = query.Where(t => t.id_danh_muc == categoryId);
            }

            // 3. Select ra ViewModel
            var data = query.Select(t => new
            {
                Thuoc = t,
                LoThuocs = t.LO_THUOC // Lấy danh sách lô để tính toán
            }).ToList().Select(item => new ThuocAdminViewModel
            {
                IdThuoc = item.Thuoc.id_thuoc,
                TenThuoc = item.Thuoc.ten_thuoc,
                AnhThuoc = item.Thuoc.anh_thuoc,
                TenDanhMuc = item.Thuoc.DANH_MUC_THUOC?.ten_danh_muc,
                TenLoaiThuoc = item.Thuoc.LOAI_THUOC?.ten_loai,
                DonViTinh = item.Thuoc.don_vi_tinh,
                GiaBan = item.Thuoc.gia_ban ?? 0,
                ChoPhepBanOnline = (item.Thuoc.cho_phep_ban_online ?? 1) == 1,

                // Tính tổng tồn kho từ các lô
                TongTonKho = item.LoThuocs.Sum(l => l.so_luong_ton) ?? 0,

                // Đếm số lô sắp hết hạn (ví dụ: còn dưới 90 ngày)
                SoLoSapHetHan = item.LoThuocs.Count(l => l.han_su_dung.HasValue &&
                               (l.han_su_dung.Value - DateTime.Now).TotalDays <= 90 &&
                                l.so_luong_ton > 0)
                
            }).ToList();

            // Chuẩn bị dữ liệu cho Dropdown lọc
            ViewBag.DanhMuc = new SelectList(db.DANH_MUC_THUOC, "id_danh_muc", "ten_danh_muc");

            return View(data);
        }
        [HttpPost]

        public JsonResult UpdateTrangThaiBan(int id, int status) 
        {
            try
            {
                var thuoc = db.THUOCs.Find(id);
                if (thuoc != null)
                {
                    thuoc.cho_phep_ban_online = status;

                    db.SaveChanges();
                    return Json(new { success = true, msg = "Cập nhật thành công!" });
                }
                return Json(new { success = false, msg = "Không tìm thấy thuốc!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Lỗi: " + ex.Message });
            }
        }

       
    }
}