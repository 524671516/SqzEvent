using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SqzEvent.DAL;

namespace SqzEvent.Controllers
{
    public class WeChatMiniController : Controller
    {
        // GET: WeChatMini
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult GetResponse()
        {
            return Json(new { data = "content" });
        }
        [HttpPost]
        public ActionResult SaveFiles(FormCollection form)
        {
            var files = Request.Files;
            string msg = string.Empty;
            string error = string.Empty;
            string imgurl;
            if (files.Count > 0)
            {
                if (files[0].ContentLength > 0 )
                {
                    string filename = files[0].FileName;
                    //files[0].SaveAs(Server.MapPath("/Content/checkin-img/") + filename);
                    AliOSSUtilities util = new AliOSSUtilities();
                    util.PutObject(files[0].InputStream, "WeChatFiles/" + filename);
                    msg = "成功! 文件大小为:" + files[0].ContentLength;
                    imgurl = filename;
                    return Json(new { error = error, msg = msg, imgurl = imgurl });
                }
                else
                {
                    error = "文件错误";
                }
            }
            string err_res = "{ error:'" + error + "', msg:'" + msg + "',imgurl:''}";
            return Json(new { error = error, msg = msg, imgurl = "" });
        }
        public ActionResult GetFiles(string filename)
        {
            AliOSSUtilities util = new AliOSSUtilities();
            var obj = util.GetObject("WeChatFiles/" + filename);
            return File(obj, "application/octet-stream", filename);
        }
    }
}