using HastaneYonetimSistemi.Models;

namespace HastaneYonetimSistemi.DataAccessLayer.Interfaces
{
    public interface IReceteRepository
    {
        bool ReceteVarMi(int randevuId);
        void OlusturRecete(int randevuId, string tani, int ilacId, int adet, string kullanimSekli);
        IEnumerable<YazilanReceteViewModel> GetYazilanRecetelerByDoktorId(Guid doktorId);
        Guid? GetDoktorIdByTcHash(string tcHash);
        (string Ad, string Soyad, Guid DoktorId, byte[]? ProfilFoto)? GetDoktorProfilByTcHash(string tcHash);
        IEnumerable<RandevuViewModel> GetAktifRandevularByDoktorId(Guid doktorId);
        byte[]? GetTibbiBelge(int randevuId);
        void UpdateDoktorSifre(string tcHash, string sifreHash, string sifreSifreli);
        (string Ad, string Soyad)? GetDoktorAdSoyadByTcHash(string tcHash);
    }
}
