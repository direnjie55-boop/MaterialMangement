using System.ComponentModel.DataAnnotations;

namespace MaterialMangement.Models;

public class Material
{
  public int Id { get; set; }

  [Required]
  [MaxLength(100)]
  public string Name { get; set; } = string.Empty;

  [MaxLength(50)]
  public string? Category { get; set; }

  [MaxLength(200)]
  public string? Specification { get; set; }

  [MaxLength(20)]
  public string? Unit { get; set; }

  public decimal Quantity { get; set; }

  public decimal UnitPrice { get; set; }

  [MaxLength(500)]
  public string? Remark { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.Now;

  public DateTime? UpdatedAt { get; set; }

  /// <summary>
  /// 库存变动记录
  /// </summary>
  public List<StockRecord> StockRecords { get; set; } = new();

}