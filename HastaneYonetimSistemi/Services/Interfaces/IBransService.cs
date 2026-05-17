using HastaneYonetimSistemi.Models;
using System.Collections.Generic;

namespace HastaneYonetimSistemi.Services.Interfaces
{
    public interface IBransService
    {
        IEnumerable<BransViewModel> GetAllBranslar();
        IEnumerable<string> GetAllBransNames();
        (bool Success, string Message) EkleBrans(string bransAd);
        (bool Success, string Message) SilBrans(int bransId);
    }
}
