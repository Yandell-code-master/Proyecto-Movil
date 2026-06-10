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

//Configuración del servicio Gometa
builder.Services.AddTransient<GometaServices>();

//Configuración del servicio PDF
builder.Services.AddTransient<IPdfServices, PdfServices>();

//Configuración del servicio Email
builder.Services.AddTransient<IEmailServices, EmailServices>();

//Configuración del servicio de bitácora
builder.Services.AddTransient<IBitacoraServices, BitacoraServices>();

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
