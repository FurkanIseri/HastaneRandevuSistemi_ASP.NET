using Microsoft.AspNetCore.Mvc;
using HastaneYonetimSistemi.Models;
using HastaneYonetimSistemi.Helpers;
using Npgsql;
using NpgsqlTypes;

namespace HastaneYonetimSistemi.Controllers
{
    public class SekreterController : Controller
    {
        private readonly sqlBaglanti _bgl;
        public SekreterController(IConfiguration configuration) => _bgl = new sqlBaglanti(configuration);
        private string? TC => HttpContext.Session.GetString("SekreterTC");
        private IActionResult RedirectIfNoSession() => RedirectToAction("SekreterGiris", "Auth");

        // ================================================================
        // ANA PANEL
        // ================================================================
        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            ViewBag.TC = TC;
            using var conn = _bgl.baglanti();
            try
            {
                var hash = SecurityHelper.Hashle(TC!);
                using var cmd = new NpgsqlCommand("SELECT sekreter_ad, sekreter_soyad FROM Sekreterler WHERE tc_hash=@p1", conn);
                cmd.Parameters.AddWithValue("@p1", hash);
                using var dr = cmd.ExecuteReader();
                if (dr.Read()) ViewBag.AdSoyad = dr[0] + " " + dr[1];
                dr.Close();

                ViewBag.Branslar = GetBranslarList(conn);
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
            var doktorlar = new List<DoktorListeItem>();
            using var conn = _bgl.baglanti();
            try
            {
                // Fetch current secretary ID
                var sekHash = SecurityHelper.Hashle(TC!);
                Guid mySekreterId = Guid.Empty;
                using (var cmdSek = new NpgsqlCommand("SELECT sekreter_id FROM sekreterler WHERE tc_hash=@p1", conn))
                {
                    cmdSek.Parameters.AddWithValue("@p1", sekHash);
                    var r = cmdSek.ExecuteScalar();
                    if (r != null) mySekreterId = Guid.Parse(r.ToString()!);
                }
                ViewBag.CurrentSekreterId = mySekreterId.ToString();

                using var cmd = new NpgsqlCommand(
                    "SELECT d.doktor_id, d.doktor_ad, d.doktor_soyad, b.brans_ad, d.profil_fotografi, d.sekreter_id " +
                    "FROM Doktorlar d JOIN Branslar b ON d.brans_id = b.brans_id ORDER BY b.brans_ad, d.doktor_soyad", conn);
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                    doktorlar.Add(new DoktorListeItem
                    {
                        DoktorId = dr["doktor_id"].ToString()!,
                        AdSoyad = dr["doktor_ad"] + " " + dr["doktor_soyad"],
                        Brans = dr["brans_ad"].ToString()!,
                        SekreterId = dr["sekreter_id"]?.ToString() ?? "",
                        ProfilFotografi = dr["profil_fotografi"] == DBNull.Value ? null : (byte[])dr["profil_fotografi"]
                    });
            }
            catch (Exception ex) { ViewBag.Error = "Hata: " + ex.Message; }
            return View(doktorlar);
        }

