using Microsoft.AspNetCore.Mvc;
using HastaneYonetimSistemi.Models;
using HastaneYonetimSistemi.Helpers;
using Npgsql;

namespace HastaneYonetimSistemi.Controllers
{
    public class DoktorController : Controller
    {
        private readonly sqlBaglanti _bgl;
        public DoktorController(IConfiguration configuration) => _bgl = new sqlBaglanti(configuration);
        private string? TC => HttpContext.Session.GetString("DoktorTC");
        private IActionResult RedirectIfNoSession() => RedirectToAction("DoktorGiris", "Auth");

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
                using var cmdAd = new NpgsqlCommand(
                    "SELECT doktor_ad, doktor_soyad, doktor_id, profil_fotografi FROM Doktorlar WHERE tc_hash=@p1", conn);
                cmdAd.Parameters.AddWithValue("@p1", hash);
                using var dr = cmdAd.ExecuteReader();
                Guid doktorId = Guid.Empty;
                if (dr.Read())
                {
                    ViewBag.AdSoyad = dr[0] + " " + dr[1];
                    doktorId = Guid.Parse(dr[2].ToString()!);
                    if (dr["profil_fotografi"] != DBNull.Value)
                        ViewBag.ProfilFotoBase64 = "data:image/jpeg;base64," + Convert.ToBase64String((byte[])dr["profil_fotografi"]);
                }
                dr.Close();

                // Randevuları Çekerken 'Reçete Yazıldı mı?' bilgisini de alıyoruz
                var randevular = new List<RandevuViewModel>();
                using var cmdR = new NpgsqlCommand(@"
                    SELECT r.randevu_id AS ID, r.randevu_tarih AS Tarih, r.randevu_saat AS Saat,
                           r.hasta_sikayet AS Sikayet, (r.tibbi_belge IS NOT NULL) AS TibbiBelgeVar,
                           (h.hasta_ad || ' ' || h.hasta_soyad) AS HastaAd,
                           (SELECT COUNT(*) FROM receteler WHERE randevu_id = r.randevu_id) AS ReceteYazildiMi
                    FROM Randevular r
                    LEFT JOIN Hastalar h ON r.hasta_id = h.hasta_id
                    WHERE r.doktor_id = @p1 AND r.randevu_durum = TRUE
                    ORDER BY r.randevu_tarih DESC", conn);

                cmdR.Parameters.AddWithValue("@p1", doktorId);
                using var drR = cmdR.ExecuteReader();
                while (drR.Read())
                {
                    randevular.Add(new RandevuViewModel
                    {
                        ID = Convert.ToInt32(drR["ID"]),
                        HastaAd = drR["HastaAd"].ToString()!,
                        Tarih = DateTime.Parse(drR["Tarih"].ToString()!),
                        Saat = TimeSpan.Parse(drR["Saat"].ToString()!),
                        Sikayet = drR["Sikayet"]?.ToString() ?? "",
                        TibbiBelgeVar = Convert.ToBoolean(drR["TibbiBelgeVar"]),
                        // Bu özellik ile butonu pasif yapabileceğiz:
                        IsCompleted = Convert.ToInt32(drR["ReceteYazildiMi"]) > 0
                    });
                }
                drR.Close();
                ViewBag.Randevular = randevular;

                ViewBag.Ilaclar = GetIlaclarList(conn);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Hata: " + ex.Message;
            }
            return View();
        }

