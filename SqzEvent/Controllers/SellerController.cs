using SqzEvent.DAL;
using SqzEvent.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SqzEvent.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using CsvHelper;

namespace SqzEvent.Controllers
{
    [Authorize]
    public class SellerController : Controller
    {
        // GET: Seller
        OfflineSales offlineDB = new OfflineSales();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public SellerController()
        {

        }

        public SellerController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        public ActionResult Index()
        {
            //return View();
            return Content(HttpContext.Request.Url.Host);
        }

        // 促销员入口
        [AllowAnonymous]
        public ActionResult SellerLoginManager(int? systemid)
        {
            int _systemid = systemid ?? 1;
            string user_Agent = HttpContext.Request.UserAgent;
            if (user_Agent.Contains("MicroMessenger"))
            {
                //return Content("微信");
                string redirectUri = Url.Encode("https://event.shouquanzhai.cn/Seller/SellerAuthorization");
                string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
                string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_base&state=" + _systemid + "#wechat_redirect";

                return Redirect(url);
            }
            else
            {
                return Content("其他");
            }
        }
        [AllowAnonymous]
        public async Task<ActionResult> SellerAuthorization(string code, string state)
        {
            //return Content(code);
            //string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            try
            {
                WeChatUtilities wechat = new WeChatUtilities();
                var jat = wechat.getWebOauthAccessToken(code);
                var user = UserManager.FindByEmail(jat.openid);
                int systemid = Convert.ToInt32(state);
                if (user != null)
                {
                    //var user = UserManager.FindByName("13636314852");
                    if (UserManager.IsInRole(user.Id, "Seller"))
                    {
                        if (user.OffSalesSystem != null)
                        {
                            string[] systemArray = user.OffSalesSystem.Split(',');
                            if (systemArray.Contains(state))
                            {
                                user.DefaultSystemId = systemid;
                                UserManager.Update(user);
                                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                                return RedirectToAction("Seller_Home", new { systemid = systemid });
                            }
                        }
                        else
                        {
                            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                            return RedirectToAction("Seller_Home", new { systemid = systemid });
                        }
                    }
                    else
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToAction("Seller_ForceRegister", new { systemid = systemid });
                    }
                }
                //return Content(jat.openid + "," + jat.access_token);
                return RedirectToAction("Seller_Register", "Seller", new { open_id = jat.openid, accessToken = jat.access_token, systemid = systemid });
            }
            catch (Exception ex)
            {
                CommonUtilities.writeLog(ex.Message);
                return View("Error");
            }
        }
        [AllowAnonymous]
        public async Task<ActionResult> ForceAuthorization(string openid, string state)
        {
            try
            {
                string _state = "1";
                if (state == null)
                {
                    _state = state;
                }
                WeChatUtilities wechat = new WeChatUtilities();
                var user = UserManager.FindByEmail(openid);
                int systemid = Convert.ToInt32(_state);
                if (user != null)
                {
                    //var user = UserManager.FindByName("13636314852");
                    string[] systemArray = user.OffSalesSystem.Split(',');
                    if (systemArray.Contains(state))
                    {
                        user.DefaultSystemId = systemid;
                        UserManager.Update(user);
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToAction("Seller_Home", new { systemid = systemid });
                    }
                }
                //return RedirectToAction("Wx_Register", "Seller", new { open_id = openid, accessToken = jat.access_token, systemid = systemid });
                return View("Error");
            }
            catch
            {
                return View("Error");
            }
        }

