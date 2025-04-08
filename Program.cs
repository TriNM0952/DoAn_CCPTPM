using Lab03.Data;
using Lab03.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Net.payOS;

var builder = WebApplication.CreateBuilder(args);

// Thêm dịch vụ Session
builder.Services.AddDistributedMemoryCache(); // Cấu hình bộ nhớ phân tán cho session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian session hết hạn
    options.Cookie.IsEssential = true; // Đảm bảo cookie session hoạt động mặc dù không có quyền truy cập
});

// Load configuration from appsettings.json
var configuration = builder.Configuration;

// Add services to the container

// Database context
var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Initialize PayOS with configuration values
PayOS payOS = new PayOS(
    configuration["Environment:PAYOS_CLIENT_ID"] ?? throw new Exception("Cannot find PAYOS_CLIENT_ID"),
    configuration["Environment:PAYOS_API_KEY"] ?? throw new Exception("Cannot find PAYOS_API_KEY"),
    configuration["Environment:PAYOS_CHECKSUM_KEY"] ?? throw new Exception("Cannot find PAYOS_CHECKSUM_KEY")
);
builder.Services.AddSingleton(payOS);

// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// Razor Pages and MVC
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddControllersWithViews();

// Custom Repositories
builder.Services.AddScoped<IInventoryRepository, EFInventoryRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();

// Additional services
builder.Services.AddHttpContextAccessor();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();  // Đảm bảo phục vụ các tài nguyên tĩnh từ wwwroot

app.UseRouting();  // Đảm bảo sử dụng routing trước khi sử dụng session và authorization

app.UseSession();   // Kích hoạt session sau khi routing

app.UseAuthorization();  // Xử lý phân quyền

// Map Razor Pages và các tuyến đường của Controller
app.MapRazorPages();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=ProductManager}/{action=Index}/{id?}"
    );
});

// Map tuyến đường mặc định cho Controller
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
