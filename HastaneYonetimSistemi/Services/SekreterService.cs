using HastaneYonetimSistemi.Models;
using HastaneYonetimSistemi.DataAccessLayer.Interfaces;
using HastaneYonetimSistemi.Services.Interfaces;

namespace HastaneYonetimSistemi.Services
{
    public class SekreterService : ISekreterService
    {
        private readonly ISekreterRepository _sekreterRepository;

        public SekreterService(ISekreterRepository sekreterRepository)
        {
            _sekreterRepository = sekreterRepository;
        }

        public IEnumerable<SekreterListeItem> GetAllSekreterler() => _sekreterRepository.GetAll();

        public ServiceResult EkleSekreter(string ad, string soyad, string tc, string sifre, string cinsiyet)
        {
            if (string.IsNullOrWhiteSpace(ad) || string.IsNullOrWhiteSpace(soyad) ||
                string.IsNullOrWhiteSpace(tc) || string.IsNullOrWhiteSpace(sifre) || string.IsNullOrWhiteSpace(cinsiyet))
                return ServiceResult.Fail("Tüm alanlar (Cinsiyet dahil) zorunludur.");
            if (tc.Length != 11)
                return ServiceResult.Fail("TC Kimlik No 11 haneli olmalıdır.");
            try
            {
                _sekreterRepository.Add(ad, soyad, tc, sifre, cinsiyet);
                return ServiceResult.Ok($"{ad} {soyad} sekreter olarak eklendi.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail(ex.Message);
            }
        }

        public ServiceResult GuncelleSifre(string tcHash, SekreterBilgiViewModel model)
        {
            if (!string.IsNullOrEmpty(model.YeniSifre) && model.YeniSifre != model.SifreTekrar)
                return ServiceResult.Fail("Şifreler eşleşmiyor.");
            try
            {
                if (!string.IsNullOrEmpty(model.YeniSifre))
                    _sekreterRepository.UpdatePassword(tcHash, model.YeniSifre);
                return ServiceResult.Ok("Şifreniz başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail(ex.Message);
            }
        }

        public (string Ad, string Soyad)? GetSekreterAdSoyad(string tcHash)
            => _sekreterRepository.GetSekreterAdSoyadByTcHash(tcHash);

        public Guid GetSekreterId(string tcHash)
            => _sekreterRepository.GetSekreterIdByTcHash(tcHash);
    }
}
