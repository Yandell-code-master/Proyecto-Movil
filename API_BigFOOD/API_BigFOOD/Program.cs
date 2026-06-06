using API_BigFOOD.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

//Se configura el servicio del ORM
builder.Services.AddDbContext<API_BigFOOD.Models.DbContextBigFOOD>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("StringLocal")));

//Configuración del servicio JWT
builder.Services.AddScoped<IAuthorizationServices, AuthorizationServices>();

//Configuración del servicio Gometa
builder.Services.AddTransient<GometaServices>();

//Configuración del servicio PDF
builder.Services.AddTransient<IPdfServices, PdfServices>();

//Configuración del servicio Email
builder.Services.AddTransient<IEmailServices, EmailServices>();

//Configuración del servicio de bitácora
builder.Services.AddTransient<IBitacoraServices, BitacoraServices>();

//Se realiza la lectura de la key almacenada en appsettings.json
var key = builder.Configuration.GetValue<string>("JwtSettings:Key");

//Se convierte la llave en un vector de bytes
var keyBytes = Encoding.ASCII.GetBytes(key);

//Se configura los parámetros de la autenticación
builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(config =>
{
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;

    config.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Configuración de autenticación
app.UseAuthentication();

//Configuración de autorización
app.UseAuthorization();

app.MapControllers();

app.Run();
