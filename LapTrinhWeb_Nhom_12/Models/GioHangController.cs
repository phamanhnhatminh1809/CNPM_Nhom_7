using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LapTrinhWeb_Nhom_12.Models
{
    public class GioHangController : Controller
    {
        // GET: GioHang
        public ActionResult GioHang()
        {
            List<GioHangViewModel> viewModels = new List<GioHangViewModel>();

            return View(viewModels);
        }
    }
}