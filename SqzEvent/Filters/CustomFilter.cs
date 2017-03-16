using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using SqzEvent.Models;
using SqzEvent.Controllers;

namespace SqzEvent.Filters
{
    public class SettingFilter : AuthorizeAttribute
    {
        public string SettingName;
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            var UserManager = filterContext.HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var user = UserManager.FindById(filterContext.HttpContext.User.Identity.GetUserId());
            OfflineSales offlineDb = new OfflineSales();
            var item = offlineDb.Off_System_Setting.SingleOrDefault(m => m.Off_System_Id == user.DefaultSystemId && m.SettingName == SettingName);
            if (item != null)
            {
                if (item.SettingResult == false)
                {
                    filterContext.Result = new RedirectResult("/OffCommon/AuthorizeError");
                }
            }
            else
            {
                filterContext.Result = new RedirectResult("/OffCommon/AuthorizeError");
            }
        }
    }

    public class StatisticsFilter : ActionFilterAttribute
    {
        public string PageName;
        public string PageURL;
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (PageName !=null && PageURL != null)
            {
                Configuration db = new Configuration();
                WeChatStatistic item = new WeChatStatistic()
                {
                    AccessDatetime = DateTime.Now,
                    HostAddress = filterContext.HttpContext.Request.UserHostAddress,
                    PageName = PageName,
                    PageURL = PageURL
                };
                db.WeChatStatistic.Add(item);
                db.SaveChanges();
            }
            //filterContext.HttpContext.Response.Write("Action执行之前" + statistics_Id + "<br />");
        }
    }
}