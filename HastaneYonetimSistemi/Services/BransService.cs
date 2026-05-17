using HastaneYonetimSistemi.Models;
using HastaneYonetimSistemi.DataAccessLayer.Interfaces;
using HastaneYonetimSistemi.Services.Interfaces;
using Npgsql;
using System.Collections.Generic;
using System.Linq;

namespace HastaneYonetimSistemi.Services
{
    public class BransService : IBransService
    {
        private readonly IBransRepository _bransRepository;

        public BransService(IBransRepository bransRepository)
        {
            _bransRepository = bransRepository;
        }

        public IEnumerable<BransViewModel> GetAllBranslar()
        {
            return _bransRepository.GetAll();
        }

        public IEnumerable<string> GetAllBransNames()
        {
            return _bransRepository.GetAll().Select(b => b.BransAd);
        }

        public (bool Success, string Message) EkleBrans(string bransAd)
        {
            if (string.IsNullOrWhiteSpace(bransAd))
                return (false, "Branş adı boş olamaz.");

            try
            {
                if (_bransRepository.ExistsByName(bransAd))
                {
                    return (false, "Bu isimde bir branş zaten sistemde kayıtlı.");
                }

                _bransRepository.Add(bransAd);
                return (true, $"'{bransAd}' branşı eklendi.");
            }
            catch (System.Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool Success, string Message) SilBrans(int bransId)
        {
            try
            {
                _bransRepository.Delete(bransId);
                return (true, "Branş silindi.");
            }
            catch (PostgresException)
            {
                return (false, "Silinemedi: Bu branşa bağlı kayıtlar mevcut.");
            }
            catch (System.Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
