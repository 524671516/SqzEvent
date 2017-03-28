using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SqzEvent.DAL
{
    public class WeChatMini_JSCode2Session
    {
        public int? errcode { get; set; }
        public string errmsg { get; set; }
        public string openid { get; set; }
        public string session_key { get; set; }
    }
}