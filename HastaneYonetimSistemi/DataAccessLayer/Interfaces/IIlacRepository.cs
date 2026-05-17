using HastaneYonetimSistemi.Models;
using System.Collections.Generic;

namespace HastaneYonetimSistemi.DataAccessLayer.Interfaces
{
    public interface IIlacRepository
    {
        IEnumerable<IlacViewModel> GetAll();
        bool ExistsByName(string ilacAd);
        void Add(string ilacAd, int stokAdet);
        void Delete(int ilacId);
    }
}
