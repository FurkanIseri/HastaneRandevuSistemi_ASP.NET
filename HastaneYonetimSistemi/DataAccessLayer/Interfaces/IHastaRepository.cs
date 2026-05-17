using HastaneYonetimSistemi.Models;

namespace HastaneYonetimSistemi.DataAccessLayer.Interfaces
{
    public interface IHastaRepository
    {
        (string Ad, string Soyad)? GetAdSoyadByTcHash(string tcHash);
        Guid GetHastaIdByTcHash(string tcHash);
        IEnumerable<RandevuViewModel> GetGecmisRandevular(string tcHash);
        IEnumerable<RandevuViewModel> GetAktifRandevularByDoktorId(Guid doktorId);
        IEnumerable<string> GetAllBransNames();
        IEnumerable<object> GetDoktorlarByBrans(string brans);
        void RandevuAl(int randevuId, Guid hastaId, string sikayet, byte[] tibbiBelge);
        IEnumerable<ReceteViewModel> GetRecetelerByTcHash(string tcHash);
        (string Ad, string Soyad, string Telefon)? GetBilgiByTcHash(string tcHash);
        void UpdateTelefon(string tcHash, string telefon);
        void UpdateSifre(string tcHash, string sifreHash, string sifreSifreli);
    }
}
