using System;

namespace HastaneYonetimSistemi.DataAccessLayer.Interfaces
{
    public interface IRandevuRepository
    {
        void AddRandevuYuvası(DateTime tarih, string saat, int bransId, Guid doktorId, bool durum);
    }
}
