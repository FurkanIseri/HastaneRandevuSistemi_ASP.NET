using HastaneYonetimSistemi.Models;

namespace HastaneYonetimSistemi.Services.Interfaces
{
    public interface IBransService
    {
        IEnumerable<BransViewModel> GetAllBranslar();
        IEnumerable<string> GetAllBransNames();
        ServiceResult EkleBrans(string bransAd);
        ServiceResult SilBrans(int bransId);
    }
}
