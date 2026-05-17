using HastaneYonetimSistemi.Models;
using HastaneYonetimSistemi.DataAccessLayer.Interfaces;
using HastaneYonetimSistemi.Services.Interfaces;
using Npgsql;

namespace HastaneYonetimSistemi.Services
{
    public class IlacService : IIlacService
    {
        private readonly IIlacRepository _ilacRepository;

        public IlacService(IIlacRepository ilacRepository)
        {
            _ilacRepository = ilacRepository;
        }

        public IEnumerable<IlacViewModel> GetAllIlaclar() => _ilacRepository.GetAll();

        public ServiceResult EkleIlac(string ilacAd, int stokAdet)
        {
            if (string.IsNullOrWhiteSpace(ilacAd))
                return ServiceResult.Fail("İlaç adı boş olamaz.");
            try
            {
                if (_ilacRepository.ExistsByName(ilacAd))
                    return ServiceResult.Fail("Bu isimde bir ilaç zaten sistemde kayıtlı.");
                _ilacRepository.Add(ilacAd, stokAdet);
                return ServiceResult.Ok($"'{ilacAd}' ilacı eklendi.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail(ex.Message);
            }
        }

        public ServiceResult SilIlac(int ilacId)
        {
            try
            {
                _ilacRepository.Delete(ilacId);
                return ServiceResult.Ok("İlaç silindi.");
            }
            catch (PostgresException)
            {
                return ServiceResult.Fail("Silinemedi: Bu ilaca bağlı reçeteler mevcut.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail(ex.Message);
            }
        }
    }
}
