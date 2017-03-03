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

namespace SqzEvent.Controllers
{
    public class PromotionController : Controller
    {
        private QuestionModels db = new QuestionModels();
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
        // 春糖会 登陆
        public ActionResult Tjh_UserAttendanceStart(int type)
        {
            int _type = type == 1? 1 : 0;
            string redirectUri = Url.Encode("https://event.shouquanzhai.cn/Promotion/Tjh_UserAttendance_Authorize");
            string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_userinfo&state=" + _type + "#wechat_redirect";
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
        public ActionResult Tjh_UserAttendance_Register(string openid)
        {
            // 待添加
            Tjh_UserAttendance model = new Tjh_UserAttendance();
            model.openid = openid;
            return View(model);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Tjh_UserAttendance_Register(Tjh_UserAttendance model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                Tjh_UserAttendance tjh = new Tjh_UserAttendance();
                if (TryUpdateModel(tjh))
                {
                    tjh.ConfirmedDatetime = DateTime.Now;
                    tjh.SignupDatetime = DateTime.Now;
                    tjh.Status = 1;

                }
                else
                {
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
            return View(model);
        }
        public ActionResult Tjh_UserAttendance_Register_Done()
        {
            return View();
        }

        // 春糖会 参会确认
        public ActionResult Tjh_UserAttendance_Signup(string openid)
        {
            // 待添加
            return View();
        }

        public ActionResult Tjh_UserAttendance_Signup_Fail()
        {
            return View();
        }
    }
}