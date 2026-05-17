using HastaneYonetimSistemi.Models;
using HastaneYonetimSistemi.DataAccessLayer.Interfaces;
using HastaneYonetimSistemi.Services.Interfaces;
using Npgsql;

namespace HastaneYonetimSistemi.Services
{
    public class BransService : IBransService
    {
        private readonly IBransRepository _bransRepository;

        public BransService(IBransRepository bransRepository)
        {
            _bransRepository = bransRepository;
        }

        public IEnumerable<BransViewModel> GetAllBranslar() => _bransRepository.GetAll();

        public IEnumerable<string> GetAllBransNames() => _bransRepository.GetAll().Select(b => b.BransAd);

        public ServiceResult EkleBrans(string bransAd)
        {
            if (string.IsNullOrWhiteSpace(bransAd))
                return ServiceResult.Fail("Branş adı boş olamaz.");
            try
            {
                if (_bransRepository.ExistsByName(bransAd))
                    return ServiceResult.Fail("Bu isimde bir branş zaten sistemde kayıtlı.");
                _bransRepository.Add(bransAd);
                return ServiceResult.Ok($"'{bransAd}' branşı eklendi.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail(ex.Message);
            }
        }

        public ServiceResult SilBrans(int bransId)
        {
            try
            {
                _bransRepository.Delete(bransId);
                return ServiceResult.Ok("Branş silindi.");
            }
            catch (PostgresException)
            {
                return ServiceResult.Fail("Silinemedi: Bu branşa bağlı kayıtlar mevcut.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail(ex.Message);
            }
        }
    }
}
