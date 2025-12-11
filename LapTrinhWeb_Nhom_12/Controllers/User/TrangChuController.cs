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
    }
}