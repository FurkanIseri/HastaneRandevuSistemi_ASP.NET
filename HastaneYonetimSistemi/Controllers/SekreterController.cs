using Microsoft.AspNetCore.Mvc;
using HastaneYonetimSistemi.Models;
using HastaneYonetimSistemi.Helpers;
using Npgsql;
using NpgsqlTypes;
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

        // ================================================================
        // ANA PANEL
        // ================================================================
        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            ViewBag.TC = TC;
            try
            {
                var hash = SecurityHelper.Hashle(TC!);
                var sekreter = _sekreterService.GetSekreterAdSoyad(hash);
                if (sekreter.HasValue)
                {
                    ViewBag.AdSoyad = sekreter.Value.Ad + " " + sekreter.Value.Soyad;
                }

                ViewBag.Branslar = _bransService.GetAllBransNames();
            }
            catch (Exception ex) { ViewBag.Error = "Hata: " + ex.Message; }
            return View();
        }

        // ================================================================
        // DOKTOR YÖNETİMİ
        // ================================================================
        [HttpGet]
        public IActionResult DoktorListesi()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            try
            {
                var hash = SecurityHelper.Hashle(TC!);
                var sekreterId = _sekreterService.GetSekreterId(hash);
                ViewBag.CurrentSekreterId = sekreterId.ToString();

                var doktorlar = _doktorService.GetAllDoktorlar();
                return View(doktorlar);
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
            
            var hash = SecurityHelper.Hashle(TC!);
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
            var hash = SecurityHelper.Hashle(TC!);
            var result = _doktorService.SilDoktor(doktorId, hash);
            return Json(new { success = result.Success, message = result.Message });
        }

        // ================================================================
        // BRANÅ YÖNETİMİ
        // ================================================================
        [HttpGet]
        public IActionResult BransYonetimi()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            var branslar = _bransService.GetAllBranslar();
            return View(branslar);
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

        // ================================================================
        // İLAÇ YÖNETİMİ
        // ================================================================
        [HttpGet]
        public IActionResult IlacYonetimi()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            var ilaclar = _ilacService.GetAllIlaclar();
            return View(ilaclar);
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

        // ================================================================
        // SEKRETER YÖNETİMİ
        // ================================================================
        [HttpGet]
        public IActionResult SekreterYonetimi()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            var sekreterler = _sekreterService.GetAllSekreterler();
            return View(sekreterler);
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
            // For data integrity, secretary records cannot be deleted via UI
            return Json(new { success = false, message = "Güvenlik politikası gereği sekreter kayıtları sistemden silinemez." });
        }

        // ================================================================
        // DUYURU
        // ================================================================
        [HttpPost]
        public IActionResult DuyuruOlustur(string duyuru)
        {
            if (string.IsNullOrEmpty(TC)) return Json(new { success = false, message = "Oturum süresi doldu." });
            var result = _duyuruService.OlusturDuyuru(duyuru, TC);
            return Json(new { success = result.Success, message = result.Message });
        }

        // ================================================================
        // RANDEVU YUVASI
        // ================================================================
        [HttpPost]
        public IActionResult RandevuOlustur(string tarih, string saat, string brans, string doktorId)
        {
            if (string.IsNullOrEmpty(TC)) return Json(new { success = false, message = "Oturum süresi doldu." });
            var hash = SecurityHelper.Hashle(TC!);
            var result = _randevuService.OlusturRandevuYuvası(tarih, saat, brans, doktorId, hash);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpGet]
        public JsonResult GetDoktorlarByBrans(string brans)
        {
            var doktorlar = _doktorService.GetDoktorlarByBrans(brans);
            return Json(doktorlar);
        }

        // ================================================================
        // BİLGİ DÜZENLE (Sekreter â€“ Åifre Güncelleme)
        // ================================================================
        [HttpGet]
        public IActionResult BilgiDuzenle()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            var model = new SekreterBilgiViewModel();
            var hash = SecurityHelper.Hashle(TC!);
            var sekreter = _sekreterService.GetSekreterAdSoyad(hash);
            if (sekreter.HasValue)
            {
                model.AdSoyad = sekreter.Value.Ad + " " + sekreter.Value.Soyad;
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult BilgiDuzenle(SekreterBilgiViewModel model)
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            var hash = SecurityHelper.Hashle(TC!);
            var result = _sekreterService.GuncelleSifre(hash, model);

            if (result.Success)
            {
                TempData["Message"] = result.Message;
                return RedirectToAction("Index");
            }
            
            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        // ================================================================
        // DOKTOR GÜNCELLE (Sadece kaydeden sekreter)
        // ================================================================
        [HttpGet]
        public IActionResult DoktorDuzenle(string doktorId)
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            var hash = SecurityHelper.Hashle(TC!);
            var result = _doktorService.GetDoktorForEdit(doktorId, hash);

            if (!result.Success)
            {
                if (result.Model != null)
                {
                    ViewBag.Error = result.Message;
                    ViewBag.Branslar = _bransService.GetAllBransNames();
                    return View(result.Model);
                }
                TempData["Error"] = result.Message;
                return RedirectToAction("DoktorListesi");
            }

            ViewBag.Branslar = _bransService.GetAllBransNames();
            return View(result.Model);
        }

        [HttpPost]
        public async Task<IActionResult> DoktorDuzenle(DoktorGuncelleViewModel model)
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            ViewBag.Branslar = _bransService.GetAllBransNames();
            
            if (!ModelState.IsValid) return View(model);

            var hash = SecurityHelper.Hashle(TC!);
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
