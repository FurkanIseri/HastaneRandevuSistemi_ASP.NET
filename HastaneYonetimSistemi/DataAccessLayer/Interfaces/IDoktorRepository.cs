using HastaneYonetimSistemi.Models;
using System;
using System.Collections.Generic;

namespace HastaneYonetimSistemi.DataAccessLayer.Interfaces
{
    public interface IDoktorRepository
    {
        IEnumerable<DoktorListeItem> GetAllDoktorlar();
        void Add(DoktorEkleViewModel model, int bransId, Guid sekreterId, byte[]? foto);
        Guid GetSekreterIdByDoktorId(Guid doktorId);
        void Delete(Guid doktorId);
        DoktorGuncelleViewModel? GetDoktorById(Guid doktorId);
        void Update(Guid doktorId, DoktorGuncelleViewModel model, int bransId, byte[]? foto);
        IEnumerable<object> GetDoktorlarByBrans(string brans);
    }
}
