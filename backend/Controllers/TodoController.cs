using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using backend.Data;
using backend.Models;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IDistributedCache _cache;
    private const string CacheKey = "todo_list_all";

    public TodoController(AppDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    // GET: api/todo
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // 1. 优先尝试从 Redis 读取缓存
        var cachedData = await _cache.GetStringAsync(CacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            var cachedList = JsonSerializer.Deserialize<List<TodoItem>>(cachedData);
            return Ok(new { source = "Redis", data = cachedList });
        }

        // 2. 缓存未命中，从 PostgreSQL 读取
        var todos = await _context.Todos.ToListAsync();

        // 3. 将结果写回 Redis，并设置 2 分钟过期时间
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
        };
        await _cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(todos), cacheOptions);

        return Ok(new { source = "PostgreSQL", data = todos });
    }

    // POST: api/todo
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] TodoItem item)
    {
        // 1. 写入 PostgreSQL
        _context.Todos.Add(item);
        await _context.SaveChangesAsync();

        // 2. 淘汰 Redis 缓存，保证数据一致性
        await _cache.RemoveAsync(CacheKey);

        return Ok(item);
    }
}
