using System.ComponentModel.DataAnnotations;
using MaterialMangement.Models;

namespace MaterialMangement.DTOs;

/// <summary>
/// 入库请求
/// </summary>
public class StockInboundDto
{
    [Required(ErrorMessage = "物料ID不能为空")]
    public int MaterialId { get; set; }

    [Required(ErrorMessage = "入库数量不能为空")]
    [Range(0.01, double.MaxValue, ErrorMessage = "入库数量必须大于0")]
    public decimal Quantity { get; set; }

    /// <summary>
    /// 入库单价（可选，不填则沿用物料原价）
    /// </summary>
    public decimal? UnitPrice { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    [MaxLength(50)]
    public string? Operator { get; set; }
}

/// <summary>
/// 出库请求
/// </summary>
public class StockOutboundDto
{
    [Required(ErrorMessage = "物料ID不能为空")]
    public int MaterialId { get; set; }

    [Required(ErrorMessage = "出库数量不能为空")]
    [Range(0.01, double.MaxValue, ErrorMessage = "出库数量必须大于0")]
    public decimal Quantity { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    [MaxLength(50)]
    public string? Operator { get; set; }
}

/// <summary>
/// 库存查询响应
/// </summary>
public class StockInfoDto
{
    public int MaterialId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Specification { get; set; }
    public string? Unit { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalValue => Quantity * UnitPrice;
}

/// <summary>
/// 库存变动记录响应
/// </summary>
public class StockRecordDto
{
    public int Id { get; set; }
    public int MaterialId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public StockOperation Operation { get; set; }
    public string OperationText => Operation == StockOperation.Inbound ? "入库" : "出库";
    public decimal Quantity { get; set; }
    public decimal BeforeQuantity { get; set; }
    public decimal AfterQuantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? Reason { get; set; }
    public string? Operator { get; set; }
    public DateTime CreatedAt { get; set; }
}