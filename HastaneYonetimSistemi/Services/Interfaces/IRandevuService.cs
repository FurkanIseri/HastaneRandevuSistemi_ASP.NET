using HastaneYonetimSistemi.Models;

namespace HastaneYonetimSistemi.Services.Interfaces
{
    public interface IRandevuService
    {
        ServiceResult OlusturRandevuYuvası(string tarih, string saat, string brans, string doktorId, string sekreterTcHash);
    }
}
