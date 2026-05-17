# Antigravity - Hastane Yönetim Sistemi
## Kapsamlı Kod İnceleme ve Mimari Analiz Raporu

**Tarih:** 16 Mayıs 2026
**Rol:** Kıdemli (Senior) .NET Yazılım Mühendisi
**Proje:** ASP.NET Core tabanlı Hastane Yönetim Sistemi (Antigravity)

---

### 1. Proje Mimarisi ve Dizin Yapısı
Proje, klasik **ASP.NET Core MVC** dizin yapısına (`Models`, `Views`, `Controllers`) sadık kalınarak oluşturulmuştur. Ancak kurumsal mimari standartları açısından değerlendirildiğinde bazı geliştirme fırsatları bulunmaktadır:

* **Fat Controller (Şişman Controller) Antipattern:** Projede "Service" (İş kuralları) veya "Repository" (Veri erişim) katmanları oluşturulmamıştır. Veritabanı sorguları (SQL komutları) doğrudan Controller sınıflarının (`DoktorController`, `SekreterController` vb.) içerisine yazılmıştır. Bu durum, Controller sınıflarının çok fazla büyümesine ve "Tek Sorumluluk Prensibi"nin (Single Responsibility) ihlal edilmesine yol açmıştır.
* **Helpers Katmanı:** Ortak kullanılan işlevler `Helpers` klasörüne toplanmıştır. Veritabanı bağlantılarını yöneten `sqlBaglanti.cs` ve şifreleme/hash işlemlerini yürüten `SecurityHelper.cs` burada konumlandırılmıştır.
* **Veri Erişim Yöntemi:** Projede Entity Framework Core gibi bir ORM aracı kullanılmamıştır. Bunun yerine `Npgsql` kütüphanesi ile **ADO.NET** standartlarında, saf SQL sorgularıyla veri tabanı iletişimi kurulmuştur.

---

### 2. Kod Seviyesi İnceleme: Anotasyonlar ve Validasyonlar
Projede doğrulama ve yönlendirme işlemleri büyük ölçüde C# Data Annotations (Veri Anotasyonları) kullanılarak yapılmıştır.

* **Yönlendirme ve HTTP Metotları:** Controller içindeki eylemler `[HttpGet]` ve `[HttpPost]` attribute'ları ile ayrıştırılmıştır. Bu yapı GET ve POST işlemlerini ayırmak için doğru kurgulanmıştır.
* **Veri Doğrulama (Data Annotations):** `Models/ViewModels.cs` içerisindeki modellerde bolca `[Required]`, `[StringLength]`, `[Range]` attribute'u kullanılmıştır.
  ```csharp
  // Örnek Kullanım:
  [Required(ErrorMessage = "TC Kimlik No zorunludur.")]
  [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır.")]
  public string TC { get; set; } = string.Empty;
  ```
* **Güvenlik ve Yetkilendirme (Zayıf Yön):** ASP.NET Core'un güçlü `[Authorize]` attribute'u kullanılmamıştır. Bunun yerine her Controller'ın içine manuel bir "Session" kontrolü eklenmiştir.
  ```csharp
  // Mevcut sistemdeki manuel kontrol
  private string? TC => HttpContext.Session.GetString("HastaTC");
  if (string.IsNullOrEmpty(TC)) return RedirectToAction("HastaGiris", "Auth");
  ```
  Bu yöntem hem kod tekrarına yol açar hem de güvenlik açıklarına (Session Hijacking) zemin hazırlayabilir.

---

### 3. Özel İşlemler ve Mekanizmalar

#### Dosya ve Resim Yükleme (Kritik Bulgu)
İncelediğim kodlarda (örneğin doktor profil fotoğrafı) dosyalar `IFormFile` arayüzü ile alınmaktadır. Ancak **dosyalar sunucuda `wwwroot` klasörüne KAYDEDİLMEMEKTEDİR!** 

Dosya bir `MemoryStream` aracılığıyla Bayt Dizisine (`byte[]`) dönüştürülüp veritabanına doğrudan `BYTEA` (PostgreSQL binary type) olarak gömülmektedir. 
* *Risk:* Bu yöntem veritabanı boyutunu çok hızlı şişirir ve sunucu maliyetlerini artırır. Resimlerin fiziksel diske kaydedilip, veritabanında sadece yolunun (path) tutulması gerekmektedir.

#### Antigravity - Şifreleme Mekanizması
Projedeki en başarılı bulduğum güvenlik katmanı `SecurityHelper.cs` olmuştur. 
* TC Kimlik numaraları direkt veritabanına yazılmamakta; arama işlemlerini hızlı yapabilmek için **SHA256 ile Hash'lenirken**, ileride okunabilmesi için **AES algoritması ile şifrelenmektedir**. Parolalar da güvenli bir şekilde hash'lenerek saklanmaktadır.

---

### 4. Veritabanı ve Konfigürasyon
* **Konfigürasyon:** Veritabanı ayarları doğru bir şekilde `appsettings.json` içerisindeki `ConnectionStrings:DefaultConnection` altına alınmıştır. 
* **Dependency Injection (DI):** `Program.cs` dosyasına baktığımızda Dependency Injection'ın yeterince efektif kullanılmadığını görüyoruz. `sqlBaglanti.cs` sınıflarda Dependency Injection ile alınmak yerine `new sqlBaglanti(...)` denilerek "Tight Coupling" (Sıkı Bağımlılık) yaratacak şekilde manuel oluşturulmuştur.

---

### 5. Soru Önerileri (Mentörlük ve Gelişim İçin)
Projeyi geliştirmek ve mimariyi Senior seviyesine taşımak için aşağıdaki soruları sorarak çalışmalara devam edebilirsiniz:

1. **"Controller'lar içine yazılmış saf SQL sorgularını, Repository Pattern kullanarak Service katmanına nasıl taşıyabiliriz?"**
2. **"Şu anki manuel Session (Oturum) kontrolleri yerine, projeyi .NET Core'un yerleşik Cookie Authentication'ına ve `[Authorize]` attribute'larına nasıl geçirebiliriz?"**
3. **"Resimleri veritabanına byte[] olarak kaydetmek yerine, sunucudaki `wwwroot/uploads` dizinine kaydedip veritabanında sadece dosya yolunu tutacak şekilde sistemi nasıl güncelleriz?"**
4. **"Mevcut Data Annotations yapısını kaldırıp, kodları daha temiz hale getirecek olan 'Fluent Validation' kütüphanesini projeye nasıl entegre edebilirim?"**

*Rapor Sonu*
