using Microsoft.AspNetCore.Mvc;
using HastaneYonetimSistemi.Models;
using HastaneYonetimSistemi.Helpers;
using Npgsql;

namespace HastaneYonetimSistemi.Controllers
{
    public class AuthController : Controller
    {
        private readonly sqlBaglanti _bgl;

        public AuthController(IConfiguration configuration)
        {
            _bgl = new sqlBaglanti(configuration);
        }

        // ======================== HASTA GİRİÅİ ========================
        [HttpGet]
        public IActionResult HastaGiris()
        {
            return View();
        }

        [HttpPost]
        public IActionResult HastaGiris(GirisViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string tcHash = SecurityHelper.Hashle(model.TC);
            string sifreHash = SecurityHelper.Hashle(model.Sifre);

            using var conn = _bgl.baglanti();
            try
            {
                using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Hastalar WHERE tc_hash = @p1 AND sifre_hash = @p2", conn);
                cmd.Parameters.AddWithValue("@p1", tcHash);
                cmd.Parameters.AddWithValue("@p2", sifreHash);

                int count = Convert.ToInt32(cmd.ExecuteScalar());

                if (count > 0)
                {
                    HttpContext.Session.SetString("HastaTC", model.TC);
                    TempData["Message"] = "Hasta sistemine başarıyla giriş yaptınız!";
                    return RedirectToAction("Index", "Hasta");
                }

                ViewBag.Error = "Hatalı TC Kimlik No veya Åifre!";
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Giriş işlemi sırasında bir hata oluştu: " + ex.Message;
                return View(model);
            }
        }

        // ======================== DOKTOR GİRİÅİ ========================
        [HttpGet]
        public IActionResult DoktorGiris()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DoktorGiris(GirisViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string tcHash = SecurityHelper.Hashle(model.TC);
            string sifreHash = SecurityHelper.Hashle(model.Sifre);

            using var conn = _bgl.baglanti();
            try
            {
                using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Doktorlar WHERE tc_hash = @p1 AND sifre_hash = @p2", conn);
                cmd.Parameters.AddWithValue("@p1", tcHash);
                cmd.Parameters.AddWithValue("@p2", sifreHash);

                int count = Convert.ToInt32(cmd.ExecuteScalar());

                if (count > 0)
                {
                    HttpContext.Session.SetString("DoktorTC", model.TC);
                    TempData["Message"] = "Doktor paneline başarıyla giriş yaptınız!";
                    return RedirectToAction("Index", "Doktor");
                }

                ViewBag.Error = "Hatalı TC Kimlik No veya Åifre!";
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Giriş işlemi sırasında bir hata oluştu: " + ex.Message;
                return View(model);
            }
        }

        // ======================== SEKRETER GİRİÅİ ========================
        [HttpGet]
        public IActionResult SekreterGiris()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SekreterGiris(GirisViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string tcHash = SecurityHelper.Hashle(model.TC);
            string sifreHash = SecurityHelper.Hashle(model.Sifre);

            using var conn = _bgl.baglanti();
            try
            {
                using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Sekreterler WHERE tc_hash = @p1 AND sifre_hash = @p2", conn);
                cmd.Parameters.AddWithValue("@p1", tcHash);
                cmd.Parameters.AddWithValue("@p2", sifreHash);

                int count = Convert.ToInt32(cmd.ExecuteScalar());

                if (count > 0)
                {
                    HttpContext.Session.SetString("SekreterTC", model.TC);
                    TempData["Message"] = "Sekreter paneline başarıyla giriş yaptınız!";
                    return RedirectToAction("Index", "Sekreter");
                }

                ViewBag.Error = "Hatalı TC Kimlik No veya Åifre!";
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Giriş işlemi sırasında bir hata oluştu: " + ex.Message;
                return View(model);
            }
        }

        // ======================== HASTA KAYIT ========================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string tcHash = SecurityHelper.Hashle(model.TC);
            string tcSifreli = SecurityHelper.Sifrele(model.TC);
            string sifreHash = SecurityHelper.Hashle(model.Sifre);
            string sifreSifreli = SecurityHelper.Sifrele(model.Sifre);

            using var conn = _bgl.baglanti();

