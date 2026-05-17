using System;
using System.Collections.Generic;
using HastaneYonetimSistemi.Helpers;
using HastaneYonetimSistemi.Models;
using HastaneYonetimSistemi.DataAccessLayer.Interfaces;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace HastaneYonetimSistemi.DataAccessLayer
{
    public class SekreterRepository : ISekreterRepository
    {
        private readonly sqlBaglanti _bgl;

        public SekreterRepository(IConfiguration configuration)
        {
            _bgl = new sqlBaglanti(configuration);
        }

        public Guid GetSekreterIdByTcHash(string tcHash)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("SELECT sekreter_id FROM sekreterler WHERE tc_hash=@p1", conn);
            cmd.Parameters.AddWithValue("@p1", tcHash);
            var res = cmd.ExecuteScalar();
            if (res != null && res != DBNull.Value)
            {
                return Guid.Parse(res.ToString()!);
            }
            return Guid.Empty;
        }

        public (string Ad, string Soyad)? GetSekreterAdSoyadByTcHash(string tcHash)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("SELECT sekreter_ad, sekreter_soyad FROM sekreterler WHERE tc_hash=@p1", conn);
            cmd.Parameters.AddWithValue("@p1", tcHash);
            using var dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                return (dr[0].ToString()!, dr[1].ToString()!);
            }
            return null;
        }

        public IEnumerable<SekreterListeItem> GetAll()
        {
            var sekreterler = new List<SekreterListeItem>();
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("SELECT sekreter_id, sekreter_ad, sekreter_soyad FROM Sekreterler ORDER BY sekreter_soyad", conn);
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                sekreterler.Add(new SekreterListeItem
                {
                    SekreterID = dr["sekreter_id"].ToString()!,
                    AdSoyad = dr["sekreter_ad"] + " " + dr["sekreter_soyad"]
                });
            }
            return sekreterler;
        }

        public void Add(string ad, string soyad, string tc, string sifre, string cinsiyet)
        {
            using var conn = _bgl.baglanti();
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
        }

        public void UpdatePassword(string tcHash, string yeniSifre)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand(
                "UPDATE sekreterler SET sifre_hash=@p1, sifre_sifreli=@p2 WHERE tc_hash=@p3", conn);
            cmd.Parameters.AddWithValue("@p1", SecurityHelper.Hashle(yeniSifre));
            cmd.Parameters.AddWithValue("@p2", SecurityHelper.Sifrele(yeniSifre));
            cmd.Parameters.AddWithValue("@p3", tcHash);
            cmd.ExecuteNonQuery();
        }
    }
}
