namespace SqzEvent.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public class QualityControlModels : DbContext
    {
        //您的上下文已配置为从您的应用程序的配置文件(App.config 或 Web.config)
        //使用“QualityControlModels”连接字符串。默认情况下，此连接字符串针对您的 LocalDb 实例上的
        //“SqzEvent.Models.QualityControlModels”数据库。
        // 
        //如果您想要针对其他数据库和/或数据库提供程序，请在应用程序配置文件中修改“QualityControlModels”
        //连接字符串。
        public QualityControlModels()
            : base("name=QualityControlConnection")
        {
        }

        //为您要在模型中包含的每种实体类型都添加 DbSet。有关配置和使用 Code First  模型
        //的详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=390109。

        // public virtual DbSet<MyEntity> MyEntities { get; set; }

        public virtual DbSet<QCStaff> QCStaff { get; set; }
        public virtual DbSet<Factory> Factory { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<QCEquipment> QCEquipment { get; set; }
        public virtual DbSet<BreakdownType> BreakdownType { get; set; }
        public virtual DbSet<BreakdownReport> BreadkdownReport { get; set; }
        public virtual DbSet<QCAgenda> QCAgenda { get; set; }
        public virtual DbSet<ProductionDetails> ProductionDetails { get; set; }
        public virtual DbSet<QualityTestTemplate> QualityTestTemplate { get; set; }
        public virtual DbSet<QualityTest> QualityTest { get; set; }
        

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Factory>().HasMany(e => e.QCEquipment).WithRequired(e => e.Factory).HasForeignKey(e => e.FactoryId).WillCascadeOnDelete(true);
            modelBuilder.Entity<QCEquipment>().HasMany(e => e.BreakdownType).WithRequired(e => e.QCEquipment).HasForeignKey(e => e.EquipmentId).WillCascadeOnDelete(true);
            modelBuilder.Entity<QCStaff>().HasMany(e => e.BreakdownReport).WithRequired(e => e.QCStaff).HasForeignKey(e => e.QCStaffId).WillCascadeOnDelete(true);
            modelBuilder.Entity<BreakdownType>().HasMany(e => e.BreakdownReport).WithRequired(e => e.BreakdownType).HasForeignKey(e => e.BreakDownTypeId).WillCascadeOnDelete(false);
            modelBuilder.Entity<QCEquipment>().HasMany(e => e.BreakdownReport).WithRequired(e => e.QCEquipment).HasForeignKey(e => e.QCEquipmentId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Factory>().HasMany(e => e.QCAgenda).WithRequired(e => e.Factory).HasForeignKey(e => e.FactoryId).WillCascadeOnDelete(false);
            modelBuilder.Entity<QCStaff>().HasMany(e => e.QCAgenda).WithRequired(e => e.QCStaff).HasForeignKey(e => e.QCStaffId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Product>().HasMany(e => e.ProductionDetails).WithRequired(e => e.Product).HasForeignKey(e => e.ProductId).WillCascadeOnDelete(false);
            modelBuilder.Entity<QCAgenda>().HasMany(e => e.ProductionDetails).WithRequired(e => e.QCAgenda).HasForeignKey(e => e.QCAgendaId).WillCascadeOnDelete(true);
            modelBuilder.Entity<Product>().HasMany(e => e.QualityTestTemplate).WithRequired(e => e.Product).HasForeignKey(e => e.ProductId).WillCascadeOnDelete(true);
            modelBuilder.Entity<Product>().HasMany(e => e.QualityTest).WithRequired(e => e.Product).HasForeignKey(e => e.ProductId).WillCascadeOnDelete(true);
            modelBuilder.Entity<QCStaff>().HasMany(e => e.QualityTest).WithRequired(e => e.QCStaff).HasForeignKey(e => e.QCStaffId).WillCascadeOnDelete(true);
            modelBuilder.Entity<Factory>().HasMany(e => e.QualityTest).WithRequired(e => e.Factory).HasForeignKey(e => e.FactoryId).WillCascadeOnDelete(true);
        }
    }

    /// <summary>
    /// 质检员
    /// </summary>
    [Table("QCStaff")]  
    public partial class QCStaff
    {
        public int Id { get; set; }

        [StringLength(128)]
        public string UserId { get; set; }
        
        [StringLength(16)]
        public string Name { get; set; }
        
        [StringLength(16)]
        public string Mobile { get; set; }

        // 对应关系
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Factory> Factory { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BreakdownReport> BreakdownReport { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QCAgenda> QCAgenda { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QualityTest> QualityTest { get; set; }
    }

    /// <summary>
    /// 工厂信息
    /// </summary>
    [Table("Factory")]
    public partial class Factory
    {
        public int Id { get; set; }
        
        [StringLength(64)]
        public string Name { get; set; }

        [StringLength(16)]
        public string SimpleName { get; set; }

        [StringLength(128)]
        public string Address { get; set; }

        [StringLength(16)]
        public string ZIP { get; set; }

        // 对应关系
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QCStaff> QCStaff { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Product> Product { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QCEquipment> QCEquipment { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QCAgenda> QCAgenda { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QualityTest> QualityTest { get; set; }
    }

    /// <summary>
    /// 质检产品信息
    /// </summary>
    [Table("Product")]
    public partial class Product
    {
        public int Id { get; set; }

        public bool QCProduct { get; set; }

        [StringLength(64)]
        public string Name { get; set; }

        [StringLength(16)]
        public string SimpleName { get; set; }

        [StringLength(16)]
        public String ProductCode { get; set; }

        [StringLength(16)]
        public string Specification { get; set; }

        [StringLength(128)]
        public string ProductDescribe { get; set; }

        // 对应关系
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Factory> Factory { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductionDetails> ProductionDetails { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QualityTestTemplate> QualityTestTemplate { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QualityTest> QualityTest { get; set; }
    }

    /// <summary>
    /// 设备信息
    /// </summary>
    [Table("QCEquipment")]
    public partial class QCEquipment
    {
        public int Id { get; set; }

        public int FactoryId { get; set; }

        [StringLength(64)]
        public string Name { get; set; }

        [StringLength(16)]
        public string SimpleName { get; set; }

        public DateTime? ManufactureDate { get; set; }

        [StringLength(32)]
        public string ModelNumber { get; set; }

        [StringLength(128)]
        public string Subscribe { get; set; }

        // 对应关系
        public virtual Factory Factory { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BreakdownType> BreakdownType { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BreakdownReport> BreakdownReport { get; set; }

    }

    /// <summary>
    /// 故障类型
    /// </summary>
    [Table("BreakdownType")]
    public partial class BreakdownType
    {
        public int Id { get; set; }

        public int EquipmentId { get; set; }

        public int Priority { get; set; }

        [StringLength(16)]
        public string SimpleDescribe { get; set; }

        [StringLength(64)]
        public string Describe { get; set; }

        [StringLength(1024)]
        public string Recommand { get; set; }

        // 对应关系
        public virtual QCEquipment QCEquipment { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BreakdownReport> BreakdownReport { get; set; }
    }

    /// <summary>
    /// 故障报告
    /// </summary>
    [Table("BreakdownReport")]
    public partial class BreakdownReport
    {
        public int Id { get; set; }

        public int Status { get; set; }

        public int QCEquipmentId { get; set; }

        public int BreakDownTypeId { get; set; }

        public int QCStaffId { get; set; }

        public DateTime BreakDownTime { get; set; }

        public DateTime ReportTime { get; set; }

        public DateTime? RecoveryTime { get; set; } 

        public DateTime? ConfirmTime { get; set; }

        [StringLength(256)]
        public string Photos { get; set; }

        [StringLength(256)]
        public string ReportContent { get; set; }

        [StringLength(256)]
        public string RecoveryRemark { get; set; }

        // 对应关系
        public virtual QCEquipment QCEquipment { get; set; }

        public virtual QCStaff QCStaff { get; set; }

        public virtual BreakdownType BreakdownType { get; set; }
    }

    /// <summary>
    /// 质检每日日程
    /// </summary>
    [Table("QCAgenda")]
    public partial class QCAgenda
    {
        public int Id { get; set; }

        public int FactoryId { get; set; }

        public int QCStaffId { get; set; }

        public int Status { get; set; }

        public DateTime Subscribe { get; set; }

        public DateTime CheckinTime { get; set; }

        public DateTime? CheckoutTime { get; set; }

        [StringLength(256)]
        public string Photos { get; set; }

        [StringLength(256)]
        public string CheckinRemark { get; set; }

        [StringLength(256)]
        public string CheckoutRemark { get; set; }
        
        [StringLength(256)]
        public string Remark { get; set; }

        public int OfficalWorkers { get; set; }

        public int TemporaryWorkers { get; set; }

        public DateTime? SummaryTime { get; set; }

        public decimal? WorkHours { get; set; }

        // 对应关系
        public virtual Factory Factory { get; set; }

        public virtual QCStaff QCStaff { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductionDetails> ProductionDetails { get; set; }
    }

    /// <summary>
    /// 产量详情
    /// </summary>
    [Table("ProductionDetails")]
    public partial class ProductionDetails
    {
        public int Id { get; set; }

        public int QCAgendaId { get; set; }

        public int ProductId { get; set; }

        public int ProductionQty { get; set; }

        // 对应关系
        public virtual QCAgenda QCAgenda {get;set;}

        public virtual Product Product { get; set; }
    }

    /// <summary>
    /// 质检模板
    /// </summary>
    [Table("QualityTestTemplate")]
    public partial class QualityTestTemplate
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        [StringLength(32)]
        public string KeyName { get; set; }

        [StringLength(16)]
        public string KeyTitle { get; set; }


        public int ValueTypeId { get; set; }

        public int ValueLength { get; set; }

        public bool ValueRequired { get; set; }

        public bool ForceComplete { get; set; }

        [StringLength(256)]
        public string StandardValue { get; set; }

        // 对应关系
        public virtual Product Product { get; set; }
    }

    /// <summary>
    /// 质量检验
    /// </summary>
    [Table("QualityTest")]
    public partial class QualityTest
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int FactoryId { get; set; }

        public int QCStaffId { get; set; }
        
        public string Keys { get; set; }

        public string Values { get; set; }

        public int EvalScore { get; set; }

        public int EvalTotal { get; set; }

        public bool EvalResult { get; set; }

        public DateTime ApplyTime { get; set; }

        [StringLength(256)]
        public string Photos { get; set; }

        [StringLength(256)]
        public string Remark { get; set; }

        // 对应关系
        public virtual Product Product { get; set; }

        public virtual QCStaff QCStaff { get; set; }

        public virtual Factory Factory { get; set; }
    }
    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
}