using Microsoft.AspNetCore.Mvc;
using Polly;

namespace WebAPI_Pattern_Implementation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JokesController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private static int failureCount = 0;

        public JokesController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet(Name = "GetRandomeJokes")]
        public async Task<IActionResult> GetJoke()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var circuitBreakerPolicy = Policy.Handle<Exception>()
                .CircuitBreakerAsync(exceptionsAllowedBeforeBreaking: 1, durationOfBreak: TimeSpan.FromSeconds(30));
            try
            {
                var response = await circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    if (failureCount <= 2)
                    {
                        failureCount++;
                        throw new HttpRequestException("Simulated failure");
                    }
                    else
                    {
                        return await httpClient.GetAsync("https://official-joke-api.appspot.com/random_joke");
                    }
                });

                response.EnsureSuccessStatusCode();
                var jokeContent = await response.Content.ReadAsStringAsync();
                return Ok(jokeContent);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                return StatusCode(500, "An error occurred while fetching the joke.");
            }
        }
    }
}