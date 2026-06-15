using MaterialMangement.Models;
using Microsoft.EntityFrameworkCore;

namespace MaterialMangement.Data;

public class MaterialDbContext : DbContext
{
    public MaterialDbContext(DbContextOptions<MaterialDbContext> options) : base(options)
    {
    }

    public DbSet<Material> Materials => Set<Material>();
    public DbSet<StockRecord> StockRecords => Set<StockRecord>();
    public DbSet<User> Users => Set<User>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        //配置StockRecord与Material的关系
        modelBuilder.Entity<StockRecord>(entity =>
      {
          entity.HasOne(s => s.Material)
                .WithMany(m => m.StockRecords)
                .HasForeignKey(s => s.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);
      });

        // 种子数据
        modelBuilder.Entity<Material>().HasData(
            new Material
            {
                Id = 1,
                Name = "水泥",
                Category = "建材",
                Specification = "P.O 42.5",
                Unit = "吨",
                Quantity = 100,
                UnitPrice = 450.00m,
                Remark = "普通硅酸盐水泥",
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new Material
            {
                Id = 2,
                Name = "钢筋",
                Category = "建材",
                Specification = "HRB400 Φ12",
                Unit = "吨",
                Quantity = 50,
                UnitPrice = 4200.00m,
                Remark = "热轧带肋钢筋",
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new Material
            {
                Id = 3,
                Name = "砂石",
                Category = "建材",
                Specification = "中砂",
                Unit = "立方米",
                Quantity = 200,
                UnitPrice = 85.00m,
                Remark = "河砂",
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );
    }
}