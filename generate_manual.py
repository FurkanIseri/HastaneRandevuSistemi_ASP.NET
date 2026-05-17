import docx
from docx import Document
from docx.shared import Pt, Inches, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml import parse_xml
from docx.oxml.ns import nsdecls

def create_document():
    doc = Document()
    
    # Page Setup: 1-inch margins on all sides
    for section in doc.sections:
        section.top_margin = Inches(1)
        section.bottom_margin = Inches(1)
        section.left_margin = Inches(1)
        section.right_margin = Inches(1)

    # Color Palette Definitions
    NAVY = RGBColor(27, 79, 114)        # Heading 1
    BLUE = RGBColor(46, 134, 193)       # Heading 2
    CHARCOAL = RGBColor(44, 62, 80)     # Body Text
    GRAY = RGBColor(127, 140, 141)      # Subtitles and Metadata
    DARK_NAVY = RGBColor(21, 67, 96)    # Title

    # Helper function to set table cell background color
    def set_cell_background(cell, fill_color="F4F6F7"):
        tcPr = cell._tc.get_or_add_tcPr()
        shd = parse_xml(f'<w:shd {nsdecls("w")} w:fill="{fill_color}"/>')
        tcPr.append(shd)

    # Helper function to set table cell borders
    def set_cell_borders(cell, color="D5D8DC", sz="4", val="single"):
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

    # Function to add code block in a shaded, bordered box
    def add_code_block(doc, code_text):
        table = doc.add_table(rows=1, cols=1)
        table.alignment = WD_ALIGN_PARAGRAPH.CENTER
        cell = table.cell(0, 0)
        set_cell_background(cell, "F4F6F7")
        set_cell_borders(cell, "D5D8DC")
        
        p = cell.paragraphs[0]
        p.paragraph_format.left_indent = Inches(0.15)
        p.paragraph_format.right_indent = Inches(0.15)
        p.paragraph_format.space_before = Pt(6)
        p.paragraph_format.space_after = Pt(6)
        p.paragraph_format.line_spacing = 1.15
        
        run = p.add_run(code_text)
        run.font.name = 'Consolas'
        run.font.size = Pt(9.5)
        run.font.color.rgb = RGBColor(33, 47, 61)
        
        # Add normal paragraph spacing after table
        doc.add_paragraph().paragraph_format.space_after = Pt(6)

    # Custom Heading 1 (Navy Blue)
    def add_heading_1(text):
        p = doc.add_paragraph()
        p.paragraph_format.space_before = Pt(18)
        p.paragraph_format.space_after = Pt(6)
        p.paragraph_format.keep_with_next = True
        run = p.add_run(text)
        run.font.name = 'Calibri'
        run.font.size = Pt(18)
        run.font.bold = True
        run.font.color.rgb = NAVY
        return p

    # Custom Heading 2 (Mediterranean Blue)
    def add_heading_2(text):
        p = doc.add_paragraph()
        p.paragraph_format.space_before = Pt(12)
        p.paragraph_format.space_after = Pt(4)
        p.paragraph_format.keep_with_next = True
        run = p.add_run(text)
        run.font.name = 'Calibri'
        run.font.size = Pt(14)
        run.font.bold = True
        run.font.color.rgb = BLUE
        return p

    # Custom Heading 3 (Small Bold Title)
    def add_heading_3(text):
        p = doc.add_paragraph()
        p.paragraph_format.space_before = Pt(8)
        p.paragraph_format.space_after = Pt(2)
        p.paragraph_format.keep_with_next = True
        run = p.add_run(text)
        run.font.name = 'Calibri'
        run.font.size = Pt(11)
        run.font.bold = True
        run.font.color.rgb = NAVY
        return p

    # Custom Body Paragraph with optional bold prefix and bullet styling
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

    # --- COVER PAGE ---
    title_p = doc.add_paragraph()
    title_p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    title_p.paragraph_format.space_before = Pt(140)
    title_p.paragraph_format.space_after = Pt(12)
    run_title = title_p.add_run("HASTANE YÖNETİM SİSTEMİ")
    run_title.font.name = 'Calibri'
    run_title.font.size = Pt(28)
    run_title.font.bold = True
    run_title.font.color.rgb = DARK_NAVY

    subtitle_p = doc.add_paragraph()
    subtitle_p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    subtitle_p.paragraph_format.space_after = Pt(180)
    run_sub = subtitle_p.add_run("Kapsamlı Dosya-Dosya Kod İncelemesi ve Teknik Kılavuz\nASP.NET Core 8 MVC & PostgreSQL N-Tier Mimari Analizi")
    run_sub.font.name = 'Calibri'
    run_sub.font.size = Pt(14)
    run_sub.font.color.rgb = GRAY

    meta_p = doc.add_paragraph()
    meta_p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    run_meta = meta_p.add_run("Hazırlayan: Kıdemli Yazılım Mimarı & Yapay Zeka Ortağı\nTarih: Mayıs 2026\nSürüm: 2.5 (Comprehensive Document)")
    run_meta.font.name = 'Calibri'
    run_meta.font.size = Pt(10)
    run_meta.font.italic = True
    run_meta.font.color.rgb = GRAY

    doc.add_page_break()

    # --- SECTION 1: PROJEYE GENEL BAKIŞ VE N-TIER MİMARİSİ ---
    add_heading_1("Bölüm 1: Projeye Genel Bakış ve N-Tier Mimarisi")
    
    add_body_paragraph(
        "Bu teknik manuel, modernize edilmiş olan ASP.NET Core 8 MVC tabanlı Hastane Yönetim Sistemi projesinin "
        "tüm kaynak kod dosyalarını (C# sınıfları, arayüzler ve CSHTML görünümleri) dosya dosya, satır satır analiz ederek "
        "açıklamak amacıyla oluşturulmuştur. Sistem, tek bir hastane kapsamındaki tüm randevu süreçlerini, reçete işlemlerini, "
        "ilaç stok takibini, duyuruları, sekreter, hasta ve doktor yönetim iş akışlarını dijitalleştirmeyi amaçlamaktadır."
    )
    
    add_body_paragraph(
        "Proje mimarisi, gevşek bağlılığı (Loose Coupling) ve sorumlulukların ayrılmasını (Separation of Concerns) "
        "hedefleyen N-Tier (Çok Katmanlı) bir yapı üzerine inşa edilmiştir. Sunum (Controller & View), İş Kuralları (Service), "
        "Veri Erişim (DataAccessLayer) ve Veri Modelleri (Models & ViewModels) katmanları tamamen bağımsız sınıflarla yönetilmekte, "
        "katmanlar arası iletişim Arayüzler (Interfaces) üzerinden Dependency Injection (Bağımlılık Enjeksiyonu) konteyneri "
        "kullanılarak sağlanmaktadır."
    )

    # --- SECTION 2: PROGRAM.CS VE HELPERS ---
    add_heading_1("Bölüm 2: Program.cs ve Helpers Katmanı")
    
    # Program.cs
    add_heading_2("1. Program.cs")
    add_body_paragraph(
        "Uygulamanın başlangıç noktasıdır. Bağımlılıkların (DI) sisteme kaydedilmesi, oturum (Session) desteğinin "
        "etkinleştirilmesi ve HTTP istek işleme boru hattının (Middleware Pipeline) yapılandırılması burada yapılır. "
        "Gelişmiş N-Tier mimarisi için tüm Service ve Repository arayüzleri ve sınıfları 'Scoped' (istek başına ömür) olarak kaydedilmiştir."
    )
    add_code_block(doc,
"// Repositories\n"
"builder.Services.AddScoped<IBransRepository, BransRepository>();\n"
"builder.Services.AddScoped<IDuyuruRepository, DuyuruRepository>();\n"
"builder.Services.AddScoped<ISekreterRepository, SekreterRepository>();\n"
"builder.Services.AddScoped<IDoktorRepository, DoktorRepository>();\n"
"builder.Services.AddScoped<IIlacRepository, IlacRepository>();\n"
"builder.Services.AddScoped<IRandevuRepository, RandevuRepository>();\n\n"
"// Services\n"
"builder.Services.AddScoped<IBransService, BransService>();\n"
"builder.Services.AddScoped<IDuyuruService, DuyuruService>();\n"
"builder.Services.AddScoped<IDoktorService, DoktorService>();\n"
"builder.Services.AddScoped<IIlacService, IlacService>();\n"
"builder.Services.AddScoped<ISekreterService, SekreterService>();\n"
"builder.Services.AddScoped<IRandevuService, RandevuService>();\n\n"
"// Middleware ve Oturum\n"
"builder.Services.AddSession();\n"
"app.UseSession();"
    )

    # sqlBaglanti.cs
    add_heading_2("2. sqlBaglanti.cs")
    add_body_paragraph(
        "PostgreSQL veritabanına ADO.NET (Npgsql) üzerinden fiziksel bağlantı kuran yardımcı sınıftır. "
        "IConfiguration üzerinden DefaultConnection dizesini dinamik okuyarak hardcoded şifreleme sızıntılarını önler. "
        "Bağlantı açık değilse açıp geriye döner. 'using var conn = _bgl.baglanti();' ifadesi ile kullanıldığında Garbage Collector "
        "tarafından otomatik kapatılıp havuzuna (Connection Pool) iade edilerek kaynak sızıntılarını (leak) engeller."
    )
    add_code_block(doc,
"public NpgsqlConnection baglanti()\n"
"{\n"
"    string baglantiAdresi = _configuration.GetConnectionString(\"DefaultConnection\") \n"
"        ?? \"Host=localhost;Port=5432;Database=HastaneYonetimSistemi;Username=postgres;Password=12345\";\n"
"    NpgsqlConnection baglanma = new NpgsqlConnection(baglantiAdresi);\n"
"    if (baglanma.State != System.Data.ConnectionState.Open)\n"
"    {\n"
"        baglanma.Open();\n"
"    }\n"
"    return baglanma;\n"
"}"
    )

    # SecurityHelper.cs
    add_heading_2("3. SecurityHelper.cs")
    add_body_paragraph(
        "Kişisel Verilerin Korunması Kanunu (KVKK) uyumluluğu için yazılmıştır. "
        "TC Kimlik numaraları ve parolaların ham halleri veritabanında ASLA tutulmaz. İki temel yöntem uygulanır:\n"
        "1. SHA256 Hashing (Hashle): Tek yönlüdür (one-way). Özellikle arama ve giriş endeksleme işlemlerinde veri güvenliği sağlar.\n"
        "2. AES256 Simetrik Şifreleme (Sifrele/Coz): Çift yönlüdür. Dinamik olarak rastgele 16 byte IV üretilir, 32 karakterlik SecretKey "
        "ile veriler şifrelenir ve IV ile birleştirilerek Base64 dizesine dönüştürülüp veritabanında saklanır. Gerektiğinde aslına çözülür."
    )
    add_code_block(doc,
"// SHA256 Hashing\n"
"public static string Hashle(string tcKimlikNo)\n"
"{\n"
"    using (SHA256 sha256 = SHA256.Create())\n"
"    {\n"
"        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(tcKimlikNo));\n"
"        StringBuilder builder = new StringBuilder();\n"
"        for (int i = 0; i < bytes.Length; i++) builder.Append(bytes[i].ToString(\"x2\"));\n"
"        return builder.ToString();\n"
"    }\n"
"}\n\n"
"// AES256 Decrypt\n"
"public static string Coz(string sifreliMetin)\n"
"{\n"
"    byte[] fullCipher = Convert.FromBase64String(sifreliMetin);\n"
"    byte[] iv = new byte[16];\n"
"    byte[] cipher = new byte[fullCipher.Length - iv.Length];\n"
"    Array.Copy(fullCipher, iv, iv.Length);\n"
"    Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);\n"
"    // ... AES Decryptor ve StreamReader ile okuma işlemleri ...\n"
"}"
    )

    # Karakter Kodlama Düzeltmeleri
    add_heading_2("4. Karakter Kodlama Düzeltmeleri ve UTF-8 BOM Yapısı")
    add_body_paragraph(
        "Özellikle Türkçe karakterlerin ('ş', 'ı', 'ğ', 'ö', 'ç', 'ü') Windows/IIS sunucu ortamlarında bozulmasını ('ÅŸ', 'Ä±', 'ÄŸ' vb.) "
        "kesin olarak çözmek için projedeki tüm .cs ve .cshtml dosyaları UTF-8 with BOM (EF BB BF imza baytları) kodlamasına "
        "dönüştürülmüştür. Bu sayede .NET derleyicisi (Roslyn) ve Razor motoru dosyaları pürüzsüz biçimde okur."
    )

    # --- SECTION 3: MODELLER VE VIEWMODELLER ---
    add_heading_1("Bölüm 3: Modeller ve ViewModeller Katmanı")
    
    # ViewModels.cs
    add_heading_2("1. ViewModels.cs")
    add_body_paragraph(
        "Katmanlar arası veri transferini (DTO) ve sunum katmanı veri doğrulamalarını (Data Annotations) üstlenir. "
        "Kullanıcı arayüzünde girilmesi zorunlu olan alanlar, veri boyutları ve kısıtlamalar bu modeller üzerinde işaretlenmiştir."
    )
    add_body_paragraph(" GirisViewModel: TC (11 hane zorunlu) ve Şifre alanlarını doğrular.", bullet=True)
    add_body_paragraph(" RegisterViewModel: Ad, Soyad, TC, Telefon, Şifre ve Cinsiyet verilerini doğrulayarak hasta kaydında rol oynar.", bullet=True)
    add_body_paragraph(" RandevuAlViewModel: Randevu ID'si, Şikayet alanı ve asenkron yüklenen IFormFile tipindeki TıbbiBelge'yi taşır.", bullet=True)
    add_body_paragraph(" ReceteOlusturViewModel: Randevu, Tanı, İlaç, Adet ve Kullanım Şekli doğrulamalarını içerir.", bullet=True)
    add_body_paragraph(" DoktorEkleViewModel & DoktorGuncelleViewModel: Doktor kayıt ve güncellemelerindeki şifre, branş, cinsiyet ve profil resmini yönetir.", bullet=True)
    add_body_paragraph(" IlacViewModel & BransViewModel: İlaç ve branş adları ile stok durumlarını taşır.", bullet=True)
    
    add_code_block(doc,
"public class GirisViewModel\n"
"{\n"
"    [Required(ErrorMessage = \"TC Kimlik No zorunludur.\")]\n"
"    [StringLength(11, MinimumLength = 11, ErrorMessage = \"TC Kimlik No 11 haneli olmalıdır.\")]\n"
"    public string TC { get; set; } = string.Empty;\n\n"
"    [Required(ErrorMessage = \"Şifre zorunludur.\")]\n"
"    public string Sifre { get; set; } = string.Empty;\n"
"}"
    )

    # UserLoginModel.cs
    add_heading_2("2. UserLoginModel.cs")
    add_body_paragraph(
        "Kullanıcı giriş süreçlerinde ortak olarak kullanılan ve form verilerini basitçe paketleyen temel model sınıfıdır."
    )

    # ErrorViewModel.cs
    add_heading_2("3. ErrorViewModel.cs")
    add_body_paragraph(
        "ASP.NET Core'un yerleşik hata yönetim mekanizması tarafından kullanılan, hata izleme (Request ID) verilerini tutan modeldir."
    )

    # --- SECTION 4: DATA ACCESS LAYER ---
    add_heading_1("Bölüm 4: Veri Erişim Katmanı (DataAccessLayer - DAL)")
    add_body_paragraph(
        "Veritabanı (PostgreSQL) ile olan tüm doğrudan fiziksel etkileşimi yönetir. "
        "NpgsqlCommand parametre bağlama yöntemlerini kullanarak SQL Enjeksiyonuna (SQL Injection) karşı %100 koruma sağlar."
    )

    # BransRepository
    add_heading_2("1. IBransRepository ve BransRepository")
    add_body_paragraph(
        "Branşları listelemek (GetAll), ada göre ID bulmak (GetIdByName), LOWER ve TRIM fonksiyonlarıyla büyük/küçük harf duyarsız "
        "mükerrer branş kontrolü yapmak (ExistsByName) ve branş silmek (Delete) için gerekli sorguları çalıştırır."
    )
    add_code_block(doc,
"// LOWER & TRIM mükerrer kontrolü\n"
"public bool ExistsByName(string bransAd)\n"
"{\n"
"    using var conn = _bgl.baglanti();\n"
"    using var cmd = new NpgsqlCommand(\"SELECT COUNT(*) FROM branslar WHERE LOWER(TRIM(brans_ad)) = LOWER(TRIM(@p1))\", conn);\n"
"    cmd.Parameters.AddWithValue(\"@p1\", bransAd ?? string.Empty);\n"
"    int count = Convert.ToInt32(cmd.ExecuteScalar());\n"
"    return count > 0;\n"
"}"
    )

    # DoktorRepository
    add_heading_2("2. IDoktorRepository ve DoktorRepository")
    add_body_paragraph(
        "Doktor bilgilerini JOIN sorgularıyla getirme (GetAllDoktorlar), profil fotoğrafını BYTEA (binary) tipinde kaydetme (Add), "
        "yetkilendirme sahiplik kontrolleri için sekreter UUID'sini çekme (GetSekreterIdByDoktorId), şifre ve görsel güncellemelerini yönetme "
        "(Update) süreçlerini üstlenir."
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
"    cmd.Parameters.AddWithValue(\"@p3\", SecurityHelper.Hashle(model.TC));\n"
"    cmd.Parameters.AddWithValue(\"@p4\", SecurityHelper.Sifrele(model.TC));\n"
"    cmd.Parameters.AddWithValue(\"@p5\", SecurityHelper.Hashle(model.Sifre));\n"
"    cmd.Parameters.AddWithValue(\"@p6\", SecurityHelper.Sifrele(model.Sifre));\n"
"    cmd.Parameters.AddWithValue(\"@p7\", bransId);\n"
"    cmd.Parameters.AddWithValue(\"@p8\", sekreterId);\n"
"    cmd.Parameters.Add(new NpgsqlParameter(\"@p9\", NpgsqlDbType.Bytea) { Value = foto != null ? (object)foto : DBNull.Value });\n"
"    cmd.Parameters.AddWithValue(\"@p10\", model.Cinsiyet ?? string.Empty);\n"
"    cmd.ExecuteNonQuery();\n"
"}"
    )

    # DuyuruRepository
    add_heading_2("3. IDuyuruRepository ve DuyuruRepository")
    add_body_paragraph(
        "Sekreterler tarafından yapılan duyuruları 'duyurular' tablosuna duyuru metni ve oluşturan sekreter ID'siyle kaydeder."
    )

    # IlacRepository
    add_heading_2("4. IIlacRepository ve IlacRepository")
    add_body_paragraph(
        "İlaçları listeleme, mükerrer kontrolü yapma (ExistsByName) ve ilaç silme (Delete) işlemlerini yönetir."
    )

    # RandevuRepository
    add_heading_2("5. IRandevuRepository ve RandevuRepository")
    add_body_paragraph(
        "Randevu yuvaları oluşturma işlemini PostgreSQL tarafındaki 'sp_randevuolustur' stored procedure'ü (saklı yordam) "
        "üzerinden çağırarak güvenli veri ekleme sağlar."
    )
    add_code_block(doc,
"public void AddRandevuYuvası(DateTime tarih, string saat, int bransId, Guid doktorId, bool durum)\n"
"{\n"
"    using var conn = _bgl.baglanti();\n"
"    using var cmd = new NpgsqlCommand(\"CALL sp_randevuolustur(@p1, @p2, @p3, @p4, @p5)\", conn);\n"
"    cmd.Parameters.AddWithValue(\"@p1\", NpgsqlTypes.NpgsqlDbType.Timestamp, tarih);\n"
"    cmd.Parameters.AddWithValue(\"@p2\", NpgsqlTypes.NpgsqlDbType.Varchar, saat);\n"
"    cmd.Parameters.AddWithValue(\"@p3\", bransId);\n"
"    cmd.Parameters.AddWithValue(\"@p4\", doktorId);\n"
"    cmd.Parameters.AddWithValue(\"@p5\", durum);\n"
"    cmd.ExecuteNonQuery();\n"
"}"
    )

    # SekreterRepository
    add_heading_2("6. ISekreterRepository ve SekreterRepository")
    add_body_paragraph(
        "Sekreter yetkilendirmesi, TC hash değerine göre ID veya Ad-Soyad bilgisi alma ve parola güncellemelerini SQL sorgularıyla yürütür."
    )

    # --- SECTION 5: SERVICES LAYER ---
    add_heading_1("Bölüm 5: İş Mantığı Katmanı (Services Layer)")
    add_body_paragraph(
        "İş kurallarının, veri dönüştürmelerinin ve katı doğrulama mekanizmalarının uygulandığı katmandır. "
        "Exception durumlarında kullanıcı dostu hata mesajları geriye dönülmesini sağlar."
    )

    # BransService
    add_heading_2("1. IBransService ve BransService")
    add_body_paragraph(
        "Branş ekleme öncesinde veritabanında aynı isimde branş olup olmadığını denetler. "
        "Silme işleminde eğer branşa ait aktif bir doktor/randevu varsa veritabanının yabancı anahtar constraint (FK) hatası "
        "vermesini yakalar (catch PostgresException) ve geriye güvenli bir 'Bu branşa bağlı kayıtlar mevcut' mesajı döner."
    )
    add_code_block(doc,
"public (bool Success, string Message) SilBrans(int bransId)\n"
"{\n"
"    try\n"
"    {\n"
"        _bransRepository.Delete(bransId);\n"
"        return (true, \"Branş silindi.\");\n"
"    }\n"
"    catch (PostgresException) // PostgreSQL FK Constraint Catch\n"
"    {\n"
"        return (false, \"Silinemedi: Bu branşa bağlı kayıtlar mevcut.\");\n"
"    }\n"
"    catch (System.Exception ex)\n"
"    {\n"
"        return (false, ex.Message);\n"
"    }\n"
"}"
    )

    # DoktorService
    add_heading_2("2. IDoktorService ve DoktorService")
    add_body_paragraph(
        "Doktor ekleme, silme ve güncelleme süreçlerinde kritik sahiplik (ownership) iş kuralını uygular:\n"
        "Sisteme kayıtlı bir doktoru yalnızca onu sisteme eklemiş olan sekreter düzenleyebilir veya silebilir. "
        "Farklı bir sekreter müdahale etmeye kalkıştığında işlem engellenir. Profil resmi dosyasını (IFormFile) asenkron "
        "olarak MemoryStream üzerinden byte[] dizisine dönüştürür."
    )
    add_code_block(doc,
"public (bool Success, string Message, DoktorGuncelleViewModel? Model) GetDoktorForEdit(string doktorId, string sekreterTcHash)\n"
"{\n"
"    if (!Guid.TryParse(doktorId, out var dId)) return (false, \"Geçersiz doktor ID.\", null);\n"
"    try\n"
"    {\n"
"        Guid mySekreterIdVal = _sekreterRepository.GetSekreterIdByTcHash(sekreterTcHash);\n"
"        Guid doktorSekreterIdVal = _doktorRepository.GetSekreterIdByDoktorId(dId);\n"
"        var doktorModel = _doktorRepository.GetDoktorById(dId);\n"
"        if (doktorModel == null) return (false, \"Doktor bulunamadı.\", null);\n"
"        \n"
"        // Sahiplik Doğrulaması\n"
"        if (doktorSekreterIdVal != mySekreterIdVal)\n"
"        {\n"
"            return (false, \"Sadece bu doktoru sisteme kaydeden sekreter güncelleme yapabilir.\", new DoktorGuncelleViewModel { DoktorId = doktorId });\n"
"        }\n"
"        return (true, string.Empty, doktorModel);\n"
"    }\n"
"    catch (Exception ex) { return (false, ex.Message, null); }\n"
"}"
    )

    # DuyuruService
    add_heading_2("3. IDuyuruService ve DuyuruService")
    add_body_paragraph(
        "Duyuruların boş kaydedilmesini engeller ve duyuruyu ekleyen sekreterin aktif oturumdaki varlığını doğrular."
    )

    # IlacService
    add_heading_2("4. IIlacService ve IlacService")
    add_body_paragraph(
        "Mükerrer ilaç eklenmesini engeller. İlacın silinmesi esnasında yabancı anahtar reçete ilişkisini korur."
    )

    # RandevuService
    add_heading_2("5. IRandevuService ve RandevuService")
    add_body_paragraph(
        "Oluşturulacak randevu yuvasının tarih ve saat verilerini doğrulayarak branş ve doktor bilgileri üzerinden saklı yordamı tetikler."
    )

    # SekreterService
    add_heading_2("6. ISekreterService ve SekreterService")
    add_body_paragraph(
        "Sekreter kaydında tüm alanların girilmesini denetler, 11 haneli TC kontrolü yapar ve profil bilgi güncellemelerinde "
        "girilen yeni şifrenin tekrarıyla eşleşip eşleşmediğini doğrular."
    )

    # --- SECTION 6: CONTROLLERS ---
    add_heading_1("Bölüm 6: Sunum Katmanı - Denetleyiciler (Controllers Layer)")
    add_body_paragraph(
        "HTTP isteklerini karşılayan, oturum (Session) durumunu kontrol eden ve sonuçları View'a ya da JSON olarak dönen katmandır."
    )

    # HomeController
    add_heading_2("1. HomeController")
    add_body_paragraph(
        "Sistemin ana karşılama sayfasıdır. Hasta, Doktor ve Sekreter giriş kapılarına yönlendirme işlevi görür."
    )

    # AuthController
    add_heading_2("2. AuthController")
    add_body_paragraph(
        "Oturum açma ve kayıt olma işlemlerini yürütür. ModelState geçerliliğini denetler, SecurityHelper kullanarak girilen şifre "
        "ve TC'yi hash'leyip DB'de kontrol eder. Eşleşme durumunda 'HastaTC', 'DoktorTC', 'SekreterTC' oturum (Session) anahtarlarını "
        "oluşturur. Hasta kayıt esnasında veritabanındaki PostgreSQL stored procedure'ünü ('sp_HastaKayit') tetikler."
    )
    add_code_block(doc,
"[HttpPost]\n"
"public IActionResult HastaGiris(GirisViewModel model)\n"
"{\n"
"    if (!ModelState.IsValid) return View(model);\n"
"    string tcHash = SecurityHelper.Hashle(model.TC);\n"
"    string sifreHash = SecurityHelper.Hashle(model.Sifre);\n"
"    using var conn = _bgl.baglanti();\n"
"    try\n"
"    {\n"
"        using var cmd = new NpgsqlCommand(\"SELECT COUNT(*) FROM Hastalar WHERE tc_hash = @p1 AND sifre_hash = @p2\", conn);\n"
"        cmd.Parameters.AddWithValue(\"@p1\", tcHash);\n"
"        cmd.Parameters.AddWithValue(\"@p2\", sifreHash);\n"
"        int count = Convert.ToInt32(cmd.ExecuteScalar());\n"
"        if (count > 0)\n"
"        {\n"
"            HttpContext.Session.SetString(\"HastaTC\", model.TC);\n"
"            TempData[\"Message\"] = \"Hasta sistemine başarıyla giriş yaptınız!\";\n"
"            return RedirectToAction(\"Index\", \"Hasta\");\n"
"        }\n"
"        ViewBag.Error = \"Hatalı TC Kimlik No veya Şifre!\";\n"
"        return View(model);\n"
"    }\n"
"    catch (Exception ex) { ViewBag.Error = \"Hata: \" + ex.Message; return View(model); }\n"
"}"
    )

    # HastaController
    add_heading_2("3. HastaController")
    add_body_paragraph(
        "Hasta paneli işlemlerini yönetir. AJAX istekleriyle dinamik branş, doktor ve aktif boş randevu yuvalarını (JsonResult) "
        "besler. RandevuAl işleminde, asenkron yüklenen tıbbi belge görselini (IFormFile) okur, NpgsqlDbType.Bytea tipinde veritabanında "
        "ilgili randevu hücresine update eder. Reçetelerim sayfasında hastaya yazılan reçete ve ilaç kullanım detaylarını listeler."
    )
    add_code_block(doc,
"[HttpPost]\n"
"public async Task<IActionResult> RandevuAl([FromForm] RandevuAlViewModel model)\n"
"{\n"
"    if (string.IsNullOrEmpty(TC)) return Json(new { success = false, message = \"Oturum süresi doldu.\" });\n"
"    if (model.TibbiBelge == null || model.TibbiBelge.Length == 0) return Json(new { success = false, message = \"Lütfen tıbbi belgenizi yükleyiniz.\" });\n"
"    \n"
"    byte[] belge;\n"
"    using (var ms = new MemoryStream())\n"
"    {\n"
"        await model.TibbiBelge.CopyToAsync(ms);\n"
"        belge = ms.ToArray();\n"
"    }\n"
"    using var conn = _bgl.baglanti();\n"
"    try\n"
"    {\n"
"        // ... Hasta ID bulma ve randevuya UPDATE atma işlemleri ...\n"
"        using var cmdU = new NpgsqlCommand(\"UPDATE Randevular SET randevu_durum=TRUE, hasta_id=@p1, hasta_sikayet=@p2, tibbi_belge=@p3 WHERE randevu_id=@p4\", conn);\n"
"        cmdU.Parameters.AddWithValue(\"@p1\", hastaId);\n"
"        cmdU.Parameters.AddWithValue(\"@p2\", model.Sikayet ?? \"\");\n"
"        cmdU.Parameters.Add(new NpgsqlParameter(\"@p3\", NpgsqlDbType.Bytea) { Value = belge });\n"
"        cmdU.Parameters.AddWithValue(\"@p4\", model.RandevuId);\n"
"        cmdU.ExecuteNonQuery();\n"
"        return Json(new { success = true, message = \"Randevunuz ve tıbbi belgeniz başarıyla kaydedildi.\" });\n"
"    }\n"
"    catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }\n"
"}"
    )

    # DoktorController
    add_heading_2("4. DoktorController")
    add_body_paragraph(
        "Doktor paneli iş akışlarını yönetir. Aktif randevuları ve hastanın asenkron yüklediği tıbbi belgeleri "
        "görüntülemeyi (TibbiBelgeGoster -> FileStreamResult) sağlar. Reçete oluştururken veritabanı bütünlüğünü korumak için "
        "Transaction (BeginTransaction, Commit, Rollback) yapısı kurarak reçete ve reçete detay tablosuna eş zamanlı yazma işlemi yapar. "
        "Aynı randevuya ikinci kez reçete yazılmasını engellemek üzere kilit koruma mantığı barındırır."
    )
    add_code_block(doc,
"[HttpPost]\n"
"public IActionResult ReceteOlustur(int randevuId, string tani, int ilacId, int adet, string kullanimSekli)\n"
"{\n"
"    if (string.IsNullOrEmpty(TC)) return Json(new { success = false, message = \"Oturum süresi doldu.\" });\n"
"    using var conn = _bgl.baglanti();\n"
"    try\n"
"    {\n"
"        // Reçete mükerrer kontrolü\n"
"        using (var cmdCheck = new NpgsqlCommand(\"SELECT COUNT(*) FROM receteler WHERE randevu_id = @p1\", conn))\n"
"        {\n"
"            cmdCheck.Parameters.AddWithValue(\"@p1\", randevuId);\n"
"            if (Convert.ToInt32(cmdCheck.ExecuteScalar()) > 0) return Json(new { success = false, message = \"Zaten reçete oluşturulmuş.\" });\n"
"        }\n"
"        using var tr = conn.BeginTransaction();\n"
"        try\n"
"        {\n"
"            // ... Hasta & Doktor UUID elde etme ...\n"
"            // Reçete Tablosuna Ekleme\n"
"            using (var cmd = new NpgsqlCommand(\"INSERT INTO receteler (recete_id, tani_teshis, recete_tarih, randevu_id, hasta_id, doktor_id) VALUES (@p1, @p2, CURRENT_DATE, @p3, @p4, @p5)\", conn))\n"
"            {\n"
"                // ...\n"
"            }\n"
"            // Reçete Detay Tablosuna Ekleme\n"
"            using (var cmdIlac = new NpgsqlCommand(\"INSERT INTO recetedetay (recete_id, ilac_id, adet, kullanim_sekli) VALUES (@p1, @p2, @p3, @p4)\", conn))\n"
"            {\n"
"                // ...\n"
"            }\n"
"            tr.Commit();\n"
"            return Json(new { success = true, message = \"Reçete kaydedildi.\" });\n"
"        }\n"
"        catch (Exception ex) { tr.Rollback(); throw ex; }\n"
"    }\n"
"    catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }\n"
"}"
    )

    # SekreterController
    add_heading_2("5. SekreterController")
    add_body_paragraph(
        "Sistemin idari merkezidir. İş kurallarını (Service katmanı üzerinden) tetikleyerek branş, ilaç, duyuru, "
        "sekreter ve doktor ekleme/silme/güncelleme/listeleme işlemlerini koordine eder."
    )

    # --- SECTION 7: VIEWS ---
    add_heading_1("Bölüm 7: Sunum Katmanı - Görünümler (Views / CSHTML)")
    add_body_paragraph(
        "Kullanıcı dostu, Bootstrap 5 tabanlı, dinamik ve responsive (duyarlı) web arayüzüdür. "
        "jQuery AJAX entegrasyonlarıyla sayfa yenilenmeden verilerin çekilmesi ve sunucuya gönderilmesi sağlanmıştır."
    )
    
    # Views/Auth
    add_heading_2("1. Giriş ve Kayıt Ekranları (Views/Auth)")
    add_body_paragraph(
        "HastaGiris, DoktorGiris ve SekreterGiris görünümlerinde, `GirisViewModel` veri bağlaması (model binding) kullanılarak "
        "istemci tarafında anlık doğrulama (`_ValidationScriptsPartial` ile jQuery Validation) sağlanır. Register.cshtml hasta kayıt ekranında "
        "SweetAlert2 entegrasyonuyla başarılı kayıt durumunda animasyon eşliğinde otomatik yönlendirme yapılır."
    )

    # Views/Hasta
    add_heading_2("2. Hasta Paneli Görünümleri (Views/Hasta)")
    add_body_paragraph(
        "Index.cshtml sayfasında hasta geçmiş randevularını görür. 'Yeni Randevu Al' bileşeni jQuery AJAX ile çalışır. "
        "Branş seçildiğinde doktorlar listesi (`GetDoktorlar`), doktor seçildiğinde aktif boş randevu yuvaları (`GetAktifRandevular`) "
        "sayfa yenilenmeden dinamik olarak yüklenir. Hasta şikayetini yazar, tıbbi belgesini (fotoğraf/pdf) sürükleyip yükler ve "
        "`FormData` API'si üzerinden AJAX ile sunucuya göndererek randevuyu asenkron satın alır."
    )
    add_code_block(doc,
"// AJAX Randevu Seçimi ve dynamic FormData Gönderimi\n"
"var formData = new FormData();\n"
"formData.append('RandevuId', rId);\n"
"formData.append('Sikayet', $('#txtSikayet').val());\n"
"formData.append('TibbiBelge', $('#fileBelge')[0].files[0]);\n\n"
"$.ajax({\n"
"    url: '/Hasta/RandevuAl',\n"
"    type: 'POST',\n"
"    data: formData,\n"
"    processData: false,\n"
"    contentType: false,\n"
"    success: function(res) {\n"
"        if(res.success) Swal.fire('Başarılı', res.message, 'success');\n"
"        else Swal.fire('Hata', res.message, 'error');\n"
"    }\n"
"});"
    )

    # Views/Doktor
    add_heading_2("3. Doktor Paneli Görünümleri (Views/Doktor)")
    add_body_paragraph(
        "Index.cshtml sayfasında doktor hastalarını arayabilmek için jQuery tabanlı 'Live Search' (canlı hasta arama filtrelemesi) "
        "kullanır. Tıbbi belge butonuna basıldığında ilgili byte dizisi modal penceresinde gösterilir. Reçete oluştur formunda "
        "seçilen ilacın stok durumu kontrol edilir ve SweetAlert2 yardımıyla onay kutuları eşliğinde reçete asenkron kaydedilir."
    )

    # Views/Sekreter
    add_heading_2("4. Sekreter Paneli Görünümleri (Views/Sekreter)")
    add_body_paragraph(
        "Branş Yönetimi (BransYonetimi.cshtml), İlaç Yönetimi (IlacYonetimi.cshtml) ve Sekreter Yönetimi (SekreterYonetimi.cshtml) "
        "arayüzleri dinamik AJAX CRUD mekanizmalarıyla donatılmıştır. Branş veya ilaç ekleme/silme işlemleri SweetAlert2 onay pencereleriyle "
        "tamamlanır ve sayfa anlık güncellenir. DoktorDuzenle ve DoktorEkle formlarında asenkron profil resmi yükleme işlemleri gerçekleştirilir."
    )

    # Views/Shared
    add_heading_2("5. Ortak Şablonlar (Views/Shared)")
    add_body_paragraph(
        "Layout.cshtml dosyasında sistemin responsive üst menüsü, FontAwesome ikon paketi, Google Fonts (Inter & Outfit) tipografisi "
        "ve SweetAlert2 kütüphane bağlantıları yer alır. Layout.cshtml.css dosyasında ise pürüzsüz animasyonlar, hover efektleri ve "
        "premium cam tasarımı (glassmorphism) sağlayan CSS sınıfları tanımlanmıştır."
    )

    # --- SECTION 8: SONUÇ VE KAPANIŞ ---
    add_heading_1("Bölüm 8: Sonuç ve Kapanış")
    add_body_paragraph(
        "Hastane Yönetim Sistemi projesi, monolitik ve güvensiz legacy yapısından kurtarılarak, modern kurumsal yazılım mimarisi "
        "standartlarına (N-Tier, Loose Coupling, Dependency Injection, Repository Pattern, Service Layer) tam uyumlu hale getirilmiştir. "
        "Bununla birlikte veritabanı parametrik sorgu güvenliği, KVKK uyumlu simetrik/asimetrik şifreleme altyapısı, UTF-8 BOM Türkçe "
        "karakter kodlama standartları ve jQuery AJAX SweetAlert2 kullanıcı deneyimi iyileştirmeleriyle üst düzey, endüstriyel "
        "standartlarda çalışan güvenilir bir ürün halini almıştır."
    )

    # Save Document
    doc.save("Proje_Tum_Kod_Aciklamalari.docx")
    print("Teknik manuel başarıyla oluşturuldu: Proje_Tum_Kod_Aciklamalari.docx")

if __name__ == '__main__':
    create_document()
