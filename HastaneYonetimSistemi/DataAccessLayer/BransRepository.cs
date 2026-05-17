using HastaneYonetimSistemi.Helpers;
using HastaneYonetimSistemi.Models;
using HastaneYonetimSistemi.DataAccessLayer.Interfaces;
using Npgsql;

namespace HastaneYonetimSistemi.DataAccessLayer
{
    public class BransRepository : IBransRepository
    {
        private readonly sqlBaglanti _bgl;

        public BransRepository(IConfiguration configuration)
        {
            _bgl = new sqlBaglanti(configuration);
        }

        public IEnumerable<BransViewModel> GetAll()
        {
            var branslar = new List<BransViewModel>();
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("SELECT brans_id, brans_ad FROM Branslar ORDER BY brans_ad", conn);
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                branslar.Add(new BransViewModel { BransId = Convert.ToInt32(dr[0]), BransAd = dr[1].ToString()! });
            }
            return branslar;
        }

        public int GetIdByName(string bransAd)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("SELECT brans_id FROM Branslar WHERE brans_ad=@p1", conn);
            cmd.Parameters.AddWithValue("@p1", bransAd ?? string.Empty);
            var res = cmd.ExecuteScalar();
            return res != null && res != DBNull.Value ? Convert.ToInt32(res) : 0;
        }

        public bool ExistsByName(string bransAd)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM branslar WHERE LOWER(TRIM(brans_ad)) = LOWER(TRIM(@p1))", conn);
            cmd.Parameters.AddWithValue("@p1", bransAd ?? string.Empty);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public void Add(string bransAd)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("INSERT INTO Branslar (brans_ad) VALUES (@p1)", conn);
            cmd.Parameters.AddWithValue("@p1", (bransAd ?? string.Empty).Trim());
            cmd.ExecuteNonQuery();
        }

        public void Delete(int bransId)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("DELETE FROM Branslar WHERE brans_id=@p1", conn);
            cmd.Parameters.AddWithValue("@p1", bransId);
            cmd.ExecuteNonQuery();
        }

        public bool IsBransInUse(int bransId)
        {
            return false;
        }
    }
}
