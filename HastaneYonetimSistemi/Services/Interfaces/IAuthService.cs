using HastaneYonetimSistemi.Models;

namespace HastaneYonetimSistemi.Services.Interfaces
{
    public interface IAuthService
    {
        ServiceResult GirisHasta(string tc, string sifre);
        ServiceResult GirisDoktor(string tc, string sifre);
        ServiceResult GirisSekreter(string tc, string sifre);
        ServiceResult KaydetHasta(RegisterViewModel model);
        ServiceResult<string> GetHastaSifre(string tc);
        ServiceResult<string> GetDoktorSifre(string tc);
        ServiceResult<string> GetSekreterSifre(string tc);
    }
}
