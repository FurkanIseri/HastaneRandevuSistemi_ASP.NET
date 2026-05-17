using HastaneYonetimSistemi.Models;

namespace HastaneYonetimSistemi.Services.Interfaces
{
    public interface IHastaService
    {
        ServiceResult<(string AdSoyad, IEnumerable<RandevuViewModel> GecmisRandevular)> GetPanelData(string tc);
        IEnumerable<string> GetBranslar();
        IEnumerable<object> GetDoktorlarByBrans(string brans);
        IEnumerable<RandevuViewModel> GetAktifRandevular(string doktorId);
        Task<ServiceResult> RandevuAl(string tc, RandevuAlViewModel model);
        IEnumerable<ReceteViewModel> GetReceteler(string tc);
        ServiceResult<HastaBilgiViewModel> GetBilgi(string tc);
        ServiceResult UpdateBilgi(string tc, HastaBilgiViewModel model);
    }
}
