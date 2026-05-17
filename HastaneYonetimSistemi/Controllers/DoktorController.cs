using Microsoft.AspNetCore.Mvc;
using HastaneYonetimSistemi.Models;
using HastaneYonetimSistemi.Services.Interfaces;
using HastaneYonetimSistemi.DataAccessLayer.Interfaces;

namespace HastaneYonetimSistemi.Controllers
{
    public class DoktorController : Controller
    {
        private readonly IDoktorService _doktorService;
        private readonly IReceteRepository _receteRepository;

        public DoktorController(IDoktorService doktorService, IReceteRepository receteRepository)
        {
            _doktorService = doktorService;
            _receteRepository = receteRepository;
        }

        private string? TC => HttpContext.Session.GetString("DoktorTC");
        private IActionResult RedirectIfNoSession() => RedirectToAction("DoktorGiris", "Auth");

        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            ViewBag.TC = TC;
            var result = _doktorService.GetPanelData(TC!);
            if (result.Success)
            {
                ViewBag.AdSoyad = result.Data.AdSoyad;
                ViewBag.ProfilFotoBase64 = result.Data.ProfilBase64;
                ViewBag.Randevular = result.Data.Randevular;
                ViewBag.Ilaclar = result.Data.Ilaclar;
            }
            else
            {
                ViewBag.Error = result.Message;
            }
            return View();
        }

        [HttpGet]
        public IActionResult TibbiBelgeGoster(int randevuId)
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            var data = _receteRepository.GetTibbiBelge(randevuId);
            if (data == null) return NotFound("Tıbbi belge bulunamadı.");
            return File(data, "image/jpeg", $"tibbi-belge-{randevuId}.jpg");
        }

        [HttpPost]
        public IActionResult ReceteOlustur(int randevuId, string tani, int ilacId, int adet, string kullanimSekli)
        {
            if (string.IsNullOrEmpty(TC))
                return Json(new { success = false, message = "Oturum süresi doldu." });
            var result = _doktorService.OlusturRecete(randevuId, tani, ilacId, adet, kullanimSekli);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpGet]
        public IActionResult YazilanReceteler()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            var result = _doktorService.GetYazilanReceteler(TC!);
            if (!result.Success)
            {
                ViewBag.Error = result.Message;
                return View(new List<YazilanReceteViewModel>());
            }
            return View(result.Data);
        }

        public IActionResult Duyurular()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            return View(_doktorService.GetDuyurular());
        }

        [HttpGet]
        public IActionResult BilgiDuzenle()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            var result = _doktorService.GetBilgiProfil(TC!);
            var model = new DoktorBilgiViewModel();
            if (result.Success) model.AdSoyad = result.Data.AdSoyad;
            return View(model);
        }

        [HttpPost]
        public IActionResult BilgiDuzenle(DoktorBilgiViewModel model)
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            var result = _doktorService.UpdateSifre(TC!, model);
            if (result.Success)
            {
                TempData["Message"] = result.Message;
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        [HttpGet]
        public JsonResult GetIlaclar()
        {
            if (string.IsNullOrEmpty(TC)) return Json(new List<object>());
            var result = _doktorService.GetPanelData(TC!);
            if (!result.Success) return Json(new List<object>());
            return Json(result.Data.Ilaclar.Select(i => new { id = i.IlacID, ad = i.IlacAd, stok = i.StokAdet }));
        }
    }
}
