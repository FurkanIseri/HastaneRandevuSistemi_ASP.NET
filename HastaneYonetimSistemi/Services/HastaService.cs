using HastaneYonetimSistemi.DataAccessLayer.Interfaces;
using HastaneYonetimSistemi.Helpers;
using HastaneYonetimSistemi.Models;
using HastaneYonetimSistemi.Services.Interfaces;

namespace HastaneYonetimSistemi.Services
{
    public class HastaService : IHastaService
    {
        private readonly IHastaRepository _hastaRepository;

        public HastaService(IHastaRepository hastaRepository)
        {
            _hastaRepository = hastaRepository;
        }

        public ServiceResult<(string AdSoyad, IEnumerable<RandevuViewModel> GecmisRandevular)> GetPanelData(string tc)
        {
            try
            {
                string tcHash = SecurityHelper.Hashle(tc);
                var adSoyad = _hastaRepository.GetAdSoyadByTcHash(tcHash);
                string displayName = adSoyad.HasValue ? adSoyad.Value.Ad + " " + adSoyad.Value.Soyad : tc;
                var gecmis = _hastaRepository.GetGecmisRandevular(tcHash);
                return ServiceResult<(string, IEnumerable<RandevuViewModel>)>.Ok((displayName, gecmis));
            }
            catch (Exception ex)
            {
                return ServiceResult<(string, IEnumerable<RandevuViewModel>)>.Fail("Ana sayfa yüklenirken hata: " + ex.Message);
            }
        }

        public IEnumerable<string> GetBranslar()
        {
            return _hastaRepository.GetAllBransNames();
        }

        public IEnumerable<object> GetDoktorlarByBrans(string brans)
        {
            return _hastaRepository.GetDoktorlarByBrans(brans);
        }

        public IEnumerable<RandevuViewModel> GetAktifRandevular(string doktorId)
        {
            if (string.IsNullOrEmpty(doktorId) || !Guid.TryParse(doktorId, out var dId))
                return Enumerable.Empty<RandevuViewModel>();
            return _hastaRepository.GetAktifRandevularByDoktorId(dId);
        }

        public async Task<ServiceResult> RandevuAl(string tc, RandevuAlViewModel model)
        {
            if (model.TibbiBelge == null || model.TibbiBelge.Length == 0)
                return ServiceResult.Fail("Lütfen tıbbi belgenizi yükleyiniz.");
            if (model.RandevuId <= 0)
                return ServiceResult.Fail("Geçersiz randevu seçimi.");
            try
            {
                byte[] belge;
                using (var ms = new System.IO.MemoryStream())
                {
                    await model.TibbiBelge.CopyToAsync(ms);
                    belge = ms.ToArray();
                }
                string tcHash = SecurityHelper.Hashle(tc);
                Guid hastaId = _hastaRepository.GetHastaIdByTcHash(tcHash);
                if (hastaId == Guid.Empty) return ServiceResult.Fail("Hasta kaydı bulunamadı.");
                _hastaRepository.RandevuAl(model.RandevuId, hastaId, model.Sikayet ?? "", belge);
                return ServiceResult.Ok("Randevunuz ve tıbbi belgeniz başarıyla kaydedildi.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail("Veritabanı Hatası: " + ex.Message);
            }
        }

        public IEnumerable<ReceteViewModel> GetReceteler(string tc)
        {
            string tcHash = SecurityHelper.Hashle(tc);
            return _hastaRepository.GetRecetelerByTcHash(tcHash);
        }

        public ServiceResult<HastaBilgiViewModel> GetBilgi(string tc)
        {
            try
            {
                string tcHash = SecurityHelper.Hashle(tc);
                var bilgi = _hastaRepository.GetBilgiByTcHash(tcHash);
                if (bilgi == null) return ServiceResult<HastaBilgiViewModel>.Fail("Bilgiler getirilemedi.");
                var model = new HastaBilgiViewModel
                {
                    Ad = bilgi.Value.Ad,
                    Soyad = bilgi.Value.Soyad,
                    Telefon = bilgi.Value.Telefon
                };
                return ServiceResult<HastaBilgiViewModel>.Ok(model);
            }
            catch (Exception ex)
            {
                return ServiceResult<HastaBilgiViewModel>.Fail("Bilgileriniz getirilirken hata: " + ex.Message);
            }
        }

        public ServiceResult UpdateBilgi(string tc, HastaBilgiViewModel model)
        {
            try
            {
                string tcHash = SecurityHelper.Hashle(tc);
                _hastaRepository.UpdateTelefon(tcHash, model.Telefon);
                if (!string.IsNullOrEmpty(model.YeniSifre))
                {
                    _hastaRepository.UpdateSifre(tcHash, SecurityHelper.Hashle(model.YeniSifre), SecurityHelper.Sifrele(model.YeniSifre));
                }
                return ServiceResult.Ok("Bilgileriniz başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail("Güncelleme Hatası: " + ex.Message);
            }
        }
    }
}
