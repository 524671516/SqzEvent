using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace SqzEvent.Models
{
    public class Wx_RegisterViewModel
    {
        [Required]
        [StringLength(11)]
        [RegularExpression("1[3|5|7|8|][0-9]{9}", ErrorMessage ="手机号码格式错误")]
        [Display(Name ="手机号码")]
        public string Mobile { get; set; }

        [Required]
        [StringLength(6, ErrorMessage = "手机验证码为6位")]
        [Display(Name = "手机验证码")]
        public string CheckCode { get; set; }
        
        [Display(AutoGenerateField = false)]
        public string Open_Id { get; set; }

        [Display(AutoGenerateField = false)]
        public string AccessToken { get; set; }
        

    }

    public class Wx_OffRegisterViewModel
    {
        [Required]
        [StringLength(11)]
        [RegularExpression("1[3|5|7|8|][0-9]{9}", ErrorMessage = "手机号码格式错误")]
        [Display(Name = "手机号码")]
        public string Mobile { get; set; }

        [Required]
        [StringLength(6, ErrorMessage = "手机验证码为6位")]
        [Display(Name = "手机验证码")]
        public string CheckCode { get; set; }

        [Required(ErrorMessage ="姓名不能为空")]
        [StringLength(6, ErrorMessage =("姓名长度不得超过6个字符"))]
        [Display(Name = "姓名")]
        public string NickName { get; set; }

        [Display(AutoGenerateField = false)]
        public string Open_Id { get; set; }

        [Display(AutoGenerateField = false)]
        public string AccessToken { get; set; }

        public int SystemId { get; set; }
    }

    public class Wx_SellerRegisterViewModel
    {
        [Required(ErrorMessage = "姓名不能为空")]
        [StringLength(6, ErrorMessage = ("姓名长度不得超过6个字符"))]
        [Display(Name = "姓名")]
        public string NickName { get; set; }

        public int Systemid { get; set; }
    }

    public class Wx_PressConferenceRegisterViewModel
    {
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

    public class Wx_PressConferenceOrderViewModel
    {
        public string Open_Id { get; set; }

        public string Name { get; set; }

        [Required(ErrorMessage ="订单金额格式错误")]
        [Range(1, 1000, ErrorMessage ="订单金额应介于1-1000万元")]
        [RegularExpression(@"^\d+$", ErrorMessage ="数字错误")]
        public decimal? Amount { get; set; }

        public string ImgUrl { get; set; }
    }

    public class Wx_PressConferenceOrderDetails
    {
        public string Name { get; set; }

        public string CompanyName { get; set; }

        public decimal Amount { get; set; }

        public int OrderCount { get; set; }

        public string HeadImgUrl { get; set; }
    }

    
}