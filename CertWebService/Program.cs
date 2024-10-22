using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.AspNetCore.Authentication.Certificate;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<KestrelServerOptions>(options =>
{
	options.ConfigureHttpsDefaults(options =>
	{
		options.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
	});
});

builder.Services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
	.AddCertificate(options => {
		options.RevocationMode = X509RevocationMode.NoCheck;
		options.AllowedCertificateTypes = CertificateTypes.All;
		options.Events = new CertificateAuthenticationEvents
		{
			OnChallenge = context => {
				context.HandleResponse();
				return Task.CompletedTask;
			},
			OnCertificateValidated = context => {
				context.Success();
				return Task.CompletedTask;
			},
			OnAuthenticationFailed = context => {
				context.Fail("invalid cert");
				return Task.CompletedTask;
			}
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

var summaries = new[]
{
	"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
	{
		var forecast = Enumerable.Range(1, 5).Select(index =>
				new WeatherForecast
				(
					DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
					Random.Shared.Next(-20, 55),
					summaries[Random.Shared.Next(summaries.Length)]
				))
			.ToArray();
		return forecast;
	})
	.WithName("GetWeatherForecast")
	.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
	public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}