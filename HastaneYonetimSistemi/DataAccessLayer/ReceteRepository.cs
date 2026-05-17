using HastaneYonetimSistemi.DataAccessLayer.Interfaces;
using HastaneYonetimSistemi.Helpers;
using HastaneYonetimSistemi.Models;
using Npgsql;

namespace HastaneYonetimSistemi.DataAccessLayer
{
    public class ReceteRepository : IReceteRepository
    {
        private readonly sqlBaglanti _bgl;

        public ReceteRepository(IConfiguration configuration)
        {
            _bgl = new sqlBaglanti(configuration);
        }

        public bool ReceteVarMi(int randevuId)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM receteler WHERE randevu_id = @p1", conn);
            cmd.Parameters.AddWithValue("@p1", randevuId);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public void OlusturRecete(int randevuId, string tani, int ilacId, int adet, string kullanimSekli)
        {
            using var conn = _bgl.baglanti();

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

            using var tr = conn.BeginTransaction();
            try
            {
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
            }
            catch
            {
                tr.Rollback();
                throw;
            }
        }

        public IEnumerable<YazilanReceteViewModel> GetYazilanRecetelerByDoktorId(Guid doktorId)
        {
            var list = new List<YazilanReceteViewModel>();
            using var conn = _bgl.baglanti();
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
                list.Add(new YazilanReceteViewModel
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
            return list;
        }

        public Guid? GetDoktorIdByTcHash(string tcHash)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("SELECT doktor_id FROM Doktorlar WHERE tc_hash=@p1", conn);
            cmd.Parameters.AddWithValue("@p1", tcHash);
            var res = cmd.ExecuteScalar();
            if (res != null && res != DBNull.Value) return Guid.Parse(res.ToString()!);
            return null;
        }

        public (string Ad, string Soyad, Guid DoktorId, byte[]? ProfilFoto)? GetDoktorProfilByTcHash(string tcHash)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand(
                "SELECT doktor_ad, doktor_soyad, doktor_id, profil_fotografi FROM Doktorlar WHERE tc_hash=@p1", conn);
            cmd.Parameters.AddWithValue("@p1", tcHash);
            using var dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                byte[]? foto = dr["profil_fotografi"] != DBNull.Value ? (byte[])dr["profil_fotografi"] : null;
                return (dr[0].ToString()!, dr[1].ToString()!, Guid.Parse(dr[2].ToString()!), foto);
            }
            return null;
        }

        public IEnumerable<RandevuViewModel> GetAktifRandevularByDoktorId(Guid doktorId)
        {
            var list = new List<RandevuViewModel>();
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand(@"
                SELECT r.randevu_id AS ID, r.randevu_tarih AS Tarih, r.randevu_saat AS Saat,
                       r.hasta_sikayet AS Sikayet, (r.tibbi_belge IS NOT NULL) AS TibbiBelgeVar,
                       (h.hasta_ad || ' ' || h.hasta_soyad) AS HastaAd,
                       (SELECT COUNT(*) FROM receteler WHERE randevu_id = r.randevu_id) AS ReceteYazildiMi
                FROM Randevular r
                LEFT JOIN Hastalar h ON r.hasta_id = h.hasta_id
                WHERE r.doktor_id = @p1 AND r.randevu_durum = TRUE
                ORDER BY r.randevu_tarih DESC", conn);
            cmd.Parameters.AddWithValue("@p1", doktorId);
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new RandevuViewModel
                {
                    ID = Convert.ToInt32(dr["ID"]),
                    HastaAd = dr["HastaAd"].ToString()!,
                    Tarih = DateTime.Parse(dr["Tarih"].ToString()!),
                    Saat = TimeSpan.Parse(dr["Saat"].ToString()!),
                    Sikayet = dr["Sikayet"]?.ToString() ?? "",
                    TibbiBelgeVar = Convert.ToBoolean(dr["TibbiBelgeVar"]),
                    IsCompleted = Convert.ToInt32(dr["ReceteYazildiMi"]) > 0
                });
            }
            return list;
        }

        public byte[]? GetTibbiBelge(int randevuId)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand(
                "SELECT tibbi_belge FROM Randevular WHERE randevu_id=@p1 AND tibbi_belge IS NOT NULL", conn);
            cmd.Parameters.AddWithValue("@p1", randevuId);
            var res = cmd.ExecuteScalar();
            if (res == null || res == DBNull.Value) return null;
            return (byte[])res;
        }

        public void UpdateDoktorSifre(string tcHash, string sifreHash, string sifreSifreli)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand(
                "UPDATE Doktorlar SET sifre_hash=@p1, sifre_sifreli=@p2 WHERE tc_hash=@p3", conn);
            cmd.Parameters.AddWithValue("@p1", sifreHash);
            cmd.Parameters.AddWithValue("@p2", sifreSifreli);
            cmd.Parameters.AddWithValue("@p3", tcHash);
            cmd.ExecuteNonQuery();
        }

        public (string Ad, string Soyad)? GetDoktorAdSoyadByTcHash(string tcHash)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("SELECT doktor_ad, doktor_soyad FROM Doktorlar WHERE tc_hash=@p1", conn);
            cmd.Parameters.AddWithValue("@p1", tcHash);
            using var dr = cmd.ExecuteReader();
            if (dr.Read()) return (dr[0].ToString()!, dr[1].ToString()!);
            return null;
        }
    }
}