            try
            {
                // Check existing Doctor or Secretary
                using (var cmdDoktor = new NpgsqlCommand("SELECT COUNT(*) FROM Doktorlar WHERE tc_hash = @p1", conn))
                {
                    cmdDoktor.Parameters.AddWithValue("@p1", tcHash);
                    if (Convert.ToInt32(cmdDoktor.ExecuteScalar()) > 0)
                    {
                        ViewBag.Error = "Bu TC Kimlik Numarası sistemde personel olarak kayıtlı!";
                        return View(model);
                    }
                }

                using (var cmdSekreter = new NpgsqlCommand("SELECT COUNT(*) FROM Sekreterler WHERE tc_hash = @p1", conn))
                {
                    cmdSekreter.Parameters.AddWithValue("@p1", tcHash);
                    if (Convert.ToInt32(cmdSekreter.ExecuteScalar()) > 0)
                    {
                        ViewBag.Error = "Bu TC Kimlik Numarası sistemde personel olarak kayıtlı!";
                        return View(model);
                    }
                }

                using var cmd = new NpgsqlCommand("CALL sp_HastaKayit(@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8::cinsiyet_tipi)", conn);
                cmd.Parameters.AddWithValue("@p1", model.Ad);
                cmd.Parameters.AddWithValue("@p2", model.Soyad);
                cmd.Parameters.AddWithValue("@p3", tcHash);
                cmd.Parameters.AddWithValue("@p4", tcSifreli);
                cmd.Parameters.AddWithValue("@p5", model.Telefon);
                cmd.Parameters.AddWithValue("@p6", sifreHash);
                cmd.Parameters.AddWithValue("@p7", sifreSifreli);
                cmd.Parameters.AddWithValue("@p8", model.Cinsiyet.ToUpper());

                cmd.ExecuteNonQuery();

                HttpContext.Session.SetString("HastaTC", model.TC);
                TempData["Message"] = "Kaydınız başarıyla tamamlandı ve otomatik giriş yapıldı!";
                return RedirectToAction("Index", "Hasta");
            }
            catch (PostgresException ex)
            {
                ViewBag.Error = "Kayıt Başarısız: " + ex.MessageText;
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Beklenmeyen bir hata oluştu: " + ex.Message;
                return View(model);
            }
        }

        // ======================== ÅİFREMİ UNUTTUM ========================
        [HttpGet]
        public IActionResult HastaSifremiUnuttum()
        {
            return View();
        }

        [HttpPost]
        public IActionResult HastaSifremiUnuttum(string tc)
        {
            if (string.IsNullOrEmpty(tc))
            {
                ViewBag.Error = "Lütfen TC Kimlik Numaranızı giriniz.";
                return View();
            }

            string tcHash = SecurityHelper.Hashle(tc);
            using var conn = _bgl.baglanti();
            try
            {
                using var cmd = new NpgsqlCommand("SELECT sifre_sifreli FROM Hastalar WHERE tc_hash = @p1", conn);
                cmd.Parameters.AddWithValue("@p1", tcHash);

                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    string sifre = SecurityHelper.Coz(result.ToString());
                    ViewBag.Mesaj = $"Åifreniz: {sifre}";
                    return View();
                }

                ViewBag.Error = "Bu TC Kimlik Numarasına ait kayıt bulunamadı.";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "İşlem sırasında bir hata oluştu: " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult DoktorSifremiUnuttum()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DoktorSifremiUnuttum(string tc)
        {
            if (string.IsNullOrEmpty(tc))
            {
                ViewBag.Error = "Lütfen TC Kimlik Numaranızı giriniz.";
                return View();
            }

            string tcHash = SecurityHelper.Hashle(tc);
            using var conn = _bgl.baglanti();
            try
            {
                using var cmd = new NpgsqlCommand("SELECT sifre_sifreli FROM Doktorlar WHERE tc_hash = @p1", conn);
                cmd.Parameters.AddWithValue("@p1", tcHash);

                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    string sifre = SecurityHelper.Coz(result.ToString());
                    ViewBag.Mesaj = $"Åifreniz: {sifre}";
                    return View();
                }

                ViewBag.Error = "Bu TC Kimlik Numarasına ait doktor kaydı bulunamadı.";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "İşlem sırasında bir hata oluştu: " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult SekreterSifremiUnuttum()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SekreterSifremiUnuttum(string tc)
        {
            if (string.IsNullOrEmpty(tc))
            {
                ViewBag.Error = "Lütfen TC Kimlik Numaranızı giriniz.";
                return View();
            }

            string tcHash = SecurityHelper.Hashle(tc);
            using var conn = _bgl.baglanti();
            try
            {
                using var cmd = new NpgsqlCommand("SELECT sifre_sifreli FROM Sekreterler WHERE tc_hash = @p1", conn);
                cmd.Parameters.AddWithValue("@p1", tcHash);

                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    string sifre = SecurityHelper.Coz(result.ToString());
                    ViewBag.Mesaj = $"Åifreniz: {sifre}";
                    return View();
                }

                ViewBag.Error = "Bu TC Kimlik Numarasına ait sekreter kaydı bulunamadı.";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "İşlem sırasında bir hata oluştu: " + ex.Message;
                return View();
            }
        }

        // ======================== ÇIKIÅ ========================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Message"] = "Sistemden başarıyla çıkış yaptınız.";
            return RedirectToAction("Index", "Home");
        }
    }
}
