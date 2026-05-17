using System;
using System.Collections.Generic;
using HastaneYonetimSistemi.Models;

namespace HastaneYonetimSistemi.DataAccessLayer.Interfaces
{
    public interface ISekreterRepository
    {
        Guid GetSekreterIdByTcHash(string tcHash);
        (string Ad, string Soyad)? GetSekreterAdSoyadByTcHash(string tcHash);
        IEnumerable<SekreterListeItem> GetAll();
        void Add(string ad, string soyad, string tc, string sifre, string cinsiyet);
        void UpdatePassword(string tcHash, string yeniSifre);
    }
}