        [HttpGet]
        public IActionResult DoktorEkle()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            using var conn = _bgl.baglanti();
            ViewBag.Branslar = GetBranslarList(conn);
            return View(new DoktorEkleViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> DoktorEkle(DoktorEkleViewModel model)
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            using var conn = _bgl.baglanti();
            ViewBag.Branslar = GetBranslarList(conn);
            if (!ModelState.IsValid) return View(model);
            try
            {
                int bransId = GetBransId(conn, model.Brans);
                if (bransId == 0) { ModelState.AddModelError("Brans", "Branş bulunamadı."); return View(model); }

                // Fetch the logged-in secretary's ID
                var sekHash = SecurityHelper.Hashle(TC!);
                Guid sekreterIdVal = Guid.Empty;
                using (var cmdSek = new NpgsqlCommand("SELECT sekreter_id FROM sekreterler WHERE tc_hash=@p1", conn))
                {
                    cmdSek.Parameters.AddWithValue("@p1", sekHash);
                    var sekRes = cmdSek.ExecuteScalar();
                    if (sekRes != null) sekreterIdVal = Guid.Parse(sekRes.ToString()!);
                }
                if (sekreterIdVal == Guid.Empty)
                {
                    ModelState.AddModelError("", "Oturumdaki sekreter bulunamadı.");
                    return View(model);
                }

                byte[]? foto = null;
                if (model.ProfilFotografi?.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await model.ProfilFotografi.CopyToAsync(ms);
                    foto = ms.ToArray();
                }

                using var cmd = new NpgsqlCommand(
                    "INSERT INTO doktorlar (doktor_ad, doktor_soyad, tc_hash, tc_sifreli, sifre_hash, sifre_sifreli, brans_id, sekreter_id, profil_fotografi, cinsiyet) " +
                    "VALUES (@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10)", conn);
                cmd.Parameters.AddWithValue("@p1", model.Ad);
                cmd.Parameters.AddWithValue("@p2", model.Soyad);
                cmd.Parameters.AddWithValue("@p3", SecurityHelper.Hashle(model.TC));
                cmd.Parameters.AddWithValue("@p4", SecurityHelper.Sifrele(model.TC));
                cmd.Parameters.AddWithValue("@p5", SecurityHelper.Hashle(model.Sifre));
                cmd.Parameters.AddWithValue("@p6", SecurityHelper.Sifrele(model.Sifre));
                cmd.Parameters.AddWithValue("@p7", bransId);
                cmd.Parameters.AddWithValue("@p8", sekreterIdVal);
                cmd.Parameters.Add(new NpgsqlParameter("@p9", NpgsqlDbType.Bytea) { Value = foto != null ? (object)foto : DBNull.Value });
                cmd.Parameters.AddWithValue("@p10", model.Cinsiyet);
                cmd.ExecuteNonQuery();

                TempData["Message"] = $"Dr. {model.Ad} {model.Soyad} başarıyla eklendi.";
                return RedirectToAction("DoktorListesi");
            }
            catch (PostgresException ex) { ModelState.AddModelError("", "Veritabanı hatası: " + ex.MessageText); return View(model); }
            catch (Exception ex) { ModelState.AddModelError("", "Hata: " + ex.Message); return View(model); }
        }

