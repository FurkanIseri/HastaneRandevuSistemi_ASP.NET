namespace HastaneYonetimSistemi.Services.Interfaces
{
    public interface IDuyuruService
    {
        (bool Success, string Message) OlusturDuyuru(string duyuruText, string sekreterTcHash);
    }
}
