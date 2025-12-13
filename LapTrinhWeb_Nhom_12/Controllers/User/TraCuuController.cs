using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json; // Thư viện xử lý JSON (thường có sẵn trong MVC)
using LapTrinhWeb_Nhom_12.Models;
using System.Net;
using System.Text;

namespace LapTrinhWeb_Nhom_12.Controllers
{
    public class TraCuuController : Controller
    {
        // GET: Hiển thị trang tìm kiếm ban đầu
        public ActionResult Index()
        {
            return View(new List<ThuocAPI>());
        }

        // POST: Xử lý khi người dùng bấm nút Tìm kiếm
        [HttpPost]
        public async Task<ActionResult> Index(string keyword)
        {
            var results = new List<ThuocAPI>();

            if (string.IsNullOrEmpty(keyword))
            {
                ViewBag.Message = "Vui lòng nhập tên thuốc.";
                return View(results);
            }

            try
            {
                using (var client = new HttpClient())
                {
                    // 1. Gọi API OpenFDA lấy dữ liệu tiếng Anh
                    string apiUrl = $"https://api.fda.gov/drug/label.json?search=openfda.brand_name:\"{keyword}\"&limit=5";
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonString = await response.Content.ReadAsStringAsync();
                        var apiData = JsonConvert.DeserializeObject<OpenFdaResponse>(jsonString);

                        if (apiData != null && apiData.results != null)
                        {
                            foreach (var item in apiData.results)
                            {
                                // Lấy dữ liệu thô
                                string rawPurpose = item.purpose?.FirstOrDefault() ?? "";
                                string rawDosage = item.dosage_and_administration?.FirstOrDefault() ?? "";
                                string rawWarnings = item.warnings?.FirstOrDefault() ?? "";
                                string rawStopUse = item.stop_use?.FirstOrDefault() ?? "";
                                string rawStorage = item.storage_and_handling?.FirstOrDefault() ?? "";

                                // 2. Dịch sang Tiếng Việt ngay tại Server
                                // Lưu ý: Chỉ dịch nếu có nội dung để tránh lỗi
                                results.Add(new ThuocAPI
                                {
                                    BrandName = item.openfda?.brand_name?.FirstOrDefault() ?? "Đang cập nhật",
                                    GenericName = item.openfda?.generic_name?.FirstOrDefault() ?? "Đang cập nhật",
                                    Manufacturer = item.openfda?.manufacturer_name?.FirstOrDefault() ?? "N/A",
                                    ActiveIngredient = item.active_ingredient?.FirstOrDefault(),

                                    // Gọi hàm dịch cho từng trường
                                    Purpose = !string.IsNullOrEmpty(rawPurpose) ? TranslateText(rawPurpose) : "Chưa có thông tin",
                                    Dosage = !string.IsNullOrEmpty(rawDosage) ? TranslateText(rawDosage) : "Không có hướng dẫn cụ thể",
                                    Warnings = !string.IsNullOrEmpty(rawWarnings) ? TranslateText(rawWarnings) : "Không có cảnh báo",
                                    StopUse = !string.IsNullOrEmpty(rawStopUse) ? TranslateText(rawStopUse) : "Không có thông tin",
                                    Storage = !string.IsNullOrEmpty(rawStorage) ? TranslateText(rawStorage) : "Bảo quản nơi khô ráo",

                                    InactiveIngredient = item.inactive_ingredient?.FirstOrDefault()
                                });
                            }
                        }
                    }
                    else
                    {
                        ViewBag.Error = "Không tìm thấy thuốc nào.";
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi hệ thống: " + ex.Message;
            }

            ViewBag.Keyword = keyword;
            return View(results);
        }

        // --- HÀM HỖ TRỢ DỊCH ---
        private string TranslateText(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            // Nếu văn bản quá dài (>2000 ký tự), cắt bớt để tránh lỗi URL
            if (input.Length > 2000) input = input.Substring(0, 2000) + "...";

            try
            {
                string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl=vi&dt=t&q={System.Web.HttpUtility.UrlEncode(input)}";
                using (var webClient = new WebClient())
                {
                    webClient.Encoding = Encoding.UTF8;
                    string result = webClient.DownloadString(url);

                    // Xử lý JSON trả về từ Google (Dạng mảng lồng nhau)
                    // Cách đơn giản nhất là tìm chuỗi trong JSON
                    var jsonArray = JsonConvert.DeserializeObject<dynamic>(result);
                    string translatedText = "";

                    if (jsonArray != null && jsonArray[0] != null)
                    {
                        foreach (var item in jsonArray[0])
                        {
                            translatedText += item[0];
                        }
                    }
                    return translatedText;
                }
            }
            catch
            {
                return input; // Nếu lỗi dịch thì trả về tiếng Anh gốc
            }
        }
    }
}