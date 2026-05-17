using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HastaneYonetimSistemi.Models
{
    // ======================== AUTH ========================
    public class GirisViewModel
    {
        [Required(ErrorMessage = "TC Kimlik No zorunludur.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır.")]
        public string TC { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        public string Sifre { get; set; } = string.Empty;
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        public string Soyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "TC Kimlik No alanı zorunludur.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır.")]
        public string TC { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon alanı zorunludur.")]
        public string Telefon { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        public string Sifre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cinsiyet alanı zorunludur.")]
        public string Cinsiyet { get; set; } = string.Empty;
    }

    // ======================== RANDEVU ========================
    public class RandevuViewModel
    {
        public int ID { get; set; }
        public string Brans { get; set; } = string.Empty;
        public string Doktor { get; set; } = string.Empty;
        public DateTime Tarih { get; set; }
        public TimeSpan Saat { get; set; }
        public string Sikayet { get; set; } = string.Empty;
        public bool TibbiBelgeVar { get; set; }
        public bool IsCompleted { get; set; }
        public string HastaAd { get; set; } = string.Empty;
    }

    public class RandevuAlViewModel
    {
        [Required(ErrorMessage = "Geçersiz randevu seçimi.")]
        public int RandevuId { get; set; }

        public string Sikayet { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lütfen tıbbi belgenizi veya şikayetinizin fotoğrafını yükleyiniz.")]
        public IFormFile TibbiBelge { get; set; } = null!;
    }

    // ======================== RECETE ========================
    public class ReceteViewModel
    {
        public int ReceteID { get; set; }
        public string Tani { get; set; } = string.Empty;
        public int IlacID { get; set; }
        public string IlacAd { get; set; } = string.Empty;
        public string KullanimTalimati { get; set; } = string.Empty;
        public int Adet { get; set; }
        public string HastaAd { get; set; } = string.Empty;
        public string Doktor { get; set; } = string.Empty;
    }

    public class ReceteOlusturViewModel
    {
        [Required(ErrorMessage = "Randevu seçimi zorunludur.")]
        public int RandevuId { get; set; }

        [Required(ErrorMessage = "Tanı alanı zorunludur.")]
        public string Tani { get; set; } = string.Empty;

        [Required(ErrorMessage = "İlaç seçimi zorunludur.")]
        public int IlacId { get; set; }

        [Range(1, 999, ErrorMessage = "Adet 1 ile 999 arasında olmalıdır.")]
        public int Adet { get; set; } = 1;

        [Required(ErrorMessage = "Kullanım şekli zorunludur.")]
        public string KullanimSekli { get; set; } = string.Empty;

        public int ReceteNo { get; set; }
    }

    // ======================== DOKTOR ========================
    public class DoktorEkleViewModel
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        public string Soyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "TC Kimlik No zorunludur.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır.")]
        public string TC { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        public string Sifre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Branş alanı zorunludur.")]
        public string Brans { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cinsiyet alanı zorunludur.")]
        public string Cinsiyet { get; set; } = string.Empty;

        // Optional profile photo
        public IFormFile? ProfilFotografi { get; set; }
    }

    public class DoktorListeItem
    {
        public string DoktorId { get; set; } = string.Empty;
        public string AdSoyad { get; set; } = string.Empty;
        public string Brans { get; set; } = string.Empty;
        public string SekreterId { get; set; } = string.Empty;
        public byte[]? ProfilFotografi { get; set; }
        public string ProfilFotoBase64 => ProfilFotografi != null
            ? "data:image/jpeg;base64," + Convert.ToBase64String(ProfilFotografi)
            : string.Empty;
    }

    public class DoktorBilgiViewModel
    {
        public string AdSoyad { get; set; } = string.Empty;
        public string? YeniSifre { get; set; }
        public string? SifreTekrar { get; set; }
    }

    // ======================== BRANS ========================
    public class BransViewModel
    {
        public int BransId { get; set; }

        [Required(ErrorMessage = "Branş adı zorunludur.")]
        public string BransAd { get; set; } = string.Empty;
    }

    // ======================== ILAC ========================
    public class IlacViewModel
    {
        public int IlacID { get; set; }

        [Required(ErrorMessage = "İlaç adı zorunludur.")]
        public string IlacAd { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Stok adedi 0 veya daha büyük olmalıdır.")]
        public int StokAdet { get; set; }
    }

    // ======================== SEKRETER ========================
    public class SekreterEkleViewModel
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        public string Soyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "TC Kimlik No zorunludur.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır.")]
        public string TC { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        public string Sifre { get; set; } = string.Empty;
    }

    public class SekreterListeItem
    {
        public string SekreterID { get; set; } = string.Empty;
        public string AdSoyad { get; set; } = string.Empty;
        public string RolAd { get; set; } = "Sistem Yöneticisi";
    }

    public class SekreterBilgiViewModel
    {
        public string AdSoyad { get; set; } = string.Empty;
        public string? YeniSifre { get; set; }
        public string? SifreTekrar { get; set; }
    }

    // ======================== HASTA ========================
    public class HastaBilgiViewModel
    {
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon alanı zorunludur.")]
        public string Telefon { get; set; } = string.Empty;

        public string? YeniSifre { get; set; }
        public string? SifreTekrar { get; set; }
    }

    // ======================== DOKTOR GUNCELLE ========================
    public class DoktorGuncelleViewModel
    {
        public string DoktorId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        public string Soyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Branş alanı zorunludur.")]
        public string Brans { get; set; } = string.Empty;

        // Optional: only fill to change password
        public string? YeniSifre { get; set; }

        // Optional: new profile photo
        public IFormFile? ProfilFotografi { get; set; }
    }
}

