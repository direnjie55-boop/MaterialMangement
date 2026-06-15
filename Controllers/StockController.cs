using MaterialMangement.Data;
using MaterialMangement.DTOs;
using MaterialMangement.Models;
using MaterialMangement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace MaterialMangement.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockController : ControllerBase
{
    private readonly MaterialDbContext _context;
    private readonly ICacheService _cache;
    private readonly IMessageService _messageService;

    public StockController(MaterialDbContext context, ICacheService cache, IMessageService messageService)
    {
        _context = context;
        _cache = cache;
        _messageService = messageService;
    }

    /// <summary>
    /// 查询所有物料库存
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StockInfoDto>>> GetAllStock()
    {
        var cacheKey = "stock:all";
        var cached = await _cache.GetAsync<List<StockInfoDto>>(cacheKey);
        if (cached != null) return Ok(cached);

        var stocks = await _context.Materials
            .OrderBy(m => m.Name)
            .Select(m => new StockInfoDto
            {
                MaterialId = m.Id,
                Name = m.Name,
                Category = m.Category,
                Specification = m.Specification,
                Unit = m.Unit,
                Quantity = m.Quantity,
                UnitPrice = m.UnitPrice
            })
            .ToListAsync();
        await _cache.SetAsync(cacheKey, stocks, TimeSpan.FromMinutes(5));
        return Ok(stocks);
    }

    /// <summary>
    /// 查询单个物料库存
    /// </summary>
    [HttpGet("{materialId}")]
    public async Task<ActionResult<StockInfoDto>> GetStock(int materialId)
    {
        var material = await _context.Materials.FindAsync(materialId);
        if (material == null)
        {
            return NotFound(new { message = $"未找到ID为 {materialId} 的物料" });
        }

        var stock = new StockInfoDto
        {
            MaterialId = material.Id,
            Name = material.Name,
            Category = material.Category,
            Specification = material.Specification,
            Unit = material.Unit,
            Quantity = material.Quantity,
            UnitPrice = material.UnitPrice
        };

        return Ok(stock);
    }

    /// <summary>
    /// 入库
    /// </summary>
    [HttpPost("inbound")]
    public async Task<ActionResult<StockRecordDto>> Inbound(StockInboundDto dto)
    {
        var material = await _context.Materials.FindAsync(dto.MaterialId);
        if (material == null)
        {
            return NotFound(new { message = $"未找到ID为 {dto.MaterialId} 的物料" });
        }

        var beforeQty = material.Quantity;

        // 更新库存
        material.Quantity += dto.Quantity;

        // 如果提供了新单价，更新物料单价
        if (dto.UnitPrice.HasValue && dto.UnitPrice.Value > 0)
        {
            material.UnitPrice = dto.UnitPrice.Value;
        }

        material.UpdatedAt = DateTime.Now;

        // 创建入库记录
        var record = new StockRecord
        {
            MaterialId = dto.MaterialId,
            Operation = StockOperation.Inbound,
            Quantity = dto.Quantity,
            BeforeQuantity = beforeQty,
            AfterQuantity = material.Quantity,
            UnitPrice = dto.UnitPrice ?? material.UnitPrice,
            Reason = dto.Reason,
            Operator = dto.Operator,
            CreatedAt = DateTime.Now
        };

        _context.StockRecords.Add(record);
        await _context.SaveChangesAsync();

        // 清除库存缓存
        await _cache.RemoveAsync("stock:all");
        await _cache.RemoveAsync($"stock:{dto.MaterialId}");
        await _cache.RemoveAsync("materials:all");
        await _cache.RemoveAsync($"materials:{dto.MaterialId}");

        // 发布入库消息到 RabbitMQ
        await _messageService.PublishStockOperationAsync(new StockOperationMessage
        {
            MaterialId = material.Id,
            MaterialName = material.Name,
            Operation = "Inbound",
            Quantity = dto.Quantity,
            BeforeQuantity = beforeQty,
            AfterQuantity = material.Quantity,
            Operator = dto.Operator,
            Reason = dto.Reason
        });

        var result = new StockRecordDto
        {
            Id = record.Id,
            MaterialId = record.MaterialId,
            MaterialName = material.Name,
            Operation = record.Operation,
            Quantity = record.Quantity,
            BeforeQuantity = record.BeforeQuantity,
            AfterQuantity = record.AfterQuantity,
            UnitPrice = record.UnitPrice,
            Reason = record.Reason,
            Operator = record.Operator,
            CreatedAt = record.CreatedAt
        };

        return CreatedAtAction(nameof(GetStock), new { materialId = record.MaterialId }, result);
    }

    /// <summary>
    /// 出库
    /// </summary>
    [HttpPost("outbound")]
    public async Task<ActionResult<StockRecordDto>> Outbound(StockOutboundDto dto)
    {
        var material = await _context.Materials.FindAsync(dto.MaterialId);
        if (material == null)
        {
            return NotFound(new { message = $"未找到ID为 {dto.MaterialId} 的物料" });
        }

        if (material.Quantity < dto.Quantity)
        {
            return BadRequest(new
            {
                message = $"库存不足，当前库存 {material.Quantity}，请求出库 {dto.Quantity}"
            });
        }

        var beforeQty = material.Quantity;

        // 更新库存
        material.Quantity -= dto.Quantity;
        material.UpdatedAt = DateTime.Now;

        // 创建出库记录
        var record = new StockRecord
        {
            MaterialId = dto.MaterialId,
            Operation = StockOperation.Outbound,
            Quantity = dto.Quantity,
            BeforeQuantity = beforeQty,
            AfterQuantity = material.Quantity,
            UnitPrice = material.UnitPrice,
            Reason = dto.Reason,
            Operator = dto.Operator,
            CreatedAt = DateTime.Now
        };

        _context.StockRecords.Add(record);
        await _context.SaveChangesAsync();

        // 清除库存缓存
        await _cache.RemoveAsync("stock:all");
        await _cache.RemoveAsync($"stock:{dto.MaterialId}");
        await _cache.RemoveAsync("materials:all");
        await _cache.RemoveAsync($"materials:{dto.MaterialId}");

        // 发布出库消息到 RabbitMQ
        await _messageService.PublishStockOperationAsync(new StockOperationMessage
        {
            MaterialId = material.Id,
            MaterialName = material.Name,
            Operation = "Outbound",
            Quantity = dto.Quantity,
            BeforeQuantity = beforeQty,
            AfterQuantity = material.Quantity,
            Operator = dto.Operator,
            Reason = dto.Reason
        });

        var result = new StockRecordDto
        {
            Id = record.Id,
            MaterialId = record.MaterialId,
            MaterialName = material.Name,
            Operation = record.Operation,
            Quantity = record.Quantity,
            BeforeQuantity = record.BeforeQuantity,
            AfterQuantity = record.AfterQuantity,
            UnitPrice = record.UnitPrice,
            Reason = record.Reason,
            Operator = record.Operator,
            CreatedAt = record.CreatedAt
        };

        return Ok(result);
    }

    /// <summary>
    /// 查询库存变动记录（支持按物料ID、操作类型筛选）
    /// </summary>
    [HttpGet("records")]
    public async Task<ActionResult<IEnumerable<StockRecordDto>>> GetRecords(
        [FromQuery] int? materialId,
        [FromQuery] StockOperation? operation,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _context.StockRecords
            .Include(r => r.Material)
            .AsQueryable();

        if (materialId.HasValue)
        {
            query = query.Where(r => r.MaterialId == materialId.Value);
        }

        if (operation.HasValue)
        {
            query = query.Where(r => r.Operation == operation.Value);
        }

        var records = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new StockRecordDto
            {
                Id = r.Id,
                MaterialId = r.MaterialId,
                MaterialName = r.Material.Name,
                Operation = r.Operation,
                Quantity = r.Quantity,
                BeforeQuantity = r.BeforeQuantity,
                AfterQuantity = r.AfterQuantity,
                UnitPrice = r.UnitPrice,
                Reason = r.Reason,
                Operator = r.Operator,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return Ok(records);
    }
}