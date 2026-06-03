using MaterialMangement.Data;
using MaterialMangement.DTOs;
using MaterialMangement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaterialManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaterialsController : ControllerBase
{
    private readonly MaterialDbContext _context;

    public MaterialsController(MaterialDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取所有物料列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Material>>> GetAll()
    {
        var materials = await _context.Materials
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
        return Ok(materials);
    }

    /// <summary>
    /// 根据ID获取单个物料
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Material>> GetById(int id)
    {
        var material = await _context.Materials.FindAsync(id);
        if (material == null)
        {
            return NotFound(new { message = $"未找到ID为 {id} 的物料" });
        }
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

        return NoContent();
    }
}