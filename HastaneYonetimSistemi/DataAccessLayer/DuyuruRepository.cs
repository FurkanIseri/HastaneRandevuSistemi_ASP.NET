using HastaneYonetimSistemi.DataAccessLayer.Interfaces;
using HastaneYonetimSistemi.Helpers;
using Npgsql;

namespace HastaneYonetimSistemi.DataAccessLayer
{
    public class DuyuruRepository : IDuyuruRepository
    {
        private readonly sqlBaglanti _bgl;

        public DuyuruRepository(IConfiguration configuration)
        {
            _bgl = new sqlBaglanti(configuration);
        }

        public void Add(string duyuruText, Guid sekreterId)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("INSERT INTO duyurular (duyurular_text, sekreter_id) VALUES (@p1, @p2)", conn);
            cmd.Parameters.AddWithValue("@p1", duyuruText);
            cmd.Parameters.AddWithValue("@p2", sekreterId);
            cmd.ExecuteNonQuery();
        }

        public IEnumerable<string> GetAll()
        {
            var list = new List<string>();
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("SELECT duyurular_text FROM duyurular ORDER BY duyurular_id DESC", conn);
            using var dr = cmd.ExecuteReader();
            while (dr.Read()) list.Add(dr[0].ToString()!);
            return list;
        }
    }
}
