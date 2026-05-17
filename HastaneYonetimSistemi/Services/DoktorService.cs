using HastaneYonetimSistemi.Models;
using HastaneYonetimSistemi.DataAccessLayer.Interfaces;
using HastaneYonetimSistemi.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HastaneYonetimSistemi.Services
{
    public class DoktorService : IDoktorService
    {
        private readonly IDoktorRepository _doktorRepository;
        private readonly ISekreterRepository _sekreterRepository;
        private readonly IBransRepository _bransRepository;

        public DoktorService(IDoktorRepository doktorRepository, ISekreterRepository sekreterRepository, IBransRepository bransRepository)
        {
            _doktorRepository = doktorRepository;
            _sekreterRepository = sekreterRepository;
            _bransRepository = bransRepository;
        }

        public IEnumerable<DoktorListeItem> GetAllDoktorlar()
        {
            return _doktorRepository.GetAllDoktorlar();
        }

        public async Task<(bool Success, string Message)> EkleDoktor(DoktorEkleViewModel model, string sekreterTcHash)
        {
            try
            {
                int bransId = _bransRepository.GetIdByName(model.Brans);
                if (bransId == 0) return (false, "Branş bulunamadı.");

                Guid sekreterIdVal = _sekreterRepository.GetSekreterIdByTcHash(sekreterTcHash);
                if (sekreterIdVal == Guid.Empty) return (false, "Oturumdaki sekreter bulunamadı.");

                byte[]? foto = null;
                if (model.ProfilFotografi?.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await model.ProfilFotografi.CopyToAsync(ms);
                    foto = ms.ToArray();
                }

                _doktorRepository.Add(model, bransId, sekreterIdVal, foto);
                return (true, $"Dr. {model.Ad} {model.Soyad} başarıyla eklendi.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool Success, string Message) SilDoktor(string doktorId, string sekreterTcHash)
        {
            if (!Guid.TryParse(doktorId, out var id)) return (false, "Geçersiz ID.");
            
            try
            {
                Guid mySekreterId = _sekreterRepository.GetSekreterIdByTcHash(sekreterTcHash);
                Guid ownerId = _doktorRepository.GetSekreterIdByDoktorId(id);

                if (ownerId != mySekreterId)
                {
                    return (false, "Yetkisiz işlem: Bu doktoru sadece sisteme kaydeden sekreter silebilir.");
                }

                _doktorRepository.Delete(id);
                return (true, "Doktor silindi.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool Success, string Message, DoktorGuncelleViewModel? Model) GetDoktorForEdit(string doktorId, string sekreterTcHash)
        {
            if (!Guid.TryParse(doktorId, out var dId)) return (false, "Geçersiz doktor ID.", null);

            try
            {
                Guid mySekreterIdVal = _sekreterRepository.GetSekreterIdByTcHash(sekreterTcHash);
                Guid doktorSekreterIdVal = _doktorRepository.GetSekreterIdByDoktorId(dId);

                var doktorModel = _doktorRepository.GetDoktorById(dId);
                if (doktorModel == null) return (false, "Doktor bulunamadı.", null);

                if (doktorSekreterIdVal != mySekreterIdVal)
                {
                    return (false, "Sadece bu doktoru sisteme kaydeden sekreter güncelleme yapabilir.", new DoktorGuncelleViewModel { DoktorId = doktorId });
                }

                return (true, string.Empty, doktorModel);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, new DoktorGuncelleViewModel { DoktorId = doktorId });
            }
        }

        public async Task<(bool Success, string Message)> GuncelleDoktor(DoktorGuncelleViewModel model, string sekreterTcHash)
        {
            if (!Guid.TryParse(model.DoktorId, out var dId)) return (false, "Geçersiz doktor ID.");

            try
            {
                Guid mySekreterIdVal = _sekreterRepository.GetSekreterIdByTcHash(sekreterTcHash);
                Guid doktorSekreterIdVal = _doktorRepository.GetSekreterIdByDoktorId(dId);

                if (doktorSekreterIdVal != mySekreterIdVal)
                {
                    return (false, "Sadece bu doktoru sisteme kaydeden sekreter güncelleme yapabilir.");
                }

                int bransId = _bransRepository.GetIdByName(model.Brans);
                if (bransId == 0) return (false, "Branş bulunamadı.");

                byte[]? foto = null;
                if (model.ProfilFotografi?.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await model.ProfilFotografi.CopyToAsync(ms);
                    foto = ms.ToArray();
                }

                _doktorRepository.Update(dId, model, bransId, foto);
                return (true, $"Dr. {model.Ad} {model.Soyad} bilgileri güncellendi.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public IEnumerable<object> GetDoktorlarByBrans(string brans)
        {
            try
            {
                return _doktorRepository.GetDoktorlarByBrans(brans);
            }
            catch
            {
                return new List<object>();
            }
        }
    }
}
