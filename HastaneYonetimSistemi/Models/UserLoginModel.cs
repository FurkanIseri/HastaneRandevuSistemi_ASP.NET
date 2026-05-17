namespace HastaneRandevuSistemi.Models
{
    // Bu modeli Hasta, Doktor ve Sekreter girişlerinin hepsinde ortak kullanabiliriz.
    public class UserLoginModel
    {
        public string TC { get; set; }
        public string Sifre { get; set; }
    }
}
