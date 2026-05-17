using HastaneYonetimSistemi.DataAccessLayer.Interfaces;
using HastaneYonetimSistemi.Services.Interfaces;
using System;

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

        public (bool Success, string Message) OlusturRandevuYuvası(string tarih, string saat, string brans, string doktorId, string sekreterTcHash)
        {
            if (string.IsNullOrEmpty(sekreterTcHash)) return (false, "Oturum süresi doldu.");
            if (!Guid.TryParse(doktorId, out var dId)) return (false, "Geçersiz doktor ID.");

            try
            {
                int bransId = _bransRepository.GetIdByName(brans);
                if (bransId == 0) return (false, "Branş bulunamadı.");

                _randevuRepository.AddRandevuYuvası(DateTime.Parse(tarih), saat, bransId, dId, false);
                return (true, "Randevu yuvası başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                return (false, "Veritabanı Hatası: " + ex.Message);
            }
        }
    }
}
