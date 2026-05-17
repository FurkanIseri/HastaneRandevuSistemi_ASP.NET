using Microsoft.AspNetCore.Mvc;
using HastaneYonetimSistemi.Models;
using HastaneYonetimSistemi.Services.Interfaces;

namespace HastaneYonetimSistemi.Controllers
{
    public class HastaController : Controller
    {
        private readonly IHastaService _hastaService;

        public HastaController(IHastaService hastaService)
        {
            _hastaService = hastaService;
        }

        private string? TC => HttpContext.Session.GetString("HastaTC");
        private IActionResult RedirectIfNoSession() => RedirectToAction("HastaGiris", "Auth");

        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            ViewBag.TC = TC;
            var result = _hastaService.GetPanelData(TC!);
            if (result.Success)
            {
                ViewBag.AdSoyad = result.Data.AdSoyad;
                ViewBag.GecmisRandevular = result.Data.GecmisRandevular;
            }
            else
            {
                ViewBag.Error = result.Message;
            }
            return View();
        }

        [HttpGet]
        public JsonResult GetBranslar() => Json(_hastaService.GetBranslar());

        [HttpGet]
        public JsonResult GetDoktorlar(string brans) => Json(_hastaService.GetDoktorlarByBrans(brans));

        [HttpGet]
        public JsonResult GetAktifRandevular(string brans = "", string doktorId = "")
            => Json(_hastaService.GetAktifRandevular(doktorId));

        [HttpPost]
        public async Task<IActionResult> RandevuAl([FromForm] RandevuAlViewModel model)
        {
            if (string.IsNullOrEmpty(TC))
                return Json(new { success = false, message = "Oturum süresi doldu." });
            var result = await _hastaService.RandevuAl(TC!, model);
            return Json(new { success = result.Success, message = result.Message });
        }

        public IActionResult Receteler()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            try
            {
                var receteler = _hastaService.GetReceteler(TC!);
                return View(receteler);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Reçeteler yüklenirken hata: " + ex.Message;
                return View(new List<ReceteViewModel>());
            }
        }

        [HttpGet]
        public IActionResult BilgiDuzenle()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            var result = _hastaService.GetBilgi(TC!);
            if (!result.Success)
            {
                ViewBag.Error = result.Message;
                return View(new HastaBilgiViewModel());
            }
            return View(result.Data);
        }

        [HttpPost]
        public IActionResult BilgiDuzenle(HastaBilgiViewModel model)
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            if (!string.IsNullOrEmpty(model.YeniSifre) && model.YeniSifre != model.SifreTekrar)
            {
                ModelState.AddModelError("SifreTekrar", "Şifreler eşleşmiyor.");
                return View(model);
            }
            if (!ModelState.IsValid) return View(model);
            var result = _hastaService.UpdateBilgi(TC!, model);
            if (result.Success)
            {
                TempData["Message"] = result.Message;
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", result.Message);
            return View(model);
        }
    }
}
