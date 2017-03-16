namespace SqzEvent.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class Configuration : DbContext
    {
        public Configuration()
            : base("name=ConfigurationConnection")
        {
        }

        public virtual DbSet<WeChatConfigs> WeChatConfigs { get; set; }
        public virtual DbSet<WeChatStatistic> WeChatStatistic { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }

    public partial class WeChatConfigs
    {
        public int Id { get; set; }

        [StringLength(200)]
        public string Key { get; set; }

        [StringLength(256)]
        public string Value { get; set; }

        public DateTime LastModify { get; set; }
    }
    [Table("WeChatStatistic")]
    public partial class WeChatStatistic
    {
        public int Id { get; set; }
        [StringLength(32)]
        public string PageName { get; set; }
        [StringLength(128)]
        public string PageURL { get; set; }
        public DateTime AccessDatetime { get; set; }
        [StringLength(16)]
        public string HostAddress { get; set; }

    }
}
