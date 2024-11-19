using DotNet9WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotNet9WebApi.Endpoints;

public static class WeatherForecastEndpoints
{
	private static readonly string[] Summaries =
	[
		"Freezing",
		"Bracing",
		"Chilly",
		"Cool",
		"Mild",
		"Warm",
		"Balmy",
		"Hot",
		"Sweltering",
		"Scorching"
	];

	public static void MapWeatherForecastEndpoints(this WebApplication app)
	{
		var group = app.MapGroup("/WeatherForecast");

		group.MapGet("/", () =>
		{
			var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
			{
				Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
				TemperatureC = Random.Shared.Next(-20, 55),
				Summary = Summaries[Random.Shared.Next(Summaries.Length)]
			})
			.ToArray();
			return Results.Ok(forecast);
		});

		group.MapPost("/post", ([FromForm] PostBody post, [FromForm] long[] LongArray) =>
		{
			var guid = Guid.NewGuid();
			return Results.Ok(new
			{
				post.Content,
				post.Poll,
				Guid = guid
			});
		})
		.DisableAntiforgery();

		group.MapGet("test", () =>
		{
			return Results.Ok(new
			{
				Message = "Test"
			});
		})
		.RequireAuthorization();
	}
}