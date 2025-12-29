using LVTN_BE_COFFE.Domain.Common;
using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Services;
using LVTN_BE_COFFE.Domain.Ultilities;
using LVTN_BE_COFFE.Infrastructures.Entities;
using LVTN_BE_COFFE.Services;
using LVTN_BE_COFFE.Services.Helpers;
using LVTN_BE_COFFE.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// ✅ 1. SỬA CORS - Thay AllowAnyOrigin bằng WithOrigins và thêm AllowCredentials
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001", "https://yourdomain.com")// domain
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials() // ← QUAN TRỌNG: Cho phép gửi Cookie/Session
              .WithExposedHeaders("X-Guest-Key"));
});



// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//Add Token Service
builder.Services.AddScoped<ITokenService, TokenService>();
//  Các gói Services
builder.Services.AddTransient<IAspNetUsersService, AspNetUsersService>();
builder.Services.AddTransient<IAspNetRolesService, AspNetRolesService>();
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<ISysApiService, SysApiService>();
//Các gói Cloundinary Service  
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<CloudinaryService>();
// Add this line ⬇️
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<AspNetUsers, AspNetRoles>(options =>
{
    // 1. Tắt yêu cầu phải xác thực email mới được đăng nhập
    options.SignIn.RequireConfirmedAccount = false;

    // 2. Cấu hình password (Tùy chọn: nới lỏng để dễ test)
    options.Password.RequireDigit = false; // Không bắt buộc số
    options.Password.RequireLowercase = false; // Không bắt buộc chữ thường
    options.Password.RequireNonAlphanumeric = false; // Không bắt buộc ký tự đặc biệt (@, #...)
    options.Password.RequireUppercase = false; // Không bắt buộc chữ hoa
    options.Password.RequiredLength = 6; // Độ dài tối thiểu
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();
//DEPENDENCY INJECTION
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
//builder.Services.AddSingleton<DeviceDetectionService>();
builder.Services.AddSingleton<Globals>();
builder.Services.AddScoped<IEmailSenderService, SendEmailService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICartItemService, CartItemService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<IProductImageService, ProductImageService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddScoped<IFlavorNotesService, FlavorNoteService>();
builder.Services.AddScoped<IBrewingMethodsService, BrewingMethodsService>();
builder.Services.AddScoped<IShippingAddressService, ShippingAddressService>();
//Product
builder.Services.AddScoped<IProductService, ProductService>();
// ✅ Add Session với cấu hình CORS-friendly
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.None; // ← THÊM: Cho phép cross-origin
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // ← THÊM: Yêu cầu HTTPS (nếu production)
});

// JWT Config
var jwtSettings = builder.Configuration.GetSection("JwtSettings");

// Lấy các giá trị và cung cấp chuỗi rỗng ("") nếu bị thiếu, để tránh lỗi null
// khi khởi tạo SymmetricSecurityKey.
var jwtKey = jwtSettings["Key"] ?? "";
var jwtIssuer = jwtSettings["Issuer"];
var jwtAudience = jwtSettings["Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        // 💡 Đảm bảo các giá trị không null
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// Swagger config with JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "LVTN_BE_COFFE", Version = "v1" });

    // 1. Cấu hình cho JWT Bearer
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập 'Bearer' [space] + token. \n\nVí dụ: `Bearer abc123xyz`"
    });

    // 2. THÊM MỚI: Cấu hình cho Guest Key (X-Guest-Key)
    c.AddSecurityDefinition("GuestKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "X-Guest-Key", // Tên header chính xác
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập mã định danh Guest (ví dụ: guest-123) để test giỏ hàng không đăng nhập"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        // Yêu cầu cho Bearer
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        },
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "GuestKey"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();
app.UseCors("AllowAll");
// Configure the HTTP request pipeline.
app.UseSession(); // ← Đã đúng vị trí
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();