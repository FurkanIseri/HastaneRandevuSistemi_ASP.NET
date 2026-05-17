using HastaneYonetimSistemi.Models;
using System.Collections.Generic;

namespace HastaneYonetimSistemi.Services.Interfaces
{
    public interface IIlacService
    {
        IEnumerable<IlacViewModel> GetAllIlaclar();
        (bool Success, string Message) EkleIlac(string ilacAd, int stokAdet);
        (bool Success, string Message) SilIlac(int ilacId);
    }
}
