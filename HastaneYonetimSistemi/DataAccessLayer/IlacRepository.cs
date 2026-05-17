using HastaneYonetimSistemi.Helpers;
using HastaneYonetimSistemi.Models;
using HastaneYonetimSistemi.DataAccessLayer.Interfaces;
using Npgsql;
using System;
using System.Collections.Generic;

namespace HastaneYonetimSistemi.DataAccessLayer
{
    public class IlacRepository : IIlacRepository
    {
        private readonly sqlBaglanti _bgl;

        public IlacRepository(IConfiguration configuration)
        {
            _bgl = new sqlBaglanti(configuration);
        }

        public IEnumerable<IlacViewModel> GetAll()
        {
            var ilaclar = new List<IlacViewModel>();
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("SELECT ilac_id, ilac_ad, stok_adet FROM ilaclar ORDER BY ilac_ad", conn);
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                ilaclar.Add(new IlacViewModel
                {
                    IlacID = Convert.ToInt32(dr[0]),
                    IlacAd = dr[1].ToString()!,
                    StokAdet = Convert.ToInt32(dr[2])
                });
            }
            return ilaclar;
        }

        public bool ExistsByName(string ilacAd)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM ilaclar WHERE LOWER(ilac_ad) = LOWER(@p1)", conn);
            cmd.Parameters.AddWithValue("@p1", ilacAd.Trim());
            long count = Convert.ToInt64(cmd.ExecuteScalar());
            return count > 0;
        }

        public void Add(string ilacAd, int stokAdet)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("INSERT INTO ilaclar (ilac_ad, stok_adet) VALUES (@p1,@p2)", conn);
            cmd.Parameters.AddWithValue("@p1", ilacAd.Trim());
            cmd.Parameters.AddWithValue("@p2", stokAdet);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int ilacId)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("DELETE FROM Ilaclar WHERE ilac_id=@p1", conn);
            cmd.Parameters.AddWithValue("@p1", ilacId);
            cmd.ExecuteNonQuery();
        }
    }
}
