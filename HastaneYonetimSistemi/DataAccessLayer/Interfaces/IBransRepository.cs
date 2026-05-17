using HastaneYonetimSistemi.Models;

namespace HastaneYonetimSistemi.DataAccessLayer.Interfaces
{
    public interface IBransRepository
    {
        IEnumerable<BransViewModel> GetAll();
        int GetIdByName(string bransAd);
        bool ExistsByName(string bransAd);
        void Add(string bransAd);
        void Delete(int bransId);
        bool IsBransInUse(int bransId);
    }
}
