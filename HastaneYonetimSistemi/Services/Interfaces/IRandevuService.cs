namespace HastaneYonetimSistemi.Services.Interfaces
{
    public interface IRandevuService
    {
        (bool Success, string Message) OlusturRandevuYuvası(string tarih, string saat, string brans, string doktorId, string sekreterTcHash);
    }
}
