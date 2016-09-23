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
}