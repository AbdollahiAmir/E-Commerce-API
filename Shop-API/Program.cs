using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Shop_API.Data;
using Shop_API.Logics.IServices.IMediaServices;
using Shop_API.Logics.Services.MediaServices;
using Shop_API.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<ImageStorageOptions>(builder.Configuration.GetSection(ImageStorageOptions.SectionName));

builder.Services.AddSingleton<IImageService, ImageService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
