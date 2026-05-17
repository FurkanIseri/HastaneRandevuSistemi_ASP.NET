using HastaneYonetimSistemi.DataAccessLayer.Interfaces;
using HastaneYonetimSistemi.Helpers;
using Npgsql;

namespace HastaneYonetimSistemi.DataAccessLayer
{
    public class AuthRepository : IAuthRepository
    {
        private readonly sqlBaglanti _bgl;

        public AuthRepository(IConfiguration configuration)
        {
            _bgl = new sqlBaglanti(configuration);
        }

        public bool ValidateHasta(string tcHash, string sifreHash)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Hastalar WHERE tc_hash = @p1 AND sifre_hash = @p2", conn);
            cmd.Parameters.AddWithValue("@p1", tcHash);
            cmd.Parameters.AddWithValue("@p2", sifreHash);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public bool ValidateDoktor(string tcHash, string sifreHash)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Doktorlar WHERE tc_hash = @p1 AND sifre_hash = @p2", conn);
            cmd.Parameters.AddWithValue("@p1", tcHash);
            cmd.Parameters.AddWithValue("@p2", sifreHash);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public bool ValidateSekreter(string tcHash, string sifreHash)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Sekreterler WHERE tc_hash = @p1 AND sifre_hash = @p2", conn);
            cmd.Parameters.AddWithValue("@p1", tcHash);
            cmd.Parameters.AddWithValue("@p2", sifreHash);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public bool TcExistsInDoktorlar(string tcHash)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Doktorlar WHERE tc_hash = @p1", conn);
            cmd.Parameters.AddWithValue("@p1", tcHash);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public bool TcExistsInSekreterler(string tcHash)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Sekreterler WHERE tc_hash = @p1", conn);
            cmd.Parameters.AddWithValue("@p1", tcHash);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public void KaydetHasta(string ad, string soyad, string tcHash, string tcSifreli, string telefon, string sifreHash, string sifreSifreli, string cinsiyet)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("CALL sp_HastaKayit(@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8::cinsiyet_tipi)", conn);
            cmd.Parameters.AddWithValue("@p1", ad);
            cmd.Parameters.AddWithValue("@p2", soyad);
            cmd.Parameters.AddWithValue("@p3", tcHash);
            cmd.Parameters.AddWithValue("@p4", tcSifreli);
            cmd.Parameters.AddWithValue("@p5", telefon);
            cmd.Parameters.AddWithValue("@p6", sifreHash);
            cmd.Parameters.AddWithValue("@p7", sifreSifreli);
            cmd.Parameters.AddWithValue("@p8", cinsiyet);
            cmd.ExecuteNonQuery();
        }

        public string? GetHastaSifreSifreli(string tcHash)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("SELECT sifre_sifreli FROM Hastalar WHERE tc_hash = @p1", conn);
            cmd.Parameters.AddWithValue("@p1", tcHash);
            var result = cmd.ExecuteScalar();
            return result != null && result != DBNull.Value ? result.ToString() : null;
        }

        public string? GetDoktorSifreSifreli(string tcHash)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("SELECT sifre_sifreli FROM Doktorlar WHERE tc_hash = @p1", conn);
            cmd.Parameters.AddWithValue("@p1", tcHash);
            var result = cmd.ExecuteScalar();
            return result != null && result != DBNull.Value ? result.ToString() : null;
        }

        public string? GetSekreterSifreSifreli(string tcHash)
        {
            using var conn = _bgl.baglanti();
            using var cmd = new NpgsqlCommand("SELECT sifre_sifreli FROM Sekreterler WHERE tc_hash = @p1", conn);
            cmd.Parameters.AddWithValue("@p1", tcHash);
            var result = cmd.ExecuteScalar();
            return result != null && result != DBNull.Value ? result.ToString() : null;
        }
    }
}
