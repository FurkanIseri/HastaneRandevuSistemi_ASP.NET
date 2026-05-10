using Microsoft.AspNetCore.Mvc;
using HastaneYonetimSistemi.Models;
using HastaneYonetimSistemi.Helpers;
using Npgsql;
using NpgsqlTypes;

namespace HastaneYonetimSistemi.Controllers
{
    public class HastaController : Controller
    {
        private readonly sqlBaglanti _bgl;
        public HastaController(IConfiguration configuration) => _bgl = new sqlBaglanti(configuration);

        private string? TC => HttpContext.Session.GetString("HastaTC");
        private IActionResult RedirectIfNoSession() => RedirectToAction("HastaGiris", "Auth");

        // ================================================================
        // PANEL ANA SAYFA
        // ================================================================
        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            ViewBag.TC = TC;

            using var conn = _bgl.baglanti();
            try
            {
                var hash = SecurityHelper.Hashle(TC!);
                using var cmdAd = new NpgsqlCommand("SELECT hasta_ad, hasta_soyad FROM Hastalar WHERE tc_hash=@p1", conn);
                cmdAd.Parameters.AddWithValue("@p1", hash);
                using var dr = cmdAd.ExecuteReader();
                if (dr.Read()) ViewBag.AdSoyad = dr[0] + " " + dr[1];
                dr.Close();

                // Geçmiş Randevular
                var gecmis = new List<RandevuViewModel>();
                using var cmdG = new NpgsqlCommand(@"
                    SELECT b.brans_ad AS Brans, (d.doktor_ad||' '||d.doktor_soyad) AS Doktor,
                           r.randevu_tarih AS Tarih, r.randevu_saat AS Saat, r.hasta_sikayet AS Sikayet
                    FROM Randevular r
                    JOIN Hastalar h ON r.hasta_id = h.hasta_id
                    JOIN Doktorlar d ON r.doktor_id = d.doktor_id
                    JOIN Branslar b ON r.brans_id = b.brans_id
                    WHERE h.tc_hash = @p1 ORDER BY r.randevu_tarih DESC", conn);
                cmdG.Parameters.AddWithValue("@p1", hash);
                using var drG = cmdG.ExecuteReader();

                while (drG.Read())
                {
                    gecmis.Add(new RandevuViewModel
                    {
                        Brans = drG["Brans"].ToString()!,
                        Doktor = drG["Doktor"].ToString()!,
                        // DateOnly hatasını çözen güvenli dönüşüm:
                        Tarih = DateTime.Parse(drG["Tarih"].ToString()!),
                        // Saat verisindeki hatayı çözen dönüşüm:
                        Saat = TimeSpan.Parse(drG["Saat"].ToString()!),
                        Sikayet = drG["Sikayet"]?.ToString() ?? ""
                    });
                }
                drG.Close();
                ViewBag.GecmisRandevular = gecmis;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Ana sayfa yüklenirken hata: " + ex.Message;
            }
            return View();
        }

        // ================================================================
        // AJAX ENDPOINTS (Açılır Listeler ve Tablolar için)
        // ================================================================
        [HttpGet]
        public JsonResult GetBranslar()
        {
            var list = new List<string>();
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("SELECT DISTINCT brans_ad FROM Branslar ORDER BY brans_ad", conn);
            using var dr = cmd.ExecuteReader();
            while (dr.Read()) list.Add(dr[0].ToString()!);
            return Json(list);
        }

        [HttpGet]
        public JsonResult GetDoktorlar(string brans)
        {
            var list = new List<object>();
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand(
                "SELECT d.doktor_id, (d.doktor_ad||' '||d.doktor_soyad) FROM Doktorlar d " +
                "JOIN Branslar b ON d.brans_id=b.brans_id WHERE b.brans_ad=@p1", conn);
            cmd.Parameters.AddWithValue("@p1", brans ?? "");
            using var dr = cmd.ExecuteReader();
            while (dr.Read()) list.Add(new { id = dr[0].ToString(), ad = dr[1].ToString() });
            return Json(list);
        }

