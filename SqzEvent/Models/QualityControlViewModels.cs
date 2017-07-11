using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SqzEvent.Models
{
    public class QC_RegisterViewModel
    {
        [Required]
        [StringLength(11)]
        [RegularExpression("1[3|4|5|7|8|][0-9]{9}", ErrorMessage = "手机号码格式错误")]
        [Display(Name = "手机号码")]
        public string Mobile { get; set; }

        [Required]
        [StringLength(6, ErrorMessage = "手机验证码为6位")]
        [Display(Name = "手机验证码")]
        public string CheckCode { get; set; }

        [Required(ErrorMessage = "姓名不能为空")]
        [StringLength(6, ErrorMessage = ("姓名长度不得超过6个字符"))]
        [Display(Name = "姓名")]
        public string NickName { get; set; }

        [Display(AutoGenerateField = false)]
        public string Open_Id { get; set; }

        [Display(AutoGenerateField = false)]
        public string AccessToken { get; set; }
    }

    public class QC_ForceRegisterViewModel
    {
        [Required(ErrorMessage = "姓名不能为空")]
        [StringLength(6, ErrorMessage = ("姓名长度不得超过6个字符"))]
        [Display(Name = "姓名")]
        public string NickName { get; set; }

        public int Systemid { get; set; }
    }
    public class QC_ManagerLoginViewModel
    {
        [Required]
        [StringLength(11)]
        [RegularExpression("1[3|4|5|7|8|][0-9]{9}", ErrorMessage = "手机号码格式错误")]
        [Display(Name = "手机号码")]
        public string Mobile { get; set; }

        [Required]
        [StringLength(6, ErrorMessage = "手机验证码为6位")]
        [Display(Name = "手机验证码")]
        public string CheckCode { get; set; }
    }

    public class QC_StaffViewModel
    {
        public string OpenId { get; set; }

        public string Name { get; set; }

        public string Mobile { get; set; }

        public string ImgUrl { get; set; }

        public string NickName { get; set; }
    }

    public class TestTemplateItem
    {
        public int type { get; set; }

        public string key { get; set; }

        public string value { get; set; }

        public string title { get; set; }

        public string default_value { get; set; }
    }
    #region 新元素
    // 内容组成JSON
    public class QCContent
    {
        public int TemplateId { get; set; }

        public ICollection<QCContentCategory> CategoryItems { get; set; }
    }

    public class QCContentCategory
    {
        public bool State { get; set; }

        public string CategoryName { get; set; }

        public ICollection<QCContentItem> Columns { get; set; }
    }
    
    public class QCContentItem
    {
        public int Type { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public string Title { get; set; }

        public string Default_value { get; set; }
    }
    // 模板组成JSON
    public class QCTemplateContent
    {
        public string CatetoryName { get; set; }

        public ICollection<QCTemplateColumns> QCTemplateColumns { get; set; }
    }

    public class QCTemplateColumns
    {
        public string KeyName { get; set; }

        public string KeyTitle { get; set; }

        public int ValueTypeId { get; set; }

        public int LengthValue { get; set; }

        public bool Required { get; set; }

        public bool ForceComplete { get; set; }

        public string Unit { get; set; }
    }
    #endregion

    public class FactoryGroup
    {
        public int FactoryId { get; set; }

        public string FactoryName { get; set; }
    }
    public class AgendaDetailsViewModel
    {
        public int? FactoryId { get; set; }
        public string SelectDate { get; set; }
        public string FactoryName { get; set; }
    }

    public class QualityTestViewModel
    {
        public int? Qt_fid { get; set; }
        public string Qt_SelectDate { get; set; }
        public string Qt_FactoryName { get; set; }
    }
    public class BreakdownViewModel
    {
        public int? Bd_fid { get; set; }
        public string Bd_SelectDate { get; set; }
        public string Bd_FactoryName { get; set; }
    }

    public class Manager_HomeViewModel
    {
        public string FatoryName { get; set; }

        public int FactoryId { get; set; }

        public bool Tips { get; set; }

        public bool Status { get; set; }
    }

    public class MonthSchedule
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string FactoryName { get; set; }
        public int FactoryId { get; set; }
        public int ProductionPlan { get; set; }
        public int? Qty { get; set; }

    }
    public class MonthProduction
    {
        public Product Product { get; set; }
        public int FactoryId { get; set; }
        public int ProductQty { get; set; }
    }
}