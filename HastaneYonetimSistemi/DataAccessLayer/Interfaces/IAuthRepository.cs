namespace HastaneYonetimSistemi.DataAccessLayer.Interfaces
{
    public interface IAuthRepository
    {
        bool ValidateHasta(string tcHash, string sifreHash);
        bool ValidateDoktor(string tcHash, string sifreHash);
        bool ValidateSekreter(string tcHash, string sifreHash);
        bool TcExistsInDoktorlar(string tcHash);
        bool TcExistsInSekreterler(string tcHash);
        void KaydetHasta(string ad, string soyad, string tcHash, string tcSifreli, string telefon, string sifreHash, string sifreSifreli, string cinsiyet);
        string? GetHastaSifreSifreli(string tcHash);
        string? GetDoktorSifreSifreli(string tcHash);
        string? GetSekreterSifreSifreli(string tcHash);
    }
}
