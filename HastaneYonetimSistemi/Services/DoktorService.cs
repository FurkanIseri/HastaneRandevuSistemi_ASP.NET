using HastaneYonetimSistemi.DataAccessLayer.Interfaces;
using HastaneYonetimSistemi.Helpers;
using HastaneYonetimSistemi.Models;
using HastaneYonetimSistemi.Services.Interfaces;

namespace HastaneYonetimSistemi.Services
{
    public class DoktorService : IDoktorService
    {
        private readonly IDoktorRepository _doktorRepository;
        private readonly ISekreterRepository _sekreterRepository;
        private readonly IBransRepository _bransRepository;
        private readonly IReceteRepository _receteRepository;
        private readonly IDuyuruRepository _duyuruRepository;
        private readonly IIlacRepository _ilacRepository;

        public DoktorService(
            IDoktorRepository doktorRepository,
            ISekreterRepository sekreterRepository,
            IBransRepository bransRepository,
            IReceteRepository receteRepository,
            IDuyuruRepository duyuruRepository,
            IIlacRepository ilacRepository)
        {
            _doktorRepository = doktorRepository;
            _sekreterRepository = sekreterRepository;
            _bransRepository = bransRepository;
            _receteRepository = receteRepository;
            _duyuruRepository = duyuruRepository;
            _ilacRepository = ilacRepository;
        }

        public IEnumerable<DoktorListeItem> GetAllDoktorlar()
        {
            return _doktorRepository.GetAllDoktorlar();
        }

