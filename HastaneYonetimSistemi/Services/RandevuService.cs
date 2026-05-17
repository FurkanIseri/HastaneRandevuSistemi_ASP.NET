using HastaneYonetimSistemi.DataAccessLayer.Interfaces;
using HastaneYonetimSistemi.Models;
using HastaneYonetimSistemi.Services.Interfaces;

namespace HastaneYonetimSistemi.Services
{
    public class RandevuService : IRandevuService
    {
        private readonly IRandevuRepository _randevuRepository;
        private readonly IBransRepository _bransRepository;

        public RandevuService(IRandevuRepository randevuRepository, IBransRepository bransRepository)
        {
            _randevuRepository = randevuRepository;
            _bransRepository = bransRepository;
        }

        public ServiceResult OlusturRandevuYuvası(string tarih, string saat, string brans, string doktorId, string sekreterTcHash)
        {
            if (string.IsNullOrEmpty(sekreterTcHash)) return ServiceResult.Fail("Oturum süresi doldu.");
            if (!Guid.TryParse(doktorId, out var dId)) return ServiceResult.Fail("Geçersiz doktor ID.");
            try
            {
                int bransId = _bransRepository.GetIdByName(brans);
                if (bransId == 0) return ServiceResult.Fail("Branş bulunamadı.");
                _randevuRepository.AddRandevuYuvası(DateTime.Parse(tarih), saat, bransId, dId, false);
                return ServiceResult.Ok("Randevu yuvası başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail("Veritabanı Hatası: " + ex.Message);
            }
        }
    }
}
