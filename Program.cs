using Microsoft.OpenApi.Models;

namespace DotNet9WebApi;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);
		builder.Services.AddControllers();
		builder.Services.AddOpenApi(options =>
		{
			options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
		});
		builder.Services.AddSwaggerGen(options =>
		{
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
			app.MapOpenApi();
			app.UseSwaggerUI(options =>
			{
				options.SwaggerEndpoint("/openapi/v1.json", "v1");
			});
			app.MapGet("/", async context =>
			{
				await Task.Run(() => context.Response.Redirect("./swagger/index.html", permanent: false));
			});
		}
		app.UseHttpsRedirection();
		app.UseAuthorization();
		app.MapControllers();
		app.Run();
	}
}