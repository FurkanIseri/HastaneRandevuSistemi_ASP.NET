using HastaneYonetimSistemi.DataAccessLayer.Interfaces;
using HastaneYonetimSistemi.Helpers;
using HastaneYonetimSistemi.Models;
using HastaneYonetimSistemi.Services.Interfaces;

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

        public ServiceResult OlusturDuyuru(string duyuruText, string sekreterTcHash)
        {
            if (string.IsNullOrWhiteSpace(duyuruText))
                return ServiceResult.Fail("Duyuru boş olamaz.");
            try
            {
                string tcHash = SecurityHelper.Hashle(sekreterTcHash);
                Guid sekreterId = _sekreterRepository.GetSekreterIdByTcHash(tcHash);
                if (sekreterId == Guid.Empty)
                    return ServiceResult.Fail("Sekreter bilgisi bulunamadı.");
                _duyuruRepository.Add(duyuruText, sekreterId);
                return ServiceResult.Ok("Duyuru yayınlandı.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail(ex.Message);
            }
        }
    }
}
