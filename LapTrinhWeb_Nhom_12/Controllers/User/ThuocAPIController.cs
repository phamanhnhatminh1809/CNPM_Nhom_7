using System;
using System.Linq;
using System.Web.Http; // Lưu ý: Dùng thư viện này cho API, không phải System.Web.Mvc
using LapTrinhWeb_Nhom_12.Models;

namespace LapTrinhWeb_Nhom_12.Controllers
{
    public class ThuocApiController : ApiController
    {
        private NHA_THUOCEntities db = new NHA_THUOCEntities();

        // GET: api/ThuocApi
        [HttpGet]
        public IHttpActionResult GetDanhSachThuoc()
        {
            // Chỉ lấy các trường cần thiết để nhẹ dữ liệu
            var listThuoc = db.THUOCs.Select(t => new
            {
                Id = t.id_thuoc,
                Ten = t.ten_thuoc,
                Gia = t.gia_ban,
                Anh = t.anh_thuoc,
                DanhMuc = t.DANH_MUC_THUOC.ten_danh_muc
            }).ToList();

            return Ok(listThuoc); // Trả về dạng JSON chuẩn status 200
        }

        // GET: api/ThuocApi/5 (Tìm thuốc theo ID)
        [HttpGet]
        public IHttpActionResult GetThuoc(int id)
        {
            var t = db.THUOCs.Find(id);
            if (t == null) return NotFound();

            var thuocResult = new
            {
                Id = t.id_thuoc,
                Ten = t.ten_thuoc,
                Gia = t.gia_ban,
            };
            return Ok(thuocResult);
        }
    }
}