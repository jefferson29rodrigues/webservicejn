using Microsoft.AspNetCore.RateLimiting;
using SorrisoApi.Middlewares;
using SorrisoApi.Services;
using SorrisoApi.Settings;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<IOcrService, ProcessarImagemService>();
builder.Services.AddScoped<AcessaSiteSeleniumService>();

builder.Services.Configure<SeleniumSettings>(builder.Configuration.GetSection("SeleniumSettings"));

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 2;
    });
    options.RejectionStatusCode = 429;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseMiddleware<ApiKeyMiddleware>();

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

app.Run();
