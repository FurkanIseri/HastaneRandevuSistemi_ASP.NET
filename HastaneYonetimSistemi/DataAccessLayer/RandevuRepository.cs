using HastaneYonetimSistemi.Helpers;
using HastaneYonetimSistemi.DataAccessLayer.Interfaces;
using Npgsql;
using System;

namespace HastaneYonetimSistemi.DataAccessLayer
{
    public class RandevuRepository : IRandevuRepository
    {
        private readonly sqlBaglanti _bgl;

        public RandevuRepository(IConfiguration configuration)
        {
            _bgl = new sqlBaglanti(configuration);
        }

        public void AddRandevuYuvası(DateTime tarih, string saat, int bransId, Guid doktorId, bool durum)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("CALL sp_randevuolustur(@p1, @p2, @p3, @p4, @p5)", conn);
            cmd.Parameters.AddWithValue("@p1", NpgsqlTypes.NpgsqlDbType.Timestamp, tarih);
            cmd.Parameters.AddWithValue("@p2", NpgsqlTypes.NpgsqlDbType.Varchar, saat);
            cmd.Parameters.AddWithValue("@p3", bransId);
            cmd.Parameters.AddWithValue("@p4", doktorId);
            cmd.Parameters.AddWithValue("@p5", durum);
            cmd.ExecuteNonQuery();
        }
    }
}
