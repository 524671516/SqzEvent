﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SqzEvent.DAL;
using SqzEvent.Models;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Threading.Tasks;

namespace SqzEvent.Controllers
{
    public class WeChatMiniController : Controller
    {
        // GET: WeChatMini
        private WeChatMiniModels _db = new WeChatMiniModels();
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

        // 登陆后更新Session
        [HttpPost]
        public async Task<JsonResult> getStorageSession(string jscode)
        {
            const string appid = "wx5ced489620d0a558";
            const string appsecret = "de9a4073b7b00217ab0b104704af79c1";
            // 换取session_key, open_id
            string api_url = String.Format("https://api.weixin.qq.com/sns/jscode2session?appid={0}&secret={1}&js_code={2}&grant_type=authorization_code", appid, appsecret, jscode);
            Uri url = new Uri(api_url);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string jsonresult = new StreamReader(response.GetResponseStream()).ReadToEnd();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            WeChatMini_JSCode2Session j2s = serializer.Deserialize<WeChatMini_JSCode2Session>(jsonresult);
            if (j2s.errcode == null)
            {
                try
                {
                    // 判断是否存在对应的open_id，即用户是否首次登陆
                    DateTime loginDate = DateTime.Now;
                    string storage_session = Guid.NewGuid().ToString();
                    CommonUtilities.writeLog(storage_session);
                    var exist_user = _db.WechatUser.SingleOrDefault(m => m.open_id == j2s.openid);
                    if (exist_user != null)
                    {
                        // 非首次登陆，更新session_key, storage_session, lastlogin_time
                        exist_user.session_key = j2s.session_key;
                        exist_user.storage_session = storage_session;
                        exist_user.lastlogin_time = loginDate;
                        _db.Entry(exist_user).State = System.Data.Entity.EntityState.Modified;
                        await _db.SaveChangesAsync();
                        return Json(new { result = "SUCCESS", session = storage_session });
                    }
                    else
                    {
                        // 首次登陆，添加
                        WechatUser item = new WechatUser()
                        {
                            authorize = false,
                            open_id = j2s.openid,
                            lastlogin_time = loginDate,
                            signup_time = loginDate,
                            session_key = j2s.session_key,
                            storage_session = storage_session,
                            user_status = 0,
                            gender = false
                        };
                        _db.WechatUser.Add(item);
                        await _db.SaveChangesAsync();
                        return Json(new { result = "SUCCESS", session = storage_session });
                    }
                }
                catch(Exception ex)
                {
                    return Json(new { result = "FAIL", error = ex.Message });
                }
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }

        // 保存语音
        [HttpPost]
        public async Task<JsonResult> saveVoiceRecord(FormCollection form, string storage_session)
        {
            try
            {
                // 判断用户状态
                var user = getWechatUser(storage_session);
                if (user != null)
                {
                    var files = Request.Files;
                    if (files.Count > 0)
                    {
                        if (files[0].ContentLength > 0)
                        {
                            // 定义本地文件名
                            var filename = Guid.NewGuid().ToString("N");
                            // 保存文件
                            AliOSSUtilities util = new AliOSSUtilities();
                            util.PutObject(files[0].InputStream, "WeChatFiles/" + filename);
                            // 保存到数据库
                            VoiceRecord record = new VoiceRecord()
                            {
                                record_time = DateTime.Now,
                                status = 0,
                                user_id = user.id,
                                voice_path = filename
                            };
                            _db.VoiceRecord.Add(record);
                            await _db.SaveChangesAsync();
                            return Json(new { result = "SUCCESS", localpath = files[0].FileName });
                        }
                    }
                    return Json(new { result = "FAIL", errmsg = "文件无法上传" });
                }
                return Json(new { result = "FAIL", errmsg = "获取用户失败" });
            }
            catch(Exception ex)
            {
                return Json(new { result = "FAIL", errmsg = ex.Message });
            }
        }

        // 读取语音
        [HttpPost]
        public ActionResult getVoiceRecord(int id, string storage_session)
        {
            try
            {
                // 获取用户
                var user = getWechatUser(storage_session);
                if (user != null)
                {
                    var record = _db.VoiceRecord.SingleOrDefault(m => m.id == id);
                    // 判断所属权
                    if (record.user_id == user.id || record.receiver_mobile == user.mobile)
                    {
                        // 读取文件并返回
                        AliOSSUtilities util = new AliOSSUtilities();
                        var obj = util.GetObject("WeChatFiles/" + record.voice_path);
                        return File(obj, "application/octet-stream", record.voice_path);
                    }
                    return HttpNotFound("无法访问文件");
                }
                return HttpNotFound("找不到用户");
            }
            catch
            {
                return HttpNotFound();
            }
        }

        // 个人语言列表
        [HttpPost]
        public JsonResult getUserVoiceRecordList(string storage_session)
        {
            try
            {
                var user = getWechatUser(storage_session);
                if(user != null)
                {
                    var list = from m in _db.VoiceRecord
                               where m.user_id == user.id
                               orderby m.record_time descending
                               select m;
                    return Json(new { result = "SUCCESS", data = list });
                }
                return Json(new { result = "FAIL", errmsg = "无法获取用户信息" });
            }
            catch
            {
                return Json(new { result = "FAIL", errmsg = "无法获取列表" });
            }
        }

        // 收到的语音列表
        [HttpPost]
        public JsonResult getRecievedVoiceRecordList(string storage_session)
        {
            try
            {
                var user = getWechatUser(storage_session);
                if (user != null)
                {
                    if (user.user_status == 1)
                    {
                        var list = from m in _db.VoiceRecord
                                   where m.receiver_mobile == user.mobile
                                   orderby m.record_time descending
                                   select m;
                        return Json(new { result = "SUCCESS", data = list });
                    }
                    else
                    {
                        return Json(new { result = "FAIL", errmsg = "当前用户未绑定" });
                    }
                }
                return Json(new { result = "FAIL", errmsg = "无法获取用户信息" });
            }
            catch
            {
                return Json(new { result = "FAIL", errmsg = "无法获取列表" });
            }
        }


        #region 通用代码
        /// <summary>
        /// 通过storage_session获取当前用户
        /// </summary>
        private WechatUser getWechatUser(string storage_session)
        {
            try
            {
                return _db.WechatUser.SingleOrDefault(m => m.storage_session == storage_session);
            }
            catch
            {
                return null;
            }
        }
        #endregion
    }
}