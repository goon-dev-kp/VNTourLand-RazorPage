using BLL.Hubs;
using BLL.Providers;
using BLL.Services.Implement;
using BLL.Services.Interface;
using BLL.Utilities;
using Common.Settings;
using DAL.Data;
using DAL.Repositories.Implement;
using DAL.Repositories.Interface;
using DAL.UnitOfWork;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Stripe;
using VNPAY.NET;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

//// Thêm cấu hình Data Protection để lưu key cố định (không bị mất sau restart)
//builder.Services.AddDataProtection()
//    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "keys")))
//    .SetApplicationName("VNTourLand");

//var keyPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "keys");
//Directory.CreateDirectory(keyPath);

//builder.Services.AddDataProtection()
//    .PersistKeysToFileSystem(new DirectoryInfo(keyPath))
//    .SetApplicationName("VNTourLand");


// Thêm Session vào DI container
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Timeout 30 phút
    options.Cookie.HttpOnly = true;  // Bảo mật chống XSS
    options.Cookie.IsEssential = true;
});



// Đăng ký IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITourService, TourService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IIncludeService, IncludedService>();
builder.Services.AddScoped<INotIncludeService, NotIncludedService>();
builder.Services.AddScoped<IItineraryService, ItineraryService>();
builder.Services.AddScoped<IReviewerService, ReviewerService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<IBlogCategoryService, BlogCategoryService>();
builder.Services.AddScoped<IIUserService, UserService>();
//builder.Services.AddScoped<IAddOnOptionService, AddOnOptionService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IRequestOfCustomerRepository, RequestOfCustomerRepository>();
builder.Services.AddScoped<IRequestOfCustomerService, RequestOfCustomerService>();
builder.Services.AddScoped<ILocationOfStoryService, LocationOfStoryService>();
builder.Services.AddScoped<IStoryService, StoryService>();
builder.Services.AddScoped<IContactService, ContactService>();

builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();


builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<UserUtility>();
builder.Services.AddHttpContextAccessor();

//Paypal
builder.Services.Configure<PayPalSettings>(builder.Configuration.GetSection("PayPal"));
builder.Services.AddHttpClient();
builder.Services.AddScoped<IPayPalService, PayPalService>();

//VNPay
builder.Services.AddScoped<IVnPayService, VnPayService>();

builder.Services.AddSingleton<IVnpay>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var vnpay = new Vnpay();
    vnpay.Initialize(
        config["VNPAY:TmnCode"],
        config["VNPAY:HashSecret"],
        config["VNPAY:Url"], // 🔥 Sửa "BaseUrl" -> "Url"
        config["VNPAY:ReturnUrl"] // 🔥 Đảm bảo đúng key
    );
    return vnpay;
});

//Spripe
builder.Services.AddScoped<IStripePaymentService, StripePaymentService>();
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection("Stripe"));


//Email
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<IEmailService, EmailService>();

//MapBox
builder.Services.Configure<MapboxOptions>(builder.Configuration.GetSection("Mapbox"));

//Login with Google
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = "58351490541-4o1m3rr8fkdqm3tc8456upne26k19r2q.apps.googleusercontent.com";
    options.ClientSecret = "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";
    options.CallbackPath = "/Auth/GoogleCallback";
});


//Social link
builder.Services.Configure<SocialLinks>(builder.Configuration.GetSection("SocialLinks"));

//SEPay
builder.Services.Configure<SePayOptions>(builder.Configuration.GetSection("SePay"));

builder.Services.AddScoped<ISepayService, SepayService>();






var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 🟢 Đăng ký DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString,
    b => b.MigrationsAssembly("DAL")
)

);



// Lấy đường dẫn tuyệt đối file json
var env = builder.Environment; // IWebHostEnvironment
var credentialPath = Path.Combine(env.ContentRootPath, "firebase", "vntourland-551b5-firebase-adminsdk-fbsvc-3cf27b8437.json");

// Tạo FirebaseApp với đường dẫn tuyệt đối
FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile(credentialPath),
});

// Cấu hình Forwarded Headers Middleware để hỗ trợ HTTPS
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear(); // Đảm bảo nhận từ mọi IP proxy (Render không cố định IP)
    options.KnownProxies.Clear();
});



var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//    db.Database.Migrate(); // Tự động update database
//}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    
    // 🟢 Chuyển hướng trang chủ ("/") đến Razor Page Home/Index
    endpoints.MapGet("/", async context =>
    {
        context.Response.Redirect("/Home/Index"); // 🟢 Không cần ghi /RazorPage vì đã cấu hình RootDirectory
    });
});
app.MapHub<ChatHub>("/chathub");
app.MapHub<ReloadHub>("/reloadHub");



app.MapRazorPages();

app.Run();
