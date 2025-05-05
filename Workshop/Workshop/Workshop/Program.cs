using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Workshop.Data;
using Workshop.Service;
using Workshop.Service.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register BookService for dependency injection
builder.Services.AddScoped<Workshop.Service.BookService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDbContext>(options => 
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
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
            Encoding.ASCII.GetBytes(builder.Configuration["JwtSettings:Secret"] ?? "YourSuperSecretKeyHereMakeItLongAndSecure12345")
        ),
        ValidateIssuer = false,
        ValidateLifetime = true,
        ValidateAudience = false
    };
});

// Configure authorization policies
builder.Services.AddAuthorization(options =>
{
    // Policy for Admin role
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));

    // Policy for User role
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User"));
});
builder.Services.AddScoped<TokenServices>();
builder.Services.AddScoped<Workshop.Service.BookService>(); // Only register BookService once
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<IBookmarkService, BookmarkService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", context => {
    context.Response.Redirect("/Register.html");
    return Task.CompletedTask;
    
});

app.Run();
