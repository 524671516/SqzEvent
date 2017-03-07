using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SqzEvent.Models
{
    public class PromotionModels : DbContext
    {

        public virtual DbSet<Question> Question { get; set; }
        public virtual DbSet<QuestionLib> QuestionLib { get; set; }
        public virtual DbSet<Result> Result { get; set; }
        public virtual DbSet<SurveyedUser> SurveyedUser { get; set; }
        public virtual DbSet<UserResult> UserResult { get; set; }
        public virtual DbSet<Tjh_UserAttendance> Tjh_UserAttendance { get; set; }
        public PromotionModels() : base("PromotionConnection")
        {

        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Question>()
                .HasMany(e => e.Result)
                .WithRequired(e => e.Question)
                .HasForeignKey(e => e.QuestionId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<QuestionLib>()
                .HasMany(e => e.Question)
                .WithRequired(e => e.QuestionLib)
                .HasForeignKey(e => e.QuestionLibId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<SurveyedUser>()
               .HasMany(e => e.UserResult)
               .WithRequired(e => e.SurveyedUser)
               .HasForeignKey(e => e.SurveyedUserId)
               .WillCascadeOnDelete(true);
        }
    }
    /// <summary>
    /// 被调查用户
    /// </summary>
    [Table("SurveyedUser")]
    public partial class SurveyedUser
    {
        public int Id { get; set; }
        public int QuestionLibId { get; set; }
        public int SurveyedUserStatus { get; set; }
        public DateTime StartTime { get; set; }
        public int LastQuestion { get; set; }
        [StringLength(256)]
        public string ImgUrl { get; set; }

        [StringLength(64)]
        public string City { get; set; }

        [StringLength(64)]
        public string Province { get; set; }

        public bool Sex { get; set; }

        [StringLength(64)]
        public string OpenId { get; set; }

        [StringLength(32)]
        public string NickName { get; set; }
        public virtual QuestionLib QuestionLib { get; set; }
        // 对应关系
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserResult> UserResult { get; set; }
    }
    /// <summary>
    /// 题库
    /// </summary>
    [Table("QuestionLib")]
    public partial class QuestionLib
    {
        public int Id { get; set; }
        public string LibName { get; set; }
        public int EndType { get; set; }
        public int StartQuestionId { get; set; }
        public string RouterUrl { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Question> Question { get; set; }

    }
    /// <summary>
    /// 题目
    /// </summary>
    [Table("Question")]
    public partial class Question
    {
        public int Id { get; set; }
        public int QuestionLibId { get; set; }
        [StringLength(128)]
        public string QuestionTitle { get; set; }
        public int QuestionType { get; set; }
        public int DefaultRouter { get; set; }
        public virtual QuestionLib QuestionLib { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Result> Result { get; set; }
    }
    /// <summary>
    /// 题目结果
    /// </summary>
    [Table("Result")]
    public partial class Result
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string AnswerContent { get; set; }
        public int DefaultRouter { get; set; }
        public virtual Question Question { get; set; }
    }
    /// <summary>
    /// 用户结果
    /// </summary>
    [Table("UserResult")]
    public partial class UserResult
    {
        public int Id { get; set; }
        public int SurveyedUserId { get; set; }
        public int QuestionLibId { get; set; }
        public int QuestionId { get; set; }
        [StringLength(128)]
        public string QuestionTitle { get; set; }
        public string AnswerContent { get; set; }
        public virtual SurveyedUser SurveyedUser { get; set; }
    }

    // 2017糖酒会活动
    [Table("Tjh_UserAttendance")]
    public partial class Tjh_UserAttendance
    {
        public int Id { get; set; }

        [Required, StringLength(32)]
        public string openid { get; set; }

        public int Status { get; set; }

        [Required, StringLength(8)]
        public string Name { get; set; }

        [Required, StringLength(16)]
        [RegularExpression("1[3|4|5|7|8|][0-9]{9}", ErrorMessage = "手机号码格式错误")]
        public string Mobile { get; set; }

        [Required, StringLength(64)]
        public string Area { get; set; }

        [StringLength(16)]
        public string SalesChannel { get; set; }

        [StringLength(16)]
        public string RecommandUser { get; set; }

        public DateTime SignupDatetime { get; set; }

        public bool Confirmed { get; set; }

        public DateTime? ConfirmedDatetime { get; set; }
    }
}