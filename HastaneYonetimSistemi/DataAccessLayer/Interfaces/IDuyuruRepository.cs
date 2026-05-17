using System;

namespace HastaneYonetimSistemi.DataAccessLayer.Interfaces
{
    public interface IDuyuruRepository
    {
        void Add(string duyuruText, Guid sekreterId);
    }
}
