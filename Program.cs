using System.Text;
using DotNet9WebApi.Endpoints;
using DotNet9WebApi.SwaggerFilters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace DotNet9WebApi;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);
		builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
		{
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("CF5E756A97884E388A1C4BF25257B156")),
				ValidateLifetime = true,
				ValidateIssuer = false,
				ValidateAudience = false,
				ClockSkew = TimeSpan.Zero
			};
		});
		builder.Services.AddControllers();
		builder.Services.AddSwaggerGen(options =>
		{
			options.OperationAsyncFilter<SwaggerArrayParameterFilter>();
			options.AddSecurityDefinition("bearerAuth", new()
			{
				Type = SecuritySchemeType.Http,
				In = ParameterLocation.Header,
				Scheme = "Bearer",
				BearerFormat = "JWT",
				Name = "Authorization",
				Description = "Enter your bearer token"
			});
			options.AddSecurityRequirement(new()
			{
				{
					new()
					{
						Reference = new()
						{
							Type = ReferenceType.SecurityScheme,
							Id = "bearerAuth"
						}
					},
					Array.Empty<string>()
				}
			});
		});
		var app = builder.Build();
		if (app.Environment.IsDevelopment())
		{
			app.MapSwagger();
			app.UseSwaggerUI(options =>
			{
				options.EnablePersistAuthorization();
			});
			app.MapGet("/", async context =>
			{
				await Task.Run(() => context.Response.Redirect("./swagger/index.html", permanent: false));
			});
		}
		app.UseHttpsRedirection();
		app.UseAuthentication();
		app.UseAuthorization();
		app.MapWeatherForecastEndpoints();
		app.Run();
	}
}