        // ================================================================
        // TIBBİ BELGE GÖRÜNTÜLE
        // ================================================================
        [HttpGet]
        public IActionResult TibbiBelgeGoster(int randevuId)
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            using var conn = _bgl.baglanti();
            try
            {
                using var cmd = new NpgsqlCommand(
                    "SELECT tibbi_belge FROM Randevular WHERE randevu_id=@p1 AND tibbi_belge IS NOT NULL", conn);
                cmd.Parameters.AddWithValue("@p1", randevuId);
                var res = cmd.ExecuteScalar();
                if (res == null || res == DBNull.Value) return NotFound("Tıbbi belge bulunamadı.");
                return File((byte[])res, "image/jpeg", $"tibbi-belge-{randevuId}.jpg");
            }
            catch (Exception ex) { return BadRequest("Hata: " + ex.Message); }
        }

        // ================================================================
        // REÇETE OLUÅTUR (TEK REÇETE KONTROLÜ EKLENDİ)
        // ================================================================
        [HttpPost]
        public IActionResult ReceteOlustur(int randevuId, string tani, int ilacId, int adet, string kullanimSekli)
        {
            if (string.IsNullOrEmpty(TC)) return Json(new { success = false, message = "Oturum süresi doldu." });

            using var conn = _bgl.baglanti();
            try
            {
                // GÜVENLİK KONTROLÜ: Bu randevuya daha önce reçete yazılmış mı?
                using (var cmdCheck = new NpgsqlCommand("SELECT COUNT(*) FROM receteler WHERE randevu_id = @p1", conn))
                {
                    cmdCheck.Parameters.AddWithValue("@p1", randevuId);
                    var count = Convert.ToInt32(cmdCheck.ExecuteScalar());
                    if (count > 0)
                    {
                        return Json(new { success = false, message = "Bu randevu için zaten bir reçete oluşturulmuş." });
                    }
                }

                using var tr = conn.BeginTransaction();
                try
                {
                    Guid hastaId = Guid.Empty;
                    Guid doktorId = Guid.Empty;
                    using (var cmdInfo = new NpgsqlCommand("SELECT hasta_id, doktor_id FROM Randevular WHERE randevu_id=@p1", conn))
                    {
                        cmdInfo.Parameters.AddWithValue("@p1", randevuId);
                        using var drInfo = cmdInfo.ExecuteReader();
                        if (drInfo.Read())
                        {
                            hastaId = Guid.Parse(drInfo[0].ToString()!);
                            doktorId = Guid.Parse(drInfo[1].ToString()!);
                        }
                    }

                    int yeniReceteNo = 100;
                    using (var cmdMax = new NpgsqlCommand("SELECT MAX(recete_id) FROM receteler", conn))
                    {
                        var maxRes = cmdMax.ExecuteScalar();
                        if (maxRes != DBNull.Value && maxRes != null) yeniReceteNo = Convert.ToInt32(maxRes) + 1;
                    }

                    using (var cmd = new NpgsqlCommand(@"
                        INSERT INTO receteler (recete_id, tani_teshis, recete_tarih, randevu_id, hasta_id, doktor_id) 
                        VALUES (@p1, @p2, CURRENT_DATE, @p3, @p4, @p5)", conn))
                    {
                        cmd.Parameters.AddWithValue("@p1", yeniReceteNo);
                        cmd.Parameters.AddWithValue("@p2", tani);
                        cmd.Parameters.AddWithValue("@p3", randevuId);
                        cmd.Parameters.AddWithValue("@p4", hastaId);
                        cmd.Parameters.AddWithValue("@p5", doktorId);
                        cmd.ExecuteNonQuery();
                    }

                    // TABLO ADI DÜZELTİLDİ: recetedetay
                    using (var cmdIlac = new NpgsqlCommand(@"
                        INSERT INTO recetedetay (recete_id, ilac_id, adet, kullanim_sekli) 
                        VALUES (@p1, @p2, @p3, @p4)", conn))
                    {
                        cmdIlac.Parameters.AddWithValue("@p1", yeniReceteNo);
                        cmdIlac.Parameters.AddWithValue("@p2", ilacId);
                        cmdIlac.Parameters.AddWithValue("@p3", adet);
                        cmdIlac.Parameters.AddWithValue("@p4", kullanimSekli ?? "");
                        cmdIlac.ExecuteNonQuery();
                    }

                    tr.Commit();
                    return Json(new { success = true, message = $"Reçete başarıyla kaydedildi. (No: {yeniReceteNo})" });
                }
                catch (Exception ex) { tr.Rollback(); throw ex; }
            }
            catch (Exception ex) { return Json(new { success = false, message = "Hata: " + ex.Message }); }
        }

        // ================================================================
        // YAZILAN REÇETELER (HATALI TABLO ADI DÜZELTİLDİ)
        // ================================================================
        [HttpGet]
        public IActionResult YazilanReceteler()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            var receteler = new List<YazilanReceteViewModel>();
            using var conn = _bgl.baglanti();
            try
            {
                var hash = SecurityHelper.Hashle(TC!);
                using var cmdId = new NpgsqlCommand("SELECT doktor_id FROM Doktorlar WHERE tc_hash=@p1", conn);
                cmdId.Parameters.AddWithValue("@p1", hash);
                var resId = cmdId.ExecuteScalar();
                if (resId == null) return RedirectIfNoSession();
                Guid doktorId = Guid.Parse(resId.ToString()!);

                // Fix join to recetedetay and map to HastaAdSoyad
                using var cmd = new NpgsqlCommand(@"
                    SELECT r.recete_id, (h.hasta_ad || ' ' || h.hasta_soyad) AS HastaAd, 
                           r.tani_teshis, i.ilac_ad, rd.adet, rd.kullanim_sekli, r.recete_tarih
                    FROM receteler r
                    JOIN hastalar h ON r.hasta_id = h.hasta_id
                    JOIN recetedetay rd ON r.recete_id = rd.recete_id
                    JOIN ilaclar i ON rd.ilac_id = i.ilac_id
                    WHERE r.doktor_id = @p1
                    ORDER BY r.recete_tarih DESC", conn);

                cmd.Parameters.AddWithValue("@p1", doktorId);
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    receteler.Add(new YazilanReceteViewModel
                    {
                        ReceteNo = Convert.ToInt32(dr["recete_id"]),
                        Tarih = DateTime.Parse(dr["recete_tarih"].ToString()!),
                        HastaAdSoyad = dr["HastaAd"].ToString()!,
                        Tani = dr["tani_teshis"].ToString()!,
                        IlacAd = dr["ilac_ad"].ToString()!,
                        Adet = Convert.ToInt32(dr["adet"]),
                        KullanimSekli = dr["kullanim_sekli"].ToString()!
                    });
                }
            }
            catch (Exception ex) { ViewBag.Error = "Hata: " + ex.Message; }
            return View(receteler);
        }

        // ================================================================
        // DUYURULAR
        // ================================================================
        public IActionResult Duyurular()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            var duyurular = new List<string>();
            using var conn = _bgl.baglanti();
            try
            {
                using var cmd = new NpgsqlCommand("SELECT duyurular_text FROM duyurular ORDER BY duyurular_id DESC", conn);
                using var dr = cmd.ExecuteReader();
                while (dr.Read()) duyurular.Add(dr[0].ToString()!);
            }
            catch (Exception ex) { ViewBag.Error = "Hata: " + ex.Message; }
            return View(duyurular);
        }

        // ================================================================
        // BİLGİ DÜZENLE (Åifre Güncelleme)
        // ================================================================
        [HttpGet]
        public IActionResult BilgiDuzenle()
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            var model = new DoktorBilgiViewModel();
            using var conn = _bgl.baglanti();
            try
            {
                var hash = SecurityHelper.Hashle(TC!);
                using var cmd = new NpgsqlCommand("SELECT doktor_ad, doktor_soyad FROM Doktorlar WHERE tc_hash=@p1", conn);
                cmd.Parameters.AddWithValue("@p1", hash);
                using var dr = cmd.ExecuteReader();
                if (dr.Read()) model.AdSoyad = dr[0] + " " + dr[1];
            }
            catch { }
            return View(model);
        }

        [HttpPost]
        public IActionResult BilgiDuzenle(DoktorBilgiViewModel model)
        {
            if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();
            if (!string.IsNullOrEmpty(model.YeniSifre) && model.YeniSifre != model.SifreTekrar)
            {
                ModelState.AddModelError("SifreTekrar", "Åifreler eşleşmiyor.");
                return View(model);
            }
            using var conn = _bgl.baglanti();
            try
            {
                var hash = SecurityHelper.Hashle(TC!);
                if (!string.IsNullOrEmpty(model.YeniSifre))
                {
                    using var cmd = new NpgsqlCommand(
                        "UPDATE Doktorlar SET sifre_hash=@p1, sifre_sifreli=@p2 WHERE tc_hash=@p3", conn);
                    cmd.Parameters.AddWithValue("@p1", SecurityHelper.Hashle(model.YeniSifre));
                    cmd.Parameters.AddWithValue("@p2", SecurityHelper.Sifrele(model.YeniSifre));
                    cmd.Parameters.AddWithValue("@p3", hash);
                    cmd.ExecuteNonQuery();
                }
                TempData["Message"] = "Bilgileriniz güncellendi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex) { ModelState.AddModelError("", "Hata: " + ex.Message); return View(model); }
        }

        // ================================================================
        // HELPERS (AJAX ve Liste)
        // ================================================================
        [HttpGet]
        public JsonResult GetIlaclar()
        {
            var ilaclar = new List<object>();
            using var conn = _bgl.baglanti();
            try
            {
                using var cmd = new NpgsqlCommand("SELECT ilac_id, ilac_ad, stok_adet FROM Ilaclar ORDER BY ilac_ad", conn);
                using var dr = cmd.ExecuteReader();
                while (dr.Read()) ilaclar.Add(new { id = dr[0], ad = dr[1].ToString(), stok = Convert.ToInt32(dr[2]) });
            }
            catch { }
            return Json(ilaclar);
        }

        private List<IlacViewModel> GetIlaclarList(NpgsqlConnection conn)
        {
            var list = new List<IlacViewModel>();
            using var cmd = new NpgsqlCommand("SELECT ilac_id, ilac_ad, stok_adet FROM Ilaclar ORDER BY ilac_ad", conn);
            using var dr = cmd.ExecuteReader();
            while (dr.Read()) list.Add(new IlacViewModel { IlacID = Convert.ToInt32(dr[0]), IlacAd = dr[1].ToString()!, StokAdet = Convert.ToInt32(dr[2]) });
            return list;
        }
    }

    // VİEW MODEL (Yazılan Reçeteler sayfası için)
    public class YazilanReceteViewModel
    {
        public int ReceteNo { get; set; }
        public DateTime Tarih { get; set; }
        public string HastaAdSoyad { get; set; } = string.Empty;
        public string Tani { get; set; } = string.Empty;
        public string IlacAd { get; set; } = string.Empty;
        public int Adet { get; set; }
        public string KullanimSekli { get; set; } = string.Empty;
    }
}
