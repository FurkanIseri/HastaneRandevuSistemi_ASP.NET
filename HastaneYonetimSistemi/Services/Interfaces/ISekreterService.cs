using HastaneYonetimSistemi.Models;

namespace HastaneYonetimSistemi.Services.Interfaces
{
    public interface ISekreterService
    {
        IEnumerable<SekreterListeItem> GetAllSekreterler();
        ServiceResult EkleSekreter(string ad, string soyad, string tc, string sifre, string cinsiyet);
        ServiceResult GuncelleSifre(string tcHash, SekreterBilgiViewModel model);
        (string Ad, string Soyad)? GetSekreterAdSoyad(string tcHash);
        Guid GetSekreterId(string tcHash);
    }
}
