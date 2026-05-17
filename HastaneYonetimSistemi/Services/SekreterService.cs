using HastaneYonetimSistemi.Models;
using HastaneYonetimSistemi.DataAccessLayer.Interfaces;
using HastaneYonetimSistemi.Services.Interfaces;
using System.Collections.Generic;

namespace HastaneYonetimSistemi.Services
{
    public class SekreterService : ISekreterService
    {
        private readonly ISekreterRepository _sekreterRepository;

        public SekreterService(ISekreterRepository sekreterRepository)
        {
            _sekreterRepository = sekreterRepository;
        }

        public IEnumerable<SekreterListeItem> GetAllSekreterler()
        {
            return _sekreterRepository.GetAll();
        }

        public (bool Success, string Message) EkleSekreter(string ad, string soyad, string tc, string sifre, string cinsiyet)
        {
            if (string.IsNullOrWhiteSpace(ad) || string.IsNullOrWhiteSpace(soyad) ||
                string.IsNullOrWhiteSpace(tc) || string.IsNullOrWhiteSpace(sifre) || string.IsNullOrWhiteSpace(cinsiyet))
            {
                return (false, "Tüm alanlar (Cinsiyet dahil) zorunludur.");
            }

            if (tc.Length != 11)
            {
                return (false, "TC Kimlik No 11 haneli olmalıdır.");
            }

            try
            {
                _sekreterRepository.Add(ad, soyad, tc, sifre, cinsiyet);
                return (true, $"{ad} {soyad} sekreter olarak eklendi.");
            }
            catch (System.Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool Success, string Message) GuncelleSifre(string tcHash, SekreterBilgiViewModel model)
        {
            if (!string.IsNullOrEmpty(model.YeniSifre) && model.YeniSifre != model.SifreTekrar)
            {
                return (false, "Åifreler eşleşmiyor.");
            }

            try
            {
                if (!string.IsNullOrEmpty(model.YeniSifre))
                {
                    _sekreterRepository.UpdatePassword(tcHash, model.YeniSifre);
                }
                return (true, "Åifreniz başarıyla güncellendi.");
            }
            catch (System.Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (string Ad, string Soyad)? GetSekreterAdSoyad(string tcHash)
        {
            return _sekreterRepository.GetSekreterAdSoyadByTcHash(tcHash);
        }

        public System.Guid GetSekreterId(string tcHash)
        {
            return _sekreterRepository.GetSekreterIdByTcHash(tcHash);
        }
    }
}
