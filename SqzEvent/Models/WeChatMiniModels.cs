namespace SqzEvent.Models
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Collections.Generic;

    public class WeChatMiniModels : DbContext
    {
        //您的上下文已配置为从您的应用程序的配置文件(App.config 或 Web.config)
        //使用“WeChatMiniModels”连接字符串。默认情况下，此连接字符串针对您的 LocalDb 实例上的
        //“SqzEvent.Models.WeChatMiniModels”数据库。
        // 
        //如果您想要针对其他数据库和/或数据库提供程序，请在应用程序配置文件中修改“WeChatMiniModels”
        //连接字符串。
        public WeChatMiniModels()
            : base("name=WeChatMiniModels")
        {
        }

        public virtual DbSet<WechatUser> WechatUser { get; set; }
        public virtual DbSet<VoiceRecord> VoiceRecord { get; set; }
        public virtual DbSet<ProgrammLog> ProgrammLog { get; set; }
        public virtual DbSet<SmsValidate> SmsValidate { get; set; }

        //为您要在模型中包含的每种实体类型都添加 DbSet。有关配置和使用 Code First  模型
        //的详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=390109。

        // public virtual DbSet<MyEntity> MyEntities { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WechatUser>().HasMany(e => e.VoiceRecord).WithRequired(e => e.WechatUser).HasForeignKey(e => e.user_id).WillCascadeOnDelete(false);
        }
    }
    [Table("WechatUser")]
    public partial class WechatUser
    {
        public int id { get; set; }

        [StringLength(64), Required]
        public string open_id { get; set; }


        // 用户状态 0:未绑定手机;1:已绑定手机;2:;
        public int user_status { get; set; }

        [StringLength(16)]
        public string mobile { get; set; }

        // 授权信息
        public bool authorize { get; set; }

        [StringLength(64)]
        public string union_id { get; set; }

        [StringLength(64)]
        public string nickname { get; set; }

        [StringLength(256)]
        public string avatar_url { get; set; }

        public int gender { get; set; }

        [StringLength(32)]
        public string province { get; set; }

        [StringLength(32)]
        public string city { get; set; }

        [StringLength(32)]
        public string country { get; set; }

        // 注册&登陆信息
        public DateTime signup_time { get; set; }

        public DateTime lastlogin_time { get; set; }

        // Session信息
        [StringLength(128)]
        public string session_key { get; set; }

        [StringLength(128)]
        public string storage_session { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VoiceRecord> VoiceRecord { get; set; }
    }
    [Table("ProgrammLog")]
    public partial class ProgrammLog
    {
        public int id { get; set; }

        [Required, StringLength(64)]
        public string open_id { get; set; }

        // 1:登陆(包含重新登陆); 2:录音记录;3:录音存储记录;4:录音收听记录;
        public int type { get; set; }

        public DateTime log_time { get; set; }
    }

    [Table("SmsValidate")]
    public partial class SmsValidate
    {
        public int id { get; set; }

        [Required, StringLength(16), RegularExpression("1[3|4|5|7|8|][0-9]{9}")]
        public string mobile { get; set; }

        // 短信状态 1:已发送;2:已确认;
        public int status { get; set; }

        [StringLength(8)]
        public string validate_code { get; set; }

        public DateTime send_time { get; set; }
    }

    [Table("VoiceRecord")]
    public partial class VoiceRecord
    {
        public int id { get; set; }

        // 发送人ID
        public int user_id { get; set; }

        // 祝福名称
        [StringLength(32)]
        public string voice_title { get; set; }

        // 状态 -1:已删除;0:已保存;1:已发送;2:已收听;
        public int status { get; set; }

        // 文件地址
        [StringLength(128)]
        public string voice_path { get; set; }

        // 祝福语
        [StringLength(64)]
        public string voice_wish { get; set; }

        // 接收人姓名
        [StringLength(16)]
        public string receiver_name { get; set; }

        // 接收人手机号
        [StringLength(16),RegularExpression("1[3|4|5|7|8|][0-9]{9}")]
        public string receiver_mobile { get; set; }

        // 录音时间
        public DateTime record_time { get; set; }

        // 发送时间
        public DateTime? send_time { get; set; }

        // 接收时间
        public DateTime? receive_time { get; set; }

        public virtual WechatUser WechatUser { get; set; }
    }

    public class UserInfoViewModel
    {
        public string nickName { get; set; }

        public string avatarUrl { get; set; }

        public int gender { get; set; }

        public string province { get; set; }

        public string city { get; set; }

        public string country { get; set; }
    }
    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
}