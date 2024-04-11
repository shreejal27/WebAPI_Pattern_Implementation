using Microsoft.AspNetCore.Mvc;

namespace WebAPI_Pattern_Implementation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {

        private readonly HttpClient _httpClient;

        //private static readonly string[] Summaries = new[]
        //{
        //    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        //};

        private readonly ILogger<StudentController> _logger;

        public StudentController(ILogger<StudentController> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        private static int failureCount = 0;

        [HttpGet(Name = "GetStudentDetails")]
        public async Task<IActionResult> Get()
        {
            int maxRetryAttempts = 3;
            TimeSpan retryInterval = TimeSpan.FromSeconds(1);
            int retryCount = 0;

            do
            {
                try
                {
                    if (failureCount < 2)
                    {
                        failureCount++;
                        throw new HttpRequestException("Simulated failure");
                    }
                    else
                    {
                    var response = await _httpClient.GetAsync("https://jsonplaceholder.typicode.com/users");
                    response.EnsureSuccessStatusCode();
                    var students = await response.Content.ReadFromJsonAsync<Student[]>();
                    return Ok(students);
                    }
                }
                catch (HttpRequestException)
                {
                    if (retryCount < maxRetryAttempts)
                    {
                        await Task.Delay(retryInterval);
                        retryCount++;
                    }
                    else
                    {
                        return StatusCode(500, "Failed to fetch student data after multiple attempts.");
                    }
                }
            } while (retryCount < maxRetryAttempts);

            return StatusCode(500, "Failed to fetch student data after multiple attempts.");
        }
    }
}
