using HastaneYonetimSistemi.DataAccessLayer.Interfaces;
using HastaneYonetimSistemi.Helpers;
using HastaneYonetimSistemi.Models;
using Npgsql;
using NpgsqlTypes;

namespace HastaneYonetimSistemi.DataAccessLayer
{
    public class HastaRepository : IHastaRepository
    {
        private readonly sqlBaglanti _bgl;

        public HastaRepository(IConfiguration configuration)
        {
            _bgl = new sqlBaglanti(configuration);
        }

        public (string Ad, string Soyad)? GetAdSoyadByTcHash(string tcHash)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("SELECT hasta_ad, hasta_soyad FROM Hastalar WHERE tc_hash=@p1", conn);
            cmd.Parameters.AddWithValue("@p1", tcHash);
            using var dr = cmd.ExecuteReader();
            if (dr.Read()) return (dr[0].ToString()!, dr[1].ToString()!);
            return null;
        }

        public Guid GetHastaIdByTcHash(string tcHash)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("SELECT hasta_id FROM Hastalar WHERE tc_hash=@p1", conn);
            cmd.Parameters.AddWithValue("@p1", tcHash);
            var res = cmd.ExecuteScalar();
            if (res != null && res != DBNull.Value) return Guid.Parse(res.ToString()!);
            return Guid.Empty;
        }

        public IEnumerable<RandevuViewModel> GetGecmisRandevular(string tcHash)
        {
            var list = new List<RandevuViewModel>();
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand(@"
                SELECT b.brans_ad AS Brans, (d.doktor_ad||' '||d.doktor_soyad) AS Doktor,
                       r.randevu_tarih AS Tarih, r.randevu_saat AS Saat, r.hasta_sikayet AS Sikayet
                FROM Randevular r
                JOIN Hastalar h ON r.hasta_id = h.hasta_id
                JOIN Doktorlar d ON r.doktor_id = d.doktor_id
                JOIN Branslar b ON r.brans_id = b.brans_id
                WHERE h.tc_hash = @p1 ORDER BY r.randevu_tarih DESC", conn);
            cmd.Parameters.AddWithValue("@p1", tcHash);
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new RandevuViewModel
                {
                    Brans = dr["Brans"].ToString()!,
                    Doktor = dr["Doktor"].ToString()!,
                    Tarih = DateTime.Parse(dr["Tarih"].ToString()!),
                    Saat = TimeSpan.Parse(dr["Saat"].ToString()!),
                    Sikayet = dr["Sikayet"]?.ToString() ?? ""
                });
            }
            return list;
        }

        public IEnumerable<RandevuViewModel> GetAktifRandevularByDoktorId(Guid doktorId)
        {
            var list = new List<RandevuViewModel>();
            using var conn = _bgl.baglanti();
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
            cmd.Parameters.AddWithValue("@p1", doktorId);
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new RandevuViewModel
                {
                    ID = Convert.ToInt32(dr["ID"]),
                    Brans = dr["Brans"].ToString()!,
                    Doktor = dr["Doktor"].ToString()!,
                    Tarih = DateTime.Parse(dr["Tarih"].ToString()!),
                    Saat = TimeSpan.Parse(dr["Saat"].ToString()!)
                });
            }
            return list;
        }

        public IEnumerable<string> GetAllBransNames()
        {
            var list = new List<string>();
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("SELECT DISTINCT brans_ad FROM Branslar ORDER BY brans_ad", conn);
            using var dr = cmd.ExecuteReader();
            while (dr.Read()) list.Add(dr[0].ToString()!);
            return list;
        }

        public IEnumerable<object> GetDoktorlarByBrans(string brans)
        {
            var list = new List<object>();
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand(
                "SELECT d.doktor_id, (d.doktor_ad||' '||d.doktor_soyad) FROM Doktorlar d " +
                "JOIN Branslar b ON d.brans_id=b.brans_id WHERE b.brans_ad=@p1", conn);
            cmd.Parameters.AddWithValue("@p1", brans ?? "");
            using var dr = cmd.ExecuteReader();
            while (dr.Read()) list.Add(new { id = dr[0].ToString(), ad = dr[1].ToString() });
            return list;
        }

        public void RandevuAl(int randevuId, Guid hastaId, string sikayet, byte[] tibbiBelge)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand(
                "UPDATE Randevular SET randevu_durum=TRUE, hasta_id=@p1, hasta_sikayet=@p2, tibbi_belge=@p3 WHERE randevu_id=@p4", conn);
            cmd.Parameters.AddWithValue("@p1", hastaId);
            cmd.Parameters.AddWithValue("@p2", sikayet ?? "");
            cmd.Parameters.Add(new NpgsqlParameter("@p3", NpgsqlDbType.Bytea) { Value = tibbiBelge });
            cmd.Parameters.AddWithValue("@p4", randevuId);
            cmd.ExecuteNonQuery();
        }

        public IEnumerable<ReceteViewModel> GetRecetelerByTcHash(string tcHash)
        {
            var list = new List<ReceteViewModel>();
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand(@"
                SELECT r.recete_id, r.tani_teshis, i.ilac_id, i.ilac_ad, ri.kullanim_sekli,
                       COALESCE(ri.adet, 1) AS adet
                FROM receteler r
                JOIN randevular rn ON r.randevu_id = rn.randevu_id
                JOIN hastalar h ON rn.hasta_id = h.hasta_id
                JOIN recete_ilac ri ON r.recete_id = ri.recete_id
                JOIN ilaclar i ON ri.ilac_id = i.ilac_id
                WHERE h.tc_hash = @p1", conn);
            cmd.Parameters.AddWithValue("@p1", tcHash);
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new ReceteViewModel
                {
                    ReceteID = Convert.ToInt32(dr["recete_id"]),
                    Tani = dr["tani_teshis"].ToString()!,
                    IlacID = Convert.ToInt32(dr["ilac_id"]),
                    IlacAd = dr["ilac_ad"].ToString()!,
                    KullanimTalimati = dr["kullanim_sekli"].ToString()!,
                    Adet = Convert.ToInt32(dr["adet"])
                });
            }
            return list;
        }

        public (string Ad, string Soyad, string Telefon)? GetBilgiByTcHash(string tcHash)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("SELECT hasta_ad, hasta_soyad, telefon_no FROM Hastalar WHERE tc_hash=@p1", conn);
            cmd.Parameters.AddWithValue("@p1", tcHash);
            using var dr = cmd.ExecuteReader();
            if (dr.Read()) return (dr[0].ToString()!, dr[1].ToString()!, dr[2].ToString()!);
            return null;
        }

        public void UpdateTelefon(string tcHash, string telefon)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("UPDATE Hastalar SET telefon_no=@p1 WHERE tc_hash=@p2", conn);
            cmd.Parameters.AddWithValue("@p1", telefon);
            cmd.Parameters.AddWithValue("@p2", tcHash);
            cmd.ExecuteNonQuery();
        }

        public void UpdateSifre(string tcHash, string sifreHash, string sifreSifreli)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("UPDATE Hastalar SET sifre_hash=@p1, sifre_sifreli=@p2 WHERE tc_hash=@p3", conn);
            cmd.Parameters.AddWithValue("@p1", sifreHash);
            cmd.Parameters.AddWithValue("@p2", sifreSifreli);
            cmd.Parameters.AddWithValue("@p3", tcHash);
            cmd.ExecuteNonQuery();
        }
    }
}
