using System.ComponentModel.DataAnnotations;

namespace MaterialMangement.Models;

///<summary>
/// 库存变动类型
/// </summary>
public enum StockOperation
{
    Inbound = 1,//入库
    Outbound = 2 //出库
}

///<summary>
/// 库存变动记录
/// </summary>
public class StockRecord
{
    public int Id { get; set; }
    /// <summary>
    /// 关联物料ID
    /// </summary>
    public int MaterialId { get; set; }

    /// <summary>
    /// 关联物料
    /// </summary>
    public Material Material { get; set; } = null!;

    /// <summary>
    /// 操作类型：入库/出库
    /// </summary>
    public StockOperation Operation { get; set; }

    /// <summary>
    /// 数量（正数）
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// 操作前库存
    /// </summary>
    public decimal BeforeQuantity { get; set; }

    /// <summary>
    /// 操作后库存
    /// </summary>
    public decimal AfterQuantity { get; set; }

    /// <summary>
    /// 单价（入库时可更新）
    /// </summary>
    public decimal? UnitPrice { get; set; }

    /// <summary>
    /// 操作原因/备注
    /// </summary>
    [MaxLength(500)]
    public string? Reason { get; set; }

    /// <summary>
    /// 操作人
    /// </summary>
    [MaxLength(50)]
    public string? Operator { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}