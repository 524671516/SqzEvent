using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
    }
}