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
                                UserId = model.Open_Id,
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
            if (System.Text.RegularExpressions.Regex.IsMatch(mobile, "1[3|5|7|8|][0-9]{9}"))
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
                    UserId = user.OpenId,
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

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> QCCheckin(QCAgenda model)
        {
            if (ModelState.IsValid)
            {
                QCAgenda agenda = new QCAgenda();
                if (TryUpdateModel(agenda))
                {
                    var exist_agenda = from m in _qcdb.QCAgenda
                                       where m.FactoryId == agenda.FactoryId && m.Subscribe == agenda.Subscribe && m.Status == 1
                                       select m;
                    // 判断是否存在同一天，同一个工厂的日程
                    if (exist_agenda.Count() == 0)
                    {
                        // 没有 则添加日程
                        agenda.CheckinTime = DateTime.Now;
                        agenda.Subscribe = DateTime.Now.Date;
                        agenda.Status = 1; // 已签到
                        _qcdb.QCAgenda.Add(agenda);
                        await _qcdb.SaveChangesAsync();
                        return Content("SUCCESS");
                    }
                    else
                    {
                        // 如果有的话 修改日程记录
                        agenda.CheckinTime = DateTime.Now;
                        agenda.Subscribe = DateTime.Now.Date;
                        agenda.Status = 1; // 已签到
                        _qcdb.Entry(agenda).State = System.Data.Entity.EntityState.Modified;
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
                var list = from m in _qcdb.BreadkdownReport
                           where m.QCStaffId == staff.Id && m.ReportTime >= _start && m.ReportTime < _end
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
                       where m.FactoryId == factoryId
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
        public async Task<ContentResult> RecoveryBreakdown(BreakdownReport model)
        {
            if (ModelState.IsValid)
            {
                BreakdownReport report = new BreakdownReport();
                QCStaff staff = getStaff(User.Identity.Name);
                if (model.QCStaffId == staff.Id)
                {
                    if (TryUpdateModel(report))
                    {
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
                             select new { Key = m.Id, Value = m.Subscribe };
            ViewBag.AgendaList = new SelectList(agendalist, "Key", "Value");
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
                             select new { Key = m.Id, Value = m.Subscribe };
            ViewBag.AgendaList = new SelectList(agendalist, "Key", "Value");
            return PartialView();
        }
        public PartialViewResult QCDailySummaryPartial(int agendaId)
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
        public async Task<ContentResult> QCDailySummaryPartial(FormCollection form)
        {
            if (ModelState.IsValid)
            {
                QCAgenda agenda = new QCAgenda();
                if (TryUpdateModel(agenda))
                {
                    agenda.SummaryTime = DateTime.Now;
                    agenda.Status = 3; // 已提报数据
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
        

        // 辅助类
        public QCStaff getStaff(string username)
        {
            QCStaff user = _qcdb.QCStaff.SingleOrDefault(m => m.UserId == username);
            return user;
        }

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
    }
}