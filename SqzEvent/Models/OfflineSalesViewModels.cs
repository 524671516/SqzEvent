using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace SqzEvent.Models
{
    
    public class Wx_ManagerReportListViewModel
    {
        public int Id { get; set; }

        public int Status { get; set; }

        public string StoreName { get; set; }

        public string SellerName { get; set; }

        public int? Rep_Total { get; set; }

        public decimal? AVG_Total { get; set; }

        public int StoreId { get; set; }

        [Required(ErrorMessage = "金额不能为空")]
        [Range(0, 200, ErrorMessage = "奖金金额不能大于200元")]
        public decimal? Bonus { get; set; }

        [Required(ErrorMessage = "红包说明不能为空")]
        [StringLength(128, ErrorMessage = "不超过128个字符")]
        public string Bonus_Remark { get; set; }
    }

    public class Wx_ManagerCreateScheduleViewModel
    {
        public int ScheduleId { get; set; }

        [Required(ErrorMessage = "至少选择一个门店")]
        public int Off_Store_Id { get; set; }

        [Required(ErrorMessage ="至少选择一个模板")]
        public int Off_Template_Id { get; set; }

        public DateTime Subscribe { get; set; }

        [RegularExpression("[012]\\d:[0-6]\\d", ErrorMessage = "格式错误")]
        [Required(ErrorMessage = "标准上班时间不能为空")]
        public string Standard_CheckIn { get; set; }

        [RegularExpression("[012]\\d:[0-6]\\d", ErrorMessage = "格式错误")]
        [Required(ErrorMessage = "标准下班时间不能为空")]
        public string Standard_CheckOut { get; set; }

        [Required(ErrorMessage = "标准薪资不能为空")]
        public decimal Standard_Salary { get; set; }
    }

    public class Wx_SellerCreditViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Mobile { get; set; }

        [Required]
        [StringLength(20)]
        public string AccountName { get; set; }

        [Required]
        [StringLength(20)]
        [RegularExpression("(^\\d{18}$)|(^\\d{15}$)|(^\\d{17}(\\d|X|x))", ErrorMessage = "格式错误")]
        public string IdNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string CardName { get; set; }

        [Required]
        [StringLength(50)]
        public string CardNo { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountSource { get; set; }
    }
    public class Wx_ReportItemsViewModel
    {
        public bool StorageRequired { get; set; }

        public bool AmountRequried { get; set; }

        public ICollection<Wx_TemplateProduct> ProductList { get; set; }
    }
    public class Wx_TemplateProduct
    {
        public int ProductId { get; set; }

        public string SimpleName { get; set; }

        public string ItemCode { get; set; }

        public int? SalesCount { get; set; }

        public int? Storage { get; set; }

        public decimal? SalesAmount { get; set; }
    }
    public class Wx_SellerTaskMonthStatistic
    {
        public Off_Seller Off_Seller{ get; set; }

        public int AttendanceCount { get; set; }
    }
    public class Wx_SellerTaskAlert
    {
        public int Id { get; set; }
        public DateTime ApplyDate { get; set; }
        public int? MinStorage { get; set; }
        public string StoreName { get; set; }
    }
    public class Wx_ManagerRecruitBindViewModel
    {
        public int StoreId { get; set; }

        public string Name { get; set; }

        public int RecruitId { get; set; }

        public string Mobile { get; set; }

        public string IdNumber { get; set; }
    }

    public class Wx_RecruitViewModel
    {
        [Required]
        [StringLength(11)]
        [RegularExpression("1[3|4|5|7|8|][0-9]{9}", ErrorMessage = "手机号码格式错误")]
        [Display(Name = "手机号码")]
        public string Mobile { get; set; }

        [Required(ErrorMessage = "姓名不能为空")]
        [StringLength(6, ErrorMessage = ("姓名长度不得超过6个字符"))]
        [Display(Name = "姓名")]
        public string Name { get; set; }

        [Display(AutoGenerateField = false)]
        public string Open_Id { get; set; }

        [Display(AutoGenerateField = false)]
        public string AccessToken { get; set; }

        [StringLength(11, ErrorMessage = "字符长度不得超过11个字符")]
        [Display(Name = "推荐码")]
        public string RecommandCode { get; set; }

        [Required]
        [StringLength(6, ErrorMessage = "手机验证码为6位")]
        [Display(Name = "手机验证码")]
        public string CheckCode { get; set; }

        public int SystemId { get; set; }
    }
    public class Wx_RecruitForceViewModel
    {
        [Required(ErrorMessage = "姓名不能为空")]
        [StringLength(6, ErrorMessage = ("姓名长度不得超过6个字符"))]
        [Display(Name = "姓名")]
        public string Name { get; set; }

        [StringLength(11, ErrorMessage = "字符长度不得超过11个字符")]
        [Display(Name = "推荐码")]
        public string RecommandCode { get; set; }

        [Display(AutoGenerateField = false)]
        public string Open_Id { get; set; }

        [Display(AutoGenerateField = false)]
        public string AccessToken { get; set; }

        public int SystemId { get; set; }
    }

    public class Wx_RecruitCompleteViewModel
    {
        [Display(AutoGenerateField = false)]
        public string UserName { get; set; }

        [Required]
        [StringLength(20)]
        [RegularExpression("(^\\d{18}$)|(^\\d{15}$)|(^\\d{17}(\\d|X|x))", ErrorMessage = "格式错误")]
        public string IdNumber { get; set; }

        public string AreaProvince { get; set; }

        public string AreaCity { get; set; }

        public string AreaDistrict { get; set; }

        public bool Weekday { get; set; }

        public bool Weekend { get; set; }

        public bool Holiday { get; set; }
    }
    public class Wx_WeekendBreakItem
    {
        public int ProductId { get; set; }
        public int SalesCount { get; set; }
        public string ProductName { get; set; }
    }
}