        public async Task<ServiceResult> EkleDoktor(DoktorEkleViewModel model, string sekreterTcHash)
        {
            try
            {
                int bransId = _bransRepository.GetIdByName(model.Brans);
                if (bransId == 0) return ServiceResult.Fail("Branş bulunamadı.");

                Guid sekreterIdVal = _sekreterRepository.GetSekreterIdByTcHash(sekreterTcHash);
                if (sekreterIdVal == Guid.Empty) return ServiceResult.Fail("Oturumdaki sekreter bulunamadı.");

                byte[]? foto = null;
                if (model.ProfilFotografi?.Length > 0)
                {
                    using var ms = new System.IO.MemoryStream();
                    await model.ProfilFotografi.CopyToAsync(ms);
                    foto = ms.ToArray();
                }

                _doktorRepository.Add(model, bransId, sekreterIdVal, foto);
                return ServiceResult.Ok($"Dr. {model.Ad} {model.Soyad} başarıyla eklendi.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail(ex.Message);
            }
        }

        public ServiceResult SilDoktor(string doktorId, string sekreterTcHash)
        {
            if (!Guid.TryParse(doktorId, out var id)) return ServiceResult.Fail("Geçersiz ID.");
            try
            {
                Guid mySekreterId = _sekreterRepository.GetSekreterIdByTcHash(sekreterTcHash);
                Guid ownerId = _doktorRepository.GetSekreterIdByDoktorId(id);
                if (ownerId != mySekreterId)
                    return ServiceResult.Fail("Yetkisiz işlem: Bu doktoru sadece sisteme kaydeden sekreter silebilir.");
                _doktorRepository.Delete(id);
                return ServiceResult.Ok("Doktor silindi.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail(ex.Message);
            }
        }

        public ServiceResult<DoktorGuncelleViewModel> GetDoktorForEdit(string doktorId, string sekreterTcHash)
        {
            if (!Guid.TryParse(doktorId, out var dId)) return ServiceResult<DoktorGuncelleViewModel>.Fail("Geçersiz doktor ID.");
            try
            {
                Guid mySekreterIdVal = _sekreterRepository.GetSekreterIdByTcHash(sekreterTcHash);
                Guid doktorSekreterIdVal = _doktorRepository.GetSekreterIdByDoktorId(dId);
                var doktorModel = _doktorRepository.GetDoktorById(dId);
                if (doktorModel == null) return ServiceResult<DoktorGuncelleViewModel>.Fail("Doktor bulunamadı.");
                if (doktorSekreterIdVal != mySekreterIdVal)
                    return ServiceResult<DoktorGuncelleViewModel>.Fail("Sadece bu doktoru sisteme kaydeden sekreter güncelleme yapabilir.");
                return ServiceResult<DoktorGuncelleViewModel>.Ok(doktorModel);
            }
            catch (Exception ex)
            {
                return ServiceResult<DoktorGuncelleViewModel>.Fail(ex.Message);
            }
        }

        public async Task<ServiceResult> GuncelleDoktor(DoktorGuncelleViewModel model, string sekreterTcHash)
        {
            if (!Guid.TryParse(model.DoktorId, out var dId)) return ServiceResult.Fail("Geçersiz doktor ID.");
            try
            {
                Guid mySekreterIdVal = _sekreterRepository.GetSekreterIdByTcHash(sekreterTcHash);
                Guid doktorSekreterIdVal = _doktorRepository.GetSekreterIdByDoktorId(dId);
                if (doktorSekreterIdVal != mySekreterIdVal)
                    return ServiceResult.Fail("Sadece bu doktoru sisteme kaydeden sekreter güncelleme yapabilir.");
                int bransId = _bransRepository.GetIdByName(model.Brans);
                if (bransId == 0) return ServiceResult.Fail("Branş bulunamadı.");
                byte[]? foto = null;
                if (model.ProfilFotografi?.Length > 0)
                {
                    using var ms = new System.IO.MemoryStream();
                    await model.ProfilFotografi.CopyToAsync(ms);
                    foto = ms.ToArray();
                }
                _doktorRepository.Update(dId, model, bransId, foto);
                return ServiceResult.Ok($"Dr. {model.Ad} {model.Soyad} bilgileri güncellendi.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail(ex.Message);
            }
        }

        public IEnumerable<object> GetDoktorlarByBrans(string brans)
        {
            try { return _doktorRepository.GetDoktorlarByBrans(brans); }
            catch { return new List<object>(); }
        }

        public ServiceResult<(string AdSoyad, Guid DoktorId, string? ProfilBase64, IEnumerable<RandevuViewModel> Randevular, IEnumerable<IlacViewModel> Ilaclar)> GetPanelData(string tc)
        {
            try
            {
                string tcHash = SecurityHelper.Hashle(tc);
                var profil = _receteRepository.GetDoktorProfilByTcHash(tcHash);
                if (profil == null) return ServiceResult<(string, Guid, string?, IEnumerable<RandevuViewModel>, IEnumerable<IlacViewModel>)>.Fail("Doktor profili bulunamadı.");
                string adSoyad = profil.Value.Ad + " " + profil.Value.Soyad;
                string? profilBase64 = profil.Value.ProfilFoto != null ? "data:image/jpeg;base64," + Convert.ToBase64String(profil.Value.ProfilFoto) : null;
                var randevular = _receteRepository.GetAktifRandevularByDoktorId(profil.Value.DoktorId);
                var ilaclar = _ilacRepository.GetAll();
                return ServiceResult<(string, Guid, string?, IEnumerable<RandevuViewModel>, IEnumerable<IlacViewModel>)>.Ok((adSoyad, profil.Value.DoktorId, profilBase64, randevular, ilaclar));
            }
            catch (Exception ex)
            {
                return ServiceResult<(string, Guid, string?, IEnumerable<RandevuViewModel>, IEnumerable<IlacViewModel>)>.Fail("Hata: " + ex.Message);
            }
        }

        public ServiceResult OlusturRecete(int randevuId, string tani, int ilacId, int adet, string kullanimSekli)
        {
            try
            {
                if (_receteRepository.ReceteVarMi(randevuId))
                    return ServiceResult.Fail("Bu randevu için zaten bir reçete oluşturulmuş.");
                _receteRepository.OlusturRecete(randevuId, tani, ilacId, adet, kullanimSekli);
                return ServiceResult.Ok("Reçete başarıyla kaydedildi.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail("Hata: " + ex.Message);
            }
        }

        public ServiceResult<IEnumerable<YazilanReceteViewModel>> GetYazilanReceteler(string tc)
        {
            try
            {
                string tcHash = SecurityHelper.Hashle(tc);
                Guid? doktorId = _receteRepository.GetDoktorIdByTcHash(tcHash);
                if (doktorId == null) return ServiceResult<IEnumerable<YazilanReceteViewModel>>.Fail("Doktor bulunamadı.");
                return ServiceResult<IEnumerable<YazilanReceteViewModel>>.Ok(_receteRepository.GetYazilanRecetelerByDoktorId(doktorId.Value));
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<YazilanReceteViewModel>>.Fail("Hata: " + ex.Message);
            }
        }

        public IEnumerable<string> GetDuyurular()
        {
            try { return _duyuruRepository.GetAll(); }
            catch { return new List<string>(); }
        }

        public ServiceResult<(string AdSoyad, string? ProfilBase64)> GetBilgiProfil(string tc)
        {
            try
            {
                string tcHash = SecurityHelper.Hashle(tc);
                var adSoyad = _receteRepository.GetDoktorAdSoyadByTcHash(tcHash);
                if (adSoyad == null) return ServiceResult<(string, string?)>.Fail("Doktor bulunamadı.");
                return ServiceResult<(string, string?)>.Ok((adSoyad.Value.Ad + " " + adSoyad.Value.Soyad, null));
            }
            catch (Exception ex)
            {
                return ServiceResult<(string, string?)>.Fail(ex.Message);
            }
        }

        public ServiceResult UpdateSifre(string tc, DoktorBilgiViewModel model)
        {
            if (!string.IsNullOrEmpty(model.YeniSifre) && model.YeniSifre != model.SifreTekrar)
                return ServiceResult.Fail("Şifreler eşleşmiyor.");
            try
            {
                if (!string.IsNullOrEmpty(model.YeniSifre))
                {
                    string tcHash = SecurityHelper.Hashle(tc);
                    _receteRepository.UpdateDoktorSifre(tcHash, SecurityHelper.Hashle(model.YeniSifre), SecurityHelper.Sifrele(model.YeniSifre));
                }
                return ServiceResult.Ok("Bilgileriniz güncellendi.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail("Hata: " + ex.Message);
            }
        }
    }
}
