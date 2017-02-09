﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SqzEvent.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using SqzEvent.DAL;
using System.IO;
using CsvHelper;
using System.Runtime.Serialization.Json;
using System.Text;
using Newtonsoft.Json;

namespace SqzEvent.Controllers
{
    [Authorize(Roles = "Manager")]
    public class WeekendBreakController : Controller
    {
        OfflineSales offlineDB = new OfflineSales();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public WeekendBreakController()
        {

        }

        public WeekendBreakController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // 质检员入口
        [AllowAnonymous]
        public ActionResult LoginManager()
        {
            string user_Agent = HttpContext.Request.UserAgent;
            if (user_Agent.Contains("MicroMessenger"))
            {
                //return Content("微信");
                string redirectUri = Url.Encode("https://event.shouquanzhai.cn/WeekendBreak/Authorization");
                string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
                string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_base&state=0#wechat_redirect";
                return Redirect(url);
            }
            else
            {
                return Content("其他");
            }
        }
        [AllowAnonymous]
        public async Task<ActionResult> Authorization(string code, string state)
        {
            try
            {
                WeChatUtilities wechat = new WeChatUtilities();
                var jat = wechat.getWebOauthAccessToken(code);
                var user = UserManager.FindByEmail(jat.openid);
                if (user != null)
                {
                    if (UserManager.IsInRole(user.Id, "Manager"))
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToAction("Weekend_Redirect");
                    }
                }
                return View("NotAuthorized");
            }
            catch (Exception ex)
            {

                CommonUtilities.writeLog(ex.Message);
                return View("Error");
            }
        }

        // GET: WeekendBreak
        public ActionResult Weekend_Redirect()
        {
            var manager = getOff_StoreManager(User.Identity.Name);
            var today = DateTime.Now.Date;
            var breakitem = offlineDB.Off_WeekendBreak.SingleOrDefault(m => m.StoreManagerId == manager.Id && m.Subscribe == today);
            if (breakitem != null)
            {
                return RedirectToAction("WeekendBreak_Home");
            }
            return RedirectToAction("WeekendBreak_Start");
        }
        public ActionResult WeekendBreak_Start()
        {
            var manager = getOff_StoreManager(User.Identity.Name);
            var today = DateTime.Now.Date;
            var storelist = from m in manager.Off_Store
                            select m.Id;
            var scheduleList = from m in offlineDB.Off_Checkin_Schedule
                               where m.Subscribe == today &&
                               storelist.Contains(m.Off_Store_Id)
                               select new { Key = m.Id, Value = m.Off_Store.StoreName };
            ViewBag.StoreListSelectList = new SelectList(scheduleList, "Key", "Value");
            return View(manager);
        }
        [HttpPost,ValidateAntiForgeryToken]
        public async Task<ActionResult> WeekendBreak_Start(FormCollection form)
        {
            var manager = getOff_StoreManager(User.Identity.Name);
            Off_WeekendBreak model = new Off_WeekendBreak()
            {
                TrailDefault = 300,
                SignInTime = DateTime.Now,
                StoreManagerId = manager.Id,
                Subscribe = DateTime.Now.Date,
                UserName = manager.UserName,
                ScheduleId = Convert.ToInt32(form["ScheduleId"].ToString())
            };
            offlineDB.Off_WeekendBreak.Add(model);
            await offlineDB.SaveChangesAsync();
            return Content("SUCCESS");
        }

        // 首页
        public ActionResult WeekendBreak_Home()
        {
            // 获取当天的签到
            var today = DateTime.Now.Date;
            var weekendbreak = offlineDB.Off_WeekendBreak.SingleOrDefault(m => m.UserName == User.Identity.Name && m.Subscribe == today);
            return View(weekendbreak);
        }

