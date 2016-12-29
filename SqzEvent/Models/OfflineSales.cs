namespace SqzEvent.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class OfflineSales : DbContext
    {
        public OfflineSales()
            : base("name=OfflineSalesConnection")
        {
        }
        

        public virtual DbSet<Off_SalesInfo_Daily> Off_SalesInfo_Daily { get; set; }
        public virtual DbSet<Off_SalesInfo_Month> Off_SalesInfo_Month { get; set; }
        public virtual DbSet<Off_Seller> Off_Seller { get; set; }
        public virtual DbSet<Off_Store> Off_Store { get; set; }
        public virtual DbSet<Off_Expenses> Off_Expenses { get; set; }
        public virtual DbSet<Off_Expenses_Details> Off_Expenses_Details { get; set; }
        public virtual DbSet<Off_Expenses_Payment> Off_Expenses_Payment { get; set; }
        public virtual DbSet<Off_Membership_Bind> Off_Membership_Bind { get; set; }
        public virtual DbSet<Off_Checkin_Schedule> Off_Checkin_Schedule { get; set; }
        public virtual DbSet<Off_Checkin> Off_Checkin { get; set; }
        public virtual DbSet<Off_StoreManager> Off_StoreManager { get; set; }
        public virtual DbSet<Off_Manager_Task> Off_Manager_Task { get; set; }
        public virtual DbSet<Off_Manager_CheckIn> Off_Manager_CheckIn { get; set; }
        public virtual DbSet<Off_Manager_Announcement> Off_Manager_Announcement { get; set; }
        public virtual DbSet<Off_Manager_Request> Off_Manager_Request { get; set; }
        public virtual DbSet<Off_BonusRequest> Off_BonusRequest { get; set; }
        public virtual DbSet<Off_System> Off_System { get; set; }
        public virtual DbSet<Off_Checkin_Product> Off_Checkin_Product { get; set; }
        public virtual DbSet<Off_Product> Off_Product { get; set; }
        public virtual DbSet<Off_Daily_Product> Off_Daily_Product { get; set; }
        public virtual DbSet<Off_Sales_Template> Off_Sales_Template { get; set; }
        public virtual DbSet<Off_AVG_Info> Off_AVG_Info { get; set; }
        public virtual DbSet<Off_System_Setting> Off_System_Setting { get; set; }
        public virtual DbSet<Off_SellerTask> Off_SellerTask { get; set; }
        public virtual DbSet<Off_SellerTaskProduct> Off_SellerTaskProduct { get; set; }
        public virtual DbSet<Off_CompetitionInfo> Off_CompetitionInfo { get; set; }
        public virtual DbSet<Off_Recruit> Off_Recruit { get; set; }
        public virtual DbSet<Off_WeekendBreak> Off_WeekendBreak { get; set; }
        public virtual DbSet<Off_WeekendBreakRecord> Off_WeekendBreakRecord { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Off_SalesInfo_Daily>()
                .Property(e => e.Salary)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Off_SalesInfo_Daily>()
                .Property(e => e.Bonus)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Off_Seller>()
                .HasMany(e => e.Off_SalesInfo_Daily)
                .WithOptional(e => e.Off_Seller)
                .HasForeignKey(e => e.SellerId);

            modelBuilder.Entity<Off_Seller>()
                .HasMany(e => e.Off_Membership_Bind)
                .WithOptional(e => e.Off_Seller)
                .HasForeignKey(e => e.Off_Seller_Id)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_Seller>()
                .HasMany(e => e.Off_Checkin)
                .WithRequired(e => e.Off_Seller)
                .HasForeignKey(e => e.Off_Seller_Id)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_Seller>()
                .HasMany(e => e.Off_SellerTask)
                .WithRequired(e => e.Off_Seller)
                .HasForeignKey(e => e.SellerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_Checkin_Schedule>()
                .HasMany(e => e.Off_Checkin)
                .WithRequired(e => e.Off_Checkin_Schedule)
                .HasForeignKey(e => e.Off_Schedule_Id)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_Checkin_Schedule>()
                .HasMany(e => e.Off_WeekendBreak)
                .WithRequired(e => e.Off_Checkin_Schedule)
                .HasForeignKey(e => e.ScheduleId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_Checkin>()
                .HasMany(e => e.Off_BonusRequest)
                .WithRequired(e => e.Off_Checkin)
                .HasForeignKey(e => e.CheckinId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_Checkin>()
                .HasMany(e => e.Off_Checkin_Product)
                .WithRequired(e => e.Off_Checkin)
                .HasForeignKey(e => e.CheckinId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_SalesInfo_Daily>()
                .HasMany(e => e.Off_Daily_Product)
                .WithRequired(e => e.Off_SalesInfo_Daily)
                .HasForeignKey(e => e.DailyId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_Store>()
                .HasMany(e => e.Off_SalesInfo_Daily)
                .WithRequired(e => e.Off_Store)
                .HasForeignKey(e => e.StoreId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_Store>()
                .HasMany(e => e.Off_SalesInfo_Month)
                .WithRequired(e => e.Off_Store)
                .HasForeignKey(e => e.StoreId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_Store>()
                .HasMany(e => e.Off_Seller)
                .WithRequired(e => e.Off_Store)
                .HasForeignKey(e => e.StoreId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_Store>()
                .HasMany(e => e.Off_Checkin_Schedule)
                .WithRequired(e => e.Off_Store)
                .HasForeignKey(e => e.Off_Store_Id)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_Store>()
                .HasMany(e => e.Off_Manager_Request)
                .WithRequired(e => e.Off_Store)
                .HasForeignKey(e => e.StoreId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_Store>()
                .HasMany(e => e.Off_AVG_Info)
                .WithRequired(e => e.Off_Store)
                .HasForeignKey(e => e.StoreId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_Store>()
                .HasMany(e => e.Off_SellerTask)
                .WithRequired(e => e.Off_Store)
                .HasForeignKey(e => e.StoreId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_Store>()
                .HasMany(e => e.Off_CompetitionInfo)
                .WithRequired(e => e.Off_Store)
                .HasForeignKey(e => e.StoreId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_System>()
                .HasMany(e => e.Off_Store)
                .WithRequired(e => e.Off_System)
                .HasForeignKey(e => e.Off_System_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_System>()
                .HasMany(e => e.Off_Membership_Bind)
                .WithRequired(e => e.Off_System)
                .HasForeignKey(e => e.Off_System_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_System>()
                .HasMany(e => e.Off_Seller)
                .WithRequired(e => e.Off_System)
                .HasForeignKey(e => e.Off_System_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_System>()
                .HasMany(e => e.Off_StoreManager)
                .WithRequired(e => e.Off_System)
                .HasForeignKey(e => e.Off_System_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_System>()
                .HasMany(e => e.Off_Checkin_Schedule)
                .WithRequired(e => e.Off_System)
                .HasForeignKey(e => e.Off_System_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_System>()
                .HasMany(e => e.Off_Expenses)
                .WithRequired(e => e.Off_System)
                .HasForeignKey(e => e.Off_System_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_System>()
                .HasMany(e => e.Off_Manager_Task)
                .WithRequired(e => e.Off_System)
                .HasForeignKey(e => e.Off_System_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_System>()
                .HasMany(e => e.Off_Product)
                .WithRequired(e => e.Off_System)
                .HasForeignKey(e => e.Off_System_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_System>()
                .HasMany(e => e.Off_Manager_Announcement)
                .WithRequired(e => e.Off_System)
                .HasForeignKey(e => e.Off_System_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_System>()
                .HasMany(e => e.Off_Sales_Template)
                .WithRequired(e => e.Off_System)
                .HasForeignKey(e => e.Off_System_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_System>()
                .HasMany(e => e.Off_System_Setting)
                .WithRequired(e => e.Off_System)
                .HasForeignKey(e => e.Off_System_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_System>()
                .HasMany(e => e.Off_Recruit)
                .WithRequired(e => e.Off_System)
                .HasForeignKey(e => e.Off_System_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_Sales_Template>()
                .HasMany(e => e.Off_Checkin_Schedule)
                .WithRequired(e => e.Off_Sales_Template)
                .HasForeignKey(e => e.TemplateId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_StoreManager>()
                .HasMany(e => e.Off_WeekendBreak).WithRequired(e => e.Off_StoreManager).HasForeignKey(e => e.StoreManagerId).WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_Product>()
                .HasMany(e => e.Off_Checkin_Product)
                .WithRequired(e => e.Off_Product)
                .HasForeignKey(e => e.ProductId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_Product>()
                .HasMany(e => e.Off_Daily_Product)
                .WithRequired(e => e.Off_Product)
                .HasForeignKey(e => e.ProductId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_Product>()
                .HasMany(e => e.Off_SellerTaskProduct)
                .WithRequired(e => e.Off_Product)
                .HasForeignKey(e => e.ProductId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_Expenses>()
                .HasMany(e => e.Off_Expenses_Details)
                .WithRequired(e => e.Off_Expenses)
                .HasForeignKey(e => e.ExpensesId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_Expenses>()
                .HasMany(e=>e.Off_Expenses_Payment)
                .WithRequired(e => e.Off_Expenses)
                .HasForeignKey(e => e.ExpensesId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_Manager_Task>()
                .HasMany(e => e.Off_Manager_CheckIn)
                .WithRequired(e => e.Off_Manager_Task)
                .HasForeignKey(e => e.Manager_EventId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_SellerTask>()
                .HasMany(e => e.Off_SellerTaskProduct)
                .WithRequired(e => e.Off_SellerTask)
                .HasForeignKey(e => e.SellerTaskId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_WeekendBreak>().HasMany(e => e.Off_WeekendBreakRecord).WithRequired(e => e.Off_WeekendBreak).HasForeignKey(e => e.WeekendBreakId).WillCascadeOnDelete(true);
        }
    }
    public partial class Off_System
    {
        public int Id { get; set; }

        [StringLength(64)]
        public string SystemName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Membership_Bind> Off_Membership_Bind { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Seller> Off_Seller { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Store> Off_Store { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_StoreManager> Off_StoreManager { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Checkin_Schedule> Off_Checkin_Schedule { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Expenses> Off_Expenses { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Manager_Announcement> Off_Manager_Announcement { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Manager_Task> Off_Manager_Task { get; set; }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Product> Off_Product { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Sales_Template> Off_Sales_Template { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_System_Setting> Off_System_Setting { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Recruit> Off_Recruit { get; set; }
    }
    public partial class Off_System_Setting
    {
        public int Id { get; set; }
        public int Off_System_Id { get; set; }
        public virtual Off_System Off_System { get; set; }
        public string SettingName { get; set; }
        public bool SettingResult  { get; set; }
        public string SettingValue { get; set; }
    }
    public partial class Off_Product
    {
        public int Id { get; set; }

        public int Off_System_Id { get; set; }

        public virtual Off_System Off_System { get; set; }

        public int status { get; set; }

        [StringLength(64)]
        public string ItemCode { get; set; }

        [Required]
        [StringLength(128)]
        public string ItemName { get; set; }

        [Required]
        [StringLength(8)]
        public string SimpleName { get; set; }

        [StringLength(32)]
        public string Spec { get; set; }

        public decimal? SalesPrice { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Checkin_Product> Off_Checkin_Product { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Daily_Product> Off_Daily_Product { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_SellerTaskProduct> Off_SellerTaskProduct { get; set; }
    }

    public partial class Off_Store
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Off_Store()
        {
            Off_SalesInfo_Daily = new HashSet<Off_SalesInfo_Daily>();
            Off_SalesInfo_Month = new HashSet<Off_SalesInfo_Month>();
        }

        public int Id { get; set; }

        [StringLength(50)]
        public string StoreSystem { get; set; }

        [Required]
        [StringLength(255)]
        public string StoreName { get; set; }

        [StringLength(20)]
        public string Region { get; set; }

        [StringLength(50)]
        public string Distributor { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        [StringLength(50)]
        public string Longitude { get; set; }

        [StringLength(50)]
        public string Latitude { get; set; }

        public DateTime? UploadTime { get; set; }

        [StringLength(255)]
        public string UploadUser { get; set; }
        

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_SalesInfo_Daily> Off_SalesInfo_Daily { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_SalesInfo_Month> Off_SalesInfo_Month { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Seller> Off_Seller { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Checkin_Schedule> Off_Checkin_Schedule { get; set; }
        

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_StoreManager> Off_StoreManager { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Manager_Request> Off_Manager_Request { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_AVG_Info> Off_AVG_Info { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_SellerTask> Off_SellerTask { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_CompetitionInfo> Off_CompetitionInfo { get; set; }

        public int Off_System_Id { get; set; }

        public virtual Off_System Off_System { get; set; }
    }

    public partial class Off_Seller
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Off_Seller()
        {
            Off_SalesInfo_Daily = new HashSet<Off_SalesInfo_Daily>();
            Off_Membership_Bind = new HashSet<Off_Membership_Bind>();
            Off_Checkin = new HashSet<Off_Checkin>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Mobile { get; set; }

        public int StoreId { get; set; }

        [StringLength(20)]
        public string AccountName { get; set; }

        [StringLength(20)]
        public string IdNumber { get; set; }

        [StringLength(50)]
        public string CardName { get; set; }

        [StringLength(50)]
        public string CardNo { get; set; }

        [StringLength(50)]
        public string AccountSource { get; set; }

        public int StandardSalary { get; set; }

        public DateTime? UploadTime { get; set; }

        [StringLength(255)]
        public string UploadUser { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_SalesInfo_Daily> Off_SalesInfo_Daily { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Membership_Bind> Off_Membership_Bind { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Checkin> Off_Checkin { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_SellerTask> Off_SellerTask { get; set; }

        public virtual Off_Store Off_Store { get; set; }

        public int Off_System_Id { get; set; }

        public virtual Off_System Off_System { get; set; }
    }

    public partial class Off_SalesInfo_Daily
    {
        public int Id { get; set; }

        public int StoreId { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        public int? Item_Brown { get; set; }

        public int? Item_Black { get; set; }

        public int? Item_Lemon { get; set; }

        public int? Item_Honey { get; set; }

        public int? Item_Dates { get; set; }

        public int? SellerId { get; set; }

        public int? Attendance { get; set; }

        public decimal? Salary { get; set; }

        public decimal? Debit { get; set; }

        public decimal? Bonus { get; set; }

        public bool isMultiple { get; set; }

        public string remarks { get; set; }

        public DateTime? UploadTime { get; set; }

        [StringLength(255)]
        public string UploadUser { get; set; }

        public virtual Off_Seller Off_Seller { get; set; }

        public virtual Off_Store Off_Store { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Daily_Product> Off_Daily_Product { get; set; }
    }
    public partial class Off_Daily_Product
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int DailyId { get; set; }

        [StringLength(64)]
        public string ItemCode { get; set; }

        public int? SalesCount { get; set; }

        public int? StorageCount { get; set; }

        public decimal? SalesAmount { get; set; }

        public virtual Off_SalesInfo_Daily Off_SalesInfo_Daily { get; set; }

        public virtual Off_Product Off_Product { get; set; }
    }
    public partial class Off_SalesInfo_Month
    {
        public int Id { get; set; }

        public int StoreId { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        public int? Item_Brown { get; set; }

        public int? Item_Black { get; set; }

        public int? Item_Lemon { get; set; }

        public int? Item_Honey { get; set; }

        public int? Item_Dates { get; set; }

        public decimal? TotalFee { get; set; }

        public DateTime? UploadTime { get; set; }

        [StringLength(255)]
        public string UploadUser { get; set; }

        public virtual Off_Store Off_Store { get; set; }
    }

    public partial class Off_Expenses
    {
        public int Id { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ApplicationDate { get; set; }

        public int Status { get; set; }

        public string StoreSystem { get; set; }

        public string Distributor { get; set; }

        public int PaymentType { get; set; }

        public string Remarks { get; set; }

        public DateTime? CheckTime { get; set; }

        public DateTime? BalanceTime { get; set; }

        [StringLength(255)]
        public string UploadUser { get; set; }

        public DateTime? UploadTime { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Expenses_Details> Off_Expenses_Details { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Expenses_Payment> Off_Expenses_Payment { get; set; }

        public int Off_System_Id { get; set; }

        public virtual Off_System Off_System { get; set; }
    }

    public partial class Off_Expenses_Details
    {
        public int Id { get; set; }

        public int ExpensesId { get; set; }

        public int ExpensesType { get; set; }

        public string DetailsName { get; set; }

        public decimal DetailsFee { get; set; }

        public string Remarks { get; set; }

        [StringLength(255)]
        public string UploadUser { get; set; }

        public DateTime? UploadTime { get; set; }

        public virtual Off_Expenses Off_Expenses { get; set; }
    }

    public partial class Off_Expenses_Payment
    {
        public int Id { get; set; }

        public int ExpensesId { get; set; }

        public DateTime? ApplicationDate { get; set; }

        public int VerifyType { get; set; }

        public decimal? VerifyCost { get; set; }

        public string Remarks { get; set; }

        [StringLength(255)]
        public string UploadUser { get; set; }

        public DateTime? UploadTime { get; set; }

        public virtual Off_Expenses Off_Expenses { get; set; }
    }

    public partial class Off_Membership_Bind
    {
        public int Id { get; set; }

        [Required]
        [StringLength(128)]
        public string UserName { get; set; }

        [Required]
        [StringLength(64)]
        public string NickName { get; set; }

        [Required]
        [StringLength(64)]
        public string Mobile { get; set; }
        
        public bool Bind { get; set; }

        public bool Recruit { get; set; }

        public int? Off_Seller_Id { get; set; }
        
        public DateTime ApplicationDate { get; set; }

        public virtual Off_Seller Off_Seller { get; set; }

        public int Off_System_Id { get; set; }

        public virtual Off_System Off_System { get; set; }

        public int Type { get; set; }
    }
    public partial class Off_Sales_Template
    {
        public int Id { get; set; }

        [Required]
        [StringLength(128)]
        public string TemplateName { get; set; }

        public int Status { get; set; }

        public bool RequiredStorage { get; set; }

        public bool RequiredAmount { get; set; }

        [Required]
        public string ProductList { get; set; }

        public int Off_System_Id { get; set; }

        public virtual Off_System Off_System { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Checkin_Schedule> Off_Checkin_Schedule { get; set; }
    }
    public partial class Off_Checkin_Schedule
    {
        public int Id { get; set; }

        public int Off_Store_Id { get; set; }

        public DateTime Subscribe { get; set; }

        public DateTime Standard_CheckIn { get; set; }

        public DateTime Standard_CheckOut { get; set; }

        public decimal? Standard_Salary { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Checkin> Off_Checkin { get; set; }

        public virtual Off_Store Off_Store { get; set; }

        public int Off_System_Id { get; set; }

        public virtual Off_System Off_System { get; set; }

        public int TemplateId { get; set; }
        public virtual Off_Sales_Template Off_Sales_Template { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_WeekendBreak> Off_WeekendBreak { get; set; }
    }
    public partial class Off_Checkin
    {
        public int Id { get; set; }

        public int Off_Schedule_Id { get; set; }

        public int Status { get; set; }

        public bool Proxy { get; set; }

        public int Off_Seller_Id { get; set; }

        public DateTime? CheckinTime { get; set; }

        [StringLength(64)]
        public string CheckinLocation { get; set; }

        [StringLength(64)]
        public string CheckinPhoto { get; set; }

        public DateTime? CheckoutTime { get; set; }

        [StringLength(64)]
        public string CheckoutPhoto { get; set; }

        [StringLength(64)]
        public string CheckoutLocation { get; set; }

        public int? Rep_Brown { get; set; }

        public int? Rep_Black { get; set; }

        public int? Rep_Honey { get; set; }

        public int? Rep_Lemon { get; set; }

        public int? Rep_Dates { get; set; }

        public int? Rep_Other { get; set; }

        [StringLength(512, ErrorMessage ="不超过512个字符")]
        public string Rep_Image { get; set; }

        [StringLength(512,ErrorMessage ="不超过512个字符")]
        public string Remark { get; set; }

        public DateTime? Report_Time { get; set; }

        [StringLength(64, ErrorMessage = "不超过64个字符")]
        public string ConfirmUser { get; set; }

        public DateTime? ConfirmTime { get; set; }

        [StringLength(512, ErrorMessage = "不超过512个字符")]
        public string Confirm_Remark { get; set; }

        public decimal? Bonus { get; set; }

        [StringLength(128, ErrorMessage = "不超过128个字符")]
        public string Bonus_Remark { get; set; }

        [StringLength(64, ErrorMessage = "不超过64个字符")]
        public string Bonus_User { get; set; }

        [StringLength(64, ErrorMessage = "不超过64个字符")]
        public string SubmitUser { get; set; }

        public DateTime? SubmitTime { get; set; }

        public virtual Off_Seller Off_Seller { get; set; }

        public virtual Off_Checkin_Schedule Off_Checkin_Schedule { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_BonusRequest> Off_BonusRequest{ get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Checkin_Product> Off_Checkin_Product { get; set; }
    }

    public partial class Off_Checkin_Product
    {
        public int Id { get; set; }

        public int CheckinId { get; set; }

        public int ProductId { get; set; }

        [StringLength(64)]
        public string ItemCode { get; set; }

        public int? SalesCount { get; set; }

        public int? StorageCount { get; set; }

        public decimal? SalesAmount { get; set; }

        public virtual Off_Checkin Off_Checkin { get; set; }

        public virtual Off_Product Off_Product { get; set; }
    }

    public partial class Off_StoreManager
    {
        public int Id { get; set; }

        public int Status { get; set; }

        [StringLength(32)]
        public string UserName { get; set; }

        [StringLength(32)]
        public string NickName { get; set; }

        [StringLength(32)]
        public string Mobile { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Store> Off_Store { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_WeekendBreak> Off_WeekendBreak { get; set; }

        public int Off_System_Id { get; set; }

        public virtual Off_System Off_System { get; set; }
    }

    public partial class Off_Manager_Task
    {
        public int Id { get; set; }

        public int Status { get; set; }
        [StringLength(32)]
        public string UserName{get;set;}

        [StringLength(32)]
        public string NickName { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime TaskDate { get; set; }
                                          
        [StringLength(512)]
        public string Photo { get; set; }

        [StringLength(512)]
        public string Event_Complete { get; set; }

        [StringLength(512)]
        public string Event_UnComplete { get; set; }

        [StringLength(512)]
        public string Event_Assistance { get; set; }

        public int? Eval_Value { get; set; }

        [StringLength(256)]
        public string Eval_Remark { get; set; }

        [StringLength(32)]
        public string Eval_User { get; set; }

        public DateTime? Eval_Time { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Manager_CheckIn> Off_Manager_CheckIn { get; set; }

        public int Off_System_Id { get; set; }

        public virtual Off_System Off_System { get; set; }
    }

    public partial class Off_Manager_CheckIn
    {
        public int Id { get; set; }

        public int Manager_EventId { get; set; }

        public bool Canceled { get; set; }

        [StringLength(64)]
        public string Location { get; set; }

        [StringLength(128)]
        public string Location_Desc { get; set; }

        [StringLength(128)]
        public string Photo { get; set; }

        [StringLength(64)]
        public string Remark { get; set; }

        public DateTime CheckIn_Time { get; set; }

        public virtual Off_Manager_Task Off_Manager_Task { get; set; }
    }

    public partial class Off_AVG_Info
    {
        public int Id { get; set; }

        public int StoreId { get; set; }

        public int DayOfWeek { get; set; }

        public decimal? AVG_SalesData { get; set; }

        public decimal? AVG_AmountData { get; set; }

        public virtual Off_Store Off_Store { get; set; }
    }

    public partial class Off_Manager_Announcement
    {
        public int Id { get; set; }

        [StringLength(512)]
        public string ManagerUserName { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime FinishTime { get; set; }

        [Range(1, 5)]
        public int Priority { get; set; }

        [StringLength(64)]
        public string Title { get; set; }

        [StringLength(1024)]
        public string Content { get; set; }

        [StringLength(64)]
        public string SubmitUser { get; set; }

        public DateTime SubmitTime { get; set; }

        public bool Status { get; set; }

        public int Off_System_Id { get; set; }

        public virtual Off_System Off_System { get; set; }
    }

    public partial class Off_Manager_Request
    {
        public int Id { get; set; }

        [StringLength(64)]
        public string ManagerUserName { get; set; }

        public int StoreId { get; set; }

        public int Status { get; set; }

        [StringLength(64)]
        public string RequestType { get; set; }

        [StringLength(1024)]
        public string RequestContent { get; set; }

        [StringLength(512)]
        public string RequestRemark { get; set; }

        public DateTime RequestTime { get; set; }

        [StringLength(512)]
        public string ReplyContent { get; set; }

        [StringLength(64)]
        public string ReplyUser { get; set; }

        public DateTime? ReplyTime { get; set; }

        public virtual Off_Store Off_Store { get; set; }
    }

    public partial class Off_BonusRequest
    {
        public int Id { get; set; }

        public int CheckinId { get; set; }

        public int Status { get; set; }

        [StringLength(64)]
        public string ReceiveOpenId { get; set; }

        [StringLength(128)]
        public string ReceiveUserName { get; set; }
        
        public int ReceiveAmount { get; set; }

        [StringLength(128)]
        public string RequestUserName { get; set; }

        public DateTime RequestTime { get; set; }

        [StringLength(128)]
        public string CommitUserName { get; set; }

        public DateTime? CommitTime { get; set; }

        [StringLength(256)]
        public string Mch_BillNo { get; set; }

        public virtual Off_Checkin Off_Checkin { get; set; }
    }

    public partial class Off_SellerTask
    {
        public int Id { get; set; }

        public int StoreId { get; set; }

        public int SellerId { get; set; }

        public DateTime ApplyDate { get; set; }

        [StringLength(256)]
        public string TaskPhotoList { get; set; }

        [StringLength(64)]
        public string LastUpdateUser { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public virtual Off_Seller Off_Seller { get; set; }

        public virtual Off_Store Off_Store { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_SellerTaskProduct> Off_SellerTaskProduct { get; set; }
    }

    public partial class Off_SellerTaskProduct
    {
        public int Id { get; set; }

        public int SellerTaskId { get; set; }

        public int ProductId { get; set; }

        [StringLength(64)]
        public string ItemCode { get; set; }

        public decimal? SalesAmount { get; set; }

        public int? SalesCount { get; set; }

        public int? StorageCount { get; set; }

        public virtual Off_SellerTask Off_SellerTask { get; set; }

        public virtual Off_Product Off_Product { get; set; }
    }

    public partial class Off_CompetitionInfo
    {
        public int Id { get; set; }

        public int StoreId { get; set; }

        [StringLength(512, ErrorMessage = "不超过512个字符")]
        public string CompetitionImage { get; set; }

        [StringLength(512, ErrorMessage = "不超过512个字符")]
        public string Remark { get; set; }

        [StringLength(64)]
        public string ReceiveOpenId { get; set; }

        [StringLength(128)]
        public string ReceiveUserName { get; set; }

        [StringLength(128)]
        public string NickName { get; set; }

        public decimal? BonusAmount { get; set; }

        public int Status { get; set; }

        public DateTime ApplicationDate { get; set; }

        public DateTime? BonusApplyDate { get; set; }

        public string BonusApplyUser { get; set; }

        [StringLength(256)]
        public string Mch_BillNo { get; set; }

        public virtual Off_Store Off_Store { get; set; }
    }

    public partial class Off_Recruit
    {
        public int Id { get; set; }

        [StringLength(32)]
        public string Name { get; set; }

        [StringLength(32)]
        public string UserName { get; set; }

        [StringLength(32)]
        public string Mobile { get; set; }

        public int Status { get; set; }

        [StringLength(256)]
        public string Area { get; set; }

        [StringLength(64)]
        public string WorkType { get; set; }

        [StringLength(64)]
        public string IdNumber { get; set; }

        [StringLength(128)]
        public string RecommandUserId { get; set; }

        public bool Reward { get; set; }

        public DateTime ApplyTime { get; set; }

        public int Off_System_Id { get; set; }

        public virtual Off_System Off_System { get; set; }
    }

    public partial class Off_WeekendBreak
    {
        public int Id { get; set; }

        public int StoreManagerId { get; set; }

        public int ScheduleId { get; set; }

        public DateTime Subscribe { get; set; }

        public DateTime SignInTime { get; set; }

        [StringLength(32)]
        public string UserName { get; set; }

        public DateTime? LastUploadTime { get; set; }

        public int? TrailDefault { get; set; }

        public virtual Off_StoreManager Off_StoreManager { get; set; }

        public virtual Off_Checkin_Schedule Off_Checkin_Schedule { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_WeekendBreakRecord> Off_WeekendBreakRecord { get; set; }
    }

    public partial class Off_WeekendBreakRecord
    {
        public int Id { get; set; }

        public int WeekendBreakId { get; set; }

        public DateTime UploadTime { get; set; }

        public int SalesCount { get; set; }

        public int TrailCount { get; set; }

        public string SalesDetails { get; set; }

        public virtual Off_WeekendBreak Off_WeekendBreak { get;set;}
    }
    
}
