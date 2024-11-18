namespace DotNet9WebApi
{
	public class WeatherForecast
	{
		public DateOnly Date { set; get; }
		public int TemperatureC { set; get; }
		public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
		public string? Summary { set; get; }
	}
}