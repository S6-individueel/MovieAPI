using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace MovieAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieController : Controller
    {
        private readonly IDistributedCache _distributedCache;
        static HttpClient client = new HttpClient();

        public MovieController(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }


        [HttpGet("top-rated")]
        public async Task<IActionResult> GetTopRated()
        {
            string movies;
            string cachedTodosString = string.Empty;
            cachedTodosString = await _distributedCache.GetStringAsync("_top-rated");
            if (!string.IsNullOrEmpty(cachedTodosString))
            {
                // loaded data from the redis cache.
                movies = JsonSerializer.Deserialize<string>(cachedTodosString);
            }
            else
            {
                // loading from code (in real-time from database)
                // then saving to the redis cache 
                HttpResponseMessage response = await client.GetAsync("https://api.themoviedb.org/3/movie/now_playing?api_key=aa6aea63fa5c40f2773090bf5efee6ba&language=en-US&page=1");
                response.EnsureSuccessStatusCode();
                movies = await response.Content.ReadAsStringAsync();
                cachedTodosString = JsonSerializer.Serialize<string>(movies);
                await _distributedCache.SetStringAsync("_top-rated", cachedTodosString);
            }
            return Ok(movies);
        }

        [HttpDelete("clear-cache")]
        public async Task<IActionResult> ClearCache(string key)
        {
            await _distributedCache.RemoveAsync(key);
            return Ok(new { Message = $"cleared cache for key - {key}" });
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
