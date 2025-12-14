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
        private NHA_THUOCEntities NhaThuocDatabase = new NHA_THUOCEntities();

        public ActionResult TrangChu()
        {
            var danhSachThuoc = NhaThuocDatabase.THUOCs.Select(t => new TheThuocViewModel
            {
                IdThuoc = t.id_thuoc,
                TenThuoc = t.ten_thuoc,
                AnhThuoc = t.anh_thuoc,
                GiaBan = t.gia_ban ?? 0,
                TenDanhMuc = t.DANH_MUC_THUOC.ten_danh_muc,
                SoLuongTon = t.LO_THUOC.Sum(lt => lt.so_luong_ton) ?? 0,
                DonViTinh = t.don_vi_tinh,
                Hang = t.hang
            });
            return View(danhSachThuoc);
        }
        public ActionResult ChiTietThuoc(int id)
        {
            var thuoc = NhaThuocDatabase.THUOCs.FirstOrDefault(t => t.id_thuoc == id);

            if (thuoc == null)
            {
                return HttpNotFound();
            }

            int hanSuDungThang = 0; 

            var loThuocMoiNhat = thuoc.LO_THUOC
                                    .OrderByDescending(lt => lt.han_su_dung) 
                                    .FirstOrDefault();

            if (loThuocMoiNhat != null && loThuocMoiNhat.han_su_dung.HasValue && loThuocMoiNhat.ngay_san_xuat.HasValue)
            {
                DateTime ngayHetHan = loThuocMoiNhat.han_su_dung.Value;
                DateTime ngaySanXuat = loThuocMoiNhat.ngay_san_xuat.Value;

                hanSuDungThang = ((ngayHetHan.Year - ngaySanXuat.Year) * 12) + (ngayHetHan.Month - ngaySanXuat.Month);
            }

            // 3. Map dữ liệu từ Entity sang ViewModel
            var model = new ChiTietThuocViewModel
            {
                IdThuoc = thuoc.id_thuoc,
                TenThuoc = thuoc.ten_thuoc,
                GiaBan = thuoc.gia_ban ?? 0,
                DonViTinh = thuoc.don_vi_tinh,
                TenDanhMuc = thuoc.DANH_MUC_THUOC?.ten_danh_muc ?? "Đang cập nhật",
                DangBaoChe = thuoc.dang_bao_che,
                HoatChat = thuoc.hoat_chat,
                Hang = thuoc.hang,
                CongDung = thuoc.cong_dung,
                LieuDung = thuoc.lieu_dung,
                ChongChiDinh = thuoc.chong_chi_dinh,

                // Gán giá trị đã tính toán ở trên vào đây
                HanSuDungThang = hanSuDungThang,

                AnhThuoc = thuoc.anh_thuoc,
            };

            return View(model);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                NhaThuocDatabase.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult TimKiem(string query)
        {
            // 1. Nếu từ khóa rỗng thì quay về trang chủ
            if (string.IsNullOrEmpty(query))
            {
                return RedirectToAction("TrangChu");
            }

            // 2. Tìm kiếm trong database (Theo tên thuốc, gần đúng)
            var ketQua = NhaThuocDatabase.THUOCs
                            .Where(t => t.ten_thuoc.Contains(query)) // Logic tìm kiếm: Chứa từ khóa
                            .Select(t => new TheThuocViewModel
                            {
                                IdThuoc = t.id_thuoc,
                                TenThuoc = t.ten_thuoc,
                                AnhThuoc = t.anh_thuoc,
                                GiaBan = t.gia_ban ?? 0,
                                TenDanhMuc = t.DANH_MUC_THUOC.ten_danh_muc,
                                // Tính tổng tồn kho để hiển thị nút mua hàng đúng logic
                                SoLuongTon = t.LO_THUOC.Sum(lt => lt.so_luong_ton) ?? 0,
                                DonViTinh = t.don_vi_tinh,
                                Hang = t.hang
                            })
                            .ToList();

            // 3. Gửi từ khóa qua ViewBag để hiển thị thông báo (VD: Kết quả tìm kiếm cho 'Panadol')
            ViewBag.TuKhoaTimKiem = query;

            // 4. Tái sử dụng View "TrangChu" để hiển thị kết quả giống hệt trang chủ
            return View("TrangChu", ketQua);
        }
    }
}