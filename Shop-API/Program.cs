using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Shop_API.Data;
using Shop_API.Handlers;
using Shop_API.Logics.IServices.IAttributeServices;
using Shop_API.Logics.IServices.IBrandServices;
using Shop_API.Logics.IServices.ICategoryServices;
using Shop_API.Logics.IServices.IMediaServices;
using Shop_API.Logics.Services.AttributeServices;
using Shop_API.Logics.Services.BrandServices;
using Shop_API.Logics.Services.CategoryServices;
using Shop_API.Logics.Services.MediaServices;
using Shop_API.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<ImageStorageOptions>(builder.Configuration.GetSection(ImageStorageOptions.SectionName));

builder.Services.PostConfigure<ImageStorageOptions>(options =>
{
    if (!string.IsNullOrWhiteSpace(options.RootPath) && !Path.IsPathFullyQualified(options.RootPath))
    {
        options.RootPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, options.RootPath));
    }
});

builder.Services.AddSingleton<IImageService, ImageService>();
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAttributeService, AttributeService>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();