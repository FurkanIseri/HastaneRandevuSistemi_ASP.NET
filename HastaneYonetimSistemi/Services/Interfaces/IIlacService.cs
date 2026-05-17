using HastaneYonetimSistemi.Models;

namespace HastaneYonetimSistemi.Services.Interfaces
{
    public interface IIlacService
    {
        IEnumerable<IlacViewModel> GetAllIlaclar();
        ServiceResult EkleIlac(string ilacAd, int stokAdet);
        ServiceResult SilIlac(int ilacId);
    }
}
