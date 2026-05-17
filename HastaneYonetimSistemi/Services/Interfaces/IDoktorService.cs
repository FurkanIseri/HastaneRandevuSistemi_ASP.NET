using HastaneYonetimSistemi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HastaneYonetimSistemi.Services.Interfaces
{
    public interface IDoktorService
    {
        IEnumerable<DoktorListeItem> GetAllDoktorlar();
        Task<(bool Success, string Message)> EkleDoktor(DoktorEkleViewModel model, string sekreterTcHash);
        (bool Success, string Message) SilDoktor(string doktorId, string sekreterTcHash);
        (bool Success, string Message, DoktorGuncelleViewModel? Model) GetDoktorForEdit(string doktorId, string sekreterTcHash);
        Task<(bool Success, string Message)> GuncelleDoktor(DoktorGuncelleViewModel model, string sekreterTcHash);
        IEnumerable<object> GetDoktorlarByBrans(string brans);
    }
}
