using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SqzEvent.Models;
using Microsoft.AspNet.Identity.Owin;
using SqzEvent.DAL;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using SqzEvent.Filters;

namespace SqzEvent.Controllers
{
    public class PromotionController : Controller
    {
        private PromotionModels db = new PromotionModels();
        private AppPay paydb = new AppPay();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public PromotionController()
        {

        }

        public PromotionController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        public ViewResult Start()
        {
            return View();
        }
        public ActionResult Question_Start(int libId)
        {
            string redirectUri = Url.Encode("https://event.shouquanzhai.cn/Promotion/Question_Authorize");
            string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_userinfo&state=" + libId + "#wechat_redirect";
            return Redirect(url);
        }

        public async Task<ActionResult> Question_Authorize(string code, string state)
        {

            int libId = Convert.ToInt32(state);
            WeChatUtilities wechat = new WeChatUtilities();
            var jat = wechat.getWebOauthAccessToken(code);
            var user = db.SurveyedUser.SingleOrDefault(m => m.OpenId == jat.openid && m.QuestionLibId == libId);
            if (user != null)
            {
                //跳转
                return RedirectToAction("Index", new { openId = jat.openid, libId = libId });
            }
            else
            {
                //新增
                var userinfo = wechat.getWebOauthUserInfo(jat.access_token, jat.openid);
                var lib = db.QuestionLib.SingleOrDefault(m => m.Id == libId);
                user = new SurveyedUser()
                {
                    QuestionLibId = libId,
                    SurveyedUserStatus = 0,
                    StartTime = DateTime.Now,
                    OpenId = userinfo.openid,
                    NickName = userinfo.nickname,
                    City = userinfo.city,
                    ImgUrl = userinfo.headimgurl,
                    Province = userinfo.province,
                    Sex = userinfo.sex == "1" ? true : false,
                    LastQuestion = lib.StartQuestionId
                };
                db.SurveyedUser.Add(user);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { openId = jat.openid, libId = libId });
            }
        }
        // GET: Question
        public ActionResult Index(string openId, int libId)
        {
            try
            {
                var surveyedUser = db.SurveyedUser.SingleOrDefault(m => m.OpenId == openId && m.QuestionLibId == libId);
                return View(surveyedUser);
            }
            catch
            {
                return View("NotFound");
            }
        }

