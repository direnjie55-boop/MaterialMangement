using System.ComponentModel.DataAnnotations;

namespace MaterialMangement.DTOs;

public class MaterialCreateDto
{
    [Required(ErrorMessage = "物料名称不能为空")]
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
}

public class MaterialUpdateDto
{
    [Required(ErrorMessage = "物料名称不能为空")]
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
}