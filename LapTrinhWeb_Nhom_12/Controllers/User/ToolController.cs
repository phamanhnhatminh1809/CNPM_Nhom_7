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

namespace LapTrinhWeb_Nhom_12.Controllers
{
    public class ToolController : Controller
    {
        private NHA_THUOCEntities db = new NHA_THUOCEntities();

        // Cấu hình Google API (Thay bằng key của bạn)
        private const string API_KEY = "AIzaSyDeHm-65_GmM3TwVOlIm8BQFzUGETT4H_4";
        private const string SEARCH_ENGINE_ID = "173a299cc7efe4c35";
        // Action này sẽ chạy tool
        public async Task<ActionResult> AutoUpdateImages()
        {
            // Lấy danh sách thuốc cần kiểm tra (ví dụ 10 thuốc mới nhất hoặc có thể lọc thuốc chưa có ảnh trước)
            var listThuoc = db.THUOCs.OrderByDescending(t => t.id_thuoc).ToList();

            int countSuccess = 0;
            var logs = new List<string>();

            using (var client = new HttpClient())
            {
                foreach (var item in listThuoc)
                {
                    try
                    {
                        // --- LOGIC TẠO TÊN FILE MỚI (Đã chỉnh sửa) ---
                        string targetFileName;
                        bool isDbNameMissing = string.IsNullOrEmpty(item.anh_thuoc);

                        if (isDbNameMissing)
                        {
                            // 1. Làm sạch tên (xóa ký tự đặc biệt, chuyển thành chữ thường, thay khoảng trắng bằng _)
                            string cleanName = SanitizeFileName(item.ten_thuoc);

                            // 2. Lấy tối đa 6 ký tự đầu
                            string shortName = cleanName.Length > 6
                                               ? cleanName.Substring(0, 6)
                                               : cleanName;

                            // 3. Thêm hậu tố "_image.jpg"
                            targetFileName = shortName + "_image.jpg";
                        }
                        else
                        {
                            // Nếu DB đã có tên thì giữ nguyên để kiểm tra file cũ
                            targetFileName = item.anh_thuoc;
                        }
                        // ----------------------------------------------

                        // Tạo đường dẫn vật lý
                        string relativePath = "~/Images/AnhThuoc/" + targetFileName;
                        string physicalPath = Server.MapPath(relativePath);

                        // Kiểm tra file tồn tại
                        if (!System.IO.File.Exists(physicalPath))
                        {
                            // File chưa có -> Gọi API tải về
                            string query = $"{item.ten_thuoc} medicine packaging";
                            string googleUrl = $"https://www.googleapis.com/customsearch/v1?q={query}&cx={SEARCH_ENGINE_ID}&searchType=image&key={API_KEY}&num=1";

                            var response = await client.GetStringAsync(googleUrl);
                            var json = JObject.Parse(response);
                            var imageUrl = json["items"]?[0]?["link"]?.ToString();

                            if (!string.IsNullOrEmpty(imageUrl))
                            {
                                DownloadImage(imageUrl, physicalPath);

                                // Cập nhật tên mới vào DB nếu trước đó chưa có
                                if (isDbNameMissing)
                                {
                                    item.anh_thuoc = targetFileName;
                                }

                                countSuccess++;
                                logs.Add($"Đã tải: {item.ten_thuoc} -> {targetFileName}");
                            }
                            else
                            {
                                logs.Add($"Không tìm thấy ảnh: {item.ten_thuoc}");
                            }
                        }
                        else
                        {
                            // File đã tồn tại -> Chỉ cập nhật DB nếu DB đang trống
                            if (isDbNameMissing)
                            {
                                item.anh_thuoc = targetFileName;
                                logs.Add($"File đã có sẵn, cập nhật DB: {item.ten_thuoc} -> {targetFileName}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logs.Add($"Lỗi ({item.ten_thuoc}): {ex.Message}");
                    }
                }

                db.SaveChanges();
            }

            ViewBag.Logs = logs;
            ViewBag.Count = countSuccess;
            return View();
        }

        // Hàm làm sạch tên giữ nguyên, nhưng quan trọng là nó trả về lowercase để bước sau xử lý
        private string SanitizeFileName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "thuoc";

            var invalidChars = Path.GetInvalidFileNameChars();
            string cleanName = new string(name
                .Where(ch => !invalidChars.Contains(ch)) // Xóa ký tự không hợp lệ trong tên file
                .ToArray());

            // Trim khoảng trắng thừa, thay khoảng trắng giữa bằng _, chuyển thường
            return cleanName.Trim().Replace(" ", "_").ToLower();
        }

        private void DownloadImage(string url, string savePath)
        {
            using (WebClient webClient = new WebClient())
            {
                // Thêm SecurityProtocol nếu gặp lỗi SSL/TLS cũ
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                webClient.DownloadFile(url, savePath);
            }
        }
    }
}