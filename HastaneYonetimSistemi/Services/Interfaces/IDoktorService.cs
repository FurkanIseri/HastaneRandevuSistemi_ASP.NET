using HastaneYonetimSistemi.Models;

namespace HastaneYonetimSistemi.Services.Interfaces
{
    public interface IDoktorService
    {
        IEnumerable<DoktorListeItem> GetAllDoktorlar();
        Task<ServiceResult> EkleDoktor(DoktorEkleViewModel model, string sekreterTcHash);
        ServiceResult SilDoktor(string doktorId, string sekreterTcHash);
        ServiceResult<DoktorGuncelleViewModel> GetDoktorForEdit(string doktorId, string sekreterTcHash);
        Task<ServiceResult> GuncelleDoktor(DoktorGuncelleViewModel model, string sekreterTcHash);
        IEnumerable<object> GetDoktorlarByBrans(string brans);
        ServiceResult<(string AdSoyad, Guid DoktorId, string? ProfilBase64, IEnumerable<RandevuViewModel> Randevular, IEnumerable<IlacViewModel> Ilaclar)> GetPanelData(string tc);
        ServiceResult OlusturRecete(int randevuId, string tani, int ilacId, int adet, string kullanimSekli);
        ServiceResult<IEnumerable<YazilanReceteViewModel>> GetYazilanReceteler(string tc);
        IEnumerable<string> GetDuyurular();
        ServiceResult<(string AdSoyad, string? ProfilBase64)> GetBilgiProfil(string tc);
        ServiceResult UpdateSifre(string tc, DoktorBilgiViewModel model);
    }
}
