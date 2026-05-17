var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession(); // Hafıza servisini ekledik

// Repositories
builder.Services.AddScoped<HastaneYonetimSistemi.DataAccessLayer.Interfaces.IBransRepository, HastaneYonetimSistemi.DataAccessLayer.BransRepository>();
builder.Services.AddScoped<HastaneYonetimSistemi.DataAccessLayer.Interfaces.IDuyuruRepository, HastaneYonetimSistemi.DataAccessLayer.DuyuruRepository>();
builder.Services.AddScoped<HastaneYonetimSistemi.DataAccessLayer.Interfaces.ISekreterRepository, HastaneYonetimSistemi.DataAccessLayer.SekreterRepository>();
builder.Services.AddScoped<HastaneYonetimSistemi.DataAccessLayer.Interfaces.IDoktorRepository, HastaneYonetimSistemi.DataAccessLayer.DoktorRepository>();
builder.Services.AddScoped<HastaneYonetimSistemi.DataAccessLayer.Interfaces.IIlacRepository, HastaneYonetimSistemi.DataAccessLayer.IlacRepository>();
builder.Services.AddScoped<HastaneYonetimSistemi.DataAccessLayer.Interfaces.IRandevuRepository, HastaneYonetimSistemi.DataAccessLayer.RandevuRepository>();

// Services
builder.Services.AddScoped<HastaneYonetimSistemi.Services.Interfaces.IBransService, HastaneYonetimSistemi.Services.BransService>();
builder.Services.AddScoped<HastaneYonetimSistemi.Services.Interfaces.IDuyuruService, HastaneYonetimSistemi.Services.DuyuruService>();
builder.Services.AddScoped<HastaneYonetimSistemi.Services.Interfaces.IDoktorService, HastaneYonetimSistemi.Services.DoktorService>();
builder.Services.AddScoped<HastaneYonetimSistemi.Services.Interfaces.IIlacService, HastaneYonetimSistemi.Services.IlacService>();
builder.Services.AddScoped<HastaneYonetimSistemi.Services.Interfaces.ISekreterService, HastaneYonetimSistemi.Services.SekreterService>();
builder.Services.AddScoped<HastaneYonetimSistemi.Services.Interfaces.IRandevuService, HastaneYonetimSistemi.Services.RandevuService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession(); // Hafızayı kullanıma açtık

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