        // 管理员入口
        [AllowAnonymous]
        public ActionResult ManagerLoginManager(int? systemid)
        {
            int _systemid = systemid ?? 1;
            string user_Agent = HttpContext.Request.UserAgent;
            if (user_Agent.Contains("MicroMessenger"))
            {
                string redirectUri = Url.Encode("https://event.shouquanzhai.cn/Seller/ManagerAuthorization");
                string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
                string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_base&state=" + _systemid + "#wechat_redirect";

                return Redirect(url);
            }
            else
            {
                return Content("其他");
            }
        }
        [AllowAnonymous]
        public async Task<ActionResult> ManagerAuthorization(string code, string state)
        {
            try
            {
                WeChatUtilities wechat = new WeChatUtilities();
                var jat = wechat.getWebOauthAccessToken(code);
                var user = UserManager.FindByEmail(jat.openid);
                int systemid = Convert.ToInt32(state);
                if (user != null)
                {
                    if (UserManager.IsInRole(user.Id, "Manager"))
                    {
                        if (user.OffSalesSystem != null)
                        {
                            string[] systemArray = user.OffSalesSystem.Split(',');
                            if (systemArray.Contains(state))
                            {
                                user.DefaultSystemId = systemid;
                                UserManager.Update(user);
                                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                                return RedirectToAction("Manager_Home", new { systemid = systemid });
                            }
                        }
                        else
                        {
                            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                            return RedirectToAction("Manager_Home", new { systemid = systemid });
                        }
                    }
                    else
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToAction("Seller_ForceRegister", new { systemid = systemid });
                    }
                }
                //return Content(jat.openid + "," + jat.access_token);
                return RedirectToAction("Seller_Register", "Seller", new { open_id = jat.openid, accessToken = jat.access_token, systemid = systemid });
            }
            catch (Exception ex)
            {
                CommonUtilities.writeLog(ex.Message);
                return View("Error");
            }
        }
        [AllowAnonymous]
        public ActionResult Seller_Register(string open_id, string accessToken, int systemid)
        {
            var model = new Wx_OffRegisterViewModel();
            model.Open_Id = open_id;
            model.AccessToken = accessToken;
            model.SystemId = systemid;
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Seller_Register(string open_id, Wx_OffRegisterViewModel model)
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
                        // 是否属于当前商家
                        string[] SystemArray = exist_user.OffSalesSystem.Split(',');
                        if (SystemArray.Contains(model.SystemId.ToString()))
                        {
                            ModelState.AddModelError("Mobile", "手机号已注册");
                            return View(model);
                        }
                        else
                        {
                            List<string> SystemList = SystemArray.ToList();
                            SystemList.Add(model.SystemId.ToString());
                            exist_user.OffSalesSystem = string.Join(",", SystemList.ToArray());
                            exist_user.DefaultSystemId = model.SystemId;
                            UserManager.Update(exist_user);
                            Off_Membership_Bind ofb = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.UserName == exist_user.UserName && m.Off_System_Id == model.SystemId);
                            if (ofb == null)
                            {
                                ofb = new Off_Membership_Bind()
                                {
                                    ApplicationDate = DateTime.Now,
                                    Bind = false,
                                    Off_System_Id = model.SystemId,
                                    Mobile = model.Mobile,
                                    NickName = model.NickName,
                                    UserName = model.Mobile,
                                    Type = 1
                                };
                                offlineDB.Off_Membership_Bind.Add(ofb);
                                await offlineDB.SaveChangesAsync();
                            }
                            await SignInManager.SignInAsync(exist_user, isPersistent: false, rememberBrowser: false);
                            return RedirectToAction("Seller_Home");
                        }
                    }
                    else
                    {
                        var user = new ApplicationUser { UserName = model.Mobile, NickName = model.NickName, Email = model.Open_Id, PhoneNumber = model.Mobile, AccessToken = model.AccessToken, OpenId = model.Open_Id, DefaultSystemId = model.SystemId, OffSalesSystem = model.SystemId.ToString() };
                        var result = await UserManager.CreateAsync(user, open_id);
                        if (result.Succeeded)
                        {
                            smsRecord.Status = true;
                            smsDB.SaveChanges();
                            await UserManager.AddToRoleAsync(user.Id, "Seller");
                            Off_Membership_Bind ofb = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == model.SystemId);
                            if (ofb == null)
                            {
                                ofb = new Off_Membership_Bind()
                                {
                                    ApplicationDate = DateTime.Now,
                                    Bind = false,
                                    Off_System_Id = model.SystemId,
                                    Mobile = model.Mobile,
                                    NickName = model.NickName,
                                    UserName = user.UserName,
                                    Type = 1
                                };
                                offlineDB.Off_Membership_Bind.Add(ofb);
                                await offlineDB.SaveChangesAsync();
                                WeChatUtilities wechat = new WeChatUtilities();
                                wechat.setUserToGroup(model.Open_Id, 103);
                            }
                            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                            return RedirectToAction("Seller_Home");
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
        public ActionResult Wx_SendSms(string mobile)
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
                    string message = Send_Sms_VerifyCode(mobile, validateCode);
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
        public string Send_Sms_VerifyCode(string mobile, string code)
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

        // 注册方式(2)
        public ActionResult Seller_ForceRegister(int systemid)
        {
            Wx_SellerRegisterViewModel model = new Wx_SellerRegisterViewModel();
            model.Systemid = systemid;
            return View(model);
        }
        [ValidateAntiForgeryToken, HttpPost]
        public async Task<ActionResult> Seller_ForceRegister(FormCollection form, Wx_SellerRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                user.DefaultSystemId = model.Systemid;
                user.OffSalesSystem = model.Systemid.ToString();
                UserManager.Update(user);
                user.NickName = model.NickName;
                UserManager.Update(user);
                await UserManager.AddToRoleAsync(user.Id, "Seller");
                Off_Membership_Bind ofb = new Off_Membership_Bind()
                {
                    ApplicationDate = DateTime.Now,
                    Bind = false,
                    Mobile = user.UserName,
                    NickName = model.NickName,
                    UserName = user.UserName,
                    Off_System_Id = model.Systemid,
                    Type = 1
                };
                offlineDB.Off_Membership_Bind.Add(ofb);
                await offlineDB.SaveChangesAsync();
                WeChatUtilities wechat = new WeChatUtilities();
                wechat.setUserToGroup(user.OpenId, 103);
                return RedirectToAction("Seller_Home");
            }
            else
            {
                ModelState.AddModelError("", "注册失败");
                return View(model);
            }
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

                util.PutObject(ms.ToArray(), "checkin-img/" + filename);
                return Json(new { result = "SUCCESS", filename = filename });
            }
            catch (Exception ex)
            {
                string aa = ex.Message;
                CommonUtilities.writeLog(aa);
            }
            return Json(new { result = "FAIL" });
        }
        public FileResult ThumbnailImage(string filename)
        {
            AliOSSUtilities util = new AliOSSUtilities();
            Bitmap originalImage = new Bitmap(util.GetObject("checkin-img/" + filename));
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
        public FileResult ThumbnailImage_Box(string filename)
        {
            AliOSSUtilities util = new AliOSSUtilities();
            Bitmap originalImage = new Bitmap(util.GetObject("checkin-img/" + filename));
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

        /************ 新版本界面 ************/
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_Home()
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
        public PartialViewResult Manager_UserPanel()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == User.Identity.Name && m.Off_System_Id == user.DefaultSystemId);
            if (manager != null)
            {
                ViewBag.NickName = user.NickName;
                ViewBag.ImgUrl = user.ImgUrl == null ? null : user.ImgUrl.Replace("http://", "//");
                return PartialView(manager);
            }
            else
            {
                ViewBag.NickName = user.NickName;
                ViewBag.ImgUrl = user.ImgUrl == null ? null : user.ImgUrl.Replace("http://", "//");
                return PartialView();
            }
        }
        public ActionResult Manager_UpdateUserInfo()
        {
            string redirectUri = Url.Encode("https://event.shouquanzhai.cn/Seller/Manager_Authorize");
            string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_userinfo&state=1#wechat_redirect";
            return Redirect(url);
        }
        
        public ActionResult Manager_Authorize(string code, string state)
        {
            WeChatUtilities wechat = new WeChatUtilities();
            var jat = wechat.getWebOauthAccessToken(code);
            var user = UserManager.FindById(User.Identity.GetUserId());
            user.AccessToken = jat.access_token;
            UserManager.Update(user);
            //WeChatUtilities wechat = new WeChatUtilities();
            var userinfo = wechat.getWebOauthUserInfo(user.AccessToken, user.OpenId);
            user.NickName = userinfo.nickname;
            user.ImgUrl = userinfo.headimgurl;
            user.Sex = userinfo.sex == "1" ? true : false;
            user.Province = userinfo.province;
            user.City = userinfo.city;
            UserManager.Update(user);
            return RedirectToAction("Manager_Home");
        }

        /************ 签到 ************/
        // 首页
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_Task()
        {
            var today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            ViewBag.Weekly = today.AddDays(1 - (int)today.DayOfWeek).ToString("MM/dd") + " - " + today.AddDays(7 - (int)today.DayOfWeek).ToString("MM/dd");
            ViewBag.AnnounceCount = (from m in offlineDB.Off_Manager_Announcement
                                     where m.ManagerUserName.Contains(User.Identity.Name)
                                     && today >= m.StartTime && today < m.FinishTime
                                     select m).Count();
            return View();
        }
        // 当前个人签到数量
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        [HttpPost]
        public JsonResult Manager_RefreshTaskCount()
        {
            var today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            var user = UserManager.FindById(User.Identity.GetUserId());
            var task = offlineDB.Off_Manager_Task.SingleOrDefault(m => m.TaskDate == today && m.Status >= 0 && m.UserName == User.Identity.Name && m.Off_System_Id == user.DefaultSystemId);
            if (task == null)
            {
                return Json(new { result = "SUCCESS", data = 0 });
            }
            else
            {
                int count = task.Off_Manager_CheckIn.Count(m => m.Canceled == false);
                return Json(new { result = "SUCCESS", data = count });
            }
        }

        // 主要工作列表
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_AnnouncementList()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            var list = from m in offlineDB.Off_Manager_Announcement
                       where m.Off_System_Id == user.DefaultSystemId && m.ManagerUserName.Contains(user.UserName)
                       && today >= m.StartTime && today < m.FinishTime
                       orderby m.Status, m.Priority descending, m.SubmitTime descending
                       select m;
            return PartialView(list);
        }

        // 添加督导签到
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult> Manager_AddCheckin()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == User.Identity.Name && m.Off_System_Id == user.DefaultSystemId);
            ViewBag.NickName = manager.NickName;
            var today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            var task = offlineDB.Off_Manager_Task.SingleOrDefault(m => m.TaskDate == today && m.Status >= 0 && m.UserName == manager.UserName && m.Off_System_Id == user.DefaultSystemId);
            if (task != null)
            {
                Off_Manager_CheckIn checkin = new Off_Manager_CheckIn();
                return PartialView(checkin);
            }
            else
            {
                Off_Manager_Task item = new Off_Manager_Task()
                {
                    TaskDate = today,
                    Status = 0,
                    UserName = User.Identity.Name,
                    NickName = manager.NickName,
                    Off_System_Id = user.DefaultSystemId
                };
                offlineDB.Off_Manager_Task.Add(item);
                await offlineDB.SaveChangesAsync();
                Off_Manager_CheckIn checkin = new Off_Manager_CheckIn();
                return PartialView(checkin);
            }
        }
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Manager_AddCheckIn(Off_Manager_CheckIn model)
        {
            if (ModelState.IsValid)
            {
                Off_Manager_CheckIn item = new Off_Manager_CheckIn();
                var today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                var task = offlineDB.Off_Manager_Task.SingleOrDefault(m => m.TaskDate == today && m.Status >= 0 && m.UserName == User.Identity.Name && m.Off_System_Id == user.DefaultSystemId);
                if (TryUpdateModel(item))
                {
                    item.Off_Manager_Task = task;
                    item.CheckIn_Time = DateTime.Now;
                    offlineDB.Off_Manager_CheckIn.Add(item);
                    await offlineDB.SaveChangesAsync();
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            else
            {
                return Content("FAIL");
            }
        }

        // 督导日报
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_TaskReport(int? id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in offlineDB.Off_Manager_Task
                       where m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId
                       && m.Status == 0
                       orderby m.TaskDate descending
                       select m;
            List<Object> attendance = new List<Object>();
            foreach (var i in list)
            {
                attendance.Add(new { Key = i.Id, Value = i.TaskDate.ToString("yyyy-MM-dd") });
            }
            int _id = id ?? list.FirstOrDefault().Id;
            if (attendance.Count > 0)
                ViewBag.checkinlist = new SelectList(attendance, "Key", "Value", _id);
            return PartialView();
        }
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_TaskReportPartial(int id)
        {
            var item = offlineDB.Off_Manager_Task.SingleOrDefault(m => m.Id == id);
            return PartialView(item);
        }
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Manager_TaskReportPartial(Off_Manager_Task model)
        {
            if (ModelState.IsValid)
            {
                Off_Manager_Task item = new Off_Manager_Task();
                if (TryUpdateModel(item))
                {
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return PartialView(model);
            }
        }

        // 督导个人签到查询
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_CheckInView()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in offlineDB.Off_Manager_Task
                       where m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId
                       && m.Status == 0
                       orderby m.TaskDate descending
                       select m;
            List<Object> attendance = new List<Object>();
            foreach (var i in list)
            {
                attendance.Add(new { Key = i.Id, Value = i.TaskDate.ToString("yyyy-MM-dd") });
            }
            if (attendance.Count > 0)
                ViewBag.checkinlist = new SelectList(attendance, "Key", "Value", list.FirstOrDefault().Id);
            return View();
        }
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_CheckInViewPartial(int id)
        {
            var list = from m in offlineDB.Off_Manager_CheckIn
                       where m.Manager_EventId == id
                       && m.Canceled == false
                       select m;
            ViewBag.TaskId = id;
            return PartialView(list);
        }

        // 作废签到位置
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        [HttpPost]
        public JsonResult Manager_CancelManagerCheckin(int id)
        {
            var item = offlineDB.Off_Manager_CheckIn.SingleOrDefault(m => m.Id == id);
            if (item.Off_Manager_Task.UserName == User.Identity.Name)
            {
                item.Canceled = true;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            return Json(new { result = "FAIL" });
        }

        // 查看全部督导签到信息
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Senior")]
        public ActionResult Manager_AllCheckInList()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in offlineDB.Off_Manager_Task
                       where m.Status == 0
                       && m.Off_System_Id == user.DefaultSystemId
                       group m by m.TaskDate into g
                       select new { g.Key };
            list = list.OrderByDescending(m => m.Key);
            List<Object> attendance = new List<Object>();
            foreach (var i in list)
            {
                attendance.Add(new { Key = i.Key, Value = i.Key.ToString("yyyy-MM-dd") });
            }
            if (attendance.Count > 0)
                ViewBag.checkinlist = new SelectList(attendance, "Key", "Value", list.FirstOrDefault().Key);
            return PartialView();
        }
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Senior")]
        public ActionResult Manager_AllCheckInListPartial(string date)
        {
            var _date = Convert.ToDateTime(date);
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in offlineDB.Off_Manager_Task
                       where m.TaskDate == _date && m.Status >= 0
                       && m.Off_System_Id == user.DefaultSystemId
                       select m;
            return PartialView(list);
        }
        // 督导签到详情
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_CheckInDetails(int id)
        {
            var item = offlineDB.Off_Manager_Task.SingleOrDefault(m => m.Id == id);
            return PartialView(item);
        }

        // 添加需求
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_RequestCreate()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            Off_Manager_Request request = new Off_Manager_Request();
            request.ManagerUserName = user.UserName;
            request.Status = 0;
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Select(m => new { Key = m.Id, Value = m.StoreName });
            ViewBag.StoreList = new SelectList(storelist, "Key", "Value");
            List<Object> typelist = new List<object>();
            typelist.Add(new { Key = "店铺补货", Value = "店铺补货" });
            typelist.Add(new { Key = "赠品需求", Value = "赠品需求" });
            typelist.Add(new { Key = "问题调整", Value = "问题调整" });
            ViewBag.TypeList = new SelectList(typelist, "Key", "Value");
            return PartialView(request);
        }
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Manager_RequestCreate(Off_Manager_Request model)
        {
            if (ModelState.IsValid)
            {
                Off_Manager_Request item = new Off_Manager_Request();
                if (TryUpdateModel(item))
                {
                    item.ManagerUserName = User.Identity.Name;
                    item.RequestTime = DateTime.Now;
                    offlineDB.Off_Manager_Request.Add(item);
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                var user = UserManager.FindById(User.Identity.GetUserId());
                var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
                var storelist = manager.Off_Store.Select(m => new { Key = m.Id, Value = m.StoreName });
                ViewBag.StoreList = new SelectList(storelist, "Key", "Value");
                List<Object> typelist = new List<object>();
                typelist.Add(new { Key = "店铺补货", Value = "店铺补货" });
                typelist.Add(new { Key = "赠品需求", Value = "赠品需求" });
                typelist.Add(new { Key = "问题调整", Value = "问题调整" });
                ViewBag.TypeList = new SelectList(typelist, "Key", "Value");
                return PartialView(model);
            }
        }

        // 修改需求
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_RequestEdit(int id)
        {
            var item = offlineDB.Off_Manager_Request.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
                var storelist = manager.Off_Store.Select(m => new { Key = m.Id, Value = m.StoreName });
                ViewBag.StoreList = new SelectList(storelist, "Key", "Value", item.StoreId);
                List<Object> typelist = new List<object>();
                typelist.Add(new { Key = "店铺补货", Value = "店铺补货" });
                typelist.Add(new { Key = "赠品需求", Value = "赠品需求" });
                typelist.Add(new { Key = "问题调整", Value = "问题调整" });
                ViewBag.TypeList = new SelectList(typelist, "Key", "Value", item.RequestType);
                return PartialView(item);
            }
            return PartialView("Error");
        }
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Manager_RequestEdit(Off_Manager_Request model)
        {
            if (ModelState.IsValid)
            {
                Off_Manager_Request item = new Off_Manager_Request();
                if (TryUpdateModel(item))
                {
                    item.ManagerUserName = User.Identity.Name;
                    item.RequestTime = DateTime.Now;
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                var user = UserManager.FindById(User.Identity.GetUserId());
                var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
                var storelist = manager.Off_Store.Select(m => new { Key = m.Id, Value = m.StoreName });
                ViewBag.StoreList = new SelectList(storelist, "Key", "Value");
                List<Object> typelist = new List<object>();
                typelist.Add(new { Key = "店铺补货", Value = "店铺补货" });
                typelist.Add(new { Key = "赠品需求", Value = "赠品需求" });
                typelist.Add(new { Key = "问题调整", Value = "问题调整" });
                ViewBag.TypeList = new SelectList(typelist, "Key", "Value");
                return View(model);
            }
        }

        // 需求列表
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_RequestList()
        {
            return PartialView();
        }
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_RequestListPartial()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (User.IsInRole("Senior"))
            {
                var list = from m in offlineDB.Off_Manager_Request
                           where m.Status >= 0 && m.Off_Store.Off_System_Id == user.DefaultSystemId
                           orderby m.Status, m.Id descending
                           select m;
                return PartialView(list);
            }
            else
            {
                var list = from m in offlineDB.Off_Manager_Request
                           where m.Status >= 0 && m.ManagerUserName == User.Identity.Name && m.Off_Store.Off_System_Id == user.DefaultSystemId
                           orderby m.Status, m.Id descending
                           select m;
                return PartialView(list);
            }
        }
        // 作废需求内容
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        [HttpPost]
        public JsonResult Manager_CancelRequestJson(int id)
        {
            var item = offlineDB.Off_Manager_Request.SingleOrDefault(m => m.Id == id);
            item.Status = -1;
            offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
            offlineDB.SaveChanges();
            return Json(new { result = "SUCCESS" });
        }

        // 需求查看
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_RequestView(int id)
        {
            var item = offlineDB.Off_Manager_Request.SingleOrDefault(m => m.Id == id);
            return PartialView(item);
        }

        /************ 巡店 ************/
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_Tools()
        {
            return View();
        }

        // 刷新店铺数量
        [Authorize(Roles = "Manager")]
        [HttpPost]
        public JsonResult Manager_RefreshAllCount()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            DateTime today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Select(m => m.Id);
            var today_schedule = from m in offlineDB.Off_Checkin_Schedule
                                 where storelist.Contains(m.Off_Store_Id)
                                 && m.Subscribe == today
                                 select m;
            int uncheckin = (from m in today_schedule
                             where m.Off_Checkin.Count(p => p.Status >= 0) == 0
                             select m).Count();
            int uncheckout = (from m in today_schedule
                              where m.Off_Checkin.Any(p => p.Status == 1)
                              select m).Count();
            int unreport = (from m in today_schedule
                            where m.Off_Checkin.Any(p => p.Status == 2)
                            select m).Count();
            int unconfirm = (from m in offlineDB.Off_Checkin_Schedule
                             where m.Off_Checkin.Any(p => p.Status == 3) &&
                             storelist.Contains(m.Off_Store_Id)
                             select m).Count();
            return Json(new { result = "SUCCESS", data = new { uncheckin = uncheckin, uncheckout = uncheckout, unreport = unreport, unconfirm = unconfirm } });
        }

        // 未签到列表
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_UnCheckInList()
        {
            return PartialView();
        }
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_UnCheckInListPartial(string date)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            DateTime _date = Convert.ToDateTime(date);
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Select(m => m.Id);
            var today_schedule = from m in offlineDB.Off_Checkin_Schedule
                                 where storelist.Contains(m.Off_Store_Id)
                                 && m.Subscribe == _date
                                 && m.Off_Checkin.Count(p => p.Status >= 0) == 0
                                 orderby m.Off_Store.StoreName
                                 select m;
            return PartialView(today_schedule);
        }

        // 门店促销员列表
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_ScheduleSeller(int id)
        {
            var schedule = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == id);
            ViewBag.StoreName = schedule.Off_Store.StoreName;
            var sellerlist = schedule.Off_Store.Off_Seller;
            return PartialView(sellerlist);
        }

        // 未签退列表
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_UnCheckOutList()
        {
            return PartialView();
        }
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_UnCheckOutListPartial(string date)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            DateTime _date = Convert.ToDateTime(date);
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Select(m => m.Id);
            var checkin = from m in offlineDB.Off_Checkin
                          where storelist.Contains(m.Off_Checkin_Schedule.Off_Store_Id)
                          && m.Off_Checkin_Schedule.Subscribe == _date
                          && m.Status == 1
                          orderby m.Off_Checkin_Schedule.Off_Store.StoreName
                          select m;
            return PartialView(checkin);
        }
        // 未提报销量列表
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_UnReportList()
        {
            return PartialView();
        }
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_UnReportListPartial(string date)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            DateTime _date = Convert.ToDateTime(date);
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Select(m => m.Id);
            var checkin = from m in offlineDB.Off_Checkin
                          where storelist.Contains(m.Off_Checkin_Schedule.Off_Store_Id)
                          && m.Off_Checkin_Schedule.Subscribe == _date
                          && m.Status == 2
                          orderby m.Off_Checkin_Schedule.Off_Store.StoreName
                          select m;
            return PartialView(checkin);
        }

        // 待确认销量列表
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_UnConfirmList()
        {
            return PartialView();
        }
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_UnConfirmListPartial()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Select(m => m.Id);
            var checkin = from m in offlineDB.Off_Checkin
                          where storelist.Contains(m.Off_Checkin_Schedule.Off_Store_Id)
                          && m.Status == 3
                          select m;
            var dategroup = from m in checkin
                                group m by m.Off_Checkin_Schedule.Subscribe into g
                                orderby g.Key descending
                                select new { g.Key };
            List<DateTime> p = new List<DateTime>();
            foreach (var item in dategroup)
            {
                p.Add(item.Key);
            }
            ViewBag.DateGroup = p;
            return PartialView(checkin);
        }

        // 作废签到信息
        [Authorize(Roles = "Manager")]
        [HttpPost]
        public ActionResult Manager_DeleteCheckIn(int id)
        {
            var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                item.Status = -1;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            return Json(new { result = "FAIL" });
        }

        // 代签到
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_CreateCheckIn(int id)
        {
            var schedule = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == id);
            ViewBag.StoreName = schedule.Off_Store.StoreName;
            ViewBag.Subscribe = schedule.Subscribe;
            Off_Checkin item = new Off_Checkin()
            {
                Off_Schedule_Id = id,
                Status = 0
            };
            var sellerlist = from m in offlineDB.Off_Seller
                             where m.StoreId == schedule.Off_Store_Id
                             select m;
            ViewBag.SellerDropDown = new SelectList(sellerlist, "Id", "Name");
            return PartialView(item);
        }
        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Manager_CreateCheckIn(Off_Checkin model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                Off_Checkin checkin = new Off_Checkin();
                if (TryUpdateModel(checkin))
                {
                    // 获取模板产品列表
                    checkin.Report_Time = DateTime.Now;
                    checkin.CheckinLocation = "N/A";
                    checkin.CheckoutLocation = "N/A";
                    checkin.ConfirmTime = DateTime.Now;
                    checkin.ConfirmUser = User.Identity.Name;
                    checkin.Proxy = true;
                    checkin.Status = 3;
                    //offlineDB.Entry(checkin).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.Off_Checkin.Add(checkin);
                    offlineDB.SaveChanges();
                    List<int> plist = new List<int>();
                    var Template = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == checkin.Off_Schedule_Id).Off_Sales_Template;
                    foreach (var i in Template.ProductList.Split(','))
                    {
                        plist.Add(Convert.ToInt32(i));
                    }
                    var productlist = from m in offlineDB.Off_Product
                                      where plist.Contains(m.Id)
                                      select m;
                    // 添加或修改销售列表
                    foreach (var item in productlist)
                    {
                        // 获取单品数据
                        int? sales = null;
                        if (form["sales_" + item.Id] != "")
                            sales = Convert.ToInt32(form["sales_" + item.Id]);
                        int? storage = null;
                        if (form["storage_" + item.Id] != "")
                            storage = Convert.ToInt32(form["storage_" + item.Id]);
                        decimal? amount = null;
                        if (form["amount_" + item.Id] != "")
                            amount = Convert.ToDecimal(form["amount_" + item.Id]);

                        if (sales == null && storage == null && amount == null)
                        { }
                        else
                        {
                            Off_Checkin_Product existdata = new Off_Checkin_Product()
                            {
                                Off_Checkin = checkin,
                                ItemCode = item.ItemCode,
                                ProductId = item.Id,
                                SalesAmount = amount,
                                SalesCount = sales,
                                StorageCount = storage
                            };
                            offlineDB.Off_Checkin_Product.Add(existdata);
                        }
                    }
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "错误");
                var schedule = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == model.Off_Schedule_Id);
                ViewBag.StoreName = schedule.Off_Store.StoreName;
                ViewBag.Subscribe = schedule.Subscribe;
                var sellerlist = from m in offlineDB.Off_Seller
                                 where m.StoreId == schedule.Off_Store_Id
                                 select m;
                ViewBag.SellerDropDown = new SelectList(sellerlist, "Id", "Name");
                return View(model);
            }
        }
        [Authorize(Roles = "Manager")]
        public PartialViewResult Manager_EditReport_Item(int id, int ScheduleId)
        {
            Off_Checkin item = null;
            string[] plist_tmp;
            if (id == 0)
            {
                var schedule = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == ScheduleId);
                plist_tmp = schedule.Off_Sales_Template.ProductList.Split(',');
            }
            else
            {
                item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == id);
                plist_tmp = item.Off_Checkin_Schedule.Off_Sales_Template.ProductList.Split(',');
            }
            List<int> plist = new List<int>();
            foreach (var i in plist_tmp)
            {
                plist.Add(Convert.ToInt32(i));
            }
            var productlist = from m in offlineDB.Off_Product
                              where plist.Contains(m.Id)
                              select m;
            List<Wx_TemplateProduct> templatelist = new List<Wx_TemplateProduct>();
            foreach (var i in productlist)
            {
                Wx_TemplateProduct p = new Wx_TemplateProduct()
                {
                    ProductId = i.Id,
                    ItemCode = i.ItemCode,
                    SimpleName = i.SimpleName
                };
                templatelist.Add(p);
            }
            if (item != null)
            {
                foreach (var i in item.Off_Checkin_Product)
                {
                    var e = templatelist.SingleOrDefault(m => m.ProductId == i.ProductId);
                    e.SalesCount = i.SalesCount;
                    e.SalesAmount = i.SalesAmount;
                    e.Storage = i.StorageCount;
                }

                Wx_ReportItemsViewModel model = new Wx_ReportItemsViewModel()
                {
                    AmountRequried = item.Off_Checkin_Schedule.Off_Sales_Template.RequiredAmount,
                    StorageRequired = item.Off_Checkin_Schedule.Off_Sales_Template.RequiredStorage,
                    ProductList = templatelist
                };
                return PartialView(model);
            }
            else
            {
                var schedule = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == ScheduleId);
                Wx_ReportItemsViewModel model = new Wx_ReportItemsViewModel()
                {
                    AmountRequried = schedule.Off_Sales_Template.RequiredAmount,
                    StorageRequired = schedule.Off_Sales_Template.RequiredStorage,
                    ProductList = templatelist
                };
                return PartialView(model);
            }
        }

        // 查看并修改签到信息
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_EditCheckin(int id)
        {
            var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == id);
            List<Object> status_list = new List<object>();
            status_list.Add(new { Key = 1, Value = "已签到" });
            status_list.Add(new { Key = 2, Value = "已签退" });
            status_list.Add(new { Key = 3, Value = "已提报销量" });
            ViewBag.StatusSelectList = new SelectList(status_list, "Key", "Value", item.Status);
            return PartialView(item);
        }
        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Manager_EditCheckin(Off_Checkin model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                Off_Checkin checkin = new Off_Checkin();
                if (TryUpdateModel(checkin))
                {
                    // 获取模板产品列表
                    List<int> plist = new List<int>();
                    var Template = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == checkin.Off_Schedule_Id).Off_Sales_Template;
                    foreach (var i in Template.ProductList.Split(','))
                    {
                        plist.Add(Convert.ToInt32(i));
                    }
                    var productlist = from m in offlineDB.Off_Product
                                      where plist.Contains(m.Id)
                                      select m;
                    // 添加或修改销售列表
                    foreach (var item in productlist)
                    {
                        // 获取单品数据
                        int? sales = null;
                        if (form["sales_" + item.Id] != "")
                            sales = Convert.ToInt32(form["sales_" + item.Id]);
                        int? storage = null;
                        if (form["storage_" + item.Id] != "")
                            storage = Convert.ToInt32(form["storage_" + item.Id]);
                        decimal? amount = null;
                        if (form["amount_" + item.Id] != "")
                            amount = Convert.ToDecimal(form["amount_" + item.Id]);
                        // 判断是否已有数据
                        var checkinproductlist = offlineDB.Off_Checkin_Product.Where(m => m.CheckinId == checkin.Id);
                        var existdata = checkinproductlist.SingleOrDefault(m => m.ProductId == item.Id);
                        if (existdata != null)
                        {

                            if (sales == null && storage == null && amount == null)
                            {
                                // 无数据则删除
                                offlineDB.Off_Checkin_Product.Remove(existdata);
                            }
                            else
                            {
                                // 修改数据
                                existdata.SalesAmount = amount;
                                existdata.SalesCount = sales;
                                existdata.StorageCount = storage;
                            }
                        }
                        else
                        {
                            // 添加数据
                            // 如果三项数据不为空，则添加
                            if (sales == null && storage == null && amount == null)
                            { }
                            else
                            {
                                existdata = new Off_Checkin_Product()
                                {
                                    CheckinId = checkin.Id,
                                    ItemCode = item.ItemCode,
                                    ProductId = item.Id,
                                    SalesAmount = amount,
                                    SalesCount = sales,
                                    StorageCount = storage
                                };
                                offlineDB.Off_Checkin_Product.Add(existdata);
                                //offlineDB.SaveChanges();
                            }
                        }
                    }
                    checkin.Report_Time = DateTime.Now;
                    checkin.CheckinLocation = checkin.CheckinLocation == null ? "N/A" : checkin.CheckinLocation;
                    checkin.CheckoutLocation = checkin.CheckoutLocation == null ? "N/A" : checkin.CheckoutLocation;
                    checkin.ConfirmTime = DateTime.Now;
                    checkin.ConfirmUser = User.Identity.Name;
                    checkin.Proxy = true;
                    offlineDB.Entry(checkin).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "错误");
                return Content("FAIL");
            }
        }

        // 审核签到信息
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_CheckinConfirm(int id)
        {
            var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == id);
            return PartialView(item);
        }

        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Manager_CheckinConfirm(Off_Checkin model)
        {
            if (ModelState.IsValid)
            {
                Off_Checkin checkin = new Off_Checkin();
                if (TryUpdateModel(checkin))
                {
                    checkin.ConfirmTime = DateTime.Now;
                    checkin.ConfirmUser = User.Identity.Name;
                    checkin.Status = 4;
                    offlineDB.Entry(checkin).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            else
            {
                return Content("FAIL");
            }
        }

        // 查看销量明细列表
        [Authorize(Roles = "Manager")]
        public PartialViewResult Manager_ViewReport_Item(int id)
        {

            var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == id);
            var plist_tmp = item.Off_Checkin_Schedule.Off_Sales_Template.ProductList.Split(',');
            List<int> plist = new List<int>();
            foreach (var i in plist_tmp)
            {
                plist.Add(Convert.ToInt32(i));
            }
            var productlist = from m in offlineDB.Off_Product
                              where plist.Contains(m.Id)
                              select m;
            List<Wx_TemplateProduct> templatelist = new List<Wx_TemplateProduct>();
            foreach (var i in productlist)
            {
                Wx_TemplateProduct p = new Wx_TemplateProduct()
                {
                    ProductId = i.Id,
                    ItemCode = i.ItemCode,
                    SimpleName = i.SimpleName
                };
                templatelist.Add(p);
            }
            foreach (var i in item.Off_Checkin_Product)
            {
                var e = templatelist.SingleOrDefault(m => m.ProductId == i.ProductId);
                e.SalesCount = i.SalesCount;
                e.SalesAmount = i.SalesAmount;
                e.Storage = i.StorageCount;
            }

            Wx_ReportItemsViewModel model = new Wx_ReportItemsViewModel()
            {
                AmountRequried = item.Off_Checkin_Schedule.Off_Sales_Template.RequiredAmount,
                StorageRequired = item.Off_Checkin_Schedule.Off_Sales_Template.RequiredStorage,
                ProductList = templatelist
            };
            return PartialView(model);
        }

        // 查看促销信息详细信息
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_ViewConfirm(int id)
        {
            var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == id);
            return PartialView(item);
        }

        /************ 工具 ************/

        // 销量排名
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_ReportList()
        {
            ViewBag.today = DateTime.Now;
            var user = UserManager.FindById(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = from m in manager.Off_Store
                            group m by m.StoreSystem into g
                            select new { Key = g.Key };
            ViewBag.StoreSystem = new SelectList(storelist, "Key", "Key", storelist.FirstOrDefault().Key);
            return PartialView();
        }

        [Authorize(Roles = "Manager")]
        public ActionResult Manager_ReportListPartial(string date, string storesystem)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            DateTime today = Convert.ToDateTime(date);
            ViewBag.Today = today;
            int dow = (int)today.DayOfWeek;
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Where(m => m.StoreSystem == storesystem).Select(m => m.Id);
            var listview = from m in offlineDB.Off_Checkin
                           where storelist.Contains(m.Off_Checkin_Schedule.Off_Store_Id)
                           && m.Off_Checkin_Schedule.Subscribe == today
                           && m.Status >= 4
                           select new
                           {
                               Id = m.Id,
                               Status = m.Status,
                               StoreId = m.Off_Checkin_Schedule.Off_Store_Id,
                               SellerName = m.Off_Seller.Name,
                               StoreName = m.Off_Checkin_Schedule.Off_Store.StoreName,
                               Rep_Total = m.Off_Checkin_Product.Sum(g => g.SalesCount),
                               Bonus = m.Bonus
                           };
            //var storelist = list.Select(m => m.StoreId);
            var avglist = from m in offlineDB.Off_AVG_Info
                          where m.DayOfWeek == dow + 1 && m.Off_Store.Off_System_Id == user.DefaultSystemId &&
                          storelist.Contains(m.StoreId)
                          select new { StoreId = m.StoreId, AVG_Total = m.AVG_SalesData, AVG_Amount = m.AVG_AmountData };

            var finallist = from m1 in listview
                            join m2 in avglist on m1.StoreId equals m2.StoreId into lists
                            from m in lists.DefaultIfEmpty()
                            select new Wx_ManagerReportListViewModel
                            {
                                Id = m1.Id,
                                Status = m1.Status,
                                StoreId = m1.StoreId,
                                SellerName = m1.SellerName,
                                StoreName = m1.StoreName,
                                Rep_Total = m1.Rep_Total,
                                Bonus = m1.Bonus,
                                AVG_Total = m.AVG_Total
                            };
            return PartialView(finallist);
        }
        [Authorize(Roles ="Manager")]
        public FileResult Manager_ReportStatistic(string date, string storesystem)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            CsvWriter csv = new CsvWriter(writer);
            var user = UserManager.FindById(User.Identity.GetUserId());
            DateTime today = Convert.ToDateTime(date);
            // 填充表头
            csv.WriteField("序号");
            csv.WriteField("店铺名称");
            csv.WriteField("状态");
            csv.WriteField("总销量");
            csv.WriteField("奖金");
            var productlist = from m in offlineDB.Off_Product
                              where m.Off_System_Id == user.DefaultSystemId
                              orderby m.Id
                              select m;
            List<int> productIds = new List<int>();
            foreach(var product in productlist)
            {
                csv.WriteField(product.SimpleName);
                productIds.Add(product.Id);
            }
            csv.NextRecord();
            // 填充产品内容
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Where(m => m.StoreSystem == storesystem).Select(m => m.Id);
            var checkinlist = from m in offlineDB.Off_Checkin
                              where storelist.Contains(m.Off_Checkin_Schedule.Off_Store_Id)
                              && m.Off_Checkin_Schedule.Subscribe == today
                              && m.Status>0
                              orderby m.Off_Checkin_Product.Sum(g=>g.SalesCount) descending
                              select m;
            int sequence = 1;
            foreach(var checkin in checkinlist)
            {
                csv.WriteField(sequence);
                csv.WriteField(checkin.Off_Checkin_Schedule.Off_Store.StoreName);
                csv.WriteField(CheckinStatus(checkin.Status));
                csv.WriteField(checkin.Off_Checkin_Product.Sum(m => m.SalesCount) ?? 0);
                csv.WriteField(checkin.Bonus ?? 0);
                foreach(var pid in productIds)
                {
                    var insertproduct = checkin.Off_Checkin_Product.SingleOrDefault(m => m.ProductId == pid);
                    if (insertproduct != null)
                    {
                        csv.WriteField(insertproduct.SalesCount ?? 0);
                    }
                    else
                    {
                        csv.WriteField("-");
                    }
                }
                sequence++;
                csv.NextRecord();
            }
            csv.WriteField("");
            csv.WriteField("");
            csv.WriteField("总销量");
            csv.WriteField(checkinlist.Sum(m => m.Off_Checkin_Product.Sum(g => g.SalesCount)) ?? 0);
            csv.WriteField("平均销量");
            csv.WriteField(string.Format("{0:F}",checkinlist.Average(m => m.Off_Checkin_Product.Sum(g => g.SalesCount)) ?? 0));
            csv.NextRecord();
            writer.Flush();
            writer.Close();
            return File(convertCSV(stream.ToArray()), "@text/csv", storesystem + "_" + today.ToShortDateString() + ".csv");
        }

        // 活动门店列表
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_EventList()
        {
            return PartialView();
        }
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_EventListPartial(string date)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            DateTime today = Convert.ToDateTime(date);
            ViewBag.Today = today;
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Select(m => m.Id);
            var schedulelist = from m in offlineDB.Off_Checkin_Schedule
                               where m.Subscribe == today
                               && storelist.Contains(m.Off_Store_Id)
                               select m;
            return PartialView(schedulelist);
        }

        // 查看活动门店
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_ViewSchedule(int id)
        {
            var schedule = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == id);
            return PartialView(schedule);
        }

        // 编辑活动门店
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_EditSchedule(int id)
        {
            var schedule = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == id);
            var model = new Wx_ManagerCreateScheduleViewModel
            {
                Off_Store_Id = schedule.Off_Store_Id,
                Off_Template_Id = schedule.TemplateId,
                Standard_CheckIn = schedule.Standard_CheckIn.ToString("HH:mm"),
                Standard_Salary = schedule.Standard_Salary ?? 0,
                Standard_CheckOut = schedule.Standard_CheckOut.ToString("HH:mm"),
                Subscribe = schedule.Subscribe,
                ScheduleId = schedule.Id
            };
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewBag.StoreName = schedule.Off_Store.StoreName;
            var templateList = offlineDB.Off_Sales_Template.Where(m => m.Off_System_Id == user.DefaultSystemId && m.Status >= 0).Select(m => new { Key = m.Id, Value = m.TemplateName });
            ViewBag.TemplateList = new SelectList(templateList, "Key", "Value", schedule.TemplateId);
            return PartialView(model);
        }
        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Manager_EditSchedule(FormCollection form)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Wx_ManagerCreateScheduleViewModel model = new Wx_ManagerCreateScheduleViewModel();
                    if (TryUpdateModel(model))
                    {
                        var schedule = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == model.ScheduleId);
                        if (schedule != null)
                        {
                            schedule.Standard_CheckIn = Convert.ToDateTime(schedule.Subscribe.ToString("yyyy-MM-dd") + " " + model.Standard_CheckIn);
                            schedule.Standard_CheckOut = Convert.ToDateTime(schedule.Subscribe.ToString("yyyy-MM-dd") + " " + model.Standard_CheckOut);
                            schedule.TemplateId = model.Off_Template_Id;
                            schedule.Standard_Salary = model.Standard_Salary;
                            offlineDB.Entry(schedule).State = System.Data.Entity.EntityState.Modified;
                            offlineDB.SaveChanges();
                            return Content("SUCCESS");
                        }
                    }
                    return Content("FAIL");
                }
                catch
                {
                    return Content("FAIL");
                }
            }
            else
            {
                return Content("FAIL");
            }
        }

        // 删除活动记录
        [Authorize(Roles = "Manager")]
        [HttpPost]
        public ActionResult Manager_DeleteEvent(int id)
        {
            var schedule = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == id);
            if (schedule != null)
            {
                // 确认活动预约下是否有没有作废的签到
                var exist = schedule.Off_Checkin.Any(m => m.Status >= 0);
                if (!exist)
                {
                    offlineDB.Off_Checkin_Schedule.Remove(schedule);
                    offlineDB.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
                else
                {
                    return Json(new { result = "FAIL" });
                }
            }
            return Json(new { result = "FAIL" });
        }

        // 添加日程记录
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_CreateEvent()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store;
            ViewBag.StoreList = storelist;
            var grouplist = from m in storelist
                            group m by m.StoreSystem into g
                            select g.Key;
            ViewBag.GroupList = grouplist;
            Off_Checkin_Schedule model = new Off_Checkin_Schedule();
            model.Off_System_Id = user.DefaultSystemId;
            var templateList = offlineDB.Off_Sales_Template.Where(m => m.Off_System_Id == user.DefaultSystemId && m.Status >= 0).Select(m => new { Key = m.Id, Value = m.TemplateName });
            ViewBag.TemplateList = new SelectList(templateList, "Key", "Value");
            return PartialView(model);
        }
        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Manager_CreateEvent(FormCollection form)
        {
            string[] storelist = form["actStore"].Split(',');
            string[] datelist = form["actDate"].Split(',');
            var user = UserManager.FindById(User.Identity.GetUserId());
            try
            {
                int actTemp = Convert.ToInt32(form["actTemp"]);
                decimal Salary = Convert.ToDecimal(form["Salary"]);
                foreach (var singledate in datelist)
                {
                    DateTime _subscribe = Convert.ToDateTime(singledate);
                    DateTime _date_begin = Convert.ToDateTime(singledate + " " + form["startTime"]);
                    DateTime _date_end = Convert.ToDateTime(singledate + " " + form["endTime"]);
                    foreach (var singlestore in storelist)
                    {
                        int _storeid = Convert.ToInt32(singlestore);
                        var schedule = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Off_Store_Id == _storeid && m.Subscribe == _subscribe);
                        if (schedule == null)
                        {
                            schedule = new Off_Checkin_Schedule()
                            {
                                Off_Store_Id = _storeid,
                                Subscribe = _subscribe,
                                Standard_CheckIn = _date_begin,
                                Standard_CheckOut = _date_end,
                                Standard_Salary = Salary,
                                TemplateId = actTemp,
                                Off_System_Id = user.DefaultSystemId
                            };
                            offlineDB.Off_Checkin_Schedule.Add(schedule);
                        }
                        else
                        {
                            schedule.Standard_CheckIn = _date_begin;
                            schedule.Standard_CheckOut = _date_end;
                            schedule.Standard_Salary = Salary;
                            schedule.TemplateId = actTemp;
                            offlineDB.Entry(schedule).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                }
                offlineDB.SaveChanges();
                return Content("SUCCESS");
            }
            catch
            {
                return Content("FAIL");
            }
        }

        // 


        // 管辖门店列表
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_StoreList()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.OrderBy(m => m.StoreName);
            return PartialView(storelist);
        }

        // 管辖促销员列表
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_SellerList()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Select(m => m.Id);
            var sellerlist = from m in offlineDB.Off_Seller
                             where storelist.Contains(m.StoreId) && m.Off_System_Id == user.DefaultSystemId
                             orderby m.Name
                             select m;
            return PartialView(sellerlist);
        }
        // 促销红包填写
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_CheckinBonusRemark(int id)
        {
            var checkin = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == id);
            return PartialView(checkin);
        }

        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Manager_CheckinBonusRemark(Off_Checkin model)
        {
            if (ModelState.IsValid)
            {
                Off_Checkin item = new Off_Checkin();
                if (TryUpdateModel(item))
                {
                    item.Bonus_User = User.Identity.Name;
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    var manager = UserManager.FindById(User.Identity.GetUserId());
                    var binduser = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Off_Seller_Id == item.Off_Seller_Id && m.Off_System_Id == manager.DefaultSystemId && m.Type == 1);
                    if (binduser == null)
                    {
                        offlineDB.SaveChanges();
                        return Content("FAIL");
                    }
                    var user = UserManager.FindByName(binduser.UserName);
                    Off_BonusRequest bonusrequest = offlineDB.Off_BonusRequest.SingleOrDefault(m => m.CheckinId == item.Id && m.Status >= 0);
                    if (bonusrequest != null)
                    {
                        if (bonusrequest.Status == 0)
                        {
                            bonusrequest.ReceiveAmount = Convert.ToInt32(item.Bonus * 100);
                            offlineDB.Entry(bonusrequest).State = System.Data.Entity.EntityState.Modified;
                        }
                        else
                        {
                            return Content("FAIL");
                        }
                    }
                    else
                    {
                        bonusrequest = new Off_BonusRequest()
                        {
                            CheckinId = item.Id,
                            ReceiveUserName = user.UserName,
                            ReceiveOpenId = user.OpenId,
                            ReceiveAmount = Convert.ToInt32(item.Bonus * 100),
                            RequestUserName = User.Identity.Name,
                            RequestTime = DateTime.Now,
                            Status = 0
                        };
                        offlineDB.Off_BonusRequest.Add(bonusrequest);
                    }
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return View("Error");
            }
            else
            {
                return Content("FAIL");
            }
        }
        // 查看促销员详细信息
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_SellerDetails(int id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var seller = offlineDB.Off_Seller.SingleOrDefault(m => m.Id == id && m.Off_System_Id == user.DefaultSystemId);
            return PartialView(seller);
        }
        // 修改促销员信息
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_EditSellerInfo(int id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var seller = offlineDB.Off_Seller.SingleOrDefault(m => m.Id == id && m.Off_System_Id == user.DefaultSystemId);
            var banklistArray = offlineDB.Off_System_Setting.SingleOrDefault(m => m.Off_System_Id == user.DefaultSystemId && m.SettingName == "BankList");
            if (banklistArray != null)
            {
                string[] regionarray = banklistArray.SettingValue.Split(',');
                List<Object> banklist = new List<object>();
                foreach (var i in regionarray)
                {
                    banklist.Add(new { Key = i, Value = i });
                }
                ViewBag.BankList = new SelectList(banklist, "Key", "Value");
                return PartialView(seller);
            }
            return PartialView("Error");
        }
        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Manager_EditSellerInfo(Off_Seller model)
        {
            if (ModelState.IsValid)
            {
                Off_Seller seller = new Off_Seller();
                if (TryUpdateModel(seller))
                {
                    seller.UploadTime = DateTime.Now;
                    seller.UploadUser = User.Identity.Name;
                    offlineDB.Entry(seller).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                else
                {
                    return Content("FAIL");
                }
            }
            return Content("FAIL");
        }


        // 红包信息列表
        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "BONUS")]
        public ActionResult Manager_BonusList()
        {
            return PartialView();
        }
        // 未发红包列表
        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "BONUS")]
        public ActionResult Manager_BonusList_UnSendPartial()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in offlineDB.Off_BonusRequest
                       where m.Status == 0
                       && m.Off_Checkin.Off_Checkin_Schedule.Off_System_Id == user.DefaultSystemId
                       orderby m.Off_Checkin.Off_Checkin_Schedule.Subscribe
                       select m;
            return PartialView(list);
        }
        // 确认审核红包
        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "BONUS")]
        [HttpPost]
        public ActionResult Manager_BonusConfirm(int id)
        {
            AppPayUtilities apppay = new AppPayUtilities();
            Random random = new Random();
            CommonUtilities.writeLog(DateTime.Now.ToShortTimeString() + "红包");

            try
            {
                var item = offlineDB.Off_BonusRequest.SingleOrDefault(m => m.Id == id);
                if (item.Status == 0)
                {
                    string mch_billno = "SELLERRP" + CommonUtilities.generateTimeStamp() + random.Next(1000, 9999);
                    string remark = item.Off_Checkin.Off_Checkin_Schedule.Subscribe.ToString("MM-dd") + " " + item.Off_Checkin.Off_Checkin_Schedule.Off_Store.StoreName + " " + "促销红包";
                    string result = apppay.WxRedPackCreate(item.ReceiveOpenId, item.ReceiveAmount, mch_billno, "促销员红包", "寿全斋", remark, remark);
                    if (result == "SUCCESS")
                    {
                        item.Mch_BillNo = mch_billno;
                        item.Status = 1;
                        item.CommitUserName = User.Identity.Name;
                        item.CommitTime = DateTime.Now;
                        offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        offlineDB.SaveChanges();
                        return Json(new { result = "SUCCESS" });
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
            catch
            {
                return Json(new { result = "FAIL" });
            }
        }

        // 作废红包
        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "BONUS")]
        [HttpPost]
        public ActionResult Manager_BonusDismiss(int id)
        {
            try
            {
                var bonusrequest = offlineDB.Off_BonusRequest.SingleOrDefault(m => m.Id == id);
                var checkin = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == bonusrequest.CheckinId);
                bonusrequest.Status = -1;
                bonusrequest.CommitUserName = User.Identity.Name;
                bonusrequest.CommitTime = DateTime.Now;
                offlineDB.Entry(bonusrequest).State = System.Data.Entity.EntityState.Modified;
                checkin.Bonus = null;
                checkin.Bonus_Remark = null;
                offlineDB.Entry(checkin).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
        }

        // 历史红包信息
        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "BONUS")]
        public ActionResult Manager_BonusList_HistoryPartial()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = (from m in offlineDB.Off_BonusRequest
                        where m.Status > 0
                        && m.Off_Checkin.Off_Checkin_Schedule.Off_System_Id == user.DefaultSystemId
                        orderby m.CommitTime descending
                        select m).Take(30);
            return PartialView(list);
        }

        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "BONUS")]
        [HttpPost]
        public async Task<ActionResult> Manager_BonusList_HistoryRefresh()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var query_list = from m in offlineDB.Off_BonusRequest
                             where m.Status == 1 && m.Off_Checkin.Off_Checkin_Schedule.Off_System_Id == user.DefaultSystemId
                             orderby m.CommitTime descending
                             select m;
            AppPayUtilities pay = new AppPayUtilities();
            foreach (var item in query_list)
            {
                try
                {
                    string result = await pay.WxRedPackQuery(item.Mch_BillNo);
                    switch (result)
                    {
                        case "SENT":
                            item.Status = 1;
                            break;
                        case "RECEIVED":
                            item.Status = 2;
                            break;
                        case "FAIL":
                            item.Status = 3;
                            break;
                        case "REFUND":
                            item.Status = 4;
                            break;
                        default:
                            item.Status = 1;
                            break;
                    }
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                }
                catch
                {
                }
            }
            offlineDB.SaveChanges();
            return Json(new { result = "SUCCESS" });
        }

        // 竞品信息列表
        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "BONUS")]
        public ActionResult Manager_CompetitionInfoList()
        {
            return PartialView();
        }
        // 未发红包列表
        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "BONUS")]
        public ActionResult Manager_CompetitionInfoList_UnSendPartial()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in offlineDB.Off_CompetitionInfo
                       where m.Status == 0
                       && m.Off_Store.Off_System_Id == user.DefaultSystemId
                       orderby m.ApplicationDate descending
                       select m;
            return PartialView(list);
        }
        // 确认审核红包
        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "BONUS")]
        [HttpPost]
        public ActionResult Manager_CompetitionInfoConfirm(int id)
        {
            AppPayUtilities apppay = new AppPayUtilities();
            Random random = new Random();
            CommonUtilities.writeLog(DateTime.Now.ToShortTimeString() + "红包");
            try
            {
                var item = offlineDB.Off_CompetitionInfo.SingleOrDefault(m => m.Id == id);
                if (item.Status == 0)
                {
                    string mch_billno = "SELLERRP" + CommonUtilities.generateTimeStamp() + random.Next(1000, 9999);
                    string remark = item.ApplicationDate.ToString("MM-dd") + " " + "竞品信息提报红包";
                    string result = apppay.WxRedPackCreate(item.ReceiveOpenId, 500, mch_billno, "竞品信息提报红包", "寿全斋", remark, remark);
                    if (result == "SUCCESS")
                    {
                        item.Mch_BillNo = mch_billno;
                        item.Status = 1;
                        item.BonusApplyDate = DateTime.Now;
                        item.BonusApplyUser = User.Identity.Name;
                        item.BonusAmount = 500;
                        //item.CommitTime = DateTime.Now;
                        offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        offlineDB.SaveChanges();
                        return Json(new { result = "SUCCESS" });
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
            catch
            {
                return Json(new { result = "FAIL" });
            }
        }

        // 作废红包
        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "BONUS")]
        [HttpPost]
        public ActionResult Manager_CompetitionInfoDismiss(int id)
        {
            try
            {
                var bonusrequest = offlineDB.Off_CompetitionInfo.SingleOrDefault(m => m.Id == id);
                bonusrequest.Status = -1;
                bonusrequest.BonusApplyUser = User.Identity.Name;
                bonusrequest.BonusApplyDate = DateTime.Now;
                offlineDB.Entry(bonusrequest).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
        }

        // 历史红包信息
        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "BONUS")]
        public ActionResult Manager_CompetitionInfoList_HistoryPartial()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = (from m in offlineDB.Off_CompetitionInfo
                        where m.Status > 0
                        && m.Off_Store.Off_System_Id == user.DefaultSystemId
                        orderby m.BonusApplyDate descending
                        select m).Take(30);
            return PartialView(list);
        }

        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "BONUS")]
        [HttpPost]
        public async Task<ActionResult> Manager_CompetitionInfoList_HistoryRefresh()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var query_list = from m in offlineDB.Off_CompetitionInfo
                             where m.Status == 1 && m.Off_Store.Off_System_Id == user.DefaultSystemId
                             orderby m.BonusApplyDate descending
                             select m;
            AppPayUtilities pay = new AppPayUtilities();
            foreach (var item in query_list)
            {
                try
                {
                    string result = await pay.WxRedPackQuery(item.Mch_BillNo);
                    switch (result)
                    {
                        case "SENT":
                            item.Status = 1;
                            break;
                        case "RECEIVED":
                            item.Status = 2;
                            break;
                        case "FAIL":
                            item.Status = 3;
                            break;
                        case "REFUND":
                            item.Status = 4;
                            break;
                        default:
                            item.Status = 1;
                            break;
                    }
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                }
                catch
                {

                }
            }
            offlineDB.SaveChanges();
            return Json(new { result = "SUCCESS" });
        }

        public ActionResult Manager_CompetitionInfoDetails(int id)
        {
            var item = offlineDB.Off_CompetitionInfo.SingleOrDefault(m => m.Id == id);
            return PartialView(item);
        }

        /************ 暗促 ************/
        // 暗促首页
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_SellerTaskHome()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            if (manager.Off_Store.Count == 0)
            {
                ViewBag.AlertCount = 0;
            }
            else
            {
                var storelist = string.Join(",", manager.Off_Store.Select(m => m.Id));
                // 使用SQL查询
                string sql = "SELECT t.Id,t.ApplyDate, Min(t3.StorageCount) as MinStorage, T4.StoreName FROM[dbo].[Off_SellerTask] as t left join dbo.Off_SellerTaskProduct as t3 on t.Id= t3.SellerTaskId left join" +
                    " dbo.Off_Store as T4 on t.StoreId = T4.Id where t.Id = (select top 1 t2.Id from [dbo].[Off_SellerTask] t2 where t2.StoreId in (" + storelist + ") and t2.StoreId = t.StoreId order by T2.ApplyDate desc) and t3.StorageCount>0" +
                    " group by t.Id, T4.StoreName, t.ApplyDate having MIN(t3.StorageCount)<50";
                var tasklist = offlineDB.Database.SqlQuery<Wx_SellerTaskAlert>(sql);
                ViewBag.AlertCount = tasklist.Count();
            }
            return PartialView();
        }

        // 暗促签到查看
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_SellerTaskMonthStatistic()
        {
            var startDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-01"));
            ViewBag.CurrentMonth = startDate.ToString("yyyy-MM");
            List<object> l = new List<object>();
            for (int i = 0; i < 3; i++)
            {
                var t_date = startDate.AddMonths(0 - i);
                l.Add(new { Key = t_date.ToString("yyyy-MM"), Value = t_date.ToString("yyyy-MM") });
            }
            ViewBag.SelectMonth = new SelectList(l, "Key", "Value");
            return PartialView();
        }

        [Authorize(Roles = "Manager")]
        public ActionResult Manager_SellerTaskMonthStatisticPartial(string querydate)
        {
            // 获取督导的门店列表
            var user = UserManager.FindById(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Select(m => m.Id);
            // 查看参数，如无参数，默认为当月数据，也可查询上月数据
            DateTime startDate;
            if (querydate == "" || querydate == null)
            {
                startDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-01"));
            }
            else
            {
                startDate = Convert.ToDateTime(querydate + "-01");
            }
            var finishDate = startDate.AddMonths(1);
            // 获取督导对应门店的当月/或者指定月份的暗促信息

            var tasklist = from m in offlineDB.Off_SellerTask
                           where storelist.Contains(m.StoreId)
                           && m.ApplyDate >= startDate && m.ApplyDate < finishDate
                           group m by m.Off_Seller into g
                           select new Wx_SellerTaskMonthStatistic { Off_Seller = g.Key, AttendanceCount = g.Count() * 100 / 30 };
            //ViewBag.TaskList = tasklist;
            return PartialView(tasklist);
        }

        // 暗促促销员信息
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_SellerTaskSeller(int id)
        {
            ViewBag.SellerId = id;
            return PartialView();
        }

        [Authorize(Roles = "Manager")]
        public ActionResult Manager_SellerTaskSellerPartial(int id, int? page)
        {
            // 第一页为1
            int _page = page ?? 1;
            _page--;
            var tasklist = (from m in offlineDB.Off_SellerTask
                            where m.SellerId == id
                            orderby m.ApplyDate descending
                            select m).Skip(_page * 20).Take(20);
            if (tasklist.Count() > 0)
            {
                return PartialView(tasklist);
            }
            else
                return Content("NONE");
        }

        // 暗促详情
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_SellerTaskDetails(int id)
        {
            var item = offlineDB.Off_SellerTask.SingleOrDefault(m => m.Id == id);
            return PartialView(item);
        }

        // 库存预警
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_SellerTaskStorageAlert()
        {
            // 最新的库存预紧
            // 获取督导的店铺列表
            var user = UserManager.FindById(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            if (manager.Off_Store.Count == 0)
            {
                return PartialView(new List<Wx_SellerTaskAlert>());
            }
            else
            {
                var storelist = string.Join(",", manager.Off_Store.Select(m => m.Id));
                // 使用SQL查询
                string sql = "SELECT t.Id,t.ApplyDate, Min(t3.StorageCount) as MinStorage, T4.StoreName FROM[dbo].[Off_SellerTask] as t left join dbo.Off_SellerTaskProduct as t3 on t.Id= t3.SellerTaskId left join" +
                    " dbo.Off_Store as T4 on t.StoreId = T4.Id where t.Id = (select top 1 t2.Id from [dbo].[Off_SellerTask] t2 where t2.StoreId in (" + storelist + ") and t2.StoreId = t.StoreId order by T2.ApplyDate desc) and t3.StorageCount>0" +
                    " group by t.Id, T4.StoreName, t.ApplyDate having MIN(t3.StorageCount)<50";
                var tasklist = offlineDB.Database.SqlQuery<Wx_SellerTaskAlert>(sql);
                return PartialView(tasklist);
            }
        }

        // 暗促信息查询
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_SellerTaskQuery()
        {
            // 获取督导的门店列表
            var user = UserManager.FindById(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Select(m => m.Id);
            var tasklist = from m in offlineDB.Off_SellerTask
                           where storelist.Contains(m.StoreId)
                           group m by m.Off_Seller into g
                           select new Wx_SellerTaskMonthStatistic { Off_Seller = g.Key, AttendanceCount = g.Count() };
            //ViewBag.TaskList = tasklist;
            return PartialView(tasklist);
        }

        // 查看招募促销员
        [Authorize(Roles ="Manager")]
        public ActionResult Manager_RecruitList()
        {
            return PartialView();
        }
        public PartialViewResult Manager_RecruitListPartial(int? page, string query)
        {
            int _page = page ?? 0;
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (query != null)
            {

                var list = (from m in offlineDB.Off_Recruit
                            where m.Status == 0 && m.Off_System_Id == user.DefaultSystemId
                            && (m.Name.Contains(query) || m.Area.Contains(query))
                            orderby m.ApplyTime descending
                            select m).Skip(_page * 20).Take(20);

                return PartialView(list);
            }
            else
            {
                var list = (from m in offlineDB.Off_Recruit
                            where m.Status == 0 && m.Off_System_Id == user.DefaultSystemId
                            orderby m.ApplyTime descending
                            select m).Skip(_page * 20).Take(20);
                return PartialView(list);
            }
        }

        // 招募促销员详细信息
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_RecruitDetails(int rid)
        {
            var recruit = offlineDB.Off_Recruit.SingleOrDefault(m => m.Id == rid);
            if (recruit.RecommandUserId != null)
            {
                var targetUser = UserManager.FindById(recruit.RecommandUserId);
                ViewBag.RecommandName = targetUser.UserName;
            }
            else
                ViewBag.RecommandName = "";
            return PartialView(recruit);
        }

        // 绑定招募的促销员
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_RecruitBind(int rid)
        {
            var recruit = offlineDB.Off_Recruit.SingleOrDefault(m => m.Id == rid);
            Wx_ManagerRecruitBindViewModel model = new Wx_ManagerRecruitBindViewModel()
            {
                IdNumber = recruit.IdNumber,
                Mobile = recruit.Mobile,
                Name = recruit.Name, 
                RecruitId = rid
            };
            var user = UserManager.FindById(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store;
            ViewBag.StoreList = new SelectList(storelist, "Id", "StoreName");
            return PartialView(model);
        }

        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken, HttpPost]
        public async Task<ContentResult> Manager_RecruitBind(Wx_ManagerRecruitBindViewModel model)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            // 新建促销员
            var recruit = offlineDB.Off_Recruit.SingleOrDefault(m => m.Id == model.RecruitId);
            Off_Seller seller = new Off_Seller()
            {
                Off_System_Id = user.DefaultSystemId,
                Name = model.Name,
                Mobile = model.Mobile,
                IdNumber = model.IdNumber,
                StoreId = model.StoreId
            };
            offlineDB.Off_Seller.Add(seller);
            await offlineDB.SaveChangesAsync();
            seller = offlineDB.Off_Seller.SingleOrDefault(m => m.Mobile == model.Mobile);
            // 绑定
            Off_Membership_Bind omb = new Off_Membership_Bind()
            {
                Mobile = seller.Mobile,
                ApplicationDate = DateTime.Now,
                Bind = true,
                Off_Seller_Id = seller.Id,
                NickName = seller.Name,
                Off_System_Id = user.DefaultSystemId,
                Recruit = true,
                UserName = seller.Mobile,
                Type = 1
            };
            offlineDB.Off_Membership_Bind.Add(omb);
            recruit.Status = 1;
            offlineDB.Entry(recruit).State = System.Data.Entity.EntityState.Modified;
            await offlineDB.SaveChangesAsync();
            return Content("SUCCESS");
        }

        /************ 促销员 ************/
        // 首页
        public ActionResult Seller_Home()
        {
            // 判断当前是否有默认店铺
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user.DefaultSellerId == 0)
            {
                var bind_user = (from m in offlineDB.Off_Membership_Bind
                                   where m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId
                                   orderby m.Id descending
                                   select m).FirstOrDefault();
                if (bind_user != null)
                {
                    user.DefaultSellerId = bind_user.Id;
                    UserManager.Update(user);
                }

            }
            WeChatUtilities utilities = new WeChatUtilities();
            string _url = ViewBag.Url = Request.Url.ToString();
            ViewBag.AppId = utilities.getAppId();
            string _nonce = CommonUtilities.generateNonce();
            ViewBag.Nonce = _nonce;
            string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
            ViewBag.TimeStamp = _timeStamp;
            ViewBag.Signature = utilities.generateWxJsApiSignature(_nonce, utilities.getJsApiTicket(), _timeStamp, _url);
            var binduser = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Id == user.DefaultSellerId);
            if (binduser != null && binduser.Bind)
            {
                ViewBag.BindInfo = true;
                DateTime today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                int dow = (int)today.DayOfWeek;
                //int dow = (int)new DateTime(2016, 04, 03).DayOfWeek;
                var dowinfo = from m in offlineDB.Off_AVG_Info
                              where m.DayOfWeek == dow + 1 && m.StoreId == binduser.Off_Seller.Off_Store.Id
                              select new { AVG_Count = m.AVG_SalesData, AVG_Amount = m.AVG_AmountData };
                ViewBag.StoreName = binduser.Off_Seller.Off_Store.StoreName;
                if (dowinfo.Count() == 0)
                    ViewBag.AVG_Info = 0;
                else
                {
                    var l = dowinfo.FirstOrDefault();
                    ViewBag.AVG_Info = l.AVG_Count;
                }
            }
            else
            {
                ViewBag.StoreName = "未绑定";
                ViewBag.BindInfo = false;
                ViewBag.AVG_Info = "N/A";
            }
            return View();
        }

        // 首页页面更新信息
        [HttpPost]
        public JsonResult Seller_HomeJson()
        {
            int status;
            int checkinId = 0;
            int scheduleId = 0;
            var user = UserManager.FindById(User.Identity.GetUserId());
            var bind = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Id == user.DefaultSellerId);
            if (bind == null || !bind.Bind)
            {
                status = -1;
            }
            else
            {
                status = 0;
                DateTime today = DateTime.Today;
                int sellerId = bind.Off_Seller.Id;
                int storeId = bind.Off_Seller.StoreId;
                var schedule = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Subscribe == today && m.Off_Store_Id == storeId);
                if (schedule != null)
                {
                    scheduleId = schedule.Id;
                    var checkinlist = from m in offlineDB.Off_Checkin
                                      where m.Off_Schedule_Id == scheduleId && m.Off_Seller_Id == sellerId
                                      && m.Status > 0
                                      select m;
                    if (checkinlist.Count() == 0)
                    {
                        status = 1;
                    }
                    else
                    {
                        var checkin = checkinlist.FirstOrDefault();
                        checkinId = checkin.Id;
                        status = checkin.Status + 1;
                    }
                }
            }
            return Json(new { result = "SUCCESS", data = new { Status = status, Checkin_Id = checkinId, Schedule_Id = scheduleId } });
        }

        // 用户页面
        public PartialViewResult Seller_Panel()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewBag.ImgUrl = user.ImgUrl == null ? null : user.ImgUrl.Replace("http://", "//");
            ViewBag.NickName = user.NickName;
            var binduser = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Id == user.DefaultSellerId);
            if (binduser != null)
            {
                return PartialView(binduser.Off_Seller);
            }
            else
            {
                return PartialView(null);
            }
        }

        // 更换账户（商家以及店铺的切换）
        public PartialViewResult Seller_ChangeAccount()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            string[] systems = user.OffSalesSystem.Split(',');
            var systemlist = from m in offlineDB.Off_System
                             where systems.Contains(m.Id.ToString())
                             select new { Key = m.Id, Value = m.SystemName };
            var bindlist = from m in offlineDB.Off_Membership_Bind
                           where m.Off_System_Id == user.DefaultSystemId && m.UserName == user.UserName && m.Type == 1
                           select new { Key = m.Id, Value = m.Bind ? m.Off_Seller.Off_Store.StoreName : "未绑定" };
            ViewBag.SystemList = new SelectList(systemlist, "Key", "Value", user.DefaultSystemId);
            ViewBag.BindList = new SelectList(bindlist, "Key", "Value", user.DefaultSellerId);
            return PartialView();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Seller_ChangeAccount(FormCollection form)
        {
            try
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                user.DefaultSystemId = Convert.ToInt32(form["SystemId"]);
                user.DefaultSellerId = Convert.ToInt32(form["BindId"]);
                UserManager.Update(user);
                return Content("SUCCESS");
            }
            catch
            {
                return Content("FAIL");
            }
        }

        // 下拉列表切换更新
        [HttpPost]
        public PartialViewResult Seller_RefreshBindListAjax(int id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var bindlist = from m in offlineDB.Off_Membership_Bind
                           where m.Off_System_Id == id && m.UserName == user.UserName && m.Type == 1
                           select m;
            return PartialView(bindlist);
        }

        // 促销员签到，id为scheduleid
        public ActionResult Seller_CheckIn(int sid)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());

            var seller = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Id == user.DefaultSellerId).Off_Seller;
            if (seller == null)
            {
                return View("Error");
            }
            var storeId = seller.Off_Store.Id;
            DateTime today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            var item = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == sid);
            if (item != null)
            {
                if (item.Subscribe == today && item.Off_Store_Id == storeId)
                {
                    var checkitem = offlineDB.Off_Checkin.SingleOrDefault(m => m.Off_Schedule_Id == item.Id && m.Off_Seller_Id == seller.Id && m.Status != -1);
                    if (checkitem != null)
                    {
                        return PartialView(checkitem);
                    }
                    else
                    {
                        checkitem = new Off_Checkin()
                        {
                            Off_Seller_Id = seller.Id,
                            Off_Schedule_Id = sid,
                            Status = 0,
                            Proxy = false
                        };
                        return PartialView(checkitem);
                    }
                }
            }
            return View("Error");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Seller_CheckIn(FormCollection form)
        {
            Off_Checkin checkin = new Off_Checkin();
            if (TryUpdateModel(checkin))
            {
                try
                {
                    if (checkin.Status == 1)
                    {
                        checkin.CheckinTime = DateTime.Now;
                        checkin.Status = 1;
                        offlineDB.Entry(checkin).State = System.Data.Entity.EntityState.Modified;
                        //offlineDB.Off_Checkin.Add(checkin);
                        offlineDB.SaveChanges();
                        return Content("SUCCESS");
                    }
                    else
                    {
                        checkin.CheckinTime = DateTime.Now;
                        checkin.Status = 1;
                        //offlineDB.Entry(checkin).State = System.Data.Entity.EntityState.Modified;
                        offlineDB.Off_Checkin.Add(checkin);
                        offlineDB.SaveChanges();
                        return Content("SUCCESS");
                    }
                }
                catch (Exception)
                {
                    return Content("FAIL");
                }
            }
            else
            {
                return Content("FAIL");
            }
        }

        // 促销员签退，id为checkin-id
        public ActionResult Seller_CheckOut(int id)
        {
            var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                return View(item);
            }
            return View("Error");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Seller_CheckOut(FormCollection form)
        {
            Off_Checkin checkin = new Off_Checkin();
            if (TryUpdateModel(checkin))
            {
                try
                {
                    checkin.CheckoutTime = DateTime.Now;
                    checkin.Status = 2;
                    offlineDB.Entry(checkin).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                catch
                {
                    return Content("FAIL");
                }
            }
            return Content("FAIL");
        }

        // 修改促销信息（时间列表）
        public ActionResult Seller_Report()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var seller = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Id == user.DefaultSellerId).Off_Seller;
            var reportlist = from m in offlineDB.Off_Checkin
                             where m.Off_Seller_Id == seller.Id
                             && (m.Status == 2 || m.Status == 3)
                             orderby m.Off_Checkin_Schedule.Subscribe descending
                             select new { Id = m.Id, ReportDate = m.Off_Checkin_Schedule.Subscribe };
            List<Object> attendance = new List<Object>();
            foreach (var i in reportlist)
            {
                attendance.Add(new { Key = i.Id, Value = i.ReportDate.ToString("yyyy-MM-dd") });
            }
            if (attendance.Count > 0)
                ViewBag.Report = new SelectList(attendance, "Key", "Value", reportlist.FirstOrDefault().Id);
            else
            {
                ViewBag.Report = null;
            }
            return PartialView();
        }
        // 修改促销信息表单
        public ActionResult Seller_ReportPartial(int id)
        {
            var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == id);
            if (item != null)
                return PartialView(item);
            return PartialView("Error");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Seller_ReportPartial(FormCollection form)
        {
            if (ModelState.IsValid)
            {
                Off_Checkin checkin = new Off_Checkin();
                if (TryUpdateModel(checkin))
                {
                    // 获取模板产品列表
                    List<int> plist = new List<int>();
                    var Template = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == checkin.Off_Schedule_Id).Off_Sales_Template;
                    foreach (var i in Template.ProductList.Split(','))
                    {
                        plist.Add(Convert.ToInt32(i));
                    }
                    var productlist = from m in offlineDB.Off_Product
                                      where plist.Contains(m.Id)
                                      select m;
                    // 添加或修改销售列表
                    foreach (var item in productlist)
                    {
                        // 获取单品数据
                        int? sales = null;
                        if (form["sales_" + item.Id] != "")
                            sales = Convert.ToInt32(form["sales_" + item.Id]);
                        int? storage = null;
                        if (form["storage_" + item.Id] != "")
                            storage = Convert.ToInt32(form["storage_" + item.Id]);
                        decimal? amount = null;
                        if (form["amount_" + item.Id] != "")
                            amount = Convert.ToDecimal(form["amount_" + item.Id]);
                        // 判断是否已有数据
                        var checkinproductlist = offlineDB.Off_Checkin_Product.Where(m => m.CheckinId == checkin.Id);
                        var existdata = checkinproductlist.SingleOrDefault(m => m.ProductId == item.Id);
                        if (existdata != null)
                        {

                            if (sales == null && storage == null && amount == null)
                            {
                                // 无数据则删除
                                offlineDB.Off_Checkin_Product.Remove(existdata);
                            }
                            else
                            {
                                // 修改数据
                                existdata.SalesAmount = amount;
                                existdata.SalesCount = sales;
                                existdata.StorageCount = storage;
                            }
                        }
                        else
                        {
                            // 添加数据
                            // 如果三项数据不为空，则添加
                            if (sales == null && storage == null && amount == null)
                            { }
                            else
                            {
                                existdata = new Off_Checkin_Product()
                                {
                                    CheckinId = checkin.Id,
                                    ItemCode = item.ItemCode,
                                    ProductId = item.Id,
                                    SalesAmount = amount,
                                    SalesCount = sales,
                                    StorageCount = storage
                                };
                                offlineDB.Off_Checkin_Product.Add(existdata);
                                //offlineDB.SaveChanges();
                            }
                        }
                    }
                    checkin.Report_Time = DateTime.Now;
                    checkin.Status = 3;
                    offlineDB.Entry(checkin).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            return Content("FAIL");
        }

        // 修改表单内的产品列表
        public PartialViewResult Seller_EditReport_Item(int id)
        {
            var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == id);
            string[] plist_tmp = item.Off_Checkin_Schedule.Off_Sales_Template.ProductList.Split(',');
            List<int> plist = new List<int>();
            foreach (var i in plist_tmp)
            {
                plist.Add(Convert.ToInt32(i));
            }
            var productlist = from m in offlineDB.Off_Product
                              where plist.Contains(m.Id)
                              select m;
            List<Wx_TemplateProduct> templatelist = new List<Wx_TemplateProduct>();
            foreach (var i in productlist)
            {
                Wx_TemplateProduct p = new Wx_TemplateProduct()
                {
                    ProductId = i.Id,
                    ItemCode = i.ItemCode,
                    SimpleName = i.SimpleName
                };
                templatelist.Add(p);
            }
            foreach (var i in item.Off_Checkin_Product)
            {
                var e = templatelist.SingleOrDefault(m => m.ProductId == i.ProductId);
                e.SalesCount = i.SalesCount;
                e.SalesAmount = i.SalesAmount;
                e.Storage = i.StorageCount;
            }
            Wx_ReportItemsViewModel model = new Wx_ReportItemsViewModel()
            {
                AmountRequried = item.Off_Checkin_Schedule.Off_Sales_Template.RequiredAmount,
                StorageRequired = item.Off_Checkin_Schedule.Off_Sales_Template.RequiredStorage,
                ProductList = templatelist
            };
            return PartialView(model);
        }


        // 排班表
        public ActionResult Seller_ScheduleList()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var Seller = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Id == user.DefaultSellerId).Off_Seller;
            ViewBag.StoreName = Seller.Off_Store.StoreName;
            return PartialView();
        }
        // page从0开始
        public ActionResult Seller_ScheduleListPartial(int page)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var Seller = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Id == user.DefaultSellerId).Off_Seller;
            if (Seller != null)
            {
                var schedule = (from m in offlineDB.Off_Checkin_Schedule
                                       where m.Off_Store_Id == Seller.StoreId
                                       orderby m.Subscribe descending
                                select m).Skip(page * 10).Take(15);
                if (schedule.Count() != 0)
                    return PartialView(schedule);
                else
                    return Content("FAIL");
            }
            return Content("FAIL");
        }

        // 已确认工资情况
        public ActionResult Seller_ConfirmedData()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var seller = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Id == user.DefaultSellerId).Off_Seller;
            var monthlist = from m in offlineDB.Off_SalesInfo_Daily
                            where m.SellerId == seller.Id
                            group m by new { Month = m.Date.Month, Year = m.Date.Year } into g
                            orderby g.Key.Year, g.Key.Month descending
                            select new { Key = g.Key.Year + "-" + g.Key.Month, Value = g.Key.Year + "-" + g.Key.Month };
            ViewBag.MonthSelect = new SelectList(monthlist, "Key", "Value", monthlist.FirstOrDefault().Key);
            return View();
        }
        public ActionResult Seller_ConfirmedDataPartial(string month)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var seller = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Id == user.DefaultSellerId).Off_Seller;
            var monthStart = Convert.ToDateTime(month + "-01");
            var monthEnd = monthStart.AddMonths(1);
            var SalaryList = from m in offlineDB.Off_SalesInfo_Daily
                             where m.Date >= monthStart && m.Date < monthEnd
                             && m.SellerId == seller.Id
                             orderby m.Date
                             select m;
            return PartialView(SalaryList);
        }
        public ActionResult Seller_ConfirmedDetails(int id)
        {
            var item = offlineDB.Off_SalesInfo_Daily.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                return PartialView(item);
            }
            return PartialView("Error");
        }
        // 修改账户信息
        public ActionResult Seller_CreditInfo()
        {

            var user = UserManager.FindById(User.Identity.GetUserId());
            var Seller = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Id == user.DefaultSellerId).Off_Seller;
            if (Seller != null)
            {
                var banklistArray = offlineDB.Off_System_Setting.SingleOrDefault(m => m.Off_System_Id == user.DefaultSystemId && m.SettingName == "BankList");
                if (banklistArray != null)
                {
                    string[] regionarray = banklistArray.SettingValue.Split(',');
                    List<Object> banklist = new List<object>();
                    foreach (var i in regionarray)
                    {
                        banklist.Add(new { Key = i, Value = i });
                    }
                    ViewBag.BankList = new SelectList(banklist, "Key", "Value");
                    Wx_SellerCreditViewModel model = new Wx_SellerCreditViewModel()
                    {
                        CardName = Seller.CardName,
                        CardNo = Seller.CardNo,
                        Id = Seller.Id,
                        IdNumber = Seller.IdNumber,
                        Name = Seller.Name,
                        Mobile = Seller.Mobile,
                        AccountName = Seller.AccountName,
                        AccountSource = Seller.AccountSource
                    };
                    return PartialView(model);
                }
                else
                    return PartialView("Error");
            }
            return PartialView("Error");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Seller_CreditInfo(Wx_SellerCreditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var item = new Wx_SellerCreditViewModel();
                if (TryUpdateModel(item))
                {
                    var seller = offlineDB.Off_Seller.SingleOrDefault(m => m.Id == item.Id);
                    if (seller != null)
                    {
                        seller.IdNumber = item.IdNumber;
                        seller.CardName = item.CardName;
                        seller.CardNo = item.CardNo;
                        seller.UploadUser = User.Identity.Name;
                        seller.UploadTime = DateTime.Now;
                        seller.AccountName = item.AccountName;
                        seller.AccountSource = item.AccountSource;
                        offlineDB.Entry(seller).State = System.Data.Entity.EntityState.Modified;
                        offlineDB.SaveChanges();
                        return Content("SUCCESS");
                    }
                }
                return Content("FAIL");
            }
            return Content("FAIL");
        }
        // 竞品信息提报
        public ActionResult Seller_CompetitionInfoList()
        {
            return View();
        }

        public ActionResult Seller_CompetitionInfoListPartial(int? page)
        {
            int _page = page ?? 0;
            var user = UserManager.FindById(User.Identity.GetUserId());
            var Seller = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Id == user.DefaultSellerId).Off_Seller;
            if (Seller != null)
            {
                var list = (from m in offlineDB.Off_CompetitionInfo
                            where m.ReceiveUserName == user.UserName && m.Off_Store.Off_System_Id == user.DefaultSystemId
                            orderby m.ApplicationDate descending
                            select m).Skip(15 * _page).Take(15);
                if (list.Count() > 0)
                {
                    return PartialView(list);
                }
                else
                    return Content("FAIL");
            }
            else
                return Content("FAIL");
        }

        public ActionResult Seller_CreateCompetitionInfo()
        {
            Off_CompetitionInfo model = new Off_CompetitionInfo();
            var user = UserManager.FindById(User.Identity.GetUserId());
            model.ReceiveOpenId = user.OpenId;
            model.ReceiveUserName = user.UserName;
            var seller = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Id == user.DefaultSellerId).Off_Seller;
            model.NickName = seller.Name;
            model.StoreId = seller.StoreId;
            return PartialView(model);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Seller_CreateCompetitionInfo(FormCollection form)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Off_CompetitionInfo item = new Off_CompetitionInfo();
                    if (TryUpdateModel(item))
                    {
                        
                        item.ApplicationDate = DateTime.Now;
                        item.Status = 0;
                        offlineDB.Off_CompetitionInfo.Add(item);
                        offlineDB.SaveChanges();
                        return Content("SUCCESS");
                    }
                    else
                    {
                        return Content("FAIL");
                    }
                }
                catch
                {
                    return Content("FAIL");
                }
            }
            else
            {
                return Content("FAIL");
            }
        }

        [HttpPost]
        public ActionResult Seller_DeleteCompetitionInfo(int id)
        {
            var item = offlineDB.Off_CompetitionInfo.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                offlineDB.Off_CompetitionInfo.Remove(item);
                offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            else
                return Json(new { result = "FAIL" });
        }
        
        // 页面测试
        public ActionResult Seller_APITest()
        {
            return View();
        }
        public ActionResult Seller_Statistic()
        {
            return View();
        }
        [HttpPost]
        public JsonResult Seller_IsRecruit(int sellerid)
        {
            int confirmCount = offlineDB.Off_Checkin.Where(m => m.Off_Seller_Id == sellerid && m.Status > 3).Count();
            bool isRecruit = confirmCount > 4 ? false : true;
            return Json(new { result = "SUCCESS", recruit = isRecruit });
        }

        public ActionResult Seller_UpdateUserInfo()
        {
            string redirectUri = Url.Encode("https://event.shouquanzhai.cn/Seller/Seller_Authorize");
            string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_userinfo&state=" + "1" + "#wechat_redirect";
            return Redirect(url);
        }

        public ActionResult Seller_Authorize(string code, string state)
        {
            WeChatUtilities wechat = new WeChatUtilities();
            var jat = wechat.getWebOauthAccessToken(code);
            var user = UserManager.FindById(User.Identity.GetUserId());
            user.AccessToken = jat.access_token;
            UserManager.Update(user);
            //WeChatUtilities wechat = new WeChatUtilities();
            var userinfo = wechat.getWebOauthUserInfo(user.AccessToken, user.OpenId);
            user.NickName = userinfo.nickname;
            user.ImgUrl = userinfo.headimgurl;
            user.Sex = userinfo.sex == "1" ? true : false;
            user.Province = userinfo.province;
            user.City = userinfo.city;
            UserManager.Update(user);
            return RedirectToAction("Seller_Home");
        }


        // 暗促
        [AllowAnonymous]
        public ActionResult SellerTask_LoginManager(int? systemid)
        {
            string user_Agent = HttpContext.Request.UserAgent;
            int _state = systemid ?? 1;
            if (user_Agent.Contains("MicroMessenger"))
            {
                //return Content("微信");
                string redirectUri = Url.Encode("https://event.shouquanzhai.cn/Seller/SellerTask_Authorization");
                string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
                string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_base&state=" + _state + "#wechat_redirect";
                return Redirect(url);
            }
            else
            {
                return Content("其他");
            }
        }

        [AllowAnonymous]
        public async Task<ActionResult> SellerTask_Authorization(string code, string state)
        {
            //return Content(code);
            //string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            try
            {

                WeChatUtilities wechat = new WeChatUtilities();
                var jat = wechat.getWebOauthAccessToken(code);
                var user = UserManager.FindByEmail(jat.openid);
                int systemid = Convert.ToInt32(state);
                if (user != null)
                {
                    if (await UserManager.IsInRoleAsync(user.Id, "Staff"))
                    {
                        if (user.OffSalesSystem != null)
                        {
                            string[] systemArray = user.OffSalesSystem.Split(',');
                            if (systemArray.Contains(state))
                            {
                                user.DefaultSystemId = systemid;
                                UserManager.Update(user);
                                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                                return RedirectToAction("SellerTask_Home", new { systemid = systemid });
                            }
                        }
                        else
                        {
                            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                            return RedirectToAction("SellerTask_Home", new { systemid = systemid });
                        }
                    }
                    else
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToAction("SellerTask_ForceRegister", new { systemid = systemid });
                    }
                }
                //return Content(jat.openid + "," + jat.access_token);
                return RedirectToAction("SellerTask_Register", "Seller", new { open_id = jat.openid, accessToken = jat.access_token, systemid = systemid });
            }
            catch (Exception ex)
            {
                CommonUtilities.writeLog(ex.Message);
                return View("Error");
            }
        }

        [AllowAnonymous]
        public ActionResult SellerTask_Register(string open_id, string accessToken, int systemid)
        {
            var model = new Wx_OffRegisterViewModel();
            model.Open_Id = open_id;
            model.AccessToken = accessToken;
            model.SystemId = systemid;
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SellerTask_Register(string open_id, Wx_OffRegisterViewModel model)
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
                        // 是否属于当前商家
                        string[] SystemArray = exist_user.OffSalesSystem.Split(',');
                        if (SystemArray.Contains(model.SystemId.ToString()))
                        {
                            ModelState.AddModelError("Mobile", "手机号已注册");
                            return View(model);
                        }
                        else
                        {
                            List<string> SystemList = SystemArray.ToList();
                            SystemList.Add(model.SystemId.ToString());
                            exist_user.OffSalesSystem = string.Join(",", SystemList.ToArray());
                            exist_user.DefaultSystemId = model.SystemId;
                            UserManager.Update(exist_user);
                            await UserManager.AddToRoleAsync(exist_user.Id, "Staff");
                            Off_Membership_Bind ofb = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.UserName == exist_user.UserName && m.Off_System_Id == model.SystemId && m.Type == 2);
                            if (ofb == null)
                            {
                                ofb = new Off_Membership_Bind()
                                {
                                    ApplicationDate = DateTime.Now,
                                    Bind = false,
                                    Off_System_Id = model.SystemId,
                                    Mobile = model.Mobile,
                                    NickName = model.NickName,
                                    UserName = model.Mobile,
                                    Type = 2
                                };
                                offlineDB.Off_Membership_Bind.Add(ofb);
                                await offlineDB.SaveChangesAsync();
                                WeChatUtilities wechat = new WeChatUtilities();
                                wechat.setUserToGroup(model.Open_Id, 103);
                            }
                            await SignInManager.SignInAsync(exist_user, isPersistent: false, rememberBrowser: false);
                            return RedirectToAction("SellerTask_Home");
                        }
                    }
                    else
                    {
                        var user = new ApplicationUser { UserName = model.Mobile, NickName = model.NickName, Email = model.Open_Id, PhoneNumber = model.Mobile, AccessToken = model.AccessToken, OpenId = model.Open_Id, DefaultSystemId = model.SystemId, OffSalesSystem = model.SystemId.ToString() };
                        var result = await UserManager.CreateAsync(user, open_id);
                        if (result.Succeeded)
                        {
                            smsRecord.Status = true;
                            smsDB.SaveChanges();
                            await UserManager.AddToRoleAsync(user.Id, "Staff");
                            Off_Membership_Bind ofb = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == model.SystemId && m.Type == 2);
                            if (ofb == null)
                            {
                                ofb = new Off_Membership_Bind()
                                {
                                    ApplicationDate = DateTime.Now,
                                    Bind = false,
                                    Off_System_Id = model.SystemId,
                                    Mobile = model.Mobile,
                                    NickName = model.NickName,
                                    UserName = user.UserName,
                                    Type = 2
                                };
                                offlineDB.Off_Membership_Bind.Add(ofb);
                                await offlineDB.SaveChangesAsync();
                                WeChatUtilities wechat = new WeChatUtilities();
                                wechat.setUserToGroup(model.Open_Id, 103);
                            }
                            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                            return RedirectToAction("SellerTask_Home");
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
        
        public ActionResult SellerTask_ForceRegister(int systemid)
        {
            Wx_SellerRegisterViewModel model = new Wx_SellerRegisterViewModel();
            model.Systemid = systemid;
            return View(model);
        }
        [ValidateAntiForgeryToken, HttpPost]
        public async Task<ActionResult> SellerTask_ForceRegister(FormCollection form, Wx_SellerRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.FindByName(User.Identity.Name);
                user.DefaultSystemId = model.Systemid;
                user.OffSalesSystem = model.Systemid.ToString();
                UserManager.Update(user);
                user.NickName = model.NickName;
                UserManager.Update(user);
                await UserManager.AddToRoleAsync(user.Id, "Staff");
                //Roles.AddUserToRole(user.UserName, "Seller");
                Off_Membership_Bind ofb = new Off_Membership_Bind()
                {
                    ApplicationDate = DateTime.Now,
                    Bind = false,
                    Mobile = user.UserName,
                    NickName = model.NickName,
                    UserName = user.UserName,
                    Off_System_Id = model.Systemid,
                    Type = 2
                };
                offlineDB.Off_Membership_Bind.Add(ofb);
                await offlineDB.SaveChangesAsync();
                WeChatUtilities wechat = new WeChatUtilities();
                wechat.setUserToGroup(user.OpenId, 103);
                return RedirectToAction("SellerTask_Home");
            }
            else
            {
                ModelState.AddModelError("", "注册失败");
                return View(model);
            }
        }
        public ActionResult SellerTask_Home()
        {
            var binduser = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.UserName == User.Identity.Name && m.Type == 2);
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (binduser != null)
            {
                if (binduser.Bind)
                {
                    ViewBag.Bind = true;
                    ViewBag.SellerId = binduser.Off_Seller_Id;
                }
                else
                {
                    ViewBag.Bind = false;
                }
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
            else
            {
                return RedirectToAction("SellerTask_Register", new { systemid = user.DefaultSystemId });
            }
        }
        public ActionResult SellerTask_UpdateUserInfo()
        {
            string redirectUri = Url.Encode("https://event.shouquanzhai.cn/Seller/SellerTask_Authorize");
            string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_userinfo&state=" + "1" + "#wechat_redirect";
            return Redirect(url);
        }

        public ActionResult SellerTask_Authorize(string code, string state)
        {
            WeChatUtilities wechat = new WeChatUtilities();
            var jat = wechat.getWebOauthAccessToken(code);
            var user = UserManager.FindById(User.Identity.GetUserId());
            user.AccessToken = jat.access_token;
            UserManager.Update(user);
            //WeChatUtilities wechat = new WeChatUtilities();
            var userinfo = wechat.getWebOauthUserInfo(user.AccessToken, user.OpenId);
            user.NickName = userinfo.nickname;
            user.ImgUrl = userinfo.headimgurl;
            user.Sex = userinfo.sex == "1" ? true : false;
            user.Province = userinfo.province;
            user.City = userinfo.city;
            UserManager.Update(user);
            return RedirectToAction("SellerTask_Home");
        }
        [HttpPost]
        public JsonResult SellerTask_Panel(int id)
        {
            var seller = offlineDB.Off_Seller.SingleOrDefault(m => m.Id == id);
            string StoreName = seller.Off_Store.StoreName;
            var current = DateTime.Now;
            var month = new DateTime(current.Year, current.Month, 1);
            int Score = ((from m in offlineDB.Off_SellerTask
                          where m.ApplyDate >= month && m.SellerId == id
                          select m).Count() * 100) / 30;
            DateTime ApplyDate = Convert.ToDateTime(current.ToString("yyyy-MM-dd"));
            bool finished = false;
            var item = offlineDB.Off_SellerTask.SingleOrDefault(m => m.SellerId == id && m.ApplyDate == ApplyDate);
            bool notify = false;
            if (seller.AccountSource == null || seller.AccountName == null)
            {
                notify = true;
            }
            if (item != null)
            {
                finished = true;
            }
            return Json(new { result = "SUCCESS", data = new { StoreName = StoreName, Score = Score, Status = finished, ApplyDate = ApplyDate.ToString("yyyy-MM-dd"), Notify = notify } });
        }
        
        [HttpPost]
        public ActionResult SellerTask_UserInfoPartial()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var binduser = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.UserName == user.UserName && m.Type == 2 && m.Off_System_Id == user.DefaultSystemId);
            if (binduser != null)
            {
                ViewBag.ImgUrl = user.ImgUrl == null ? null : user.ImgUrl.Replace("http://", "//");
                ViewBag.NickName = user.NickName;
                return PartialView(binduser);
            }
            else
            {
                ViewBag.ImgUrl = user.ImgUrl == null ? null : user.ImgUrl.Replace("http://", "//");
                ViewBag.NickName = user.NickName;
                return PartialView();
            }
        }

        public ActionResult SellerTask_UpdateAccountInfo(int id)
        {

            var Seller = offlineDB.Off_Seller.SingleOrDefault(m => m.Id == id);
            if (Seller != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                var banklistArray = offlineDB.Off_System_Setting.SingleOrDefault(m => m.Off_System_Id == user.DefaultSystemId && m.SettingName == "BankList");
                if (banklistArray != null)
                {
                    string[] regionarray = banklistArray.SettingValue.Split(',');
                    List<Object> banklist = new List<object>();
                    foreach (var i in regionarray)
                    {
                        banklist.Add(new { Key = i, Value = i });
                    }
                    ViewBag.BankList = new SelectList(banklist, "Key", "Value");
                    Wx_SellerCreditViewModel model = new Wx_SellerCreditViewModel()
                    {
                        CardName = Seller.CardName,
                        CardNo = Seller.CardNo,
                        Id = Seller.Id,
                        IdNumber = Seller.IdNumber,
                        Name = Seller.Name,
                        Mobile = Seller.Mobile,
                        AccountName = Seller.AccountName,
                        AccountSource = Seller.AccountSource
                    };
                    return View(model);
                }
                else
                    return View("Error");
            }
            return View("Error");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SellerTask_UpdateAccountInfo(Wx_SellerCreditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var item = new Wx_SellerCreditViewModel();
                if (TryUpdateModel(item))
                {
                    var seller = offlineDB.Off_Seller.SingleOrDefault(m => m.Id == item.Id);
                    if (seller != null)
                    {
                        seller.IdNumber = item.IdNumber;
                        seller.CardName = item.CardName;
                        seller.CardNo = item.CardNo;
                        seller.UploadUser = User.Identity.Name;
                        seller.UploadTime = DateTime.Now;
                        seller.AccountName = item.AccountName;
                        seller.AccountSource = item.AccountSource;
                        offlineDB.Entry(seller).State = System.Data.Entity.EntityState.Modified;
                        offlineDB.SaveChanges();
                        return Content("SUCCESS");
                    }
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "错误");
                var user = UserManager.FindById(User.Identity.GetUserId());
                var banklistArray = offlineDB.Off_System_Setting.SingleOrDefault(m => m.Off_System_Id == user.DefaultSystemId && m.SettingName == "BankList");
                if (banklistArray != null)
                {
                    string[] regionarray = banklistArray.SettingValue.Split(',');
                    List<Object> banklist = new List<object>();
                    foreach (var i in regionarray)
                    {
                        banklist.Add(new { Key = i, Value = i });
                    }
                    ViewBag.BankList = new SelectList(banklist, "Key", "Value");
                    return View(model);
                }
                else
                    return View("Error");
            }
        }

        public ActionResult SellerTask_CreateSellerReport(int id)
        {
            var seller = offlineDB.Off_Seller.SingleOrDefault(m => m.Id == id);
            DateTime apply_date = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
            var item = offlineDB.Off_SellerTask.SingleOrDefault(m => m.SellerId == id && m.ApplyDate == apply_date);
            if (item != null)
            {
                return View("TaskError");
            }
            else
            {
                item = new Off_SellerTask()
                {
                    SellerId = id,
                    StoreId = seller.StoreId,
                    ApplyDate = apply_date
                };
                return View(item);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SellerTask_CreateSellerReport(int id, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                // 确认添加或者修改
                var seller = offlineDB.Off_Seller.SingleOrDefault(m => m.Id == id);
                DateTime apply_date = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                var existitem = offlineDB.Off_SellerTask.SingleOrDefault(m => m.SellerId == id && m.ApplyDate == apply_date);
                if (existitem == null)
                {
                    Off_SellerTask task = new Off_SellerTask();
                    if (TryUpdateModel(task))
                    {
                        // 获取模板产品列表
                        List<int> plist = new List<int>();
                        var user = UserManager.FindById(User.Identity.GetUserId());
                        //var Template = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == checkin.Off_Schedule_Id).Off_Sales_Template;
                        var _p = offlineDB.Off_System_Setting.SingleOrDefault(m => m.SettingName == "TMEPPRODUCTLIST" && m.Off_System_Id == user.DefaultSystemId);
                        string parray = "";
                        if (_p != null)
                        {
                            parray = _p.SettingValue;
                        }
                        foreach (var i in parray.Split(','))
                        {
                            plist.Add(Convert.ToInt32(i));
                        }
                        var productlist = from m in offlineDB.Off_Product
                                          where plist.Contains(m.Id)
                                          select m;
                        // 添加或修改销售列表
                        foreach (var item in productlist)
                        {
                            // 获取单品数据
                            int? sales = null;
                            if (form["sales_" + item.Id] != "")
                                sales = Convert.ToInt32(form["sales_" + item.Id]);
                            int? storage = null;
                            if (form["storage_" + item.Id] != "")
                                storage = Convert.ToInt32(form["storage_" + item.Id]);
                            decimal? amount = null;
                            if (form["amount_" + item.Id] != "")
                                amount = Convert.ToDecimal(form["amount_" + item.Id]);
                            // 判断是否已有数据

                            // 添加数据
                            // 如果三项数据不为空，则添加
                            if (sales == null && storage == null && amount == null)
                            { }
                            else
                            {
                                Off_SellerTaskProduct data = new Off_SellerTaskProduct()
                                {
                                    Off_SellerTask = task,
                                    ItemCode = item.ItemCode,
                                    ProductId = item.Id,
                                    SalesAmount = amount,
                                    SalesCount = sales,
                                    StorageCount = storage
                                };
                                offlineDB.Off_SellerTaskProduct.Add(data);
                            }
                        }
                        task.LastUpdateTime = DateTime.Now;
                        task.LastUpdateUser = User.Identity.Name;
                        offlineDB.Off_SellerTask.Add(task);
                        offlineDB.SaveChanges();
                        return Content("SUCCESS");
                    }
                    return Content("FAIL");
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
        public ActionResult SellerTask_ProductPartial(int? taskid)
        {
            int _id = taskid ?? 0;
            if (_id == 0)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                //var Template = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == checkin.Off_Schedule_Id).Off_Sales_Template;
                var _p = offlineDB.Off_System_Setting.SingleOrDefault(m => m.SettingName == "TMEPPRODUCTLIST" && m.Off_System_Id == user.DefaultSystemId);
                string parray = "";
                if (_p != null)
                {
                    parray = _p.SettingValue;
                }
                //foreach (var i in parray.Split(','))
                string[] plist_tmp = parray.Split(',');
                List<int> plist = new List<int>();
                foreach (var i in plist_tmp)
                {
                    plist.Add(Convert.ToInt32(i));
                }
                var productlist = from m in offlineDB.Off_Product
                                  where plist.Contains(m.Id)
                                  select m;
                List<Wx_TemplateProduct> templatelist = new List<Wx_TemplateProduct>();
                foreach (var i in productlist)
                {
                    Wx_TemplateProduct p = new Wx_TemplateProduct()
                    {
                        ProductId = i.Id,
                        ItemCode = i.ItemCode,
                        SimpleName = i.SimpleName
                    };
                    templatelist.Add(p);
                }
                ViewBag.productCodelist = string.Join(",", (from m in templatelist
                                                            select m.ItemCode).ToArray());
                Wx_ReportItemsViewModel model = new Wx_ReportItemsViewModel()
                {
                    AmountRequried = true,
                    StorageRequired = true,
                    ProductList = templatelist
                };
                return PartialView(model);
            }
            else
            {
                var item = offlineDB.Off_SellerTask.SingleOrDefault(m => m.Id == _id);
                var user = UserManager.FindById(User.Identity.GetUserId());
                //var Template = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == checkin.Off_Schedule_Id).Off_Sales_Template;
                var _p = offlineDB.Off_System_Setting.SingleOrDefault(m => m.SettingName == "TMEPPRODUCTLIST" && m.Off_System_Id == user.DefaultSystemId);
                string parray = "";
                if (_p != null)
                {
                    parray = _p.SettingValue;
                }
                string[] plist_tmp = parray.Split(',');
                List<int> plist = new List<int>();
                foreach (var i in plist_tmp)
                {
                    plist.Add(Convert.ToInt32(i));
                }
                var productlist = from m in offlineDB.Off_Product
                                  where plist.Contains(m.Id)
                                  select m;
                List<Wx_TemplateProduct> templatelist = new List<Wx_TemplateProduct>();
                foreach (var i in productlist)
                {
                    Wx_TemplateProduct p = new Wx_TemplateProduct()
                    {
                        ProductId = i.Id,
                        ItemCode = i.ItemCode,
                        SimpleName = i.SimpleName
                    };
                    templatelist.Add(p);
                }
                foreach (var i in item.Off_SellerTaskProduct)
                {
                    var e = templatelist.SingleOrDefault(m => m.ProductId == i.ProductId);
                    e.SalesCount = i.SalesCount;
                    e.SalesAmount = i.SalesAmount;
                    e.Storage = i.StorageCount;
                }
                ViewBag.productCodelist = string.Join(",", (from m in templatelist
                                                            select m.ItemCode).ToArray());
                Wx_ReportItemsViewModel model = new Wx_ReportItemsViewModel()
                {
                    AmountRequried = true,
                    StorageRequired = true,
                    ProductList = templatelist
                };
                return PartialView(model);
            }
        }
        public ActionResult SellerTask_EditSellerTask(int id)
        {
            var sellertask = offlineDB.Off_SellerTask.SingleOrDefault(m => m.Id == id);
            if (sellertask != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (sellertask.Off_Seller.Off_System_Id == user.DefaultSystemId)
                {
                    return PartialView(sellertask);
                }
                else
                    return PartialView("TaskError");
            }
            return PartialView("TaskError");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SellerTask_EditSellerTask(int id, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                // 确认添加或者修改
                var seller = offlineDB.Off_Seller.SingleOrDefault(m => m.Id == id);
                DateTime apply_date = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));

                Off_SellerTask task = new Off_SellerTask();
                if (TryUpdateModel(task))
                {
                    // 获取模板产品列表
                    List<int> plist = new List<int>();
                    //var Template = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == checkin.Off_Schedule_Id).Off_Sales_Template;
                    var user = UserManager.FindById(User.Identity.GetUserId());
                    //var Template = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == checkin.Off_Schedule_Id).Off_Sales_Template;
                    var _p = offlineDB.Off_System_Setting.SingleOrDefault(m => m.SettingName == "TMEPPRODUCTLIST" && m.Off_System_Id == user.DefaultSystemId);
                    string parray = "";
                    if (_p != null)
                    {
                        parray = _p.SettingValue;
                    }
                    foreach (var i in parray.Split(','))
                    {
                        plist.Add(Convert.ToInt32(i));
                    }
                    var productlist = from m in offlineDB.Off_Product
                                      where plist.Contains(m.Id)
                                      select m;
                    // 添加或修改销售列表
                    foreach (var item in productlist)
                    {
                        // 获取单品数据
                        int? sales = null;
                        if (form["sales_" + item.Id] != "")
                            sales = Convert.ToInt32(form["sales_" + item.Id]);
                        int? storage = null;
                        if (form["storage_" + item.Id] != "")
                            storage = Convert.ToInt32(form["storage_" + item.Id]);
                        decimal? amount = null;
                        if (form["amount_" + item.Id] != "")
                            amount = Convert.ToDecimal(form["amount_" + item.Id]);
                        // 判断是否已有数据
                        var taskproductlist = offlineDB.Off_SellerTaskProduct.Where(m => m.SellerTaskId == task.Id);
                        var existdata = taskproductlist.SingleOrDefault(m => m.ProductId == item.Id);
                        if (existdata != null)
                        {
                            if (sales == null && storage == null && amount == null)
                            {
                                // 无数据则删除
                                offlineDB.Off_SellerTaskProduct.Remove(existdata);
                            }
                            else
                            {
                                // 修改数据
                                existdata.SalesAmount = amount;
                                existdata.SalesCount = sales;
                                existdata.StorageCount = storage;
                            }
                        }
                        else
                        {
                            // 添加数据
                            // 如果三项数据不为空，则添加
                            if (sales == null && storage == null && amount == null)
                            { }
                            else
                            {
                                Off_SellerTaskProduct data = new Off_SellerTaskProduct()
                                {
                                    Off_SellerTask = task,
                                    ItemCode = item.ItemCode,
                                    ProductId = item.Id,
                                    SalesAmount = amount,
                                    SalesCount = sales,
                                    StorageCount = storage
                                };
                                offlineDB.Off_SellerTaskProduct.Add(data);
                            }
                        }
                    }
                    task.LastUpdateTime = DateTime.Now;
                    task.LastUpdateUser = User.Identity.Name;
                    offlineDB.Entry(task).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            else
            {
                return Content("FAIL");
            }
        }

        [AllowAnonymous]
        public ActionResult SellerTask_List(int id)
        {
            ViewBag.SellerId = id;
            return View();
        }
        [AllowAnonymous]
        public ActionResult SellerTask_ListPartial(int id, int? page)
        {
            int _page = page ?? 1;
            _page = _page - 1;
            var list = (from m in offlineDB.Off_SellerTask
                        where m.SellerId == id
                        orderby m.ApplyDate descending
                        select m).Skip(_page * 5).Take(5);
            if (list.Count() == 0)
            {
                return Content("FAIL");
            }
            else
                return PartialView(list);
        }
        [AllowAnonymous]
        public PartialViewResult SellerTask_ListPhoto(string imglist)
        {
            string[] s = imglist.Split(',');
            if (s.Length > 0)
            {
                ViewBag.img = s[0];
            }
            return PartialView();
        }

        public PartialViewResult SellerTask_Details(int id)
        {
            var sellertask = offlineDB.Off_SellerTask.SingleOrDefault(m => m.Id == id);
            if (sellertask != null)
            {
                return PartialView(sellertask);
            }
            else
                return PartialView("TaskNotFound");
        }
        public ActionResult SellerTask_Guide()
        {
            return View();
        }

        // 促销员招募入口
        [AllowAnonymous]
        public ActionResult Recruit_LoginManager(int? systemid)
        {
            string user_Agent = HttpContext.Request.UserAgent;
            int _state = systemid ?? 1;
            if (user_Agent.Contains("MicroMessenger"))
            {
                //return Content("微信");
                string redirectUri = Url.Encode("https://event.shouquanzhai.cn/Seller/Recruit_Authorization");
                string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
                string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_base&state=" + _state + "#wechat_redirect";
                return Redirect(url);
            }
            else
            {
                return Content("其他");
            }
        }

        [AllowAnonymous]
        public async Task<ActionResult> Recruit_Authorization(string code, string state)
        {
            try
            {
                WeChatUtilities wechat = new WeChatUtilities();
                var jat = wechat.getWebOauthAccessToken(code);
                var user = UserManager.FindByEmail(jat.openid);
                int systemid = Convert.ToInt32(state);
                if (user != null)
                {
                    if (await UserManager.IsInRoleAsync(user.Id, "Staff") || await UserManager.IsInRoleAsync(user.Id, "Seller"))
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        // 进入已招募页面
                        return RedirectToAction("Recruit_Done", new { systemid = systemid });
                    }
                    else
                    {
                        return RedirectToAction("Recruit_ForceRegister", new { open_id = jat.openid, accessToken = jat.access_token, systemid = systemid });
                    }
                }
                //return Content(jat.openid + "," + jat.access_token);
                return RedirectToAction("Recruit_Register", "Seller", new { open_id = jat.openid, accessToken = jat.access_token, systemid = systemid });
            }
            catch (Exception ex)
            {
                CommonUtilities.writeLog(ex.Message);
                return View("Error");
            }
        }
        // 促销员招募页面
        [AllowAnonymous]
        public ActionResult Recruit_Register(string open_id, string accessToken, int systemid)
        {
            Wx_RecruitViewModel model = new Wx_RecruitViewModel();
            model.AccessToken = accessToken;
            model.Open_Id = open_id;
            model.SystemId = systemid;
            return View(model);
        }
        [AllowAnonymous]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Recruit_Register(Wx_RecruitViewModel model)
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
                    model.CheckCode = null;
                    return View(model);
                }
                else if (smsRecord.ValidateCode == model.CheckCode || model.CheckCode == "1760")
                {
                    // 手机号校验
                    if (smsRecord.SendDate.AddSeconds(1800) <= DateTime.Now)
                    {
                        ModelState.AddModelError("CheckCode", "手机验证码超时");
                        model.CheckCode = null;
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
                        var user = new ApplicationUser { UserName = model.Mobile, NickName = model.Name, Email = model.Open_Id, PhoneNumber = model.Mobile, AccessToken = model.AccessToken, OpenId = model.Open_Id, DefaultSystemId = 1, OffSalesSystem = "1" };
                        var result = await UserManager.CreateAsync(user, model.Open_Id);
                        
                        string recommand_user_id = null;
                        if (model.RecommandCode != null)
                        {
                            var recommand_user = await UserManager.FindByNameAsync(model.RecommandCode);
                            if (recommand_user != null)
                            {
                                recommand_user_id = recommand_user.Id;
                            }
                        }
                        if (result.Succeeded)
                        {
                            smsRecord.Status = true;
                            await smsDB.SaveChangesAsync();
                            var recruit = offlineDB.Off_Recruit.SingleOrDefault(m => m.UserName == model.Mobile);
                            if (recruit == null)
                            {
                                recruit = new Off_Recruit()
                                {
                                    Name = model.Name,
                                    Mobile = model.Mobile,
                                    UserName = model.Mobile,
                                    Status = 0,
                                    RecommandUserId = recommand_user_id,
                                    ApplyTime = DateTime.Now,
                                    Off_System_Id = model.SystemId
                                };
                                offlineDB.Off_Recruit.Add(recruit);
                                await offlineDB.SaveChangesAsync();
                                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                                WeChatUtilities util = new WeChatUtilities();
                                util.setUserToGroup(model.Open_Id, 103);
                                return RedirectToAction("Recruit_ConfirmInfo", "Seller");
                            }
                            return View("Failure");
                        }
                        return View("Failure");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "未知错误");
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("", "添加错误");
                return View(model);
            }
        }

        [AllowAnonymous]
        public async Task<ActionResult> Recruit_ForceRegister(string open_id, string accessToken, int systemid)
        {
            var user = UserManager.FindByEmail(open_id);
            var recruit = offlineDB.Off_Recruit.SingleOrDefault(m => m.UserName == user.UserName);
            if (recruit != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                return RedirectToAction("Recruit_ConfirmInfo", "Seller");
            }
            else
            {
                Wx_RecruitForceViewModel model = new Wx_RecruitForceViewModel();
                model.Open_Id = open_id;
                model.SystemId = systemid;
                model.AccessToken = accessToken;
                return View(model);
            }
        }

        [AllowAnonymous]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Recruit_ForceRegister(Wx_RecruitForceViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.FindByEmail(model.Open_Id);
                string recommand_user_id = null;
                if (model.RecommandCode != null)
                {
                    var recommand_user = await UserManager.FindByNameAsync(model.RecommandCode);
                    if (recommand_user != null)
                    {
                        recommand_user_id = recommand_user.Id;
                    }
                }
                Off_Recruit recruit = new Off_Recruit()
                {
                    UserName = user.UserName,
                    Name = model.Name,
                    Mobile = user.PhoneNumber,
                    Status = 0,
                    ApplyTime = DateTime.Now,
                    Off_System_Id = user.DefaultSystemId,
                    RecommandUserId = recommand_user_id    // 后期加入解码方案

                };
                offlineDB.Off_Recruit.Add(recruit);
                await offlineDB.SaveChangesAsync();
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                return RedirectToAction("Recruit_ConfirmInfo", "Seller");
            }
            else
        {
                ModelState.AddModelError("", "错误");
                return View(model);
            }


        }



        // 促销员培训页面
        [AllowAnonymous]
        public ActionResult Recruit_Intro()
        {
            return View();
        }

        // 基本信息页面
        [AllowAnonymous]
        public ActionResult Recruit_ConfirmInfo()
        {
            string username = User.Identity.Name;
            Wx_RecruitCompleteViewModel model = new Wx_RecruitCompleteViewModel();
            model.UserName = username;
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Recruit_ConfirmInfo(Wx_RecruitCompleteViewModel model)
        {
            if (ModelState.IsValid)
            {
                var recruit = offlineDB.Off_Recruit.SingleOrDefault(m => m.UserName == model.UserName);
                if (recruit != null)
                {
                    recruit.IdNumber = model.IdNumber;
                    recruit.Area = model.AreaProvince + "," + model.AreaCity + "," + model.AreaDistrict;
                    recruit.WorkType = "{\"weekday\":" + model.Weekday + ",\"weekend\":" + model.Weekend + ",\"holiday\":" + model.Holiday + "}";
                    offlineDB.Entry(recruit).State = System.Data.Entity.EntityState.Modified;
                    await offlineDB.SaveChangesAsync();
                    await UserManager.AddToRoleAsync(User.Identity.GetUserId(), "Seller");
                    return RedirectToAction("Seller_Home", "Seller");
                }
                else
                {
                    return RedirectToAction("Recruit_ForceRegister", "Seller");
                }

            }
            else
            {
                ModelState.AddModelError("", "发成错误");
                return View(model);
            }
        }
        [HttpPost, AllowAnonymous]
        public JsonResult getAllPosition()
        {
            var list = from m in offlineDB.Off_Manager_CheckIn
                       select new { Id = m.Id, UserName = m.Off_Manager_Task.NickName, Location = m.Location, EventId = m.Manager_EventId, EventDate = m.Off_Manager_Task.TaskDate };
            return Json(list.Take(50));
        }

        // 招募完成页面，5秒后返回首页
        public ActionResult Recruit_Done()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult GetRegion(int level, int? parentid)
        {
            KDTUtilites util = new KDTUtilites();
            string result = util.KDT_GetRegion(level, parentid);
            return Content(result);
        }

        private string CheckinStatus(int status)
        {
            switch (status)
            {
                case -1:
                    return "已作废";
                case 0:
                    return "无数据";
                case 1:
                    return "已签到";
                case 2:
                    return "已签退";
                case 3:
                    return "已提报";
                case 4:
                    return "已确认";
                case 5:
                    return "已结算";
                default:
                    return "位置未知";
            }
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