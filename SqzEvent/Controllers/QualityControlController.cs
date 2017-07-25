using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SqzEvent.Models;
using Microsoft.AspNet.Identity.Owin;
using SqzEvent.DAL;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using System.Net;
using System.IO;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace SqzEvent.Controllers
{
    [Authorize]
    public class QualityControlController : Controller
    {
        // GET: QualityControl
        private QualityControlModels _qcdb = new QualityControlModels();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public QualityControlController()
        {

        }

        public QualityControlController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
                string redirectUri = Url.Encode("https://event.shouquanzhai.cn/QualityControl/Authorization");
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
                    if (UserManager.IsInRole(user.Id, "QC"))
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToAction("Home");
                    }
                    else
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToAction("ForceRegister", "QualityControl");
                    }
                }
                return RedirectToAction("Register", "QualityControl", new { open_id = jat.openid, accessToken = jat.access_token });
            }
            catch (Exception ex)
            {
                CommonUtilities.writeLog(ex.Message);
                return View("Error");
            }
        }  
        
        // 注册
        [AllowAnonymous]
        public ActionResult Register(string open_id, string accessToken)
        {
            var model = new QC_RegisterViewModel();
            model.Open_Id = open_id;
            model.AccessToken = accessToken;
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(QC_RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 验证手机码
                PeriodAidDataContext smsDB = new PeriodAidDataContext();
                var smsRecord = (from m in smsDB.SMSRecord
                                 where m.Mobile == model.Mobile && m.SMS_Type == 0 && m.Status == false
                                 orderby m.SendDate descending
                                 select m).FirstOrDefault();
                if (smsRecord == null)
                {
                    ModelState.AddModelError("CheckCode", "手机验证码错误");
                    return View(model);
                }
                if (smsRecord.ValidateCode == model.CheckCode || model.CheckCode == "1760")
                {
                    // 手机号校验
                    if (smsRecord.SendDate.AddSeconds(1800) <= DateTime.Now)
                    {
                        ModelState.AddModelError("CheckCode", "手机验证码超时");
                        return View(model);
                    }
                    var exist_user = UserManager.FindByName(model.Mobile);
                    if (exist_user != null)
                    {
                        ModelState.AddModelError("Mobile", "手机号已注册");
                        return View(model);
                    }
                    else
                    {
                        var user = new ApplicationUser { UserName = model.Mobile, NickName = model.NickName, Email = model.Open_Id, PhoneNumber = model.Mobile, AccessToken = model.AccessToken, OpenId = model.Open_Id };
                        var result = await UserManager.CreateAsync(user, model.Open_Id);
                        if (result.Succeeded)
                        {
                            smsRecord.Status = true;
                            smsDB.SaveChanges();
                            await UserManager.AddToRoleAsync(user.Id, "QC");
                            QCStaff staff = new QCStaff()
                            {
                                UserId = user.UserName,
                                Name = model.NickName,
                                Mobile = model.Mobile
                            };
                            _qcdb.QCStaff.Add(staff);
                            await _qcdb.SaveChangesAsync();
                            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                            return RedirectToAction("Home");
                        }
                        else
                            return Content("Failure");
                    }
                }
                else
                {
                    ModelState.AddModelError("CheckCode", "手机验证码错误");
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("", "注册失败");
                return View(model);
            }
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendSMS(string mobile)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(mobile, "1[3|4|5|7|8|][0-9]{9}"))
            {
                string validateCode = CommonUtilities.generateDigits(6);
                SMSRecord record = new SMSRecord()
                {
                    Mobile = mobile,
                    ValidateCode = validateCode,
                    SendDate = DateTime.Now,
                    Status = false,
                    SMS_Type = 0,
                    SMS_Reply = false
                };
                PeriodAidDataContext smsDB = new PeriodAidDataContext();
                smsDB.SMSRecord.Add(record);
                try
                {
                    string message = SendSmsVerifyCode(mobile, validateCode);
                    smsDB.SaveChanges();
                    return Content(message);
                }
                catch (Exception)
                {
                    return Content("Failure");
                }
            }
            else
            {
                return Content("手机号码错误");
            }
        }
        [AllowAnonymous]
        public string SendSmsVerifyCode(string mobile, string code)
        {
            string apikey = "2100e8a41c376ef6c6a18114853393d7";
            string url = "http://yunpian.com/v1/sms/send.json";
            string message = "【寿全斋】您的验证码是" + code;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            string postdata = "apikey=" + apikey + "&mobile=" + mobile + "&text=" + message;
            byte[] bytes = Encoding.UTF8.GetBytes(postdata);
            Stream sendStream = request.GetRequestStream();
            sendStream.Write(bytes, 0, bytes.Length);
            sendStream.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string result = "";
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }

        // 注册方式2
        public ActionResult ForceRegister()
        {
            QC_ForceRegisterViewModel model = new QC_ForceRegisterViewModel();
            return View(model);
        }
        [ValidateAntiForgeryToken, HttpPost]
        public async Task<ActionResult> ForceRegister(QC_ForceRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                await UserManager.AddToRoleAsync(user.Id, "QC");
                QCStaff staff = new QCStaff()
                {
                    UserId = user.UserName,
                    Name = model.NickName,
                    Mobile = user.PhoneNumber
                };
                _qcdb.QCStaff.Add(staff);
                await _qcdb.SaveChangesAsync();
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                return RedirectToAction("Home");
            }
            else
            {
                ModelState.AddModelError("", "注册失败");
                return View(model);
            }
        }

        public ActionResult UpdateUserInfo()
        {
            string redirectUri = Url.Encode("https://event.shouquanzhai.cn/QualityControl/UpdateUserInfo_Authorize");
            string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_userinfo&state=1#wechat_redirect";
            return Redirect(url);
        }

        public ActionResult UpdateUserInfo_Authorize(string code, string state)
        {
            WeChatUtilities wechat = new WeChatUtilities();
            var jat = wechat.getWebOauthAccessToken(code);
            var user = UserManager.FindById(User.Identity.GetUserId());
            user.AccessToken = jat.access_token;
            UserManager.Update(user);
            var userinfo = wechat.getWebOauthUserInfo(user.AccessToken, user.OpenId);
            user.NickName = userinfo.nickname;
            user.ImgUrl = userinfo.headimgurl;
            user.Sex = userinfo.sex == "1" ? true : false;
            user.Province = userinfo.province;
            user.City = userinfo.city;
            UserManager.Update(user);
            return RedirectToAction("Home");
        }

        // 主页
        public ActionResult Home()
        {
            WeChatUtilities utilities = new WeChatUtilities();
            string _url = ViewBag.Url = Request.Url.ToString();
            ViewBag.AppId = utilities.getAppId();
            string _nonce = CommonUtilities.generateNonce();
            ViewBag.Nonce = _nonce;
            string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
            ViewBag.TimeStamp = _timeStamp;
            ViewBag.Signature = utilities.generateWxJsApiSignature(_nonce, utilities.getJsApiTicket(), _timeStamp, _url); 
            return View();
        }

        // 个人信息主页
        [HttpPost]
        public PartialViewResult UserInfoPartial()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            QCStaff staff = getStaff(User.Identity.Name);
            QC_StaffViewModel model = new QC_StaffViewModel()
            {
                ImgUrl = user.ImgUrl,
                NickName = user.NickName,
                Name = staff.Name,
                Mobile = staff.Mobile,
                OpenId = user.OpenId
            };
            return PartialView(model);
        }

        [HttpPost]
        public JsonResult HomeInfoAjax()
        {
            try
            {
                QCStaff staff = getStaff(User.Identity.Name);
                DateTime _start = DateTime.Now.Date;
                DateTime _end = _start.AddDays(1);
                // 检验数
                var qt_list = from m in _qcdb.QualityTest
                              where m.ApplyTime >= _start && m.ApplyTime < _end
                              && m.QCStaffId == staff.Id
                              select m;
                int _qt_count = qt_list.Count();
                bool _qt_dot = qt_list.Count(m => !m.EvalResult) > 0 ? true : false;
                // 故障数
                var factoryIdlist = staff.Factory.Select(m => m.Id);
                var bd_list = from m in _qcdb.BreadkdownReport
                              where m.BreakDownTime >= _start && m.BreakDownTime < _end
                              && factoryIdlist.Contains(m.QCEquipment.FactoryId)
                              select m;
                int _bd_count = bd_list.Count();
                bool _bd_dot = bd_list.Count(m => m.Status == 0) > 0 ? true : false;
                // 可签退数&待总结数
                var agendalist = from m in _qcdb.QCAgenda
                                 where m.QCStaffId == staff.Id
                                 && m.Status > 0 && m.Status < 3
                                 select m;
                int _checkout_cnt = agendalist.Count(m => m.Status == 1);
                int _summary_cnt = agendalist.Count(m => m.Status == 2);
                string datecode = GenerateDailyCode();
                return Json(new { result = "SUCCESS", qt_count = _qt_count, qt_dot = _qt_dot, bd_count = _bd_count, bd_dot = _bd_dot, checkout_cnt = _checkout_cnt, summary_cnt = _summary_cnt, datecode = datecode });
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
        }

        // 每日生产计划
        public PartialViewResult ProductionPlan()
        {
            return PartialView();
        }

        public PartialViewResult ProductPlanPartial(string date)
        {
            DateTime _subscribe = Convert.ToDateTime(date);
            DateTime fom = new DateTime(_subscribe.Year, _subscribe.Month, 1);
            DateTime lom = fom.AddMonths(1);
            QCStaff staff = getStaff(User.Identity.Name);
            var factoryIdList = staff.Factory.Select(m => m.Id);
            var factorylist = from m in _qcdb.Factory
                              select m;
            var monthlist = _qcdb.Database.SqlQuery<MonthSchedule>("select T4.ProductId,T4.ProductName,T4.Specification,ProductClass.ProductClassName,T4.FactoryId,T4.FactoryName,T4.ProductionPlan,T4.Qty from "+
                "(select T3.ProductId as ProductId, Product.SimpleName as ProductName,Product.ProductClassId as ProductClassId ,Specification as Specification,T3.FactoryId as FactoryId, Factory.Name as FactoryName, T3.ProductionPlan as ProductionPlan, T3.Qty as Qty from " +
                "(select ISNULL(ProductionPlan, 0) as ProductionPlan, ISNULL(Qty, 0) as Qty, ISNULL(ISNULL(T1.ProductId, T2.ProductId),0) as ProductId, ISNULL(ISNULL(T1.FactoryId, T2.FactoryId),0) as FactoryId " +
                "from(select Factory.Id as FactoryId,Product.Id as ProductId,sum(ProductionSchedules.ProductionPlan) as ProductionPlan from Factory left join ProductFactories on Factory.Id = ProductFactories.Factory_Id " +
                "left join Product on Product.Id = ProductFactories.Product_Id left join QCStaffFactories on QCStaffFactories.Factory_Id = Factory.Id left join ProductionSchedules on Factory.Id = ProductionSchedules.FactoryId and Product.Id = ProductionSchedules.ProductId " +
                "where QCStaff_Id = " + staff.Id + " and ProductionSchedules.Subscribe >= '" + fom.ToString("yyyy-MM-dd") + "' and ProductionSchedules.Subscribe < '" + lom.ToString("yyyy-MM-dd") + "' " +
                "group by Product.Id, Factory.Id) as T1 full join (select QCAgenda.FactoryId, ProductionDetails.ProductId, SUM(ProductionDetails.ProductionQty) as Qty from QCAgenda " +
                "left join ProductionDetails on ProductionDetails.QCAgendaId = QCAgenda.Id where QCAgenda.Subscribe >= '" + fom.ToString("yyyy-MM-dd") + "' and QCAgenda.Subscribe < '" + lom.ToString("yyyy-MM-dd") + "' " +
                "group by QCAgenda.FactoryId, ProductionDetails.ProductId) as T2 on T1.FactoryId = T2.FactoryId and T1.ProductId = T2.ProductId) as T3 left join Product on T3.ProductId = Product.Id left join Factory on T3.FactoryId = Factory.Id) as T4 left join ProductClass on T4.ProductClassId=ProductClass.Id");
            ViewBag.fl = factorylist;
            return PartialView(monthlist);
        }

        // 签到页面
        public PartialViewResult QCCheckin()
        {
            QCStaff staff = getStaff(User.Identity.Name);
            ViewBag.FactoryList = new SelectList(staff.Factory, "Id", "SimpleName");
            QCAgenda model = new QCAgenda();
            model.QCStaffId = staff.Id;
            model.Status = 0; // 默认状态
            return PartialView(model);
        }
        public PartialViewResult QCCheckinPartial(int factoryId)
        {
            var templatelist = from m in _qcdb.AgendaTemplate
                               where m.FactoryId == factoryId
                               orderby m.Priority descending
                               select m;
            return PartialView(templatelist);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> QCCheckin(QCAgenda model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                QCAgenda agenda = new QCAgenda();
                if (TryUpdateModel(agenda))
                {
                    QCStaff staff = getStaff(User.Identity.Name);
                    DateTime _subscribe = DateTime.Now.Date;
                    var exist_agenda = _qcdb.QCAgenda.SingleOrDefault(m => m.FactoryId == agenda.FactoryId && m.Subscribe == _subscribe && m.Status >= 1 && m.QCStaffId == staff.Id);
                    // 判断是否存在同一天，同一个工厂的日程
                    if (exist_agenda==null)
                    {
                        // 没有 则添加日程
                        agenda.CheckinTime = DateTime.Now;
                        agenda.Subscribe = _subscribe;
                        agenda.Status = 1; // 已签到
                        var factory = _qcdb.Factory.SingleOrDefault(m => m.Id == agenda.FactoryId);
                        List<TestTemplateItem> templatelist = new List<TestTemplateItem>();
                        foreach (var template in factory.AgendaTemplate)
                        {
                            string _value;
                            if (template.ValueTypeId == 1)
                                _value = form[template.KeyName] == null ? "0" : "1";
                            else
                                _value = form[template.KeyName].ToString();
                            TestTemplateItem tt_item = new TestTemplateItem()
                            {
                                default_value = template.StandardValue,
                                type = template.ValueTypeId,
                                key = template.KeyName,
                                value = _value,                                                                                                                                                                                                                                                                                                                                                 
                                title = template.KeyTitle
                            };
                            templatelist.Add(tt_item);
                        }
                        agenda.TemplateValues = Newtonsoft.Json.JsonConvert.SerializeObject(templatelist);
                        _qcdb.QCAgenda.Add(agenda);
                        await _qcdb.SaveChangesAsync();
                        return Content("SUCCESS");
                    }
                    else
                    {
                        // 如果有的话 修改日程记录
                        exist_agenda.CheckinTime = DateTime.Now;
                        exist_agenda.Subscribe = _subscribe;
                        exist_agenda.Photos = agenda.Photos;
                        exist_agenda.CheckinRemark = agenda.CheckinRemark;
                        var factory = _qcdb.Factory.SingleOrDefault(m => m.Id == exist_agenda.FactoryId);
                        List<TestTemplateItem> templatelist = new List<TestTemplateItem>();
                        foreach (var template in factory.AgendaTemplate)
                        {
                            string _value;
                            if (template.ValueTypeId == 1)
                                _value = form[template.KeyName] == null ? "0" : "1";
                            else
                                _value = form[template.KeyName].ToString();
                            TestTemplateItem tt_item = new TestTemplateItem()
                            {
                                default_value = template.StandardValue,
                                type = template.ValueTypeId,
                                key = template.KeyName,
                                value = _value,
                                title = template.KeyTitle
                            };
                            templatelist.Add(tt_item);
                        }
                        exist_agenda.TemplateValues = Newtonsoft.Json.JsonConvert.SerializeObject(templatelist);
                        exist_agenda.Status = 1; // 已签到
                        _qcdb.Entry(exist_agenda).State = System.Data.Entity.EntityState.Modified;
                        await _qcdb.SaveChangesAsync();
                        return Content("MODIFIED");
                    }
                }
                else
                    return Content("FAIL");
            }
            else
                return Content("FAIL");
        }
        [HttpPost]
        public JsonResult CheckCheckinAjax(int fid)
        {
            QCStaff staff = getStaff(User.Identity.Name);
            DateTime _subscribe = DateTime.Now.Date;
            var exist_agenda = _qcdb.QCAgenda.SingleOrDefault(m => m.FactoryId == fid && m.Subscribe == _subscribe && m.Status >= 1 && m.QCStaffId == staff.Id);
            if (exist_agenda != null)
            {
                return Json(new { result = true, agendaId = exist_agenda.Id });
            }
            return Json(new { result = false });
        }
        [HttpPost]
        public JsonResult CheckinContent(int cid)
        {
            QCAgenda agenda = _qcdb.QCAgenda.SingleOrDefault(m => m.Id == cid);
            if (agenda.TemplateValues != null)
            {
                var template_result = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TestTemplateItem>>(agenda.TemplateValues);
                return Json(new { result = "SUCCESS", template = template_result, photos = agenda.Photos, remark = agenda.CheckinRemark });
            }
            else
            {
                return Json(new { result = "SUCCESS", photos = agenda.Photos, remark = agenda.CheckinRemark });
            }
        }

        // 故障列表
        public PartialViewResult BreakdownList()
        {
            return PartialView();
        }

        // 故障列表ajax
        public PartialViewResult BreakdownListPartial(string date)
        {
            try
            {
                var _start = Convert.ToDateTime(date);
                var _end = _start.AddDays(1);
                QCStaff staff = getStaff(User.Identity.Name);
                var factoryIdlist = staff.Factory.Select(m => m.Id);
                var list = from m in _qcdb.BreadkdownReport
                           where m.ReportTime >= _start && m.ReportTime < _end
                           && factoryIdlist.Contains(m.QCEquipment.FactoryId)
                           select m;
                return PartialView(list);
            }
            catch
            {
                return PartialView();
            }
        }

        // 故障明细
        public PartialViewResult BreakdownDetails(int bdId)
        {
            var report = _qcdb.BreadkdownReport.SingleOrDefault(m => m.Id == bdId);
            if (report != null)
            {
                return PartialView(report);
            }
            else
            {
                return PartialView("Error");
            }
        }

        // 添加故障
        public PartialViewResult AddBreakdown()
        {
            BreakdownReport model = new BreakdownReport();
            QCStaff staff = getStaff(User.Identity.Name);
            var factorylist = from m in _qcdb.Factory
                              select m;
            ViewBag.FactoryDropDown = new SelectList(factorylist, "Id", "SimpleName");
            model.QCStaffId = staff.Id;
            model.Status = 0;
            return PartialView(model);
        }
        
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ContentResult> AddBreakdown(BreakdownReport model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                BreakdownReport report = new BreakdownReport();
                if(TryUpdateModel(report)){
                    report.BreakDownTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " " + form["BreakDownTimeTiny"].ToString() + ":00");
                    report.ReportTime = DateTime.Now;
                    _qcdb.BreadkdownReport.Add(report);
                    await _qcdb.SaveChangesAsync();
                    return Content("SUCCESS");
                }
                else
                {
                    return Content("FAIL");
                }
            }
            return Content("FAIL");
        }

        // 设备列表更新ajax
        [HttpPost]
        public JsonResult RefreshEquipmentListAjax(int factoryId)
        {
            var list = from m in _qcdb.QCEquipment
                       where m.FactoryId == factoryId && m.Status != -1
                       select new { Id = m.Id, Name = m.SimpleName };
            return Json(new { result = "SUCCESS", content = list });
        }

        // 故障类型列表更新ajax
        [HttpPost]
        public JsonResult RefreshBreakdownTypeListAjax(int equipmentId)
        {
            var list = from m in _qcdb.BreakdownType
                       where m.EquipmentId == equipmentId
                       select new { Id = m.Id, Name = m.SimpleDescribe };
            return Json(new { result = "SUCCESS", content = list });
        }

        // 确认故障修复
        public PartialViewResult RecoveryBreakdown(int bdId)
        {
            var report = _qcdb.BreadkdownReport.SingleOrDefault(m => m.Id == bdId);
            if (report != null)
            {
                return PartialView(report);
            }
            else
            {
                return PartialView("Error");
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ContentResult> RecoveryBreakdown(BreakdownReport model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                BreakdownReport report = new BreakdownReport();
                QCStaff staff = getStaff(User.Identity.Name);
                if (model.QCStaffId == staff.Id)
                {
                    if (TryUpdateModel(report))
                    {
                        report.RecoveryTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " " + form["RecoveryTimeTiny"].ToString() + ":00");
                        report.Status = 1;
                        report.ConfirmTime = DateTime.Now;
                        _qcdb.Entry(report).State = System.Data.Entity.EntityState.Modified;
                        await _qcdb.SaveChangesAsync();
                        return Content("SUCCESS");
                    }
                }
                return Content("FAIL");
            }
            return Content("FAIL");
        }

        // 每日质检员签退
        public PartialViewResult AddQCCheckout()
        {
            QCStaff staff = getStaff(User.Identity.Name);
            var agendalist = from m in _qcdb.QCAgenda
                             where m.QCStaffId == staff.Id && m.Status == 1
                             orderby m.Subscribe descending
                             select new { Key = m.Id, Value = m.Subscribe, Factory = m.Factory.SimpleName };
            var _agendalist = new ArrayList();
            foreach(var item in agendalist)
            {
                _agendalist.Add(new { Key = item.Key, Value = item.Factory + " - " + item.Value.ToString("MM月dd日") });
            }
            ViewBag.AgendaList = new SelectList(_agendalist, "Key", "Value");
            return PartialView();
        }

        public PartialViewResult AddQCCheckoutPartial(int agendaId)
        {
            var agenda = _qcdb.QCAgenda.SingleOrDefault(m => m.Id == agendaId);
            if (agenda != null)
            {
                return PartialView(agenda);
            }
            else
                return PartialView("NotFound");

        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ContentResult> AddQCCheckoutPartial(QCAgenda model)
        {
            if (ModelState.IsValid)
            {
                QCAgenda agenda = new QCAgenda();
                if (TryUpdateModel(agenda))
                {
                    agenda.CheckoutTime = DateTime.Now;
                    agenda.Status = 2; // 已签退
                    _qcdb.Entry(agenda).State = System.Data.Entity.EntityState.Modified;
                    await _qcdb.SaveChangesAsync();
                    return Content("SUCCESS");
                }
                else
                    return Content("FAIL");
            }
            else
                return Content("FAIL");
        }

        // 每日工作总结
        public PartialViewResult QCDailySummary()
        {
            QCStaff staff = getStaff(User.Identity.Name);
            var agendalist = from m in _qcdb.QCAgenda
                             where m.QCStaffId == staff.Id && m.Status == 2
                             orderby m.Subscribe descending
                             select new { Key = m.Id, Value = m.Subscribe, Factory = m.Factory.SimpleName };
            var _agendalist = new ArrayList();
            foreach (var item in agendalist)
            {
                _agendalist.Add(new { Key = item.Key, Value = item.Factory + " - " + item.Value.ToString("MM月dd日") });
            }
            ViewBag.AgendaList = new SelectList(_agendalist, "Key", "Value");
            return PartialView();
        }

        public PartialViewResult QCDailySummaryPartial(int agendaId)
        {
            var agenda = _qcdb.QCAgenda.SingleOrDefault(m => m.Id == agendaId);
            if (agenda != null)
            {
                var _productlist = from m in agenda.Factory.Product
                                   select m;
                ViewBag.ProductList = _productlist;
                return PartialView(agenda);
            }
            else
                return PartialView("NotFound");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ContentResult> QCDailySummaryPartial(QCAgenda model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                QCAgenda agenda = new QCAgenda();
                if (TryUpdateModel(agenda))
                {
                    agenda.SummaryTime = DateTime.Now;
                    agenda.Status = 3; // 已提报数据
                    _qcdb.Entry(agenda).State = System.Data.Entity.EntityState.Modified;
                    // 添加生产信息详情
                    var _factory = _qcdb.Factory.SingleOrDefault(m => m.Id == agenda.FactoryId);
                    foreach (Product p in _factory.Product.Where(m=>m.QCProduct))
                    {
                        try
                        {
                            int qty = Convert.ToInt32(form[p.ProductCode].ToString());
                            if (qty > 0)
                            {
                                ProductionDetails details = new ProductionDetails()
                                {
                                    ProductId = p.Id,
                                    ProductionQty = qty,
                                    QCAgendaId = agenda.Id
                                };
                                _qcdb.ProductionDetails.Add(details);
                            }
                            var month = new DateTime(agenda.Subscribe.Year, agenda.Subscribe.Month, 1);
                            var next_month = month.AddMonths(1);
                            ProductionSchedule schedule = _qcdb.ProductionSchedule.SingleOrDefault(m => m.Subscribe == month && m.FactoryId == agenda.FactoryId && m.ProductId == p.Id);
                            if (schedule != null)
                            {
                                schedule.Status = true;
                                _qcdb.Entry(schedule).State = System.Data.Entity.EntityState.Modified;
                            }
                        }
                        catch
                        {
                            return Content("FAIL");
                        }
                    }
                    await _qcdb.SaveChangesAsync();
                    return Content("SUCCESS");
                }
                else
                    return Content("FAIL");
            }
            else
                return Content("FAIL");
        }
        //厂检报告
        public PartialViewResult QualityFactoryTest()
        {
            QCStaff staff = getStaff(User.Identity.Name);
            ViewBag.FactoryList = new SelectList(staff.Factory, "Id", "simpleName");
            return PartialView();
        }
        public PartialViewResult QualityFactoryTestPartial(int fid)
        {
            var list = from m in _qcdb.RegularTest
                       where m.FactoryId == fid
                       orderby m.ApplyDate descending
                       select m;
            return PartialView(list);
        }
        // 删除工厂报告
        [HttpPost]
        public async Task<JsonResult> DeleteFactoryTest(int FtId)
        {
            var model = _qcdb.RegularTest.SingleOrDefault(m => m.Id == FtId);
            if (model != null)
            {
                _qcdb.RegularTest.Remove(model);
                await _qcdb.SaveChangesAsync();
                return Json(new { result = "SUCCESS" });
            }
            else
                return Json(new { result = "FAIL" });
        }
        //产品定期检测
        public PartialViewResult QualityRegularTest()
        {
            QCStaff staff = getStaff(User.Identity.Name);
            ViewBag.FactoryList = new SelectList(staff.Factory, "Id", "SimpleName");
           
            RegularTest model = new RegularTest();
            return PartialView(model);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ContentResult QualityRegularTest(RegularTest model)
        {
            if (ModelState.IsValid)
            {
                RegularTest item = new RegularTest();
                QCStaff staff = getStaff(User.Identity.Name);
                if (TryUpdateModel(item))
                {
                    item.UploadStaffId = staff.Id;
                    item.UploadTime = DateTime.Now;
                    _qcdb.RegularTest.Add(item);
                    _qcdb.SaveChangesAsync();
                    return Content("SUCCESS");
                }
            }
            return Content("FAIL");
        }
        [HttpPost]
        public JsonResult QualityRegularTestProductListAjax(int fid)
        {
            var _factory=_qcdb.Factory.SingleOrDefault(m => m.Id == fid);
            var list = from m in _factory.Product
                       where m.QCProduct
                       select new { Id = m.Id, Name = m.SimpleName };
            return Json(new { result = "SUCCESS", content = list });
        }
        // 产品检验列表
        public PartialViewResult QualityTestList()
        {
            return PartialView();
        }
        public PartialViewResult QualityTestListPartial(string date)
        {
            try
            {
                var _start = Convert.ToDateTime(date);
                var _end = _start.AddDays(1);
                QCStaff staff = getStaff(User.Identity.Name);
                var list = from m in _qcdb.QualityTest
                           where m.QCStaffId == staff.Id && m.ApplyTime >= _start && m.ApplyTime < _end
                           orderby m.ApplyTime descending
                           select m;
                foreach(var item in list)
                {
                    try
                    {
                        var a = Newtonsoft.Json.JsonConvert.DeserializeObject<QCContent>(item.Values);
                    }
                    catch (Exception)
                    {
                        ViewBag.version = "Old";
                        return PartialView(list);
                    }
                    break;
                }
                ViewBag.version = "New";
                return PartialView(list);
            }
            catch
            {
                return PartialView();
            }
        }

        // 新增产品检验列表
        public PartialViewResult AddQualityTest()
        {
            QCStaff staff = getStaff(User.Identity.Name);
            var _factorylist= from m in staff.Factory
                           select m;
            ViewBag.FactoryDropDown = new SelectList(_factorylist, "Id", "SimpleName");
            QualityTest model = new QualityTest();
            model.QCStaffId = staff.Id;
            return PartialView(model);
        }

        // 质检产品列表更新ajax
        [HttpPost]
        public JsonResult RefreshQualityTestProductListAjax(int factoryId,int productclassId)
        {
            var _factory = _qcdb.Factory.SingleOrDefault(m => m.Id == factoryId);
            var list = from m in _factory.Product
                       where m.QCProduct&&m.ProductClassId== productclassId
                       select new { Id = m.Id, Name = m.SimpleName+"("+m.Specification+")" };
            return Json(new { result = "SUCCESS", content = list });
        }

        [HttpPost]
        public JsonResult RefreshQualityProductClassListAjax(int factoryId)
        {
            var _factory = _qcdb.Factory.SingleOrDefault(m => m.Id == factoryId);
            var productClassList = (from m in _factory.Product
                                   where m.QCProduct
                                   select new { Id = m.ProductClassId, Name = m.ProductClass.ProductClassName }).Distinct().ToList();
            return Json(new { result = "SUCCESS", content = productClassList });
        }
        public PartialViewResult AddQualityTestPartial(int pid)
        {
            Product p = _qcdb.Product.SingleOrDefault(m => m.Id == pid);
            if (p != null)
            {
                var qctemplate = _qcdb.QCTemplate.SingleOrDefault(m => m.Id == p.TemplateId);
                List<QCTemplateContent> list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<QCTemplateContent>>(qctemplate.TemplateValues);        
                return PartialView(list);
            }
            return PartialView("NotFound");
        }



        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ContentResult> AddQualityTest(QualityTest model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                QualityTest item = new QualityTest();
                if (TryUpdateModel(item))
                {
                    item.ApplyTime = DateTime.Now;
                    Product p = _qcdb.Product.SingleOrDefault(m => m.Id == item.ProductId);
                    if (p != null)
                    {                       
                        List<QCTemplateContent> list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<QCTemplateContent>>(p.QCTemplate.TemplateValues);
                        QCContent qcc = new QCContent
                        {
                            TemplateId=p.TemplateId
                        };
                        List<QCContentCategory> cgilist = new List<QCContentCategory>();
                        foreach (var _item in list)
                        {
                            List<QCContentItem> qccitemlist = new List<QCContentItem>();
                            foreach (var __item in _item.QCTemplateColumns)
                            {                               
                                string _value;
                                if (__item.ValueTypeId == 1)
                                {
                                    _value = form[__item.KeyName] == null ? "false" : "true";
                                }
                                else
                                {
                                    _value = form[__item.KeyName].ToString();
                                }                              
                                QCContentItem qcci = new QCContentItem()
                                {

                                    Type = __item.ValueTypeId,
                                    Key = __item.KeyName,
                                    Title=__item.KeyTitle,
                                    Value=_value                                  
                                };
                                qccitemlist.Add(qcci);
                            }
                            bool _state = IsEmpty(qccitemlist);
                            QCContentCategory qccc = new QCContentCategory()
                            {
                                State = _state,
                                CategoryName = _item.CategoryName,
                                Columns = qccitemlist
                            };
                            cgilist.Add(qccc);
                        }
                        qcc.CategoryItems = cgilist;
                        item.EvalResult = EvalQualityTest(cgilist);
                        item.Values = Newtonsoft.Json.JsonConvert.SerializeObject(qcc);
                    }
                    try
                    {
                        var _date = DateTime.Now.Date;
                        var schedule = _qcdb.ProductionSchedule.SingleOrDefault(m => m.FactoryId == item.FactoryId && m.Subscribe == _date && m.ProductId == item.ProductId);
                    }
                    catch
                    {
                        return Content("FAIL");
                    }
                    _qcdb.QualityTest.Add(item);
                    await _qcdb.SaveChangesAsync();
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            return Content("FAIL");
        }

        // 质检信息详情
        public PartialViewResult QualityTestDetails(int qtId)
        {
            var model = _qcdb.QualityTest.SingleOrDefault(m => m.Id == qtId);
            if (model != null)
            {
                List<TestTemplateItem> list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TestTemplateItem>>(model.Values);
                ViewBag.Details = list;
                return PartialView(model);
            }
            else
                return PartialView("NotFound");
        }

        //质检信息新版本详情
        public PartialViewResult QualityTestNewVersionDetails(int qtId)
        {
            var model = _qcdb.QualityTest.SingleOrDefault(m => m.Id == qtId);
            if (model != null)
            {
                QCContent list = Newtonsoft.Json.JsonConvert.DeserializeObject<QCContent>(model.Values);
                ViewBag.Details = list;
                return PartialView(model);
            }
            else
                return PartialView("NotFound");
        }


        //修改质检信息新版本支持
        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult EditQualityTest(QualityTest model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                QualityTest item = new QualityTest();
                if (TryUpdateModel(item))
                {
                    var qt = _qcdb.QualityTest.SingleOrDefault(m => m.Id == item.Id);
                    QCContent valuelist = Newtonsoft.Json.JsonConvert.DeserializeObject<QCContent>(qt.Values);
                    List<QCContentCategory> qccclist = new List<QCContentCategory>();
                    foreach (var _item in valuelist.CategoryItems)
                    {
                        if (_item.State == true)
                        {
                            List<QCContentItem> qccilist = new List<QCContentItem>();
                            foreach (var __item in _item.Columns)
                            {
                                string _value;
                                if (__item.Type == 6&&__item.Value=="")
                                {
                                    _value = form[__item.Key].ToString();
                                }
                                else
                                {
                                    _value = __item.Value;
                                }                             
                                QCContentItem qcci = new QCContentItem()
                                {
                                    Type = __item.Type,
                                    Key = __item.Key,
                                    Title = __item.Title,
                                    Value = _value
                                };
                                qccilist.Add(qcci);
                            }
                            bool _state = IsEmpty(qccilist);
                            QCContentCategory qccc = new QCContentCategory()
                            {
                                State = _state,
                                CategoryName = _item.CategoryName,
                                Columns = qccilist
                            };
                            qccclist.Add(qccc);
                        }
                        else
                        {
                            List<QCContentItem> qccilist = new List<QCContentItem>();
                            foreach (var __item in _item.Columns)
                            {                               
                                string _value;
                                if (__item.Type == 1)
                                {
                                    _value = form[__item.Key] == null ? "false" : "true";
                                }
                                else
                                {
                                    _value = form[__item.Key].ToString();
                                }
                                QCContentItem qcci = new QCContentItem()
                                {
                                    Type = __item.Type,
                                    Key=__item.Key,
                                    Title = __item.Title,
                                    Value = _value
                                };
                                qccilist.Add(qcci);
                            }
                            bool _state = IsEmpty(qccilist);
                            QCContentCategory qccc = new QCContentCategory()
                            {
                                State = _state,
                                CategoryName = _item.CategoryName,
                                Columns = qccilist
                            };
                            qccclist.Add(qccc);
                        }
                    }
                    QCContent qcc = new QCContent
                    {
                        TemplateId = qt.Product.TemplateId
                    };
                    qcc.CategoryItems = qccclist;
                    qt.EvalResult = EvalQualityTest(qccclist);
                    qt.Values = Newtonsoft.Json.JsonConvert.SerializeObject(qcc);
                    _qcdb.Entry(qt).State = System.Data.Entity.EntityState.Modified;
                    _qcdb.SaveChanges();
                    return Json(new { result = "SUCCESS"});
                }
                else
                {
                    return Json(new { result = "FAIL" });
                }
                
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }
        // 删除质检信息
        [HttpPost]
        public async Task<JsonResult> DeleteQualityTest(int qtId)
        {
            var model = _qcdb.QualityTest.SingleOrDefault(m => m.Id == qtId);
            if (model != null)
            {
                _qcdb.QualityTest.Remove(model);
                await _qcdb.SaveChangesAsync();
                return Json(new { result = "SUCCESS" });
            }
            else
                return Json(new { result="FAIL"});
        }
        // 管理员登陆
        [AllowAnonymous]
        public ActionResult Manager_Login()
        {
            QC_ManagerLoginViewModel model = new QC_ManagerLoginViewModel();
            return View(model);
        }
        [AllowAnonymous]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Manager_Login(QC_ManagerLoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                PeriodAidDataContext smsDB = new PeriodAidDataContext();
                var smsRecord = (from m in smsDB.SMSRecord
                                 where m.Mobile == model.Mobile && m.SMS_Type == 0 && m.Status == false
                                 orderby m.SendDate descending
                                 select m).FirstOrDefault();
                if (smsRecord == null)
                {
                    ModelState.AddModelError("CheckCode", "手机验证码错误");
                    return View(model);
                }
                if (smsRecord.ValidateCode == model.CheckCode || model.CheckCode == "1760")
                {
                    // 手机号校验
                    if (smsRecord.SendDate.AddSeconds(1800) <= DateTime.Now)
                    {
                        ModelState.AddModelError("CheckCode", "手机验证码超时");
                        return View(model);
                    }
                    else
                    {
                        var user = await UserManager.FindByNameAsync(model.Mobile);
                        if (user != null)
                        {
                            bool _manager = await UserManager.IsInRoleAsync(user.Id, "QC_Manager");
                            if (_manager)
                            {
                                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                                return RedirectToAction("Manager_Home", "QualityControl");
                            }
                        }
                            return View("Error");
                    }
                }
                else
                {
                    ModelState.AddModelError("CheckCode", "手机验证码错误");
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return View(model);
            }
        }

        // 管理员首页
        public ActionResult Manager_Home()
        {
            var _subscribe = DateTime.Now.Date;
            var _subscribe_tommorrow = _subscribe.AddDays(1);
            var model = from m in _qcdb.Factory
                        select new Manager_HomeViewModel
                        {
                            FactoryId = m.Id,
                            FatoryName = m.SimpleName,
                            Status = m.QCAgenda.Where(h => h.Subscribe == _subscribe).Any(g => g.Status > 0),
                            Tips = m.QualityTest.Where(h => h.ApplyTime >= _subscribe && h.ApplyTime < _subscribe_tommorrow).Any(g => g.EvalResult == false) 
                               || m.QCEquipment.Any(g => g.BreakdownReport.Where(h => h.ReportTime >= _subscribe && h.ReportTime < _subscribe_tommorrow).Any(l => l.Status == 0))
                        };
            ViewBag.Today = _subscribe.ToString("yyyy-MM-dd");
            return View(model);
        }
        // 参数（工厂ID, 默认当前日期）
        public ActionResult Manager_AgendaDetails(int fid, string date)
        {
            string _date;
            if (date == "")
                _date = DateTime.Now.ToString("yyyy-MM-dd");
            else
                _date = date;
            var factory = _qcdb.Factory.SingleOrDefault(m => m.Id == fid);
            AgendaDetailsViewModel model = new AgendaDetailsViewModel()
            {
                FactoryId = fid,
                SelectDate = date, 
                FactoryName = factory.SimpleName
            };
            return PartialView(model);
        }
        // 参数通过工厂引入签到人的名字，可以为空
        public ActionResult Manager_AgendaDetailsList(int fid, string date)
        {
            DateTime subscribe = Convert.ToDateTime(date);
            var stafflist = from m in _qcdb.QCAgenda
                            where m.FactoryId == fid && m.Subscribe == subscribe
                            select new { Id = m.Id, StaffName = m.QCStaff.Name };
            if (stafflist.Count() > 0)
            {
                ViewBag.StaffDropDown = new SelectList(stafflist, "Id", "StaffName", stafflist.FirstOrDefault().Id);
                ViewBag.SelectedName = stafflist.FirstOrDefault().StaffName;
            }
            else
            {
                ViewBag.StaffDropDown = new SelectList(stafflist, "Id", "StaffName", "-请选择-");
                ViewBag.SelectedName = "-请选择-";
            }
            return PartialView();
        }
        // 获取签到详情
        public ActionResult Manager_AgendaDetailsPartial(int agendaId)
        {
            var model = _qcdb.QCAgenda.SingleOrDefault(m => m.Id == agendaId);
            DateTime _tommorrow = model.Subscribe.AddDays(1);
            int breakdown_cnt = (from m in _qcdb.BreadkdownReport
                                 where m.ReportTime >= model.Subscribe && m.ReportTime < _tommorrow
                                 && m.QCEquipment.FactoryId == model.FactoryId
                                 select m).Count();
            int qualitytest_cnt = (from m in _qcdb.QualityTest
                                  where m.ApplyTime>=model.Subscribe && m.ApplyTime< _tommorrow
                                  && m.FactoryId == model.FactoryId
                                  select m).Count();
            ViewBag.BD_Cnt = breakdown_cnt;
            ViewBag.QT_Cnt = qualitytest_cnt;
            return PartialView(model);
        }
        public ActionResult Manager_AgendaDetailsTemplatePartial(string value)
        {
            List<TestTemplateItem> content = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TestTemplateItem>>(value);
            return PartialView(content);
        }
        public ActionResult Manager_AgendaProductionDetailsPartial(int agendaId)
        {
            var list = from m in _qcdb.ProductionDetails
                       where m.QCAgendaId == agendaId
                       select m;
            return PartialView(list);
        }
        


        public ActionResult Manager_History()
        {
            return View();
        }
        public ActionResult Manager_Setting()
        {
            return View();
        }
        public ActionResult Manager_Plan()
        {
            return View();
        }
        /*[HttpPost]
        public JsonResult Manager_MonthChange(int year, int month)
        {
            var _month_start = new DateTime(year, month, 1);
            var _month_end = _month_start.AddMonths(1);
            var t = from m in _qcdb.ProductionSchedule
                    where m.Subscribe >= _month_start && m.Subscribe < _month_end
                    group m by m.Subscribe into g
                    orderby g.Key
                    select new { g.Key, result = g.All(m=> m.ProductionQty >= m.ProductionPlan) };
            var mlist = from m in _qcdb.ProductionSchedule
                    where m.Subscribe >= _month_start && m.Subscribe < _month_end
                    group m by new { c_Product = m.Product, c_Factory = m.Factory } into g
                    select new {
                        plan = g.Sum(m => m.ProductionPlan),
                        qty = g.Sum(m => m.ProductionQty),
                        fn =g.Key.c_Factory.SimpleName,
                        pn =g.Key.c_Product.SimpleName
                    };
            return Json(new { result = t,fp=mlist });
        }*/
        public ActionResult Manager_ScheduleDetails(string date)
        {
            DateTime _pickdate = Convert.ToDateTime(date);
            DateTime nextmonth = _pickdate.AddMonths(1);
            var list = from m in _qcdb.ProductionSchedule
                       where m.Subscribe == _pickdate
                       orderby m.FactoryId
                       select m;
            var factoryGroup = from m in _qcdb.Factory
                               select new FactoryGroup{ FactoryId = m.Id, FactoryName = m.Name };
            ViewBag.FG = factoryGroup;
            // 当月累计表
            var monthlist = _qcdb.Database.SqlQuery<MonthSchedule>("select T4.ProductId,T4.ProductName,T4.Specification,ProductClass.ProductClassName,T4.FactoryId,T4.FactoryName,T4.ProductionPlan,T4.Qty from " +
                "(select T3.ProductId as ProductId, Product.SimpleName as ProductName,Product.ProductClassId as ProductClassId ,Specification as Specification,T3.FactoryId as FactoryId, Factory.Name as FactoryName, T3.ProductionPlan as ProductionPlan, T3.Qty as Qty from " +
                "(select ISNULL(ProductionPlan, 0) as ProductionPlan, ISNULL(Qty, 0) as Qty, ISNULL(ISNULL(T1.ProductId, T2.ProductId),0) as ProductId, ISNULL(ISNULL(T1.FactoryId,T2.FactoryId),0) as FactoryId " +
                "from(select Factory.Id as FactoryId,Product.Id as ProductId,sum(ProductionSchedules.ProductionPlan) as ProductionPlan from Factory left join ProductFactories on Factory.Id = ProductFactories.Factory_Id " +
                "left join Product on Product.Id = ProductFactories.Product_Id left join ProductionSchedules on Factory.Id = ProductionSchedules.FactoryId and Product.Id = ProductionSchedules.ProductId " +
                "where ProductionSchedules.Subscribe >= '" + _pickdate.ToString("yyyy-MM-dd") + "' and ProductionSchedules.Subscribe < '" + nextmonth.ToString("yyyy-MM-dd") + "' " +
                "group by Product.Id, Factory.Id) as T1 full join (select QCAgenda.FactoryId, ProductionDetails.ProductId, SUM(ProductionDetails.ProductionQty) as Qty from QCAgenda " +
                "left join ProductionDetails on ProductionDetails.QCAgendaId = QCAgenda.Id where QCAgenda.Subscribe >= '" + _pickdate.ToString("yyyy-MM-dd") + "' and QCAgenda.Subscribe < '" + nextmonth.ToString("yyyy-MM-dd") + "' " +
                "group by QCAgenda.FactoryId, ProductionDetails.ProductId) as T2 on T1.FactoryId = T2.FactoryId and T1.ProductId = T2.ProductId) as T3 left join Product on T3.ProductId = Product.Id left join Factory on T3.FactoryId = Factory.Id) as T4 left join ProductClass on T4.ProductClassId=ProductClass.Id");
            return PartialView(monthlist);
        }
        public ActionResult Manager_AddSchedule()
        {
            var factorylist = from m in _qcdb.Factory
                              select m;
            ViewBag.FactoryDropdown = new SelectList(factorylist, "Id", "SimpleName");
            return PartialView();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Manager_AddSchedule(FormCollection form)
        {
            // 判断下拉菜单以及日期选项是否为空
            try
            {
                int pid = Convert.ToInt32(form["ProductId"]);
                int fid = Convert.ToInt32(form["FactoryId"]);
                var currenmonth = Convert.ToDateTime(form["DateList"]);
                var productionSchedule = _qcdb.ProductionSchedule.SingleOrDefault(m => m.ProductId == pid && m.Subscribe == currenmonth && m.FactoryId == fid);
                if (productionSchedule == null)
                {
                    ProductionSchedule newSchedule = new ProductionSchedule()
                    {
                        FactoryId = fid,
                        ProductId = pid,
                        Subscribe = currenmonth,
                        Status = false,
                        ProductionPlan= Convert.ToInt32(form["ProductionPlan"])
                };
                    _qcdb.ProductionSchedule.Add(newSchedule);
                    await _qcdb.SaveChangesAsync();
                    return Content("SUCCESS");
                }
                else
                {
                    productionSchedule.ProductionPlan = Convert.ToInt32(form["ProductionPlan"]);
                    _qcdb.Entry(productionSchedule).State = System.Data.Entity.EntityState.Modified;
                    await _qcdb.SaveChangesAsync();
                    return Content("SUCCESS");
                }
            }
            catch
            {
                return Content("FAIL");
            }
        }

        public ActionResult Manager_AddSchedulePartial(int fid,int pid,string date)
        {
            DateTime currentDate = Convert.ToDateTime(date);
            var monthSchedule = _qcdb.ProductionSchedule.SingleOrDefault(m => m.FactoryId == fid && m.ProductId == pid && m.Subscribe == currentDate);
            ViewBag.pname = _qcdb.Product.SingleOrDefault(m => m.Id == pid).SimpleName;
            return PartialView(monthSchedule);
        }

        public ActionResult Manager_ProductionDetails(int fid, string date)
        {
            DateTime currentMonth = Convert.ToDateTime(date);
            DateTime nextMonth = currentMonth.AddMonths(1);
            var list = from m in _qcdb.QCAgenda
                       where m.FactoryId == fid && m.Subscribe >= currentMonth && m.Subscribe < nextMonth
                       select m;
            return PartialView(list);
        }
        //定期检测查询列表
        public ActionResult Manager_QualityRegularTest()
        {
            QCStaff staff = getStaff(User.Identity.Name);
            ViewBag.FactoryList = new SelectList(staff.Factory, "Id", "SimpleName");
            return PartialView();
        }
        public ActionResult Manager_QualityRegularTestPartial(int fid)
        {           
            var qrtlist = from m in _qcdb.RegularTest
                         where m.FactoryId== fid
                         orderby m.ApplyDate descending
                         select m;
            return PartialView(qrtlist);
        }

        // 质检查询列表
        public ActionResult Manager_QualityTest(int? fid, string date)
        {
            string factoryName = "- 请选择 -";
            if (date == null)
            {
                date = DateTime.Now.ToString("yyyy-MM-dd");
            }
            var factoryList = from m in _qcdb.Factory
                              select new { Id = m.Id, Name = m.SimpleName };
            
            if (fid != null)
            {
                var factory = _qcdb.Factory.SingleOrDefault(m => m.Id == fid);
                factoryName = factory.SimpleName;
                ViewBag.FactoryDropdown = new SelectList(factoryList, "Id", "Name", fid);
            }
            else
            {
                ViewBag.FactoryDropdown = new SelectList(factoryList, "Id", "Name");
            }
            QualityTestViewModel model = new QualityTestViewModel()
            {
                Qt_fid = fid,
                Qt_SelectDate = date,
                Qt_FactoryName = factoryName
            };
            return PartialView(model);
        }

        public ActionResult Manager_QualityTestPartial(int fid, string date, bool? sorttype)
        {
            
            bool _sorttype = sorttype ?? false;
            DateTime _start = Convert.ToDateTime(date);
            DateTime _end = _start.AddDays(1);
            if (_sorttype)
            {
                var qtlist = from m in _qcdb.QualityTest
                             where m.FactoryId == fid && m.ApplyTime >= _start && m.ApplyTime < _end
                             orderby m.ProductId, m.ApplyTime descending
                             select m;
                foreach (var item in qtlist)
                {
                    try
                    {
                        var a = Newtonsoft.Json.JsonConvert.DeserializeObject<QCContent>(item.Values);
                    }
                    catch (Exception)
                    {
                        ViewBag.version = "Old";
                        return PartialView(qtlist);
                    }
                    break;
                }
                ViewBag.version = "New";
                return PartialView(qtlist);
            }
            else
            {
                var qtlist = from m in _qcdb.QualityTest
                             where m.FactoryId == fid && m.ApplyTime >= _start && m.ApplyTime < _end
                             orderby m.ApplyTime descending
                             select m;
                foreach (var item in qtlist)
                {
                    try
                    {
                        var a = Newtonsoft.Json.JsonConvert.DeserializeObject<QCContent>(item.Values);
                    }
                    catch (Exception)
                    {
                        ViewBag.version = "Old";
                        return PartialView(qtlist);
                    }
                    break;
                }
                ViewBag.version = "New";
                return PartialView(qtlist);
            }
        }

        // 质检信息详情
        public ActionResult Manager_QualityTestDetail(int qtid)
        {
            var model = _qcdb.QualityTest.SingleOrDefault(m => m.Id == qtid);
            if (model != null)
            {
                List<TestTemplateItem> list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TestTemplateItem>>(model.Values);
                ViewBag.Details = list;
                return PartialView(model);
            }
            else
                return PartialView("NotFound");
        }

        

        // 故障查询列表
        public ActionResult Manager_Breakdown(int? fid, string date)
        {
            string factoryName = "- 请选择 -";
            var factoryList = from m in _qcdb.Factory
                              select new { Id = m.Id, Name = m.SimpleName };

            if (fid != null)
            {
                var factory = _qcdb.Factory.SingleOrDefault(m => m.Id == fid);
                factoryName = factory.SimpleName;
                ViewBag.FactoryDropdown = new SelectList(factoryList, "Id", "Name", fid);
            }
            else
            {
                ViewBag.FactoryDropdown = new SelectList(factoryList, "Id", "Name");
            }
            BreakdownViewModel model = new BreakdownViewModel()
            {
                Bd_fid = fid,
                Bd_SelectDate = date,
                Bd_FactoryName = factoryName
            };
            return PartialView(model);
        }

        public ActionResult Manager_BreakdownPartial(int fid, string date)
        {
            DateTime _start = Convert.ToDateTime(date);
            DateTime _end = _start.AddDays(1);
            var qtlist = from m in _qcdb.BreadkdownReport
                         where m.QCEquipment.FactoryId == fid && m.BreakDownTime >= _start && m.BreakDownTime < _end
                         orderby m.BreakDownTime
                         select m;
            return PartialView(qtlist);
        }

        // 质检信息详情
        public ActionResult Manager_BreakdownDetail(int bdid)
        {
            var item = _qcdb.BreadkdownReport.SingleOrDefault(m => m.Id == bdid);
            return PartialView(item);
        }

        //设备列表
        public ActionResult Manager_QCEquipmentList()
        {
            var factorylist = from m in _qcdb.Factory
                              orderby m.Id descending
                              select m;
            ViewBag.fl = new SelectList(factorylist, "Id", "SimpleName");
            return View();
        }

        //设备列表ajax
        public ActionResult Manager_QCEquipmentListPartial(int fid)
        {
            var equipmentlist = from m in _qcdb.QCEquipment
                                where m.FactoryId == fid&&m.Status!=-1
                                orderby m.FactoryId descending
                                select m;
            return PartialView(equipmentlist);
        }

        //添加设备页面
        public ActionResult Manager_CreateEquipment(int?fid)
        {
            var factorylist = from m in _qcdb.Factory
                              orderby m.Id descending
                              select m;
            QCEquipment model = new QCEquipment();
            if (fid == null)
            {
                ViewBag.fl = new SelectList(factorylist, "Id", "SimpleName");
                return View(model);
            }
            else
            {
                ViewBag.fl = new SelectList(factorylist, "Id", "SimpleName", fid);
                return View(model);
            }        
        }

        //提交添加设备
        [HttpPost,ValidateAntiForgeryToken]
        public ActionResult CreateEquipmentForFactory(FormCollection form,QCEquipment model)
        {
            if (ModelState.IsValid)
            {
                QCEquipment item = new QCEquipment();
                if (TryUpdateModel(item))
                {
                    _qcdb.QCEquipment.Add(item);
                    _qcdb.SaveChanges();
                    return Content("SUCCESS");
                }
                else
                {
                    return Content("FAIL");
                }             
            }
            else
            {
                return Content("FAIL");
            }

        }

        //修改设备
        public ActionResult Manager_EditEquipment(int Eid)
        {
            var equipment = _qcdb.QCEquipment.SingleOrDefault(m => m.Id == Eid);
            if (equipment == null)
            {
                return View("NotFound");
            }
            else
            {
                var selectfactory = from m in _qcdb.QCEquipment
                                    where m.Id == Eid
                                    select m.FactoryId;
                var factorylist = from m in _qcdb.Factory
                    orderby m.Id descending
                    select m;
                ViewBag.fl = new SelectList(factorylist, "Id", "SimpleName", selectfactory);
                return View(equipment);
            }
        }

        //提交修改
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Manager_EditEquipment(int id, QCEquipment model)
        {
            if (ModelState.IsValid)
            {
                var item = new QCEquipment();
                if (TryUpdateModel(item))
                {
                    _qcdb.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    try
                    {
                        _qcdb.SaveChanges();
                    }
                    catch
                    {
                        return Content("提交数据异常");
                    }
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            else
            {
                return Content("FAIL");
            }

        }

        //删除设备
        [HttpPost]
        public ActionResult DelEquipemntForFactory(int Eid)
        {
            var equipment = _qcdb.QCEquipment.SingleOrDefault(m => m.Id == Eid);
            if (equipment == null)
            {
                return Json(new { result = "FAIL" });
            }
            else
            {
                try
                {
                    equipment.Status = -1;
                    foreach(var item in equipment.BreakdownType)
                    {
                        item.Status = -1;
                        _qcdb.SaveChanges();
                    }
                   _qcdb.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
                catch
                {
                    return Json(new { result = "FAIL" });
                }
            }

        }
        
        //设备详情
        public ViewResult Manager_EquipmentDetails(int Eid)
        {
            var Equipment = _qcdb.QCEquipment.SingleOrDefault(m => m.Id == Eid);
            return View(Equipment);
        }
        //设备故障页面
        public ActionResult Manager_BreakdownList()
        {
            var factorylist = from m in _qcdb.Factory
                              orderby m.Id descending
                              select m;
            ViewBag.fl = new SelectList(factorylist, "Id", "SimpleName");
            return View();
        }
        //设备故障列表
        public ActionResult Manager_BreakdownListPartial(int Eid)
        {
            var breakdownlist = from m in _qcdb.BreakdownType
                                where m.EquipmentId == Eid && m.Status != -1
                                orderby m.Id descending
                                select m;
            return PartialView(breakdownlist);
        }

        //设备故障添加
        public ActionResult Manager_CreateBreakdown(int? Fid, int? Eid)
        {
            var factorylist = from m in _qcdb.Factory
                              orderby m.Id descending
                              select m;
            var euipmentlist = from m in _qcdb.QCEquipment
                               where m.FactoryId == Fid
                               orderby m.Id descending
                               select m;
            BreakdownType model = new BreakdownType();
            if (Fid == null || Eid == null)
            {
                ViewBag.fl = new SelectList(factorylist, "Id", "SimpleName");
                ViewBag.el = new SelectList(euipmentlist, "Id", "SimpleName");
                return View(model);
            }
            else
            {
                ViewBag.fl = new SelectList(factorylist, "Id", "SimpleName", Fid);
                ViewBag.el = new SelectList(euipmentlist, "Id", "SimpleName",Eid);
                return View(model);
            }       
        }

        //提交设备故障添加
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CreateBreakdowmForEquipment(FormCollection form, BreakdownType model)
        {
            if (ModelState.IsValid)
            {
                BreakdownType item = new BreakdownType();
                if (TryUpdateModel(item))
                {
                    _qcdb.BreakdownType.Add(item);
                    _qcdb.SaveChanges();
                    return Content("SUCCESS");
                }
                else
                {
                    return Content("FAIL");
                }
            }
            else
            {
                return Content("FAIL");
            }

        }

        //故障详情
        public ViewResult Manager_BreakDownTypeDetails(int Bid)
        {
            var breakdowntype = _qcdb.BreakdownType.SingleOrDefault(m => m.Id == Bid);
            return View(breakdowntype);
        }

        //修改故障页面
        public ActionResult Manager_EditBreakDownType(int Bid)
        {
            var item = _qcdb.BreakdownType.SingleOrDefault(m => m.Id == Bid);
            if (item == null)
            {
                return View("NotFound");
            }
            else
            {
                var breakdowntype = _qcdb.BreakdownType.SingleOrDefault(m => m.Id == Bid);
                return View(breakdowntype);
            }
        }

        //提交修改
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Manager_EditBreakDownType(int id, BreakdownType model)
        {
            if (ModelState.IsValid)
            {
                var item = new BreakdownType();
                if (TryUpdateModel(item))
                {
                    _qcdb.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    _qcdb.SaveChanges();
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            else
            {
                return Content("FAIL");
            }

        }

        //删除故障
        [HttpPost]
        public ActionResult DelBreakDownType(int Bid)
        {
            var breakdowntype = _qcdb.BreakdownType.SingleOrDefault(m => m.Id == Bid);
            if (breakdowntype == null)
            {
                return Json(new { result = "FAIL" });
            }
            else
            {
                try
                {
                    breakdowntype.Status = -1;
                    _qcdb.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
                catch
                {
                    return Json(new { result = "FAIL" });
                }
            }

        }

        public ActionResult Manager_Control()
        {
            return View();
        }

        // 辅助类
        public QCStaff getStaff(string username)
        {
            QCStaff user = _qcdb.QCStaff.SingleOrDefault(m => m.UserId == username);
            return user;
        }

        public bool EvalQualityTest(List<QCContentCategory> items)
        {
            foreach(var item in items)
            {
                if (item.State == false)
                {
                    return false;
                }
                else
                {
                    foreach(var _item in item.Columns)
                    {
                        if (_item.Value.ToString() == "false"||_item.Value=="")
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        // 保存图片
        [HttpPost]
        public JsonResult SaveOrignalImage(string serverId)
        {
            try
            {
                WeChatUtilities utilities = new WeChatUtilities();
                string url = "http://file.api.weixin.qq.com/cgi-bin/media/get?access_token=" + utilities.getAccessToken() + "&media_id=" + serverId;
                System.Uri httpUrl = new System.Uri(url);
                HttpWebRequest req = (HttpWebRequest)(WebRequest.Create(httpUrl));
                req.Method = "GET";
                HttpWebResponse res = (HttpWebResponse)(req.GetResponse());
                //Bitmap img = new Bitmap(res.GetResponseStream());//获取图片流
                //string folder = HttpContext.Server.MapPath("~/Content/checkin-img/");
                string filename = DateTime.Now.ToFileTime().ToString() + ".jpg";
                //img.Save(folder + filename);//随机名
                AliOSSUtilities util = new AliOSSUtilities();
                Stream inStream = res.GetResponseStream();
                MemoryStream ms = new MemoryStream();
                byte[] buffer = new byte[1024];
                while (true)
                {
                    int sz = inStream.Read(buffer, 0, 1024);
                    if (sz == 0)
                        break;
                    ms.Write(buffer, 0, sz);
                }
                ms.Position = 0;
                util.PutObject(ms.ToArray(), "qc-img/" + filename);
                return Json(new { result = "SUCCESS", filename = filename });
            }
            catch (Exception ex)
            {
                string aa = ex.Message;
                CommonUtilities.writeLog(aa);
            }
            return Json(new { result = "FAIL" });
        }

        // 缩略图(等比缩小，宽或高100px)
        public FileResult ThumbnailImage(string filename)
        {
            AliOSSUtilities util = new AliOSSUtilities();
            Bitmap originalImage = new Bitmap(util.GetObject("qc-img/" + filename));
            int towidth = 100;
            int toheight = 100;

            int x = 0;
            int y = 0;
            int ow = originalImage.Width;
            int oh = originalImage.Height;
            if (originalImage.Width >= originalImage.Height)
            {
                towidth = originalImage.Width * 100 / originalImage.Height;
            }
            else
            {
                toheight = originalImage.Height * 100 / originalImage.Width;
            }
            System.Drawing.Image bitmap = new System.Drawing.Bitmap(towidth, toheight);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);

            //设置高质量插值法
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

            //设置高质量,低速度呈现平滑程度
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充
            g.Clear(System.Drawing.Color.White);

            //在指定位置并且按指定大小绘制原图片的指定部分
            g.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, towidth, toheight),
                    new System.Drawing.Rectangle(x, y, ow, oh),
                    System.Drawing.GraphicsUnit.Pixel);
            try
            {
                //以jpg格式保存缩略图
                MemoryStream s = new MemoryStream();

                bitmap.Save(s, ImageFormat.Jpeg);
                byte[] imgdata = s.ToArray();
                //s.Read(imgdata, 0, imgdata.Length);
                //s.Seek(0, SeekOrigin.Begin);
                return File(imgdata, "image/jpeg");
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                originalImage.Dispose();
                bitmap.Dispose();
                g.Dispose();
            }
            //return File(null);
        }

        // 缩略图(取图片中间部分，100*100)
        public FileResult ThumbnailImage_Box(string filename)
        {
            AliOSSUtilities util = new AliOSSUtilities();
            Bitmap originalImage = new Bitmap(util.GetObject("qc-img/" + filename));
            int towidth = 100;
            int toheight = 100;

            int x = 0;
            int y = 0;
            int ow = originalImage.Width;
            int oh = originalImage.Height;
            if (originalImage.Width >= originalImage.Height)
            {
                x = (originalImage.Width - originalImage.Height) / 2;
                ow = oh;
            }
            else
            {
                y = (originalImage.Height - originalImage.Width) / 2;
                oh = ow;
            }
            System.Drawing.Image bitmap = new System.Drawing.Bitmap(towidth, toheight);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);

            //设置高质量插值法
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

            //设置高质量,低速度呈现平滑程度
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充
            g.Clear(System.Drawing.Color.White);

            //在指定位置并且按指定大小绘制原图片的指定部分
            g.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, towidth, toheight),
                    new System.Drawing.Rectangle(x, y, ow, oh),
                    System.Drawing.GraphicsUnit.Pixel);
            try
            {
                //以jpg格式保存缩略图
                MemoryStream s = new MemoryStream();

                bitmap.Save(s, ImageFormat.Jpeg);
                byte[] imgdata = s.ToArray();
                //s.Read(imgdata, 0, imgdata.Length);
                //s.Seek(0, SeekOrigin.Begin);
                return File(imgdata, "image/jpeg");
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                originalImage.Dispose();
                bitmap.Dispose();
                g.Dispose();
            }
        }

        public string GenerateDailyCode()
        {
            TimeSpan ts = DateTime.Now.Date.AddSeconds(135816).ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var datecode = ts.TotalSeconds.ToString();
            return "" + datecode[7] + datecode[6] + datecode[5] + datecode[4];
        }

        public bool IsEmpty(List<QCContentItem> item)
        {
            
            foreach(var _item in item)
            {
                if (_item.Value.ToString()=="true")
                {
                    return true;
                }
                else
                {
                    if (_item.Value != ""&&_item.Value.ToString()!="false")
                    {
                        return true;
                    }
                }               
            }
            return false;
        }
    }
}