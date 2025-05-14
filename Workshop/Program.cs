using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Workshop.Data;
using Workshop.Service;
using Workshop.Service.Interface;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddControllers();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(builder.Configuration["JwtSettings:Secret"] 
                ?? "YourSuperSecretKeyHereMakeItLongAndSecure12345")),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true
    };
});

// Authorization policies (optional roles)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User"));
    options.AddPolicy("RequireStaffRole", policy => policy.RequireRole("Staff")); // New Staff role policy
});

// Dependency Injection for services
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<AnnouncementService>();
builder.Services.AddScoped<TokenServices>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<IBookmarkService, BookmarkService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<OrderService>();  // Register OrderService in DI container
builder.Services.AddScoped<EmailService>();  // Register EmailService in DI container

// Swagger (for dev testing)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddControllers();
// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication(); // ✅ Critical for JWT
app.UseAuthorization();

app.MapControllers();

// Redirect root to registration page
app.MapGet("/", context =>
{
    context.Response.Redirect("/Register.html");
    return Task.CompletedTask;
});

app.Run();