        [HttpPost]
        public IActionResult DoktorSil(string doktorId)
        {
            if (string.IsNullOrEmpty(TC)) return Json(new { success = false, message = "Oturum süresi doldu." });
            if (!Guid.TryParse(doktorId, out var id)) return Json(new { success = false, message = "Geçersiz ID." });
            using var conn = _bgl.baglanti();
            try
            {
                // Verify ownership
                var sekHash = SecurityHelper.Hashle(TC!);
                Guid mySekreterId = Guid.Empty;
                using (var cmdSek = new NpgsqlCommand("SELECT sekreter_id FROM sekreterler WHERE tc_hash=@p1", conn))
                {
                    cmdSek.Parameters.AddWithValue("@p1", sekHash);
                    var r = cmdSek.ExecuteScalar();
                    if (r != null) mySekreterId = Guid.Parse(r.ToString()!);
                }

                Guid ownerId = Guid.Empty;
                using (var cmdCheck = new NpgsqlCommand("SELECT sekreter_id FROM doktorlar WHERE doktor_id=@p1", conn))
                {
                    cmdCheck.Parameters.AddWithValue("@p1", id);
                    var r = cmdCheck.ExecuteScalar();
                    if (r != null && r != DBNull.Value) ownerId = Guid.Parse(r.ToString()!);
                }

                if (ownerId != mySekreterId)
                {
                    return Json(new { success = false, message = "Yetkisiz işlem: Bu doktoru sadece sisteme kaydeden sekreter silebilir." });
                }

                using var cmd = new NpgsqlCommand("DELETE FROM Doktorlar WHERE doktor_id=@p1", conn);
                cmd.Parameters.AddWithValue("@p1", id);
                cmd.ExecuteNonQuery();
                return Json(new { success = true, message = "Doktor silindi." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // ================================================================
        // BRANŞ YÖNETİMİ
        // ================================================================
        [HttpGet]
        public IActionResult BransYonetimi()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            var branslar = new List<BransViewModel>();
            using var conn = _bgl.baglanti();
            try
            {
                using var cmd = new NpgsqlCommand("SELECT brans_id, brans_ad FROM Branslar ORDER BY brans_ad", conn);
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                    branslar.Add(new BransViewModel { BransId = Convert.ToInt32(dr[0]), BransAd = dr[1].ToString()! });
            }
            catch (Exception ex) { ViewBag.Error = "Hata: " + ex.Message; }
            return View(branslar);
        }

        [HttpPost]
        public IActionResult BransEkle(string bransAd)
        {
            if (string.IsNullOrEmpty(TC)) return Json(new { success = false, message = "Oturum süresi doldu." });
            if (string.IsNullOrWhiteSpace(bransAd)) return Json(new { success = false, message = "Branş adı boş olamaz." });
            using var conn = _bgl.baglanti();
            try
            {
                using (var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM branslar WHERE LOWER(TRIM(brans_ad)) = LOWER(TRIM(@p1))", conn))
                {
                    checkCmd.Parameters.AddWithValue("@p1", bransAd);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count > 0)
                    {
                        return Json(new { success = false, message = "Bu isimde bir branş zaten sistemde kayıtlı." });
                    }
                }

                using var cmd = new NpgsqlCommand("INSERT INTO Branslar (brans_ad) VALUES (@p1)", conn);
                cmd.Parameters.AddWithValue("@p1", bransAd.Trim());
                cmd.ExecuteNonQuery();
                return Json(new { success = true, message = $"'{bransAd}' branşı eklendi." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public IActionResult BransSil(int bransId)
        {
            if (string.IsNullOrEmpty(TC)) return Json(new { success = false, message = "Oturum süresi doldu." });
            using var conn = _bgl.baglanti();
            try
            {
                using var cmd = new NpgsqlCommand("DELETE FROM Branslar WHERE brans_id=@p1", conn);
                cmd.Parameters.AddWithValue("@p1", bransId);
                cmd.ExecuteNonQuery();
                return Json(new { success = true, message = "Branş silindi." });
            }
            catch (PostgresException ex) { return Json(new { success = false, message = "Silinemedi: Bu branşa bağlı kayıtlar mevcut." }); }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // ================================================================
        // İLAÇ YÖNETİMİ
        // ================================================================
        [HttpGet]
        public IActionResult IlacYonetimi()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            var ilaclar = new List<IlacViewModel>();
            using var conn = _bgl.baglanti();
            try
            {
                using var cmd = new NpgsqlCommand("SELECT ilac_id, ilac_ad, stok_adet FROM ilaclar ORDER BY ilac_ad", conn);
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                    ilaclar.Add(new IlacViewModel
                    {
                        IlacID = Convert.ToInt32(dr[0]),
                        IlacAd = dr[1].ToString()!,
                        StokAdet = Convert.ToInt32(dr[2])
                    });
            }
            catch (Exception ex) { ViewBag.Error = "Hata: " + ex.Message; }
            return View(ilaclar);
        }

        [HttpPost]
        public IActionResult IlacEkle(string ilacAd, int stokAdet)
        {
            if (string.IsNullOrEmpty(TC)) return Json(new { success = false, message = "Oturum süresi doldu." });
            if (string.IsNullOrWhiteSpace(ilacAd)) return Json(new { success = false, message = "İlaç adı boş olamaz." });
            using var conn = _bgl.baglanti();
            try
            {
                // Check for duplicate name (case-insensitive)
                using (var cmdCheck = new NpgsqlCommand("SELECT COUNT(*) FROM ilaclar WHERE LOWER(ilac_ad) = LOWER(@p1)", conn))
                {
                    cmdCheck.Parameters.AddWithValue("@p1", ilacAd.Trim());
                    long count = Convert.ToInt64(cmdCheck.ExecuteScalar());
                    if (count > 0) return Json(new { success = false, message = "Bu isimde bir ilaç zaten sistemde kayıtlı." });
                }

                using var cmd = new NpgsqlCommand("INSERT INTO ilaclar (ilac_ad, stok_adet) VALUES (@p1,@p2)", conn);
                cmd.Parameters.AddWithValue("@p1", ilacAd.Trim());
                cmd.Parameters.AddWithValue("@p2", stokAdet);
                cmd.ExecuteNonQuery();
                return Json(new { success = true, message = $"'{ilacAd}' ilacı eklendi." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public IActionResult IlacSil(int ilacId)
        {
            if (string.IsNullOrEmpty(TC)) return Json(new { success = false, message = "Oturum süresi doldu." });
            using var conn = _bgl.baglanti();
            try
            {
                using var cmd = new NpgsqlCommand("DELETE FROM Ilaclar WHERE ilac_id=@p1", conn);
                cmd.Parameters.AddWithValue("@p1", ilacId);
                cmd.ExecuteNonQuery();
                return Json(new { success = true, message = "İlaç silindi." });
            }
            catch (PostgresException) { return Json(new { success = false, message = "Silinemedi: Bu ilaca bağlı reçeteler mevcut." }); }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // ================================================================
        // SEKRETER YÖNETİMİ
        // ================================================================
        [HttpGet]
        public IActionResult SekreterYonetimi()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            var sekreterler = new List<SekreterListeItem>();
            using var conn = _bgl.baglanti();
            try
            {
                using var cmd = new NpgsqlCommand("SELECT sekreter_id, sekreter_ad, sekreter_soyad FROM Sekreterler ORDER BY sekreter_soyad", conn);
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                    sekreterler.Add(new SekreterListeItem
                    {
                        SekreterID = dr["sekreter_id"].ToString()!,
                        AdSoyad = dr["sekreter_ad"] + " " + dr["sekreter_soyad"]
                    });
            }
            catch (Exception ex) { ViewBag.Error = "Hata: " + ex.Message; }
            return View(sekreterler);
        }

        [HttpPost]
        public IActionResult SekreterEkle(string ad, string soyad, string tc, string sifre, string cinsiyet)
        {
            if (string.IsNullOrEmpty(TC)) return Json(new { success = false, message = "Oturum süresi doldu." });
            if (string.IsNullOrWhiteSpace(ad) || string.IsNullOrWhiteSpace(soyad) ||
                string.IsNullOrWhiteSpace(tc) || string.IsNullOrWhiteSpace(sifre) || string.IsNullOrWhiteSpace(cinsiyet))
                return Json(new { success = false, message = "Tüm alanlar (Cinsiyet dahil) zorunludur." });
            if (tc.Length != 11) return Json(new { success = false, message = "TC Kimlik No 11 haneli olmalıdır." });

            using var conn = _bgl.baglanti();
            try
            {
                using var cmd = new NpgsqlCommand(
                    "INSERT INTO Sekreterler (sekreter_ad, sekreter_soyad, tc_hash, tc_sifreli, sifre_hash, sifre_sifreli, cinsiyet) " +
                    "VALUES (@p1,@p2,@p3,@p4,@p5,@p6,@p7)", conn);
                cmd.Parameters.AddWithValue("@p1", ad.Trim());
                cmd.Parameters.AddWithValue("@p2", soyad.Trim());
                cmd.Parameters.AddWithValue("@p3", SecurityHelper.Hashle(tc));
                cmd.Parameters.AddWithValue("@p4", SecurityHelper.Sifrele(tc));
                cmd.Parameters.AddWithValue("@p5", SecurityHelper.Hashle(sifre));
                cmd.Parameters.AddWithValue("@p6", SecurityHelper.Sifrele(sifre));
                cmd.Parameters.AddWithValue("@p7", cinsiyet);
                cmd.ExecuteNonQuery();
                return Json(new { success = true, message = $"{ad} {soyad} sekreter olarak eklendi." });
            }
            catch (PostgresException ex) { return Json(new { success = false, message = "Veritabanı hatası: " + ex.MessageText }); }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
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
            if (string.IsNullOrWhiteSpace(duyuru)) return Json(new { success = false, message = "Duyuru boş olamaz." });
            using var conn = _bgl.baglanti();
            try
            {
                // Fetch the logged-in secretary's ID
                var hash = SecurityHelper.Hashle(TC!);
                Guid sekreterIdForDuyuru = Guid.Empty;
                using (var cmdSek = new NpgsqlCommand("SELECT sekreter_id FROM sekreterler WHERE tc_hash=@p1", conn))
                {
                    cmdSek.Parameters.AddWithValue("@p1", hash);
                    var sekRes = cmdSek.ExecuteScalar();
                    if (sekRes != null) sekreterIdForDuyuru = Guid.Parse(sekRes.ToString()!);
                }
                using var cmd = new NpgsqlCommand("INSERT INTO duyurular (duyurular_text, sekreter_id) VALUES (@p1, @p2)", conn);
                cmd.Parameters.AddWithValue("@p1", duyuru);
                cmd.Parameters.AddWithValue("@p2", sekreterIdForDuyuru);
                cmd.ExecuteNonQuery();
                return Json(new { success = true, message = "Duyuru yayınlandı." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // ================================================================
        // RANDEVU YUVASI
        // ================================================================
        [HttpPost]
        public IActionResult RandevuOlustur(string tarih, string saat, string brans, string doktorId)
        {
            string tc = HttpContext.Session.GetString("SekreterTC");

            if (string.IsNullOrEmpty(tc)) return Json(new { success = false, message = "Oturum süresi doldu." });
            if (!Guid.TryParse(doktorId, out var dId)) return Json(new { success = false, message = "Geçersiz doktor ID." });

            using var conn = _bgl.baglanti();
            try
            {
                int bransId = GetBransId(conn, brans);
                using var cmd = new NpgsqlCommand("CALL sp_randevuolustur(@p1, @p2, @p3, @p4, @p5)", conn);
                cmd.Parameters.AddWithValue("@p1", NpgsqlTypes.NpgsqlDbType.Timestamp, DateTime.Parse(tarih.ToString()));
                cmd.Parameters.AddWithValue("@p2", NpgsqlTypes.NpgsqlDbType.Varchar, saat.ToString());
                cmd.Parameters.AddWithValue("@p3", bransId);
                cmd.Parameters.AddWithValue("@p4", dId);
                cmd.Parameters.AddWithValue("@p5", false);

                cmd.ExecuteNonQuery();

                return Json(new { success = true, message = "Randevu yuvası başarıyla oluşturuldu." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Veritabanı Hatası: " + ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetDoktorlarByBrans(string brans)
        {
            var doktorlar = new List<object>();
            using var conn = _bgl.baglanti();
            try
            {
                using var cmd = new NpgsqlCommand(
                    "SELECT d.doktor_id, (d.doktor_ad || ' ' || d.doktor_soyad) AS AdSoyad " +
                    "FROM Doktorlar d JOIN Branslar b ON d.brans_id = b.brans_id WHERE b.brans_ad=@p1", conn);
                cmd.Parameters.AddWithValue("@p1", brans ?? "");
                using var dr = cmd.ExecuteReader();
                while (dr.Read()) doktorlar.Add(new { id = dr[0].ToString(), ad = dr[1].ToString() });
            }
            catch { }
            return Json(doktorlar);
        }

        // ================================================================
        // BİLGİ DÜZENLE (Sekreter – Şifre Güncelleme)
        // ================================================================
        [HttpGet]
        public IActionResult BilgiDuzenle()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            var model = new SekreterBilgiViewModel();
            using var conn = _bgl.baglanti();
            try
            {
                var hash = SecurityHelper.Hashle(TC!);
                using var cmd = new NpgsqlCommand("SELECT sekreter_ad, sekreter_soyad FROM sekreterler WHERE tc_hash=@p1", conn);
                cmd.Parameters.AddWithValue("@p1", hash);
                using var dr = cmd.ExecuteReader();
                if (dr.Read()) model.AdSoyad = dr[0] + " " + dr[1];
            }
            catch { }
            return View(model);
        }

        [HttpPost]
        public IActionResult BilgiDuzenle(SekreterBilgiViewModel model)
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            if (!string.IsNullOrEmpty(model.YeniSifre) && model.YeniSifre != model.SifreTekrar)
            {
                ModelState.AddModelError("SifreTekrar", "Şifreler eşleşmiyor.");
                return View(model);
            }
            using var conn = _bgl.baglanti();
            try
            {
                var hash = SecurityHelper.Hashle(TC!);
                if (!string.IsNullOrEmpty(model.YeniSifre))
                {
                    using var cmd = new NpgsqlCommand(
                        "UPDATE sekreterler SET sifre_hash=@p1, sifre_sifreli=@p2 WHERE tc_hash=@p3", conn);
                    cmd.Parameters.AddWithValue("@p1", SecurityHelper.Hashle(model.YeniSifre));
                    cmd.Parameters.AddWithValue("@p2", SecurityHelper.Sifrele(model.YeniSifre));
                    cmd.Parameters.AddWithValue("@p3", hash);
                    cmd.ExecuteNonQuery();
                }
                TempData["Message"] = "Şifreniz başarıyla güncellendi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex) { ModelState.AddModelError("", "Hata: " + ex.Message); return View(model); }
        }

        // ================================================================
        // DOKTOR GÜNCELLE (Sadece kaydeden sekreter)
        // ================================================================
        [HttpGet]
        public IActionResult DoktorDuzenle(string doktorId)
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            if (!Guid.TryParse(doktorId, out var dId))
            {
                TempData["Error"] = "Geçersiz doktor ID.";
                return RedirectToAction("DoktorListesi");
            }

            using var conn = _bgl.baglanti();
            try
            {
                // Get logged-in secretary's ID
                var sekHash = SecurityHelper.Hashle(TC!);
                Guid mySekreterIdVal = Guid.Empty;
                using (var cmdMySek = new NpgsqlCommand("SELECT sekreter_id FROM sekreterler WHERE tc_hash=@p1", conn))
                {
                    cmdMySek.Parameters.AddWithValue("@p1", sekHash);
                    var r = cmdMySek.ExecuteScalar();
                    if (r != null) mySekreterIdVal = Guid.Parse(r.ToString()!);
                }

                // Materialize doctor data first to avoid "Command in progress" concurrency issues
                string? ad = null, soyad = null, brans = null;
                Guid doktorSekreterIdVal = Guid.Empty;
                bool found = false;

                using (var cmdD = new NpgsqlCommand(
                    "SELECT d.doktor_ad, d.doktor_soyad, b.brans_ad, d.sekreter_id " +
                    "FROM doktorlar d JOIN branslar b ON d.brans_id = b.brans_id WHERE d.doktor_id=@p1", conn))
                {
                    cmdD.Parameters.AddWithValue("@p1", dId);
                    using (var drD = cmdD.ExecuteReader())
                    {
                        if (drD.Read())
                        {
                            found = true;
                            ad = drD["doktor_ad"].ToString();
                            soyad = drD["doktor_soyad"].ToString();
                            brans = drD["brans_ad"].ToString();
                            doktorSekreterIdVal = drD["sekreter_id"] == DBNull.Value ? Guid.Empty : Guid.Parse(drD["sekreter_id"].ToString()!);
                        }
                    } // Reader drD is closed here
                }

                if (!found)
                {
                    TempData["Error"] = "Doktor bulunamadı.";
                    return RedirectToAction("DoktorListesi");
                }

                // Check authorization
                if (doktorSekreterIdVal != mySekreterIdVal)
                {
                    ViewBag.Error = "Sadece bu doktoru sisteme kaydeden sekreter güncelleme yapabilir.";
                    ViewBag.Branslar = GetBranslarList(conn);
                    return View(new DoktorGuncelleViewModel { DoktorId = doktorId });
                }

                // Success path
                var guncelleModel = new DoktorGuncelleViewModel
                {
                    DoktorId = doktorId,
                    Ad = ad!,
                    Soyad = soyad!,
                    Brans = brans!
                };
                ViewBag.Branslar = GetBranslarList(conn);
                return View(guncelleModel);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Hata: " + ex.Message;
                ViewBag.Branslar = new List<string>();
                return View(new DoktorGuncelleViewModel { DoktorId = doktorId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DoktorDuzenle(DoktorGuncelleViewModel model)
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            if (!Guid.TryParse(model.DoktorId, out var dId))
            {
                ModelState.AddModelError("", "Geçersiz doktor ID.");
                return View(model);
            }

            using var conn = _bgl.baglanti();
            ViewBag.Branslar = GetBranslarList(conn);

            try
            {
                // Verify ownership
                var sekHash = SecurityHelper.Hashle(TC!);
                Guid mySekreterIdVal = Guid.Empty;
                using (var cmdMySek = new NpgsqlCommand("SELECT sekreter_id FROM sekreterler WHERE tc_hash=@p1", conn))
                {
                    cmdMySek.Parameters.AddWithValue("@p1", sekHash);
                    var r = cmdMySek.ExecuteScalar();
                    if (r != null) mySekreterIdVal = Guid.Parse(r.ToString()!);
                }

                Guid doktorSekreterIdVal = Guid.Empty;
                using (var cmdCheck = new NpgsqlCommand("SELECT sekreter_id FROM doktorlar WHERE doktor_id=@p1", conn))
                {
                    cmdCheck.Parameters.AddWithValue("@p1", dId);
                    var r = cmdCheck.ExecuteScalar();
                    if (r != null && r != DBNull.Value) doktorSekreterIdVal = Guid.Parse(r.ToString()!);
                }

                if (doktorSekreterIdVal != mySekreterIdVal)
                {
                    ViewBag.Error = "Sadece bu doktoru sisteme kaydeden sekreter güncelleme yapabilir.";
                    return View(model);
                }

                if (!ModelState.IsValid) return View(model);

                int bransId = GetBransId(conn, model.Brans);
                if (bransId == 0) { ModelState.AddModelError("Brans", "Branş bulunamadı."); return View(model); }

                // Update basic info
                using (var cmdU = new NpgsqlCommand(
                    "UPDATE doktorlar SET doktor_ad=@p1, doktor_soyad=@p2, brans_id=@p3 WHERE doktor_id=@p4", conn))
                {
                    cmdU.Parameters.AddWithValue("@p1", model.Ad);
                    cmdU.Parameters.AddWithValue("@p2", model.Soyad);
                    cmdU.Parameters.AddWithValue("@p3", bransId);
                    cmdU.Parameters.AddWithValue("@p4", dId);
                    cmdU.ExecuteNonQuery();
                }

                // Update password if provided
                if (!string.IsNullOrEmpty(model.YeniSifre))
                {
                    using var cmdSifre = new NpgsqlCommand(
                        "UPDATE doktorlar SET sifre_hash=@p1, sifre_sifreli=@p2 WHERE doktor_id=@p3", conn);
                    cmdSifre.Parameters.AddWithValue("@p1", SecurityHelper.Hashle(model.YeniSifre));
                    cmdSifre.Parameters.AddWithValue("@p2", SecurityHelper.Sifrele(model.YeniSifre));
                    cmdSifre.Parameters.AddWithValue("@p3", dId);
                    cmdSifre.ExecuteNonQuery();
                }

                // Update profile photo if provided
                if (model.ProfilFotografi?.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await model.ProfilFotografi.CopyToAsync(ms);
                    var foto = ms.ToArray();
                    using var cmdFoto = new NpgsqlCommand(
                        "UPDATE doktorlar SET profil_fotografi=@p1 WHERE doktor_id=@p2", conn);
                    cmdFoto.Parameters.Add(new NpgsqlParameter("@p1", NpgsqlDbType.Bytea) { Value = foto });
                    cmdFoto.Parameters.AddWithValue("@p2", dId);
                    cmdFoto.ExecuteNonQuery();
                }

                TempData["Message"] = $"Dr. {model.Ad} {model.Soyad} bilgileri güncellendi.";
                return RedirectToAction("DoktorListesi");
            }
            catch (PostgresException ex) { ModelState.AddModelError("", "Veritabanı hatası: " + ex.MessageText); return View(model); }
            catch (Exception ex) { ModelState.AddModelError("", "Hata: " + ex.Message); return View(model); }
        }

        // ================================================================
        // HELPERS
        // ================================================================
        private List<string> GetBranslarList(NpgsqlConnection conn)
        {
            var list = new List<string>();
            using var cmd = new NpgsqlCommand("SELECT brans_ad FROM Branslar ORDER BY brans_ad", conn);
            using var dr = cmd.ExecuteReader();
            while (dr.Read()) list.Add(dr[0].ToString()!);
            return list;
        }

        private int GetBransId(NpgsqlConnection conn, string bransAd)
        {
            using var cmd = new NpgsqlCommand("SELECT brans_id FROM Branslar WHERE brans_ad=@p1", conn);
            cmd.Parameters.AddWithValue("@p1", bransAd);
            var res = cmd.ExecuteScalar();
            return res != null ? Convert.ToInt32(res) : 0;
        }
    }
}
