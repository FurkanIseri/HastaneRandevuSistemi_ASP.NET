using Microsoft.AspNetCore.Mvc;
using HastaneYonetimSistemi.Models;
using HastaneYonetimSistemi.Services.Interfaces;

namespace HastaneYonetimSistemi.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult HastaGiris() => View();

        [HttpPost]
        public IActionResult HastaGiris(GirisViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var result = _authService.GirisHasta(model.TC, model.Sifre);
            if (result.Success)
            {
                HttpContext.Session.SetString("HastaTC", model.TC);
                TempData["Message"] = result.Message;
                return RedirectToAction("Index", "Hasta");
            }
            ViewBag.Error = result.Message;
            return View(model);
        }

        [HttpGet]
        public IActionResult DoktorGiris() => View();

        [HttpPost]
        public IActionResult DoktorGiris(GirisViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var result = _authService.GirisDoktor(model.TC, model.Sifre);
            if (result.Success)
            {
                HttpContext.Session.SetString("DoktorTC", model.TC);
                TempData["Message"] = result.Message;
                return RedirectToAction("Index", "Doktor");
            }
            ViewBag.Error = result.Message;
            return View(model);
        }

        [HttpGet]
        public IActionResult SekreterGiris() => View();

        [HttpPost]
        public IActionResult SekreterGiris(GirisViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var result = _authService.GirisSekreter(model.TC, model.Sifre);
            if (result.Success)
            {
                HttpContext.Session.SetString("SekreterTC", model.TC);
                TempData["Message"] = result.Message;
                return RedirectToAction("Index", "Sekreter");
            }
            ViewBag.Error = result.Message;
            return View(model);
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var result = _authService.KaydetHasta(model);
            if (result.Success)
            {
                HttpContext.Session.SetString("HastaTC", model.TC);
                TempData["Message"] = result.Message;
                return RedirectToAction("Index", "Hasta");
            }
            ViewBag.Error = result.Message;
            return View(model);
        }

        [HttpGet]
        public IActionResult HastaSifremiUnuttum() => View();

        [HttpPost]
        public IActionResult HastaSifremiUnuttum(string tc)
        {
            var result = _authService.GetHastaSifre(tc);
            if (result.Success)
                ViewBag.Mesaj = $"Şifreniz: {result.Data}";
            else
                ViewBag.Error = result.Message;
            return View();
        }

        [HttpGet]
        public IActionResult DoktorSifremiUnuttum() => View();

        [HttpPost]
        public IActionResult DoktorSifremiUnuttum(string tc)
        {
            var result = _authService.GetDoktorSifre(tc);
            if (result.Success)
                ViewBag.Mesaj = $"Şifreniz: {result.Data}";
            else
                ViewBag.Error = result.Message;
            return View();
        }

        [HttpGet]
        public IActionResult SekreterSifremiUnuttum() => View();

        [HttpPost]
        public IActionResult SekreterSifremiUnuttum(string tc)
        {
            var result = _authService.GetSekreterSifre(tc);
            if (result.Success)
                ViewBag.Mesaj = $"Şifreniz: {result.Data}";
            else
                ViewBag.Error = result.Message;
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Message"] = "Sistemden başarıyla çıkış yaptınız.";
            return RedirectToAction("Index", "Home");
        }
    }
}
