using HastaneYonetimSistemi.Models;
using System.Collections.Generic;

namespace HastaneYonetimSistemi.Services.Interfaces
{
    public interface ISekreterService
    {
        IEnumerable<SekreterListeItem> GetAllSekreterler();
        (bool Success, string Message) EkleSekreter(string ad, string soyad, string tc, string sifre, string cinsiyet);
        (bool Success, string Message) GuncelleSifre(string tcHash, SekreterBilgiViewModel model);
        (string Ad, string Soyad)? GetSekreterAdSoyad(string tcHash);
        System.Guid GetSekreterId(string tcHash);
    }
}
