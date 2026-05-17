using HastaneYonetimSistemi.DataAccessLayer.Interfaces;
using HastaneYonetimSistemi.Helpers;
using HastaneYonetimSistemi.Models;
using HastaneYonetimSistemi.Services.Interfaces;
using Npgsql;

namespace HastaneYonetimSistemi.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;

        public AuthService(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public ServiceResult GirisHasta(string tc, string sifre)
        {
            try
            {
                string tcHash = SecurityHelper.Hashle(tc);
                string sifreHash = SecurityHelper.Hashle(sifre);
                if (_authRepository.ValidateHasta(tcHash, sifreHash))
                    return ServiceResult.Ok("Hasta sistemine başarıyla giriş yaptınız!");
                return ServiceResult.Fail("Hatalı TC Kimlik No veya Şifre!");
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail("Giriş işlemi sırasında bir hata oluştu: " + ex.Message);
            }
        }

        public ServiceResult GirisDoktor(string tc, string sifre)
        {
            try
            {
                string tcHash = SecurityHelper.Hashle(tc);
                string sifreHash = SecurityHelper.Hashle(sifre);
                if (_authRepository.ValidateDoktor(tcHash, sifreHash))
                    return ServiceResult.Ok("Doktor paneline başarıyla giriş yaptınız!");
                return ServiceResult.Fail("Hatalı TC Kimlik No veya Şifre!");
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail("Giriş işlemi sırasında bir hata oluştu: " + ex.Message);
            }
        }

        public ServiceResult GirisSekreter(string tc, string sifre)
        {
            try
            {
                string tcHash = SecurityHelper.Hashle(tc);
                string sifreHash = SecurityHelper.Hashle(sifre);
                if (_authRepository.ValidateSekreter(tcHash, sifreHash))
                    return ServiceResult.Ok("Sekreter paneline başarıyla giriş yaptınız!");
                return ServiceResult.Fail("Hatalı TC Kimlik No veya Şifre!");
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail("Giriş işlemi sırasında bir hata oluştu: " + ex.Message);
            }
        }

        public ServiceResult KaydetHasta(RegisterViewModel model)
        {
            try
            {
                string tcHash = SecurityHelper.Hashle(model.TC);
                if (_authRepository.TcExistsInDoktorlar(tcHash) || _authRepository.TcExistsInSekreterler(tcHash))
                    return ServiceResult.Fail("Bu TC Kimlik Numarası sistemde personel olarak kayıtlı!");

                string tcSifreli = SecurityHelper.Sifrele(model.TC);
                string sifreHash = SecurityHelper.Hashle(model.Sifre);
                string sifreSifreli = SecurityHelper.Sifrele(model.Sifre);

                _authRepository.KaydetHasta(model.Ad, model.Soyad, tcHash, tcSifreli, model.Telefon, sifreHash, sifreSifreli, model.Cinsiyet.ToUpper());
                return ServiceResult.Ok("Kaydınız başarıyla tamamlandı ve otomatik giriş yapıldı!");
            }
            catch (PostgresException ex)
            {
                return ServiceResult.Fail("Kayıt Başarısız: " + ex.MessageText);
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail("Beklenmeyen bir hata oluştu: " + ex.Message);
            }
        }

        public ServiceResult<string> GetHastaSifre(string tc)
        {
            try
            {
                if (string.IsNullOrEmpty(tc))
                    return ServiceResult<string>.Fail("Lütfen TC Kimlik Numaranızı giriniz.");
                string tcHash = SecurityHelper.Hashle(tc);
                string? sifreSifreli = _authRepository.GetHastaSifreSifreli(tcHash);
                if (sifreSifreli == null)
                    return ServiceResult<string>.Fail("Bu TC Kimlik Numarasına ait kayıt bulunamadı.");
                return ServiceResult<string>.Ok(SecurityHelper.Coz(sifreSifreli));
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Fail("İşlem sırasında bir hata oluştu: " + ex.Message);
            }
        }

        public ServiceResult<string> GetDoktorSifre(string tc)
        {
            try
            {
                if (string.IsNullOrEmpty(tc))
                    return ServiceResult<string>.Fail("Lütfen TC Kimlik Numaranızı giriniz.");
                string tcHash = SecurityHelper.Hashle(tc);
                string? sifreSifreli = _authRepository.GetDoktorSifreSifreli(tcHash);
                if (sifreSifreli == null)
                    return ServiceResult<string>.Fail("Bu TC Kimlik Numarasına ait doktor kaydı bulunamadı.");
                return ServiceResult<string>.Ok(SecurityHelper.Coz(sifreSifreli));
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Fail("İşlem sırasında bir hata oluştu: " + ex.Message);
            }
        }

        public ServiceResult<string> GetSekreterSifre(string tc)
        {
            try
            {
                if (string.IsNullOrEmpty(tc))
                    return ServiceResult<string>.Fail("Lütfen TC Kimlik Numaranızı giriniz.");
                string tcHash = SecurityHelper.Hashle(tc);
                string? sifreSifreli = _authRepository.GetSekreterSifreSifreli(tcHash);
                if (sifreSifreli == null)
                    return ServiceResult<string>.Fail("Bu TC Kimlik Numarasına ait sekreter kaydı bulunamadı.");
                return ServiceResult<string>.Ok(SecurityHelper.Coz(sifreSifreli));
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Fail("İşlem sırasında bir hata oluştu: " + ex.Message);
            }
        }
    }
}