        // 新增数据
        public ActionResult WeekendBreak_AddRecord(int breakId)
        {
            var weekendbreak = offlineDB.Off_WeekendBreak.SingleOrDefault(m => m.Id == breakId);
            Off_WeekendBreakRecord record = new Off_WeekendBreakRecord();
            record.WeekendBreakId = breakId;
            ViewBag.ScheduleId = weekendbreak.ScheduleId;
            ViewBag.OldRecord = weekendbreak.Off_WeekendBreakRecord;
            return View(record);
        }
        public ActionResult WeekendBreak_AddRecordPartial(int scheduleId)
        {
            List<int> plist = new List<int>();
            var productlist = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m=>m.Id==scheduleId).Off_Store.Off_StoreSystem.ProductList;
            foreach (var i in productlist.Split(','))
            {
                plist.Add(Convert.ToInt32(i));
            }
            var records = offlineDB.Off_WeekendBreak.SingleOrDefault(m => m.ScheduleId == scheduleId).Off_WeekendBreakRecord;
            List<Wx_WeekendBreakItem> itemlist = new List<Wx_WeekendBreakItem>();
            foreach(var record in records)
            {
                List<Wx_WeekendBreakItem> itemlist_partial = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Wx_WeekendBreakItem>>(record.SalesDetails);
                itemlist.AddRange(itemlist_partial);
            }
            ViewBag.RecordList = itemlist;
            var model = from m in offlineDB.Off_Product
                        where plist.Contains(m.Id)
                        select m;
            return PartialView(model);
        }
        [HttpPost,ValidateAntiForgeryToken]
        public async Task<ActionResult> WeekendBreak_AddRecord(Off_WeekendBreakRecord model, FormCollection form)
        {
            var _t = form;
            if (ModelState.IsValid)
            {
                Off_WeekendBreakRecord record = new Off_WeekendBreakRecord();
                if (TryUpdateModel(record))
                {
                    List<int> plist = new List<int>();
                    int scheduleId = Convert.ToInt32(form["ScheduleId"].ToString());
                    var productIdlist = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == scheduleId).Off_Store.Off_StoreSystem.ProductList;
                    foreach (var i in productIdlist.Split(','))
                    {
                        plist.Add(Convert.ToInt32(i));
                    }
                    var productlist = from m in offlineDB.Off_Product
                                      where plist.Contains(m.Id)
                                      select m;
                    List<Wx_WeekendBreakItem> itemlist = new List<Wx_WeekendBreakItem>();
                    foreach (var item in productlist)
                    {
                        // 获取单品数据
                        int? sales = null;
                        if (form["sales_" + item.Id] != "")
                            sales = Convert.ToInt32(form["sales_" + item.Id]);
                        if (sales != null && sales!=0)
                        {
                            Wx_WeekendBreakItem b_item = new Wx_WeekendBreakItem()
                            {
                                ProductId = item.Id,
                                ProductName = item.SimpleName,
                                SalesCount = (int)sales
                            };
                            itemlist.Add(b_item);
                        }
                    }
                    DateTime lasttime = DateTime.Now;
                    record.SalesDetails = Newtonsoft.Json.JsonConvert.SerializeObject(itemlist);
                    record.SalesCount = itemlist.Sum(m => m.SalesCount);
                    record.UploadTime = lasttime;
                    offlineDB.Off_WeekendBreakRecord.Add(record);
                    var weekendbreak = offlineDB.Off_WeekendBreak.SingleOrDefault(m => m.Id == record.WeekendBreakId);
                    weekendbreak.LastUploadTime = lasttime;
                    offlineDB.Entry(weekendbreak).State = System.Data.Entity.EntityState.Modified;
                    await offlineDB.SaveChangesAsync();
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            else
            {
                //ViewBag.ScheduleId = Convert.ToInt32(form["ScheduleId"].ToString());
                return Content("FAIL");
            }
        }

        // 修改数据
        public ActionResult WeekendBreak_EditRecord(int recordId)
        {
            var record = offlineDB.Off_WeekendBreakRecord.SingleOrDefault(m => m.Id == recordId);
            ViewBag.ScheduleId = record.Off_WeekendBreak.ScheduleId;
            ViewBag.OldRecord = record.Off_WeekendBreak.Off_WeekendBreakRecord;
            return View(record);
        }
        public ActionResult WeekendBreak_EditRecordPartial(int recordId)
        {
            List<int> plist = new List<int>();
            var current_record = offlineDB.Off_WeekendBreakRecord.SingleOrDefault(m => m.Id == recordId);
            var productlist = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == current_record.Off_WeekendBreak.ScheduleId).Off_Store.Off_StoreSystem.ProductList;
            foreach (var i in productlist.Split(','))
            {
                plist.Add(Convert.ToInt32(i));
            }
            var records = current_record.Off_WeekendBreak.Off_WeekendBreakRecord;
            List<Wx_WeekendBreakItem> itemlist = new List<Wx_WeekendBreakItem>();
            foreach (var record in records)
            {
                List<Wx_WeekendBreakItem> itemlist_partial = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Wx_WeekendBreakItem>>(record.SalesDetails);
                itemlist.AddRange(itemlist_partial);
            }
            ViewBag.RecordList = itemlist;
            List<Wx_WeekendBreakItem> model = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Wx_WeekendBreakItem>>(current_record.SalesDetails);
            ViewBag.ProductList = from m in offlineDB.Off_Product
                        where plist.Contains(m.Id)
                        select m;
            return PartialView(model);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> WeekendBreak_EditRecord(Off_WeekendBreakRecord model, FormCollection form)
        {
            var _t = form;
            if (ModelState.IsValid)
            {
                Off_WeekendBreakRecord record = new Off_WeekendBreakRecord();
                if (TryUpdateModel(record))
                {
                    List<int> plist = new List<int>();
                    int scheduleId = Convert.ToInt32(form["ScheduleId"].ToString());
                    var productIdlist = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == scheduleId).Off_Store.Off_StoreSystem.ProductList;
                    foreach (var i in productIdlist.Split(','))
                    {
                        plist.Add(Convert.ToInt32(i));
                    }
                    var productlist = from m in offlineDB.Off_Product
                                      where plist.Contains(m.Id)
                                      select m;
                    List<Wx_WeekendBreakItem> itemlist = new List<Wx_WeekendBreakItem>();
                    foreach (var item in productlist)
                    {
                        // 获取单品数据
                        int? sales = null;
                        if (form["sales_" + item.Id] != "")
                            sales = Convert.ToInt32(form["sales_" + item.Id]);
                        if (sales != null && sales != 0)
                        {
                            Wx_WeekendBreakItem b_item = new Wx_WeekendBreakItem()
                            {
                                ProductId = item.Id,
                                ProductName = item.SimpleName,
                                SalesCount = (int)sales
                            };
                            itemlist.Add(b_item);
                        }
                    }
                    DateTime lasttime = DateTime.Now;
                    record.SalesDetails = Newtonsoft.Json.JsonConvert.SerializeObject(itemlist);
                    record.SalesCount = itemlist.Sum(m => m.SalesCount);
                    offlineDB.Entry(record).State = System.Data.Entity.EntityState.Modified;
                    await offlineDB.SaveChangesAsync();
                    return Content("SUCCESS");
                }
                   return Content("FAIL");
            }
            else
            {
                //ViewBag.ScheduleId = Convert.ToInt32(form["ScheduleId"].ToString());
                return Content("FAIL");
            }
        }

        // 查看数据
        public ActionResult WeekendBreak_ViewRecord(int breakId)
        {
            var item = offlineDB.Off_WeekendBreak.SingleOrDefault(m => m.Id == breakId);
            return View(item);
        }
        public ActionResult WeekendBreak_ViewRecordPartial(int recordId)
        {
            var record = offlineDB.Off_WeekendBreakRecord.SingleOrDefault(m => m.Id == recordId);
            List<Wx_WeekendBreakItem> itemlist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Wx_WeekendBreakItem>>(record.SalesDetails);
            ViewBag.ItemList = itemlist;
            return PartialView(record);
        }
        // 删除数据
        [HttpPost]
        public async Task<ActionResult> WeekendBreak_DeleteRecord(int recordId)
        {
            var item = offlineDB.Off_WeekendBreakRecord.SingleOrDefault(m => m.Id == recordId);
            offlineDB.Off_WeekendBreakRecord.Remove(item);
            await offlineDB.SaveChangesAsync();
            return Content("SUCCESS");
        }
        public Off_StoreManager getOff_StoreManager(string username)
        {
            return offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == username && m.Off_System_Id==1);
        }
        [AllowAnonymous]
        public ActionResult SeniorLoginManager()
        {
            string user_Agent = HttpContext.Request.UserAgent;
            if (user_Agent.Contains("MicroMessenger"))
            {
                //return Content("微信");
                string redirectUri = Url.Encode("https://event.shouquanzhai.cn/WeekendBreak/SeniorAuthorization");
                string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
                string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_base&state=0#wechat_redirect";
                return Redirect(url);
            }
            else
            {
                return Content("其他");
            }
        }
        [AllowAnonymous]
        public async Task<ActionResult> SeniorAuthorization(string code, string state)
        {
            try
            {
                WeChatUtilities wechat = new WeChatUtilities();
                var jat = wechat.getWebOauthAccessToken(code);
                var user = UserManager.FindByEmail(jat.openid);
                if (user != null)
                {
                    if (UserManager.IsInRole(user.Id, "Senior"))
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToAction("WeekendBreak_OverView");
                    }
                }
                return View("NotAuthorized");
            }
            catch (Exception ex)
            {

                CommonUtilities.writeLog(ex.Message);
                return View("Error");
            }
        }
        [Authorize(Roles ="Senior")]
        public ActionResult WeekendBreak_OverView()
        {
            return View();
        }
        [Authorize(Roles = "Senior")]
        [HttpPost]
        public async Task<ActionResult> WeekendBreak_DeleteStore(int breakId)
        {
            var item = offlineDB.Off_WeekendBreak.SingleOrDefault(m => m.Id == breakId);
            offlineDB.Off_WeekendBreak.Remove(item);
            await offlineDB.SaveChangesAsync();
            return Content("SUCCESS");
        }
        [Authorize(Roles = "Senior")]
        public ActionResult WeekendBreak_StoreViewPartial(string date)
        {
            var today = Convert.ToDateTime(date);

            var list = from m in offlineDB.Off_WeekendBreak
                       where m.Subscribe == today
                       orderby m.Off_WeekendBreakRecord.Sum(g=>g.SalesCount) descending
                       select m;
            
            return PartialView(list);
        }
        [Authorize(Roles = "Senior")]
        public ActionResult WeekendBreak_StoreViewDetails(int breakId)
        {
            var item = offlineDB.Off_WeekendBreak.SingleOrDefault(m => m.Id == breakId);
            return View(item);
        }
        [Authorize(Roles = "Senior")]
        public FileResult WeekendBreak_ReportStatistic(string date)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            CsvWriter csv = new CsvWriter(writer);
            var user = UserManager.FindById(User.Identity.GetUserId());
            DateTime today = Convert.ToDateTime(date);
            // 填充表头
            csv.WriteField("店名");
            csv.WriteField("督导姓名");
            // 填充产品表头
            var productlist = from m in offlineDB.Off_Product
                              where m.Off_System_Id == user.DefaultSystemId
                              orderby m.Id
                              select m;
            List<int> productIds = new List<int>();
            foreach (var product in productlist)
            {
                csv.WriteField(product.SimpleName);
                productIds.Add(product.Id);
            }
            csv.WriteField("销量合计");
            csv.WriteField("试饮合计");
            csv.NextRecord();
            // 填充产品内容         
            var weekendbreaklist = from m in offlineDB.Off_WeekendBreak
                              where m.Subscribe == today
                              orderby m.Subscribe
                              select m;
            foreach (var wb_item in weekendbreaklist)
            {
                //csv.WriteField(sequence);
                csv.WriteField(wb_item.Off_Checkin_Schedule.Off_Store.StoreName);
                csv.WriteField(wb_item.Off_StoreManager.NickName);
                /*List<Wx_WeekendBreakItem> itemlist = new List<Wx_WeekendBreakItem>();
                foreach(var details in wb_item.Off_WeekendBreakRecord)
                {
                    itemlist.AddRange(Newtonsoft.Json.JsonConvert.DeserializeObject<List<Wx_WeekendBreakItem>>(details.SalesDetails));
                }
                var ProductSum = from m in itemlist
                                 group m by m.ProductId into g
                                 select new { PId = g.Key, SalesCount = g.Sum(m => m.SalesCount) };*/
                var checkin = wb_item.Off_Checkin_Schedule.Off_Checkin.FirstOrDefault();
                int product_count = 0;
                if (checkin != null)
                {
                    foreach (var pid in productIds)
                    {
                        var _sum = checkin.Off_Checkin_Product.SingleOrDefault(m => m.ProductId == pid);
                        if (_sum != null)
                        {
                            product_count += _sum.SalesCount ?? 0;
                            csv.WriteField(_sum.SalesCount);
                        }
                        else
                        {
                            csv.WriteField("-");
                        }
                    }
                }
                else
                {
                    foreach(var pid in productIds)
                    {
                        csv.WriteField("-");
                    }
                }
                csv.WriteField(product_count);
                csv.WriteField(wb_item.Off_WeekendBreakRecord.Sum(m => m.TrailCount));
                csv.NextRecord();
            }
            writer.Flush();
            writer.Close();
            return File(convertCSV(stream.ToArray()), "text/csv", "周末突破_" + today.ToShortDateString() + ".csv");
        }
        private byte[] convertCSV(byte[] array)
        {
            byte[] outBuffer = new byte[array.Length + 3];
            outBuffer[0] = (byte)0xEF;//有BOM,解决乱码
            outBuffer[1] = (byte)0xBB;
            outBuffer[2] = (byte)0xBF;
            Array.Copy(array, 0, outBuffer, 3, array.Length);
            return outBuffer;
        }
    }
    
}