        [HttpPost]
        public ActionResult GetResult(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                var DefaultId = Convert.ToInt32(collection["DefaultRouterId"].ToString());
                int _userid = Convert.ToInt32(collection["Id"].ToString());
                var user = db.SurveyedUser.SingleOrDefault(m => m.Id == _userid);
                var userResult = user.UserResult.SingleOrDefault(m => m.QuestionId == Convert.ToInt32(collection["QuestionId"].ToString()));

                if (userResult == null)
                {

                    var newResult = new UserResult();
                    user.LastQuestion = Convert.ToInt32(collection["QuestionId"].ToString());
                    db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    newResult.SurveyedUserId = user.Id;
                    newResult.QuestionId = Convert.ToInt32(collection["QuestionId"].ToString());
                    newResult.QuestionLibId = Convert.ToInt32(collection["QuestionLibId"].ToString());
                    newResult.AnswerContent = collection["UserResult"].ToString();
                    newResult.QuestionTitle = collection["QuestionTitle"].ToString();
                    db.UserResult.Add(newResult);
                    db.SaveChanges();
                    return Content("ADD");
                }
                else
                {
                    user.LastQuestion = Convert.ToInt32(collection["QuestionId"].ToString());
                    userResult.AnswerContent = collection["UserResult"].ToString();
                    userResult.QuestionTitle = collection["QuestionTitle"].ToString();
                    db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(userResult).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return Content("MODIFY");
                }
            }
            return View("NotFound");
        }
        [HttpPost]
        public JsonResult EendorContinue(int _DefaultRouterId, int _UserId, int _QusetionId)
        {
            var user = db.SurveyedUser.SingleOrDefault(m => m.Id == _UserId);
            var res = new JsonResult();
            if (user == null)
            {
                var status = new { status = "FAIL" };
                res.Data = status;
                return res;
            }
            else
            {
                if (_DefaultRouterId == -1)
                {
                    user.SurveyedUserStatus = 1;
                    db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    user.LastQuestion = _QusetionId;
                    db.SaveChanges();
                    var status = new { status = "END" };
                    res.Data = status;
                    return res;
                }
                else
                {
                    var status = new { status = "CONTINUE" };
                    res.Data = status;
                    return res;
                }
            }
        }
        public PartialViewResult QusetionPartial(int DefaultRouterId)
        {
            var question = db.Question.SingleOrDefault(m => m.Id == DefaultRouterId);
            if (question != null)
                return PartialView(question);
            return PartialView("NotFound");
        }

        #region 春糖会 2017
        // 春糖会 登陆
        public ActionResult Tjh_UserAttendanceStart(int type)
        {
            int _type = type == 1? 1 : 0;
            string redirectUri = Url.Encode("https://event.shouquanzhai.cn/Promotion/Tjh_UserAttendance_Authorize");
            string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_base&state=" + _type + "#wechat_redirect";
            return Redirect(url);
        }

        public ActionResult Tjh_UserAttendance_Authorize(string code, string state)
        {

            int type = Convert.ToInt32(state);
            WeChatUtilities wechat = new WeChatUtilities();
            var jat = wechat.getWebOauthAccessToken(code);
            // 报名模式
            if (type == 1)
            {
                return RedirectToAction("Tjh_UserAttendance_Register", new { openid = jat.openid });
            }
            else
            {
                // 签到模式
                return RedirectToAction("Tjh_UserAttendance_Signup", new { openid = jat.openid });
            }
        }

        // 春糖会 报名
        [StatisticsFilter(PageName ="春糖会报名入口", PageURL = "/Promotion/Tjh_UserAttendance_Register")]
        public ActionResult Tjh_UserAttendance_Register(string openid)
        {
            // 确认报名是否存在
            int exist_count = db.Tjh_UserAttendance.Count(m => m.openid == openid);
            if (exist_count > 0)
            {
                return RedirectToAction("Tjh_UserAttendance_Register_Done");
            }
            else
            {
                // 添加页面
                Tjh_UserAttendance model = new Tjh_UserAttendance();
                model.openid = openid;
                WeChatUtilities utilities = new WeChatUtilities();
                string _url = ViewBag.Url = Request.Url.ToString();
                ViewBag.AppId = utilities.getAppId();
                string _nonce = CommonUtilities.generateNonce();
                ViewBag.Nonce = _nonce;
                string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
                ViewBag.TimeStamp = _timeStamp;
                ViewBag.Signature = utilities.generateWxJsApiSignature(_nonce, utilities.getJsApiTicket(), _timeStamp, _url);
                return View(model);
            }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Tjh_UserAttendance_Register(Tjh_UserAttendance model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                // 确认报名是否存在
                int exist_count = db.Tjh_UserAttendance.Count(m => m.openid == model.openid);
                if (exist_count > 0)
                {
                    return RedirectToAction("Tjh_UserAttendance_Register_Done");
                }
                // 添加报名
                Tjh_UserAttendance item = new Tjh_UserAttendance();
                if (TryUpdateModel(item))
                {
                    //item.ConfirmedDatetime = DateTime.Now;
                    item.SignupDatetime = DateTime.Now;
                    item.Confirmed = false;
                    item.Status = 1;
                    db.Tjh_UserAttendance.Add(item);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Tjh_UserAttendance_Register_Success");
                }
                else
                {
                    return View("Tjh_UserAttendance_Error");
                }
            }
            else
            {
                return View(model);
            }
        }
        public ActionResult Tjh_UserAttendance_Register_Success()
        {
            return View();
        }
        public ActionResult Tjh_UserAttendance_Register_Done()
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

        // 春糖会 参会确认
        public async Task<ActionResult> Tjh_UserAttendance_Signup(string openid)
        {
            // 待添加
            var item = db.Tjh_UserAttendance.SingleOrDefault(m => m.openid == openid);
            if (item !=null)
            {
                item.Status = 2;
                item.Confirmed = true;
                item.ConfirmedDatetime = DateTime.Now;
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                await db.SaveChangesAsync();
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
                return RedirectToAction("Tjh_UserAttendance_Signup_Fail");
            }
        }

        public ActionResult Tjh_UserAttendance_Signup_Fail()
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
        #endregion

        #region 通用微信登陆端口
        public ActionResult Wechat_Redirect(string url, string state)
        {
            string redirectUri = Url.Encode(url);
            string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            string redirect_url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_base&state=" + state + "#wechat_redirect";

            return Redirect(redirect_url);
        }
        #endregion

        #region 春糖会 5元领优惠券
        public ActionResult Tjh_WechatPay_Start(string code, string state)
        {
            WeChatUtilities util = new WeChatUtilities();
            Wx_WebOauthAccessToken token = util.getWebOauthAccessToken(code);
            return RedirectToAction("Tjh_WechatPay", new { openid = token.openid });
        }

        public ActionResult Tjh_WechatPay(string openid) 
        {
            ViewBag.OpenId = openid;
            return View();
        }
   
        public ActionResult Tjh_WechatPay_Success(string package)
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult Tjh_WechatPay_SetMoney(string _openId, string body)
        {
            //随机数字，并且生成Prepay
            string appid = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            string mch_id = WeChatUtilities.getConfigValue(WeChatUtilities.MCH_ID);
            //先确认，之后做随机数
            string nonce_str = CommonUtilities.generateNonce();
            string out_trade_no = "WXJSAPI_" + DateTime.Now.Ticks;
            int total_fee = 10;
            try
            {
                AppPayUtilities pay = new AppPayUtilities();
                Wx_OrderResult result = pay.createUnifiedOrder(_openId, body, out_trade_no, total_fee, WeChatUtilities.TRADE_TYPE_JSAPI, "");
                if (result.Result == "SUCCESS")
                {
                    WxPaymentOrder order = new WxPaymentOrder()
                    {
                        Body = body,
                        Time_Start = DateTime.Now,
                        Mch_Id = mch_id,
                        Open_Id = _openId,
                        Trade_No = out_trade_no,
                        Total_Fee = total_fee,
                        Prepay_Id = result.PrepayId,
                        Trade_Status = WeChatUtilities.TRADE_STATUS_CREATE,
                        Trade_Type = WeChatUtilities.TRADE_TYPE_JSAPI
                    };
                    paydb.WxPaymentOrder.Add(order);
                    paydb.SaveChanges();
                    return Json(new { result = "SUCCESS", prepay_id = result.PrepayId, total_fee }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { result = "FAIL", msg = result.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { result = "FAIL", msg = e.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}