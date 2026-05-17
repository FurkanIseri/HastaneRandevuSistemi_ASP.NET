using Npgsql;

namespace HastaneYonetimSistemi.Helpers
{
    public class sqlBaglanti
    {
        private readonly IConfiguration _configuration;

        public sqlBaglanti(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public NpgsqlConnection baglanti()
        {
            string baglantiAdresi = _configuration.GetConnectionString("DefaultConnection") 
                ?? "Host=localhost;Port=5432;Database=HastaneYonetimSistemi;Username=postgres;Password=12345";
            
            NpgsqlConnection baglanma = new NpgsqlConnection(baglantiAdresi);
            
            if (baglanma.State != System.Data.ConnectionState.Open)
            {
                baglanma.Open();
            }

            return baglanma;
        }
    }
}
