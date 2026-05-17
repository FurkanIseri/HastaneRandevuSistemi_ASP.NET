using HastaneYonetimSistemi.Helpers;
using HastaneYonetimSistemi.Models;
using HastaneYonetimSistemi.DataAccessLayer.Interfaces;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;

namespace HastaneYonetimSistemi.DataAccessLayer
{
    public class DoktorRepository : IDoktorRepository
    {
        private readonly sqlBaglanti _bgl;

        public DoktorRepository(IConfiguration configuration)
        {
            _bgl = new sqlBaglanti(configuration);
        }

        public IEnumerable<DoktorListeItem> GetAllDoktorlar()
        {
            var doktorlar = new List<DoktorListeItem>();
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand(
                "SELECT d.doktor_id, d.doktor_ad, d.doktor_soyad, b.brans_ad, d.profil_fotografi, d.sekreter_id " +
                "FROM Doktorlar d JOIN Branslar b ON d.brans_id = b.brans_id ORDER BY b.brans_ad, d.doktor_soyad", conn);
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                doktorlar.Add(new DoktorListeItem
                {
                    DoktorId = dr["doktor_id"].ToString()!,
                    AdSoyad = dr["doktor_ad"] + " " + dr["doktor_soyad"],
                    Brans = dr["brans_ad"].ToString()!,
                    SekreterId = dr["sekreter_id"]?.ToString() ?? "",
                    ProfilFotografi = dr["profil_fotografi"] == DBNull.Value ? null : (byte[])dr["profil_fotografi"]
                });
            }
            return doktorlar;
        }

        public void Add(DoktorEkleViewModel model, int bransId, Guid sekreterId, byte[]? foto)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand(
                "INSERT INTO doktorlar (doktor_ad, doktor_soyad, tc_hash, tc_sifreli, sifre_hash, sifre_sifreli, brans_id, sekreter_id, profil_fotografi, cinsiyet) " +
                "VALUES (@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10)", conn);
            
            cmd.Parameters.AddWithValue("@p1", model.Ad ?? string.Empty);
            cmd.Parameters.AddWithValue("@p2", model.Soyad ?? string.Empty);
            cmd.Parameters.AddWithValue("@p3", SecurityHelper.Hashle(model.TC ?? string.Empty));
            cmd.Parameters.AddWithValue("@p4", SecurityHelper.Sifrele(model.TC ?? string.Empty));
            cmd.Parameters.AddWithValue("@p5", SecurityHelper.Hashle(model.Sifre ?? string.Empty));
            cmd.Parameters.AddWithValue("@p6", SecurityHelper.Sifrele(model.Sifre ?? string.Empty));
            cmd.Parameters.AddWithValue("@p7", bransId);
            cmd.Parameters.AddWithValue("@p8", sekreterId);
            cmd.Parameters.Add(new NpgsqlParameter("@p9", NpgsqlDbType.Bytea) { Value = foto != null ? (object)foto : DBNull.Value });
            cmd.Parameters.AddWithValue("@p10", model.Cinsiyet ?? string.Empty);
            cmd.ExecuteNonQuery();
        }

        public Guid GetSekreterIdByDoktorId(Guid doktorId)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("SELECT sekreter_id FROM doktorlar WHERE doktor_id=@p1", conn);
            cmd.Parameters.AddWithValue("@p1", doktorId);
            var r = cmd.ExecuteScalar();
            if (r != null && r != DBNull.Value) return Guid.Parse(r.ToString()!);
            return Guid.Empty;
        }

        public void Delete(Guid doktorId)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("DELETE FROM Doktorlar WHERE doktor_id=@p1", conn);
            cmd.Parameters.AddWithValue("@p1", doktorId);
            cmd.ExecuteNonQuery();
        }

        public DoktorGuncelleViewModel? GetDoktorById(Guid doktorId)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand(
                "SELECT d.doktor_ad, d.doktor_soyad, b.brans_ad " +
                "FROM doktorlar d JOIN branslar b ON d.brans_id = b.brans_id WHERE d.doktor_id=@p1", conn);
            cmd.Parameters.AddWithValue("@p1", doktorId);
            using var dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                return new DoktorGuncelleViewModel
                {
                    DoktorId = doktorId.ToString(),
                    Ad = dr["doktor_ad"].ToString()!,
                    Soyad = dr["doktor_soyad"].ToString()!,
                    Brans = dr["brans_ad"].ToString()!
                };
            }
            return null;
        }

        public void Update(Guid doktorId, DoktorGuncelleViewModel model, int bransId, byte[]? foto)
        {
            using var conn = _bgl.baglanti();
            
            using (var cmdU = new NpgsqlCommand(
                "UPDATE doktorlar SET doktor_ad=@p1, doktor_soyad=@p2, brans_id=@p3 WHERE doktor_id=@p4", conn))
            {
                cmdU.Parameters.AddWithValue("@p1", model.Ad ?? string.Empty);
                cmdU.Parameters.AddWithValue("@p2", model.Soyad ?? string.Empty);
                cmdU.Parameters.AddWithValue("@p3", bransId);
                cmdU.Parameters.AddWithValue("@p4", doktorId);
                cmdU.ExecuteNonQuery();
            }

            if (!string.IsNullOrEmpty(model.YeniSifre))
            {
                using var cmdSifre = new NpgsqlCommand(
                    "UPDATE doktorlar SET sifre_hash=@p1, sifre_sifreli=@p2 WHERE doktor_id=@p3", conn);
                cmdSifre.Parameters.AddWithValue("@p1", SecurityHelper.Hashle(model.YeniSifre));
                cmdSifre.Parameters.AddWithValue("@p2", SecurityHelper.Sifrele(model.YeniSifre));
                cmdSifre.Parameters.AddWithValue("@p3", doktorId);
                cmdSifre.ExecuteNonQuery();
            }

            if (foto != null)
            {
                using var cmdFoto = new NpgsqlCommand(
                    "UPDATE doktorlar SET profil_fotografi=@p1 WHERE doktor_id=@p2", conn);
                cmdFoto.Parameters.Add(new NpgsqlParameter("@p1", NpgsqlDbType.Bytea) { Value = foto });
                cmdFoto.Parameters.AddWithValue("@p2", doktorId);
                cmdFoto.ExecuteNonQuery();
            }
        }

        public IEnumerable<object> GetDoktorlarByBrans(string brans)
        {
            var doktorlar = new List<object>();
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand(
                "SELECT d.doktor_id, (d.doktor_ad || ' ' || d.doktor_soyad) AS AdSoyad " +
                "FROM Doktorlar d JOIN Branslar b ON d.brans_id = b.brans_id WHERE b.brans_ad=@p1", conn);
            cmd.Parameters.AddWithValue("@p1", brans ?? "");
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                doktorlar.Add(new { id = dr[0].ToString(), ad = dr[1].ToString() });
            }
            return doktorlar;
        }
    }
}
