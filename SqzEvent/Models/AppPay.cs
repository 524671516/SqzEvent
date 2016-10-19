namespace SqzEvent.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Spatial;

    public partial class AppPay : DbContext
    {
        public AppPay()
            : base("name=AppPayConnection")
        {
        }

        public virtual DbSet<WxPaymentOrder> WxPaymentOrder { get; set; }
        public virtual DbSet<WxRedPackOrder> WxRedPackOrder { get; set; }
        public virtual DbSet<WxPaymentProduct> WxPaymentProduct { get; set; }
        public virtual DbSet<WxPressConferenceUser> WxPressConferenceUser { get; set; }
        public virtual DbSet<WxPressConferenceOrder> WxPressConferenceOrder { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
           
        }
    }

    [Table("WxPaymentOrder")]
    public partial class WxPaymentOrder
    {
        public int Id { get; set; }

        [Required]
        [StringLength(32)]
        public string Trade_No { get; set; }

        public int Trade_Status { get; set; }

        [Required]
        [StringLength(128)]
        public string Open_Id { get; set; }

        [Required]
        [StringLength(32)]
        public string Mch_Id { get; set; }

        [StringLength(32)]
        public string Device_Info { get; set; }

        [Required]
        [StringLength(32)]
        public string Body { get; set; }

        public string Detail { get; set; }

        [StringLength(128)]
        public string Attach { get; set; }

        [StringLength(16)]
        public string Bank_Type { get; set; }

        [StringLength(16)]
        public string Fee_Type { get; set; }

        public int Total_Fee { get; set; }

        public DateTime Time_Start { get; set; }

        public DateTime? Time_Expire { get; set; }

        [StringLength(32)]
        public string Goods_Tag { get; set; }

        [Required]
        [StringLength(16)]
        public string Trade_Type { get; set; }

        [StringLength(32)]
        public string Product_Id { get; set; }

        [StringLength(32)]
        public string Error_Msg { get; set; }

        [StringLength(128)]
        public string Error_Msg_Des { get; set; }

        [StringLength(64)]
        public string Prepay_Id { get; set; }
    }

    [Table("WxRedPackOrder")]
    public partial class WxRedPackOrder
    {
        public int Id { get; set; }

        [Required]
        [StringLength(32)]
        public string mch_billno { get; set; }

        [Required]
        [StringLength(32)]
        public string send_name { get; set; }

        [Required]
        [StringLength(32)]
        public string re_openid { get; set; }

        public int total_amount { get; set; }

        public int total_num { get; set; }

        [StringLength(128)]
        public string wishing { get; set; }

        [StringLength(32)]
        public string act_name { get; set; }

        [StringLength(256)]
        public string remark { get; set; }

        [StringLength(32)]
        public string detail_id { get; set; }

        [Required]
        [StringLength(16)]
        public string status { get; set; }

        [StringLength(32)]
        public string reason { get; set; }
        
        [StringLength(32)]
        public string hb_type { get; set; }

        [StringLength(32)]
        public string send_type { get; set; }

        [StringLength(128)]
        public string err_code_desc { get; set; }

        public DateTime? send_time { get; set; }

        public DateTime? refund_time { get; set; }

        public int? refund_amount { get; set; }

        public DateTime? rcv_time { get; set; }
    }

    [Table("WxPaymentProduct")]
    public partial class WxPaymentProduct
    {
        public int Id { get; set; }

        public string ProductName { get; set; }

        public string ProductDetails { get; set; }

        public decimal? Total_Fee { get; set; }
    }

    public partial class WxPressConferenceUser
    {
        public int Id { get; set; }

        [Required]
        [StringLength(11)]
        [RegularExpression("1[3|5|7|8|][0-9]{9}", ErrorMessage = "手机号码格式错误")]
        [Display(Name = "手机号码")]
        public string Mobile { get; set; }

        [Required(ErrorMessage = "姓名不能为空")]
        [StringLength(6, ErrorMessage = ("姓名长度不得超过6个字符"))]
        [Display(Name = "姓名")]
        public string Name { get; set; }

        [Required(ErrorMessage = "企业全称不能为空")]
        [StringLength(30, ErrorMessage = ("姓名长度不得超过30个字符"))]
        [Display(Name = "企业全称")]
        public string CompanyName { get; set; }

        [Display(AutoGenerateField = false)]
        public string Open_Id { get; set; }

        [Display(AutoGenerateField = false)]
        public string AccessToken { get; set; }

        [Display(AutoGenerateField = false)]
        public string HeadImgUrl { get; set; }

        [Display(AutoGenerateField = false)]
        public string NickName { get; set; }

        [Display(AutoGenerateField = false)]
        public bool Sex { get; set; }
    }

    public partial class WxPressConferenceOrder
    {
        public int Id { get; set; }

        public string Open_Id { get; set; }

        public string Name { get; set; }

        public decimal? Amount { get; set; }

        public DateTime ApplyTime { get; set; }

        public string ImgUrl { get; set; }

        public string OrderNo { get; set; }

        public int Status { get; set; }

        public int OrderType { get; set; }

    }
}
