using HastaneYonetimSistemi.Models;
using HastaneYonetimSistemi.DataAccessLayer.Interfaces;
using HastaneYonetimSistemi.Services.Interfaces;
using Npgsql;
using System.Collections.Generic;

namespace HastaneYonetimSistemi.Services
{
    public class IlacService : IIlacService
    {
        private readonly IIlacRepository _ilacRepository;

        public IlacService(IIlacRepository ilacRepository)
        {
            _ilacRepository = ilacRepository;
        }

        public IEnumerable<IlacViewModel> GetAllIlaclar()
        {
            return _ilacRepository.GetAll();
        }

        public (bool Success, string Message) EkleIlac(string ilacAd, int stokAdet)
        {
            if (string.IsNullOrWhiteSpace(ilacAd))
                return (false, "İlaç adı boş olamaz.");

            try
            {
                if (_ilacRepository.ExistsByName(ilacAd))
                {
                    return (false, "Bu isimde bir ilaç zaten sistemde kayıtlı.");
                }

                _ilacRepository.Add(ilacAd, stokAdet);
                return (true, $"'{ilacAd}' ilacı eklendi.");
            }
            catch (System.Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool Success, string Message) SilIlac(int ilacId)
        {
            try
            {
                _ilacRepository.Delete(ilacId);
                return (true, "İlaç silindi.");
            }
            catch (PostgresException)
            {
                return (false, "Silinemedi: Bu ilaca bağlı reçeteler mevcut.");
            }
            catch (System.Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
