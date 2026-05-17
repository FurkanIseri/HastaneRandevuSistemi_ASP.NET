using Microsoft.AspNetCore.Mvc;
using HastaneYonetimSistemi.Models;
using HastaneYonetimSistemi.Helpers;
using HastaneYonetimSistemi.Services.Interfaces;

namespace HastaneYonetimSistemi.Controllers
{
    public class SekreterController : Controller
    {
        private readonly IBransService _bransService;
        private readonly IDuyuruService _duyuruService;
        private readonly IDoktorService _doktorService;
        private readonly IIlacService _ilacService;
        private readonly ISekreterService _sekreterService;
        private readonly IRandevuService _randevuService;

        public SekreterController(
            IBransService bransService,
            IDuyuruService duyuruService,
            IDoktorService doktorService,
            IIlacService ilacService,
            ISekreterService sekreterService,
            IRandevuService randevuService)
        {
            _bransService = bransService;
            _duyuruService = duyuruService;
            _doktorService = doktorService;
            _ilacService = ilacService;
            _sekreterService = sekreterService;
            _randevuService = randevuService;
        }

        private string? TC => HttpContext.Session.GetString("SekreterTC");
        private IActionResult RedirectIfNoSession() => RedirectToAction("SekreterGiris", "Auth");

        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            ViewBag.TC = TC;
            try
            {
                string hash = SecurityHelper.Hashle(TC!);
                var sekreter = _sekreterService.GetSekreterAdSoyad(hash);
                if (sekreter.HasValue)
                    ViewBag.AdSoyad = sekreter.Value.Ad + " " + sekreter.Value.Soyad;
                ViewBag.Branslar = _bransService.GetAllBransNames();
            }
            catch (Exception ex) { ViewBag.Error = "Hata: " + ex.Message; }
            return View();
        }

        [HttpGet]
        public IActionResult DoktorListesi()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            try
            {
                string hash = SecurityHelper.Hashle(TC!);
                ViewBag.CurrentSekreterId = _sekreterService.GetSekreterId(hash).ToString();
                return View(_doktorService.GetAllDoktorlar());
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Hata: " + ex.Message;
                return View(new List<DoktorListeItem>());
            }
        }

        [HttpGet]
        public IActionResult DoktorEkle()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            ViewBag.Branslar = _bransService.GetAllBransNames();
            return View(new DoktorEkleViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> DoktorEkle(DoktorEkleViewModel model)
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            ViewBag.Branslar = _bransService.GetAllBransNames();
            if (!ModelState.IsValid) return View(model);
            string hash = SecurityHelper.Hashle(TC!);
            var result = await _doktorService.EkleDoktor(model, hash);
            if (result.Success)
            {
                TempData["Message"] = result.Message;
                return RedirectToAction("DoktorListesi");
            }
            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        [HttpPost]
        public IActionResult DoktorSil(string doktorId)
        {
            if (string.IsNullOrEmpty(TC)) return Json(new { success = false, message = "Oturum süresi doldu." });
            string hash = SecurityHelper.Hashle(TC!);
            var result = _doktorService.SilDoktor(doktorId, hash);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpGet]
        public IActionResult BransYonetimi()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            return View(_bransService.GetAllBranslar());
        }

        [HttpPost]
        public IActionResult BransEkle(string bransAd)
        {
            if (string.IsNullOrEmpty(TC)) return Json(new { success = false, message = "Oturum süresi doldu." });
            var result = _bransService.EkleBrans(bransAd);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        public IActionResult BransSil(int bransId)
        {
            if (string.IsNullOrEmpty(TC)) return Json(new { success = false, message = "Oturum süresi doldu." });
            var result = _bransService.SilBrans(bransId);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpGet]
        public IActionResult IlacYonetimi()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            return View(_ilacService.GetAllIlaclar());
        }

        [HttpPost]
        public IActionResult IlacEkle(string ilacAd, int stokAdet)
        {
            if (string.IsNullOrEmpty(TC)) return Json(new { success = false, message = "Oturum süresi doldu." });
            var result = _ilacService.EkleIlac(ilacAd, stokAdet);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        public IActionResult IlacSil(int ilacId)
        {
            if (string.IsNullOrEmpty(TC)) return Json(new { success = false, message = "Oturum süresi doldu." });
            var result = _ilacService.SilIlac(ilacId);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpGet]
        public IActionResult SekreterYonetimi()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            return View(_sekreterService.GetAllSekreterler());
        }

        [HttpPost]
        public IActionResult SekreterEkle(string ad, string soyad, string tc, string sifre, string cinsiyet)
        {
            if (string.IsNullOrEmpty(TC)) return Json(new { success = false, message = "Oturum süresi doldu." });
            var result = _sekreterService.EkleSekreter(ad, soyad, tc, sifre, cinsiyet);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        public IActionResult SekreterSil(string sekreterID)
        {
            return Json(new { success = false, message = "Güvenlik politikası gereği sekreter kayıtları sistemden silinemez." });
        }

        [HttpPost]
        public IActionResult DuyuruOlustur(string duyuru)
        {
            if (string.IsNullOrEmpty(TC)) return Json(new { success = false, message = "Oturum süresi doldu." });
            var result = _duyuruService.OlusturDuyuru(duyuru, TC!);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        public IActionResult RandevuOlustur(string tarih, string saat, string brans, string doktorId)
        {
            if (string.IsNullOrEmpty(TC)) return Json(new { success = false, message = "Oturum süresi doldu." });
            string hash = SecurityHelper.Hashle(TC!);
            var result = _randevuService.OlusturRandevuYuvası(tarih, saat, brans, doktorId, hash);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpGet]
        public JsonResult GetDoktorlarByBrans(string brans) => Json(_doktorService.GetDoktorlarByBrans(brans));

        [HttpGet]
        public IActionResult BilgiDuzenle()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            var model = new SekreterBilgiViewModel();
            string hash = SecurityHelper.Hashle(TC!);
            var sekreter = _sekreterService.GetSekreterAdSoyad(hash);
            if (sekreter.HasValue) model.AdSoyad = sekreter.Value.Ad + " " + sekreter.Value.Soyad;
            return View(model);
        }

        [HttpPost]
        public IActionResult BilgiDuzenle(SekreterBilgiViewModel model)
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            string hash = SecurityHelper.Hashle(TC!);
            var result = _sekreterService.GuncelleSifre(hash, model);
            if (result.Success)
            {
                TempData["Message"] = result.Message;
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        [HttpGet]
        public IActionResult DoktorDuzenle(string doktorId)
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            string hash = SecurityHelper.Hashle(TC!);
            var result = _doktorService.GetDoktorForEdit(doktorId, hash);
            if (!result.Success)
            {
                if (result.Data != null)
                {
                    ViewBag.Error = result.Message;
                    ViewBag.Branslar = _bransService.GetAllBransNames();
                    return View(result.Data);
                }
                TempData["Error"] = result.Message;
                return RedirectToAction("DoktorListesi");
            }
            ViewBag.Branslar = _bransService.GetAllBransNames();
            return View(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> DoktorDuzenle(DoktorGuncelleViewModel model)
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            ViewBag.Branslar = _bransService.GetAllBransNames();
            if (!ModelState.IsValid) return View(model);
            string hash = SecurityHelper.Hashle(TC!);
            var result = await _doktorService.GuncelleDoktor(model, hash);
            if (result.Success)
            {
                TempData["Message"] = result.Message;
                return RedirectToAction("DoktorListesi");
            }
            ViewBag.Error = result.Message;
            return View(model);
        }
    }
}
