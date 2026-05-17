import docx
from docx import Document
from docx.shared import Pt, Inches, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml import parse_xml
from docx.oxml.ns import nsdecls

def create_document():
    doc = Document()
    
    # Sayfa Kenar Boşluklarını Ayarla (1 inç)
    for section in doc.sections:
        section.top_margin = Inches(1)
        section.bottom_margin = Inches(1)
        section.left_margin = Inches(1)
        section.right_margin = Inches(1)

    # Renk Paleti
    NAVY = RGBColor(26, 82, 118)      # Başlıklar
    GRAY = RGBColor(127, 140, 141)    # Alt başlıklar / Meta
    CHARCOAL = RGBColor(44, 62, 80)   # Gövde Metni
    CODE_COLOR = RGBColor(192, 57, 43)# Kod içi vurgular

    # Shading Helper
    def set_cell_background(cell, fill_color="F2F4F4"):
        tcPr = cell._tc.get_or_add_tcPr()
        shd = parse_xml(f'<w:shd {nsdecls("w")} w:fill="{fill_color}"/>')
        tcPr.append(shd)

    # Border Helper
    def set_cell_borders(cell, color="BDC3C7", sz="4", val="single"):
        tcPr = cell._tc.get_or_add_tcPr()
        tcBorders = parse_xml(f'''
            <w:tcBorders {nsdecls("w")}>
                <w:top w:val="{val}" w:sz="{sz}" w:space="0" w:color="{color}"/>
                <w:left w:val="{val}" w:sz="{sz}" w:space="0" w:color="{color}"/>
                <w:bottom w:val="{val}" w:sz="{sz}" w:space="0" w:color="{color}"/>
                <w:right w:val="{val}" w:sz="{sz}" w:space="0" w:color="{color}"/>
            </w:tcBorders>
        ''')
        tcPr.append(tcBorders)

    # Kod Bloğu Ekleme Fonksiyonu
    def add_code_block(doc, code_text):
        table = doc.add_table(rows=1, cols=1)
        table.alignment = WD_ALIGN_PARAGRAPH.CENTER
        cell = table.cell(0, 0)
        set_cell_background(cell, "F8F9F9")
        set_cell_borders(cell, "D5D8DC")
        
        # İç kenar boşluğu efekti için paragrafa spacing ekleyelim
        p = cell.paragraphs[0]
        p.paragraph_format.left_indent = Inches(0.15)
        p.paragraph_format.right_indent = Inches(0.15)
        p.paragraph_format.space_before = Pt(6)
        p.paragraph_format.space_after = Pt(6)
        p.paragraph_format.line_spacing = 1.15
        
        run = p.add_run(code_text)
        run.font.name = 'Consolas'
        run.font.size = Pt(9)
        run.font.color.rgb = RGBColor(33, 47, 61)
        
        # Paragraf sonu boşluk
        doc.add_paragraph().paragraph_format.space_after = Pt(6)

    # Başlık Stili Yardımcısı
    def add_heading_1(text):
        p = doc.add_paragraph()
        p.paragraph_format.space_before = Pt(18)
        p.paragraph_format.space_after = Pt(6)
        p.paragraph_format.keep_with_next = True
        run = p.add_run(text)
        run.font.name = 'Calibri'
        run.font.size = Pt(16)
        run.font.bold = True
        run.font.color.rgb = NAVY
        return p

    def add_heading_2(text):
        p = doc.add_paragraph()
        p.paragraph_format.space_before = Pt(12)
        p.paragraph_format.space_after = Pt(4)
        p.paragraph_format.keep_with_next = True
        run = p.add_run(text)
        run.font.name = 'Calibri'
        run.font.size = Pt(13)
        run.font.bold = True
        run.font.color.rgb = RGBColor(41, 128, 185)
        return p

    def add_body_paragraph(text, bold_prefix=None, bullet=False):
        p = doc.add_paragraph(style='List Bullet' if bullet else 'Normal')
        p.paragraph_format.space_after = Pt(6)
        p.paragraph_format.line_spacing = 1.15
        
        if bold_prefix:
            run_pre = p.add_run(bold_prefix)
            run_pre.font.name = 'Calibri'
            run_pre.font.size = Pt(11)
            run_pre.font.bold = True
            run_pre.font.color.rgb = CHARCOAL

        run = p.add_run(text)
        run.font.name = 'Calibri'
        run.font.size = Pt(11)
        run.font.color.rgb = CHARCOAL
        return p

    # --- KAPAK SAYFASI ---
    title_p = doc.add_paragraph()
    title_p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    title_p.paragraph_format.space_before = Pt(120)
    title_p.paragraph_format.space_after = Pt(12)
    run_title = title_p.add_run("HASTANE YÖNETİM SİSTEMİ")
    run_title.font.name = 'Calibri'
    run_title.font.size = Pt(28)
    run_title.font.bold = True
    run_title.font.color.rgb = NAVY

    subtitle_p = doc.add_paragraph()
    subtitle_p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    subtitle_p.paragraph_format.space_after = Pt(180)
    run_sub = subtitle_p.add_run("A'dan Z'ye Mimari ve Teknik Dokümantasyon Raporu\nC# ASP.NET Core 8 MVC & PostgreSQL N-Tier Refactoring")
    run_sub.font.name = 'Calibri'
    run_sub.font.size = Pt(14)
    run_sub.font.color.rgb = GRAY

    meta_p = doc.add_paragraph()
    meta_p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    run_meta = meta_p.add_run("Hazırlayan: Kıdemli Yazılım Mühendisi & Sistem Mimarı\nTarih: Mayıs 2026\nSürüm: 2.0 (N-Tier Production)")
    run_meta.font.name = 'Calibri'
    run_meta.font.size = Pt(10)
    run_meta.font.italic = True
    run_meta.font.color.rgb = GRAY

    doc.add_page_break()

    # --- 1. PROJEYE GENEL BAKIŞ VE AMAÇ ---
    add_heading_1("1. Projeye Genel Bakış ve Amaç")
    
    add_body_paragraph(
        "Bu teknik doküman, legacy (eski nesil) Windows Forms yapısından modernize edilerek robust, "
        "modüler ve kurumsal standartlara uygun hale getirilen ASP.NET Core 8 MVC tabanlı Hastane Yönetim Sistemi'nin "
        "mimari yapısını, veritabanı entegrasyonlarını, veri akış süreçlerini ve Türkçe karakter kodlama düzeltmelerini "
        "kapsamlı bir şekilde ele almaktadır."
    )
    
    add_body_paragraph(
        "Sistem, tek bir hastane kapsamındaki iş akışlarını dijitalleştirmek üzere tasarlanmıştır. "
        "Randevu oluşturma, reçete yazma, doktor, branş ve ilaç yönetimi gibi kritik tıbbi ve idari süreçler, "
        "rol tabanlı bir mimari altında uçtan uca yönetilmektedir. Projede güvenlik ön planda tutulmuş olup, "
        "kişisel veriler (TC Kimlik No, Parolalar vb.) SHA256 hashing ve AES256 simetrik şifreleme yöntemleriyle "
        "veri tabanında güvenli bir şekilde saklanmaktadır."
    )

    # --- 2. N-TIER (ÇOK KATMANLI) MİMARİ YAPISI ---
    add_heading_1("2. N-Tier (Çok Katmanlı) Mimari Yapısı")
    
    add_body_paragraph(
        "Proje başlangıçta, tüm veritabanı sorgularının ve iş mantığının doğrudan Controller sınıflarının içerisine "
        "yazıldığı 'Fat Controller' antipattern'ine sahipti. Bu durum kodun tekrar kullanılabilirliğini sıfırlamakta, "
        "test edilebilirliği engellemekte ve bakım maliyetlerini olağanüstü artırmaktaydı. Kurumsal mimari standartlarına "
        "ulaşmak amacıyla, proje başarılı bir şekilde N-Tier (Çok Katmanlı) mimariye dönüştürülmüştür."
    )

    add_body_paragraph(
        "Katmanlı mimaride sorumlulukların ayrılması (Separation of Concerns) ve gevşek bağlılık (Loose Coupling) "
        "ilkeleri esas alınmıştır. Katmanlar arasındaki iletişim tamamen Arayüzler (Interfaces) aracılığıyla "
        "ve ASP.NET Core'un yerleşik Dependency Injection (Bağımlılık Enjeksiyonu) konteyneri kullanılarak sağlanmaktadır."
    )

    add_heading_2("Katmanların Tanımları ve Sorumlulukları")

    add_body_paragraph(
        " İstemciden gelen HTTP isteklerini (GET/POST/AJAX) karşılar, yetkilendirme / session "
        "(oturum) kontrollerini yürütür, gerekli verileri ViewModel'lere bağlar ve nihai HTML Görünümünü (View) "
        "veya JSON yanıtlarını istemciye geri döner. Kesinlikle doğrudan SQL sorgusu veya ağır iş mantığı barındırmaz.",
        bold_prefix="Presentation / Controller Katmanı: ", bullet=True
    )

    add_body_paragraph(
        " Uygulamanın beynidir. İş kurallarını (business rules) doğrular. "
        "Örneğin, bir doktorun silinmesi veya güncellenmesi işlemlerinde, işlemi yapan sekreterin o doktoru sisteme "
        "kaydeden asıl sekreter olup olmadığını doğrular. Ayrıca, formlardan gelen IFormFile formatındaki profil "
        "fotoğraflarını okuyarak veritabanına kaydedilecek byte[] dizilerine (binary data) dönüştürme gibi veri manipülasyonu "
        "işlemlerini gerçekleştirir.",
        bold_prefix="Business Logic / Service Katmanı: ", bullet=True
    )

    add_body_paragraph(
        " Veritabanı (PostgreSQL) ile olan tüm doğrudan fiziksel etkileşimi yönetir. "
        "Npgsql kütüphanesini kullanarak bağlantı havuzunu (connection pool) yöneten sqlBaglanti sınıfı ile konuşur. "
        "SQL sorgularını hazırlar, parametreleri SQL enjeksiyonuna (SQL Injection) karşı güvenli hale getirecek şekilde bind eder, "
        "stored procedure'leri ve transaction'ları tetikler.",
        bold_prefix="DataAccessLayer / Repository Katmanı: ", bullet=True
    )

    add_body_paragraph(
        " Katmanlar arasında veri taşımak ve veri tutarlılığını korumak için kullanılan sınıflardır. "
        "Özellikle ViewModels.cs dosyasında toplanan modeller, giriş seviyesindeki veri doğrulamalarını "
        "(Data Annotations) üstlenir. Örneğin, [Required], [StringLength(11)] gibi niteliklerle TC Kimlik numaralarının "
        "11 haneli ve zorunlu olması gibi kurallar daha sunum katmanının girişinde denetlenir.",
        bold_prefix="Models / ViewModels (Veri Modelleri): ", bullet=True
    )

    # --- 3. KATMANLAR ARASI VERİ AKIŞI VE SÜREÇLER ---
    add_heading_1("3. Katmanlar Arası Veri Akışı ve Süreçler")
    
    add_body_paragraph(
        "Sistemde veri akışı, istemcinin tarayıcısından başlar ve sırasıyla Controller, Service ve Repository "
        "katmanlarından geçerek PostgreSQL veritabanına ulaşır. İşlem tamamlandığında ise sonuç tam tersi "
        "istikamette geriye dönerek istemciye iletilir."
    )

    add_body_paragraph(
        "Bu akışı somutlaştırmak için 'Sekreterin yeni bir Doktor Eklemesi' (DoktorEkle POST) senaryosunu adım adım inceleyelim:"
    )

    add_body_paragraph(
        "Sekreter, tarayıcıda doktor ekleme formunu doldurur. Ad, Soyad, TC, Şifre, Branş ve isteğe bağlı profil "
        "fotoğrafını girip 'Kaydet' butonuna bastığında tarayıcı, '/Sekreter/DoktorEkle' adresine multipart/form-data "
        "tipinde bir HTTP POST isteği gönderir.",
        bold_prefix="Adım 1 - İstemci İsteyi (Client Request): ", bullet=False
    )

    add_body_paragraph(
        "SekreterController sınıfındaki [HttpPost] DoktorEkle(DoktorEkleViewModel model) "
        "eylem metodu tetiklenir. İlk olarak ModelState.IsValid kontrolü yapılarak gelen verilerin format kurallarına "
        "uygunluğu (örneğin TC'nin 11 haneli olup olmadığı) denetlenir. Ardından sekreterin aktif oturumu (Session) doğrulanır. "
        "Oturumdaki sekreterin TC kimlik numarası SecurityHelper yardımıyla hash'lenerek, iş mantığını yürütmek üzere "
        "IDoktorService.EkleDoktor(model, sekreterTcHash) metoduna parametre olarak geçirilir.",
        bold_prefix="Adım 2 - Controller Karşılama ve İlk Kontroller: ", bullet=False
    )

    add_body_paragraph(
        "DoktorService sınıfındaki EkleDoktor metodu çalışır. Burada iş mantığı kuralları devreye girer: "
        "1) Seçilen branş isminin veritabanındaki karşılığı (bransId) IBransRepository kullanılarak sorgulanır. Branş bulunamazsa hata dönülür. "
        "2) İşlemi yapan sekreterin veritabanındaki benzersiz UUID/Guid değeri ISekreterRepository ile teyit edilir. "
        "3) Gelen modeldeki ProfilFotografi (IFormFile) dosyası kontrol edilir. Eğer bir dosya yüklendiyse, bir MemoryStream "
        "yardımıyla asenkron olarak okunur ve byte[] dizisine dönüştürülür. Tüm bu hazırlıklar tamamlandıktan sonra "
        "_doktorRepository.Add(model, bransId, sekreterIdVal, foto) metodu çağrılarak veri erişim katmanına aktarılır.",
        bold_prefix="Adım 3 - Service Katmanı İş Kuralları ve Veri Hazırlama: ", bullet=False
    )

    add_body_paragraph(
        "DoktorRepository sınıfındaki Add metodu, ADO.NET (Npgsql) standartlarına uygun olarak çalışır. "
        "sqlBaglanti.baglanti() üzerinden havuzdan açık bir PostgreSQL bağlantısı alır. NpgsqlCommand nesnesi oluşturarak "
        "INSERT INTO doktorlar (...) VALUES (@p1, @p2...) sorgusunu hazırlar. Parametreler eklenirken SecurityHelper sınıfı "
        "çağrılarak doktorun TC Kimlik No ve Parolası hem SHA256 ile tek yönlü hash'lenir (arama ve eşleştirme için), "
        "hem de AES256 ile çift yönlü şifrelenir (gerektiğinde geri çözüp okuyabilmek için). Profil fotoğrafı byte dizisi "
        "NpgsqlDbType.Bytea tipinde veritabanına güvenle bind edilir. cmd.ExecuteNonQuery() komutu çalıştırılarak veri tabanında "
        "fiziksel kayıt oluşturulur.",
        bold_prefix="Adım 4 - DataAccessLayer (Veri Erişim) ve PostgreSQL İşlemi: ", bullet=False
    )

    add_body_paragraph(
        "Veritabanı kayıt işlemini başarıyla tamamladığında Repository geriye döner, Service katmanı "
        "başarılı işlem sonucunu ve kullanıcı dostu bir mesajı (Success=true, Message='Dr. Ahmet Yılmaz başarıyla eklendi.') "
        "Controller'a iletir. Controller ise TempData['Message'] üzerinden bu bilgiyi View'a aktarır ve sekreteri doktor listesi "
        "ekranına yönlendirir. SweetAlert2 kütüphanesi ile kullanıcıya başarılı kayıt animasyonu gösterilir.",
        bold_prefix="Adım 5 - Yanıt Dönüşü ve Kullanıcı Bildirimi: ", bullet=False
    )

    # --- 4. MİMARİYİ KANITLAYAN KOD ÖRNEKLERİ ---
    add_heading_1("4. Mimariyi Kanıtlayan Kod Örnekleri")
    
    add_body_paragraph(
        "N-Tier mimarimizin katı kurallara uygunluğunu göstermek amacıyla, yukarıda detaylandırılan "
        "veri akışı senaryosundaki gerçek C# kod parçacıkları aşağıda sırasıyla sunulmuştur."
    )

    add_heading_2("A. Veri Giriş ve Doğrulama Modeli (ViewModels.cs)")
    add_body_paragraph(
        "İstemciden veriyi teslim alan ve sunum katmanında ilk validasyonu gerçekleştiren model yapısı:"
    )
    add_code_block(doc, 
"namespace HastaneYonetimSistemi.Models\n"
"{\n"
"    public class DoktorEkleViewModel\n"
"    {\n"
"        [Required(ErrorMessage = \"Ad alanı zorunludur.\")]\n"
"        public string Ad { get; set; } = string.Empty;\n\n"
"        [Required(ErrorMessage = \"Soyad alanı zorunludur.\")]\n"
"        public string Soyad { get; set; } = string.Empty;\n\n"
"        [Required(ErrorMessage = \"TC Kimlik No zorunludur.\")]\n"
"        [StringLength(11, MinimumLength = 11, ErrorMessage = \"TC Kimlik No 11 haneli olmalıdır.\")]\n"
"        public string TC { get; set; } = string.Empty;\n\n"
"        [Required(ErrorMessage = \"Şifre zorunludur.\")]\n"
"        public string Sifre { get; set; } = string.Empty;\n\n"
"        [Required(ErrorMessage = \"Branş alanı zorunludur.\")]\n"
"        public string Brans { get; set; } = string.Empty;\n\n"
"        [Required(ErrorMessage = \"Cinsiyet alanı zorunludur.\")]\n"
"        public string Cinsiyet { get; set; } = string.Empty;\n\n"
"        // İsteğe bağlı profil fotoğrafı\n"
"        public IFormFile? ProfilFotografi { get; set; }\n"
"    }\n"
"}")

    add_heading_2("B. Sunum Katmanı: SekreterController.cs")
    add_body_paragraph(
        "Metot, HTTP POST isteğini karşılar, yetki ve oturum durumunu denetler ve işi Servis katmanına delege eder:"
    )
    add_code_block(doc,
"[HttpPost]\n"
"public async Task<IActionResult> DoktorEkle(DoktorEkleViewModel model)\n"
"{\n"
"    if (string.IsNullOrEmpty(TC)) return RedirectIfNoSession();\n"
"    ViewBag.Branslar = _bransService.GetAllBransNames();\n"
"    if (!ModelState.IsValid) return View(model);\n"
"    \n"
"    var hash = SecurityHelper.Hashle(TC!);\n"
"    var result = await _doktorService.EkleDoktor(model, hash);\n"
"    \n"
"    if (result.Success)\n"
"    {\n"
"        TempData[\"Message\"] = result.Message;\n"
"        return RedirectToAction(\"DoktorListesi\");\n"
"    }\n"
"    \n"
"    ModelState.AddModelError(\"\", result.Message);\n"
"    return View(model);\n"
"}")

    add_heading_2("C. İş Mantığı Katmanı: DoktorService.cs")
    add_body_paragraph(
        "Servis katmanı, branş ve sekreter doğrulamalarını yapar, profil resmini byte dizisine çevirir:"
    )
    add_code_block(doc,
"public async Task<(bool Success, string Message)> EkleDoktor(DoktorEkleViewModel model, string sekreterTcHash)\n"
"{\n"
"    try\n"
"    { \n"
"        int bransId = _bransRepository.GetIdByName(model.Brans);\n"
"        if (bransId == 0) return (false, \"Branş bulunamadı.\");\n"\
"        \n"
"        Guid sekreterIdVal = _sekreterRepository.GetSekreterIdByTcHash(sekreterTcHash);\n"
"        if (sekreterIdVal == Guid.Empty) return (false, \"Oturumdaki sekreter bulunamadı.\");\n"\
"        \n"
"        byte[]? foto = null;\n"
"        if (model.ProfilFotografi?.Length > 0)\n"
"        {\n"
"            using var ms = new MemoryStream();\n"
"            await model.ProfilFotografi.CopyToAsync(ms);\n"
"            foto = ms.ToArray();\n"
"        }\n"\
"        \n"
"        _doktorRepository.Add(model, bransId, sekreterIdVal, foto);\n"
"        return (true, $\"Dr. {model.Ad} {model.Soyad} başarıyla eklendi.\");\n"
"    }\n"
"    catch (Exception ex)\n"
"    {\n"
"        return (false, ex.Message);\n"
"    }\n"
"}")

    add_heading_2("D. Veri Erişim Katmanı: DoktorRepository.cs")
    add_body_paragraph(
        "ADO.NET ve Npgsql sürücüsü ile veritabanına doğrudan bağlantı kuran, parametrik güvenlik sağlayan katman:"
    )
    add_code_block(doc,
"public void Add(DoktorEkleViewModel model, int bransId, Guid sekreterId, byte[]? foto)\n"
"{\n"
"    using var conn = _bgl.baglanti();\n"
"    using var cmd = new NpgsqlCommand(\n"
"        \"INSERT INTO doktorlar (doktor_ad, doktor_soyad, tc_hash, tc_sifreli, sifre_hash, sifre_sifreli, brans_id, sekreter_id, profil_fotografi, cinsiyet) \" +\n"
"        \"VALUES (@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10)\", conn);\n"
"    \n"
"    cmd.Parameters.AddWithValue(\"@p1\", model.Ad ?? string.Empty);\n"
"    cmd.Parameters.AddWithValue(\"@p2\", model.Soyad ?? string.Empty);\n"
"    cmd.Parameters.AddWithValue(\"@p3\", SecurityHelper.Hashle(model.TC ?? string.Empty));\n"
"    cmd.Parameters.AddWithValue(\"@p4\", SecurityHelper.Sifrele(model.TC ?? string.Empty));\n"
"    cmd.Parameters.AddWithValue(\"@p5\", SecurityHelper.Hashle(model.Sifre ?? string.Empty));\n"
"    cmd.Parameters.AddWithValue(\"@p6\", SecurityHelper.Sifrele(model.Sifre ?? string.Empty));\n"
"    cmd.Parameters.AddWithValue(\"@p7\", bransId);\n"
"    cmd.Parameters.AddWithValue(\"@p8\", sekreterId);\n"
"    cmd.Parameters.Add(new NpgsqlParameter(\"@p9\", NpgsqlDbType.Bytea) { Value = foto != null ? (object)foto : DBNull.Value });\n"
"    cmd.Parameters.AddWithValue(\"@p10\", model.Cinsiyet ?? string.Empty);\n"
"    cmd.ExecuteNonQuery();\n"
"}")

    # --- 5. POSTGRESQL BAĞLANTI YÖNETİMİ VE GÜVENLİK ---
    add_heading_1("5. PostgreSQL Bağlantı Yönetimi ve Güvenlik")
    
    add_body_paragraph(
        "Uygulamanın veri tabanı ayağı, açık kaynak kodlu ve kurumsal düzeyde performans sunan PostgreSQL ilişkisel "
        "veritabanı yönetim sistemi üzerine inşa edilmiştir. .NET dünyasındaki PostgreSQL sürücüsü olan Npgsql "
        "kütüphanesi aracılığıyla veri tabanı iletişimi yürütülmektedir."
    )

    add_heading_2("A. PostgreSQL Bağlantı Yönetimi (sqlBaglanti.cs)")
    add_body_paragraph(
        "Veritabanı bağlantı bilgilerinin doğrudan C# kodunun içerisine yazılması (hardcoded), güvenlik açığı oluşturacağı gibi "
        "sunucu değişikliklerinde kodun yeniden derlenmesini gerektirir. Bu sebeple bağlantı dizesi (Connection String) "
        "appsettings.json dosyasına taşınmıştır. sqlBaglanti sınıfı, ASP.NET Core'un yerleşik IConfiguration servisini "
        "kullanarak bu bağlantı dizesini çalışma zamanında dinamik olarak okur:"
    )
    add_code_block(doc,
"using Npgsql;\n\n"
"namespace HastaneYonetimSistemi.Helpers\n"
"{\n"
"    public class sqlBaglanti\n"
"    {\n"
"        private readonly IConfiguration _configuration;\n\n"
"        public sqlBaglanti(IConfiguration configuration)\n"
"        { \n"
"            _configuration = configuration;\n"
"        }\n\n"
"        public NpgsqlConnection baglanti()\n"
"        {\n"
"            string baglantiAdresi = _configuration.GetConnectionString(\"DefaultConnection\") \n"
"                ?? \"Host=localhost;Port=5432;Database=HastaneYonetimSistemi;Username=postgres;Password=12345\";\n"
"            \n"
"            NpgsqlConnection baglanma = new NpgsqlConnection(baglantiAdresi);\n"
"            \n"
"            if (baglanma.State != System.Data.ConnectionState.Open)\n"
"            {\n"
"                baglanma.Open();\n"
"            }\n"
"            return baglanma;\n"
"        }\n"
"    }\n"
"}")

    add_body_paragraph(
        "NpgsqlConnection nesnesi oluşturulduktan sonra, eğer bağlantı açık değilse baglanma.Open() komutuyla bağlantı "
        "fiziksel olarak açılır. Repository katmanında kullanılan using var conn = _bgl.baglanti(); ifadesi sayesinde, "
        "ilgili metot sonlandığında C# çöp toplayıcısı (Garbage Collector) tarafından NpgsqlConnection otomatik olarak kapatılır "
        "ve veritabanı bağlantı havuzuna (Connection Pool) iade edilir. Bu sayede sunucu kaynaklarında sızıntı (resource leak) "
        "yaşanması önlenmiş olur."
    )

    add_heading_2("B. Gelişmiş Şifreleme ve Güvenlik Altyapısı (SecurityHelper.cs)")
    add_body_paragraph(
        "Kişisel Verilerin Korunması Kanunu (KVKK) standartlarına tam uyum sağlamak amacıyla sistemde hibrit bir "
        "kriptografi yaklaşımı uygulanmıştır. TC Kimlik numaraları ve şifreler veritabanına düz metin (plain text) olarak yazılmaz."
    )

    add_body_paragraph(
        " Arama ve kimlik doğrulama işlemlerinde performans kazanmak amacıyla SHA256 "
        "algoritmasıyla tek yönlü (one-way) hash oluşturulur. SHA256 hash işlemi geri döndürülemez bir parmak izi üretir. "
        "Veritabanında tc_hash ve sifre_hash alanlarında saklanır. Giriş yapılırken kullanıcının girdiği veri de hash'lenerek "
        "bu parmak izi ile karşılaştırılır.",
        bold_prefix="1. SHA256 Hashing: ", bullet=True
    )

    add_body_paragraph(
        " Hastanın veya doktorun kimlik numarasını gerektiğinde sistem içinde göstermek veya "
        "şifremi unuttum ekranlarında aslına geri döndürmek amacıyla AES256 (Advanced Encryption Standard) simetrik "
        "şifreleme algoritması kullanılır. Bu işlem için 32 karakterlik (256-bit) gizli bir SecretKey anahtarı ve "
        "rastgele üretilen 16-byte'lık bir IV (Initialization Vector) değeri harmanlanır. Şifrelenen veri ve IV birleştirilerek "
        "Base64 formatında tc_sifreli ve sifre_sifreli alanlarında saklanır.",
        bold_prefix="2. AES256 Simetrik Şifreleme: ", bullet=True
    )

    # --- 6. TÜRKÇE KARAKTER VE KODLAMA DÜZELTMELERİ ---
    add_heading_1("6. Türkçe Karakter ve Kodlama Düzeltmeleri")
    
    add_body_paragraph(
        "Yazılım geliştirme sürecinde özellikle Windows platformunda ve IIS/Visual Studio ortamlarında, "
        "kaynak kod dosyalarının karakter kodlama tablolarının uyumsuzluğundan kaynaklanan Türkçe karakter "
        "bozulmaları sıklıkla yaşanmaktadır. C# derleyicisi (Roslyn) ve ASP.NET Core Razor şablon motoru, "
        "kaynak kodları varsayılan olarak UTF-8 tablosuna göre okumaya çalışır."
    )

    add_body_paragraph(
        "Dosyaların Windows-1254 (ANSI) veya farklı UTF formatlarında yanlış kaydedilmesi durumunda, "
        "Türkçe'ye özgü ş, ı, ğ, ö, ç, ü ve büyük harfleri Ş, İ, Ğ, Ö, Ç, Ü karakterleri tarayıcı ekranlarında "
        "ve derleme çıktılarında bozularak anlamsız sembollere dönüşmekteydi. Örneğin:"
    )

    add_body_paragraph(" 'ş' yerine 'ÅŸ', 'Ş' yerine 'Åž'", bold_prefix="- ", bullet=True)
    add_body_paragraph(" 'ı' yerine 'Ä±', 'İ' yerine 'Ä°'", bold_prefix="- ", bullet=True)
    add_body_paragraph(" 'ğ' yerine 'ÄŸ', 'Ğ' yerine 'Äž'", bold_prefix="- ", bullet=True)
    add_body_paragraph(" 'ç' yerine 'Ã§', 'Ç' yerine 'Ã‡'", bold_prefix="- ", bullet=True)
    add_body_paragraph(" 'ö' yerine 'Ã¶', 'Ö' yerine 'Ã–'", bold_prefix="- ", bullet=True)
    add_body_paragraph(" 'ü' yerine 'Ã¼', 'Ü' yerine 'Ãœ'", bold_prefix="- ", bullet=True)

    add_heading_2("Uygulanan Kesin Çözüm: UTF-8 BOM ve Otomatik PowerShell Scripti")
    
    add_body_paragraph(
        "Bu hataların tamamını gidermek ve bir daha tekrarlanmasını önlemek amacıyla projenin kök dizininde "
        "çalıştırılabilen FixEncoding.ps1 PowerShell betiği hazırlanmış ve tüm kaynak kod dosyalarına uygulanmıştır."
    )

    add_body_paragraph(
        "Betik, projedeki tüm .cs ve .cshtml dosyalarını tarayarak yukarıda belirtilen bozuk karakter kombinasyonlarını "
        "doğru Türkçe karşılıklarıyla (regex replace) değiştirir. En kritik adım olarak, güncellenen dosyaları "
        "UTF-8 with BOM (Byte Order Mark) kodlamasıyla diske geri kaydeder. UTF-8 BOM, dosyanın en başına "
        "eklenen 3-byte'lık özel bir imza dizisidir (EF BB BF). Bu imza sayesinde Windows işletim sistemi, "
        "C# derleyicisi ve Razor motoru dosyanın kesinlikle UTF-8 ile kodlandığını anlar ve Türkçe karakterleri "
        "hiçbir bozulma olmaksızın, pürüzsüz bir şekilde derleyip tarayıcıya servis eder. Bu düzeltmenin ardından "
        "SweetAlert2 uyarı kutularında, veri tabanı hata bildirimlerinde ve arayüz tablolarında karakter bozulmaları "
        "tamamen ortadan kalkmıştır."
    )

    # Dökümanı Kaydet
    doc.save("Proje_Teknik_Dokuman.docx")
    print("Teknik dokümantasyon başarıyla oluşturuldu: Proje_Teknik_Dokuman.docx")

if __name__ == '__main__':
    create_document()