        [HttpGet]
        public JsonResult GetAktifRandevular(string brans = "", string doktorId = "")
        {
            var list = new List<RandevuViewModel>();
            if (string.IsNullOrEmpty(doktorId) || !Guid.TryParse(doktorId, out var dId))
                return Json(list);

            using var conn = _bgl.baglanti();
            try
            {
                using var cmd = new NpgsqlCommand(@"
                    SELECT r.randevu_id AS ID, b.brans_ad AS Brans,
                           (d.doktor_ad||' '||d.doktor_soyad) AS Doktor,
                           r.randevu_tarih AS Tarih, r.randevu_saat AS Saat
                    FROM Randevular r
                    JOIN Doktorlar d ON r.doktor_id = d.doktor_id
                    JOIN Branslar b ON r.brans_id = b.brans_id
                    WHERE r.randevu_durum = FALSE
                      AND r.doktor_id = @p1
                      AND r.randevu_tarih >= CURRENT_DATE
                    ORDER BY r.randevu_tarih, r.randevu_saat", conn);

                cmd.Parameters.AddWithValue("@p1", dId);
                using var dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(new RandevuViewModel
                    {
                        ID = Convert.ToInt32(dr["ID"]),
                        Brans = dr["Brans"].ToString()!,
                        Doktor = dr["Doktor"].ToString()!,
                        // DateOnly hatasını çözen güvenli dönüşüm:
                        Tarih = DateTime.Parse(dr["Tarih"].ToString()!),
                        // Saat verisindeki hatayı çözen dönüşüm:
                        Saat = TimeSpan.Parse(dr["Saat"].ToString()!)
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Aktif randevular getirilirken hata oluştu: " + ex.Message);
            }
            return Json(list);
        }

        // ================================================================
        // RANDEVU AL (Tıbbi Belge Yükleme İçerir)
        // ================================================================
        [HttpPost]
        public async Task<IActionResult> RandevuAl([FromForm] RandevuAlViewModel model)
        {
            if (string.IsNullOrEmpty(TC))
                return Json(new { success = false, message = "Oturum süresi doldu." });

            if (model.TibbiBelge == null || model.TibbiBelge.Length == 0)
                return Json(new { success = false, message = "Lütfen tıbbi belgenizi yükleyiniz." });

            if (model.RandevuId <= 0)
                return Json(new { success = false, message = "Geçersiz randevu seçimi." });

            byte[] belge;
            using (var ms = new MemoryStream())
            {
                await model.TibbiBelge.CopyToAsync(ms);
                belge = ms.ToArray();
            }

            using var conn = _bgl.baglanti();
            try
            {
                var hash = SecurityHelper.Hashle(TC!);
                var hastaId = Guid.Empty;
                using (var cmd = new NpgsqlCommand("SELECT hasta_id FROM Hastalar WHERE tc_hash=@p1", conn))
                {
                    cmd.Parameters.AddWithValue("@p1", hash);
                    var res = cmd.ExecuteScalar();
                    if (res != null) hastaId = Guid.Parse(res.ToString()!);
                }

                using var cmdU = new NpgsqlCommand(
                    "UPDATE Randevular SET randevu_durum=TRUE, hasta_id=@p1, hasta_sikayet=@p2, tibbi_belge=@p3 WHERE randevu_id=@p4", conn);
                cmdU.Parameters.AddWithValue("@p1", hastaId);
                cmdU.Parameters.AddWithValue("@p2", model.Sikayet ?? "");
                cmdU.Parameters.Add(new NpgsqlParameter("@p3", NpgsqlDbType.Bytea) { Value = belge });
                cmdU.Parameters.AddWithValue("@p4", model.RandevuId);
                cmdU.ExecuteNonQuery();

                return Json(new { success = true, message = "Randevunuz ve tıbbi belgeniz başarıyla kaydedildi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Veritabanı Hatası: " + ex.Message });
            }
        }

        // ================================================================
        // REÇETELERİM
        // ================================================================
        public IActionResult Receteler()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            var receteler = new List<ReceteViewModel>();
            using var conn = _bgl.baglanti();
            try
            {
                var hash = SecurityHelper.Hashle(TC!);
                using var cmd = new NpgsqlCommand(@"
                    SELECT r.recete_id, r.tani_teshis, i.ilac_id, i.ilac_ad, ri.kullanim_sekli,
                           COALESCE(ri.adet, 1) AS adet
                    FROM receteler r
                    JOIN randevular rn ON r.randevu_id = rn.randevu_id
                    JOIN hastalar h ON rn.hasta_id = h.hasta_id
                    JOIN recete_ilac ri ON r.recete_id = ri.recete_id
                    JOIN ilaclar i ON ri.ilac_id = i.ilac_id
                    WHERE h.tc_hash = @p1", conn);
                cmd.Parameters.AddWithValue("@p1", hash);
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    receteler.Add(new ReceteViewModel
                    {
                        ReceteID = Convert.ToInt32(dr["recete_id"]),
                        Tani = dr["tani_teshis"].ToString()!,
                        IlacID = Convert.ToInt32(dr["ilac_id"]),
                        IlacAd = dr["ilac_ad"].ToString()!,
                        KullanimTalimati = dr["kullanim_sekli"].ToString()!,
                        Adet = Convert.ToInt32(dr["adet"])
                    });
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Reçeteler yüklenirken hata: " + ex.Message;
            }
            return View(receteler);
        }

        // ================================================================
        // BİLGİ DÜZENLE (Telefon + Şifre)
        // ================================================================
        [HttpGet]
        public IActionResult BilgiDuzenle()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            var model = new HastaBilgiViewModel();
            using var conn = _bgl.baglanti();
            try
            {
                var hash = SecurityHelper.Hashle(TC!);
                using var cmd = new NpgsqlCommand("SELECT hasta_ad, hasta_soyad, telefon_no FROM Hastalar WHERE tc_hash=@p1", conn);
                cmd.Parameters.AddWithValue("@p1", hash);
                using var dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    model.Ad = dr[0].ToString()!;
                    model.Soyad = dr[1].ToString()!;
                    model.Telefon = dr[2].ToString()!;
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Bilgileriniz getirilirken hata: " + ex.Message;
            }
            return View(model);
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

            using var conn = _bgl.baglanti();
            try
            {
                var hash = SecurityHelper.Hashle(TC!);
                // Telefon güncelleme
                using (var cmd = new NpgsqlCommand("UPDATE Hastalar SET telefon_no=@p1 WHERE tc_hash=@p2", conn))
                {
                    cmd.Parameters.AddWithValue("@p1", model.Telefon);
                    cmd.Parameters.AddWithValue("@p2", hash);
                    cmd.ExecuteNonQuery();
                }
                // Şifre güncellenmişse veritabanına yansıt
                if (!string.IsNullOrEmpty(model.YeniSifre))
                {
                    using var cmd2 = new NpgsqlCommand("UPDATE Hastalar SET sifre_hash=@p1, sifre_sifreli=@p2 WHERE tc_hash=@p3", conn);
                    cmd2.Parameters.AddWithValue("@p1", SecurityHelper.Hashle(model.YeniSifre));
                    cmd2.Parameters.AddWithValue("@p2", SecurityHelper.Sifrele(model.YeniSifre));
                    cmd2.Parameters.AddWithValue("@p3", hash);
                    cmd2.ExecuteNonQuery();
                }
                TempData["Message"] = "Bilgileriniz başarıyla güncellendi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Güncelleme Hatası: " + ex.Message);
                return View(model);
            }
        }
    }
}