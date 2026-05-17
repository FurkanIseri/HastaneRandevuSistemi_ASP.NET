using HastaneYonetimSistemi.DataAccessLayer.Interfaces;
using HastaneYonetimSistemi.Services.Interfaces;
using System;

namespace HastaneYonetimSistemi.Services
{
    public class DuyuruService : IDuyuruService
    {
        private readonly IDuyuruRepository _duyuruRepository;
        private readonly ISekreterRepository _sekreterRepository;

        public DuyuruService(IDuyuruRepository duyuruRepository, ISekreterRepository sekreterRepository)
        {
            _duyuruRepository = duyuruRepository;
            _sekreterRepository = sekreterRepository;
        }

        public (bool Success, string Message) OlusturDuyuru(string duyuruText, string sekreterTcHash)
        {
            if (string.IsNullOrWhiteSpace(duyuruText))
                return (false, "Duyuru boş olamaz.");

            try
            {
                Guid sekreterId = _sekreterRepository.GetSekreterIdByTcHash(sekreterTcHash);
                if (sekreterId == Guid.Empty)
                {
                    return (false, "Sekreter bilgisi bulunamadı.");
                }

                _duyuruRepository.Add(duyuruText, sekreterId);
                return (true, "Duyuru yayınlandı.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
