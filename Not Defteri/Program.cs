using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotDefteriMvc.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC (Controller + View) için servis kaydı
builder.Services.AddControllersWithViews();

// IHostEnvironment'i açıkça kaydet
builder.Services.AddSingleton<IHostEnvironment>(builder.Environment);

// INoteRepository isteyen her yere FirebaseNoteRepository verilecek (veri Firebase Firestore'da).
builder.Services.AddSingleton<INoteRepository, FirebaseNoteRepository>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Varsayılan route: /Notes/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Notes}/{action=Index}/{id?}");

Console.WriteLine("Web uygulaması başlatılıyor...");
Console.WriteLine("http://localhost:5000 adresine gidin");

app.Run();

 