using HastaneYonetimSistemi.Models;

namespace HastaneYonetimSistemi.Services.Interfaces
{
    public interface IDuyuruService
    {
        ServiceResult OlusturDuyuru(string duyuruText, string sekreterTcHash);
    }
}
