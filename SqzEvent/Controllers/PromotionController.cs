using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SqzEvent.Models;
using Microsoft.AspNet.Identity.Owin;

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
        // GET: Question
        public ActionResult Index(int? UserId, int LibId)
        {
            if (UserId == null)
            {
                return View("NotFound");
            }
            else
            {
                var lib = db.QuestionLib.SingleOrDefault(m => m.Id == LibId);
                var user = db.SurveyedUser.SingleOrDefault(m => m.Id == UserId && m.QuestionLibId == LibId);
                if (lib != null)
                {
                    if (user != null)
                        return View(user);
                }
                else
                {
                    user = new SurveyedUser()
                    {
                        QuestionLibId = LibId,
                        SurveyedUserStatus = 0,
                        StartTime = DateTime.Now,
                        SurveyedUserName = "xya", //待修改
                        LastQuestion = lib.StartQuestionId
                    };
                    db.SurveyedUser.Add(user);
                    db.SaveChanges();
                    return View(user);
                }
            }
            return View("NotFound");
        }
    }
}