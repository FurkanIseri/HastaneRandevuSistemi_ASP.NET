using HastaneYonetimSistemi.DataAccessLayer;
using HastaneYonetimSistemi.DataAccessLayer.Interfaces;
using HastaneYonetimSistemi.Services;
using HastaneYonetimSistemi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession();

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IHastaRepository, HastaRepository>();
builder.Services.AddScoped<IReceteRepository, ReceteRepository>();
builder.Services.AddScoped<IBransRepository, BransRepository>();
builder.Services.AddScoped<IDuyuruRepository, DuyuruRepository>();
builder.Services.AddScoped<ISekreterRepository, SekreterRepository>();
builder.Services.AddScoped<IDoktorRepository, DoktorRepository>();
builder.Services.AddScoped<IIlacRepository, IlacRepository>();
builder.Services.AddScoped<IRandevuRepository, RandevuRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IHastaService, HastaService>();
builder.Services.AddScoped<IBransService, BransService>();
builder.Services.AddScoped<IDuyuruService, DuyuruService>();
builder.Services.AddScoped<IDoktorService, DoktorService>();
builder.Services.AddScoped<IIlacService, IlacService>();
builder.Services.AddScoped<ISekreterService, SekreterService>();
builder.Services.AddScoped<IRandevuService, RandevuService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
