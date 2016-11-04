using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SqzEvent.Models
{
    public class QC_RegisterViewModel
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
}