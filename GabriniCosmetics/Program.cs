using AutoMapper;
using GabriniCosmetics.Areas.Admin.Models;
using GabriniCosmetics.Areas.Admin.Models.Interface;
using GabriniCosmetics.Areas.Admin.Models.Mapper;
using GabriniCosmetics.Areas.Admin.Models.Services;
using GabriniCosmetics.Data;
using GabriniCosmetics.Models.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);



builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    });

// Add Connection String
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<GabriniCosmeticsContext>(options =>
    options.UseSqlServer(connectionString));

// Configure ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Configure password options
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;

    // Disable email confirmation requirement
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<GabriniCosmeticsContext>()
.AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = false;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);

    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// Add localization services
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(option =>
{
    var supportedCultuers = new[]
    {
        new CultureInfo ("en"),
        new CultureInfo ("ar")
    };
    option.DefaultRequestCulture = new RequestCulture("en");
    option.SupportedUICultures = supportedCultuers;
});

builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<ICategory, CategoryService>();
builder.Services.AddScoped<ISubcategory, SubcategoryService>();
builder.Services.AddScoped<IProduct, ProductService>();
builder.Services.AddScoped<IProductColorService, ProductColorService>();
builder.Services.AddScoped<IProductFlagService, ProductFlagService>();
builder.Services.AddScoped<IProductImageService, ProductImageService>();
builder.Services.AddScoped<IFlagService, FlagService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IContactUs, ContactUsServices>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ISliderBannerService, SliderBannerService>();
builder.Services.AddScoped<ISliderAdService, SliderAdService>();
builder.Services.AddScoped<IOrder, OrderServices>();
builder.Services.AddScoped<IAnnouncementBar, AnnouncementBarService>();
builder.Services.AddScoped<IDealOfTheDaysService, DealOfTheDaysService>();
builder.Services.AddScoped<IDiscountService, DiscountService>();




builder.Services.AddAutoMapper(typeof(CategoryMapper));

var app = builder.Build();


// Seed the database with initial data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<GabriniCosmeticsContext>();

    context.Database.Migrate();

    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        SeedData.Initialize(services, userManager).Wait();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Enable localization middleware
var supportedCultures = new[] { "en", "ar" };
var localizationOptions = new RequestLocalizationOptions().SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization();
app.UseRequestLocalization(localizationOptions);


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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");


app.Run();
