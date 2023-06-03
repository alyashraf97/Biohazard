

namespace Biohazard
{
	public class Program
	{
		public static void Main(string[] args)
		{
			///<TODO>
			///</TODO>
			/*
            // Call all three async methods without await and store their tasks in an array
            var tasks = new[] { StartIdleClientAsync(), StartWebApiServiceAsync(), StartDatabaseConnectionAsync() };

            // Await the completion of all three tasks using Task.WhenAll
            try
            {
                await Task.WhenAll(tasks);
                // All tasks completed successfully
            }
            catch (AggregateException ex)
            {
                // One or more tasks failed
                foreach (var innerEx in ex.InnerExceptions)
                {
                    // Handle each exception individually
                    Console.WriteLine(innerEx.Message);
                }
            }
            */
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddAuthorization();


			var app = builder.Build();

			// Configure the HTTP request pipeline.

			app.UseHttpsRedirection();

			app.UseAuthorization();

			var summaries = new[]
			{
			"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
		};

			app.MapGet("/weatherforecast", (HttpContext httpContext) =>
			{
				var forecast = Enumerable.Range(1, 5).Select(index =>
					new WeatherForecast
					{
						Date = DateTime.Now.AddDays(index),
						TemperatureC = Random.Shared.Next(-20, 55),
						Summary = summaries[Random.Shared.Next(summaries.Length)]
					})
					.ToArray();
				return forecast;
			});

			app.Run();
		}
	}
}