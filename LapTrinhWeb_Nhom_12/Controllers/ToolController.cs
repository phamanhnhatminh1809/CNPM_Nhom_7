using LapTrinhWeb_Nhom_12.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace TenProjectCuaBan.Controllers
{
    public class ToolController : Controller
    {
        private NHA_THUOCEntities1 db = new NHA_THUOCEntities1();

        // Cấu hình Google API (Thay bằng key của bạn)
        private const string API_KEY = "AIzaSyDeHm-65_GmM3TwVOlIm8BQFzUGETT4H_4";
        private const string SEARCH_ENGINE_ID = "173a299cc7efe4c35";

        // Action này sẽ chạy tool
        public async Task<ActionResult> AutoUpdateImages()
        {
            // 1. Lấy danh sách thuốc CHƯA CÓ ẢNH hoặc ảnh bị lỗi
            // Giới hạn Take(10) mỗi lần chạy để tránh treo server hoặc hết quota API
            var listThuoc = db.THUOCs
                              .Where(t => t.anh_thuoc == null || t.anh_thuoc == "")
                              .Take(10)
                              .ToList();

            int countSuccess = 0;
            var logs = new List<string>();

            using (var client = new HttpClient())
            {
                foreach (var item in listThuoc)
                {
                    try
                    {
                        // 2. Tạo từ khóa tìm kiếm (Thêm chữ "medicine" hoặc "packaging" để ra ảnh hộp thuốc)
                        string query = $"{item.ten_thuoc} medicine packaging";
                        string googleUrl = $"https://www.googleapis.com/customsearch/v1?q={query}&cx={SEARCH_ENGINE_ID}&searchType=image&key={API_KEY}&num=1";

                        // 3. Gọi Google API
                        var response = await client.GetStringAsync(googleUrl);
                        var json = JObject.Parse(response);

                        // Lấy link ảnh đầu tiên tìm thấy
                        var imageUrl = json["items"]?[0]?["link"]?.ToString();

                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            // 4. Tạo tên file sạch (ví dụ: Panadol Extra -> panadol_extra.jpg)
                            string safeFileName = SanitizeFileName(item.ten_thuoc) + ".jpg";
                            string savePath = Server.MapPath("~/Images/AnhThuoc/" + safeFileName);

                            // 5. Tải ảnh về Server
                            DownloadImage(imageUrl, savePath);

                            // 6. Cập nhật vào Database
                            // Lưu ý: Chỉ lưu tên file, không lưu đường dẫn tuyệt đối
                            item.anh_thuoc = safeFileName;

                            countSuccess++;
                            logs.Add($"Đã tải: {item.ten_thuoc} -> {safeFileName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        logs.Add($"Lỗi ({item.ten_thuoc}): {ex.Message}");
                    }
                }

                // Lưu DB sau khi chạy xong vòng lặp
                db.SaveChanges();
            }

            ViewBag.Logs = logs;
            ViewBag.Count = countSuccess;
            return View();
        }

        // Hàm phụ: Làm sạch tên file (Xóa ký tự đặc biệt)
        private string SanitizeFileName(string name)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            string cleanName = new string(name
                .Where(ch => !invalidChars.Contains(ch))
                .ToArray());

            // Thay khoảng trắng bằng dấu gạch dưới, chuyển về chữ thường
            return cleanName.Replace(" ", "_").ToLower();
        }

        // Hàm phụ: Tải ảnh từ URL
        private void DownloadImage(string url, string savePath)
        {
            using (WebClient webClient = new WebClient())
            {
                // Giả danh trình duyệt để tránh bị chặn 403
                webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                webClient.DownloadFile(url, savePath);
            }
        }
    }
}