using MaterialMangement.Data;
using MaterialMangement.DTOs;
using MaterialMangement.Models;
using MaterialMangement.Services;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaterialMangement.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MaterialsController : ControllerBase
{
    private readonly MaterialDbContext _context;
    private readonly ICacheService _cache;
    public MaterialsController(MaterialDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    /// <summary>
    /// 获取所有物料列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Material>>> GetAll()
    {
        var cacheKey = "materials:all";
        var cached = await _cache.GetAsync<List<Material>>(cacheKey);
        if (cached != null) return Ok(cached);

        var materials = await _context.Materials
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        await _cache.SetAsync(cacheKey, materials, TimeSpan.FromMinutes(5));
        return Ok(materials);
    }

    /// <summary>
    /// 根据ID获取单个物料
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Material>> GetById(int id)
    {
        var cacheKey = $"materials:{id}";
        var cached = await _cache.GetAsync<Material>(cacheKey);
        if (cached != null) return Ok(cached);

        var material = await _context.Materials.FindAsync(id);
        if (material == null)
        {
            return NotFound(new { message = $"未找到ID为 {id} 的物料" });
        }
        await _cache.SetAsync(cacheKey, material, TimeSpan.FromMinutes(5));
        return Ok(material);
    }

    /// <summary>
    /// 搜索物料（按名称或分类）
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Material>>> Search([FromQuery] string? keyword, [FromQuery] string? category)
    {
        var query = _context.Materials.AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(m => m.Name.Contains(keyword) ||
                                     (m.Specification != null && m.Specification.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(m => m.Category == category);
        }

        var results = await query.OrderByDescending(m => m.CreatedAt).ToListAsync();
        return Ok(results);
    }

    /// <summary>
    /// 创建新物料
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Material>> Create(MaterialCreateDto dto)
    {
        var material = new Material
        {
            Name = dto.Name,
            Category = dto.Category,
            Specification = dto.Specification,
            Unit = dto.Unit,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            Remark = dto.Remark,
            CreatedAt = DateTime.Now
        };

        _context.Materials.Add(material);
        await _context.SaveChangesAsync();

        //清除缓存列表
        await _cache.RemoveAsync("materials:all");

        return CreatedAtAction(nameof(GetById), new { id = material.Id }, material);
    }

    /// <summary>
    /// 更新物料信息
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<Material>> Update(int id, MaterialUpdateDto dto)
    {
        var material = await _context.Materials.FindAsync(id);
        if (material == null)
        {
            return NotFound(new { message = $"未找到ID为 {id} 的物料" });
        }

        material.Name = dto.Name;
        material.Category = dto.Category;
        material.Specification = dto.Specification;
        material.Unit = dto.Unit;
        material.Quantity = dto.Quantity;
        material.UnitPrice = dto.UnitPrice;
        material.Remark = dto.Remark;
        material.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        //清除相关缓存
        await _cache.RemoveAsync("materials:all");
        await _cache.RemoveAsync($"materials:{id}");

        return Ok(material);
    }

    /// <summary>
    /// 删除物料
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var material = await _context.Materials.FindAsync(id);
        if (material == null)
        {
            return NotFound(new { message = $"未找到ID为 {id} 的物料" });
        }

        _context.Materials.Remove(material);
        await _context.SaveChangesAsync();

        // 清除相关缓存
        await _cache.RemoveAsync("materials:all");
        await _cache.RemoveAsync($"materials:{id}");

        return NoContent();
    }
}