using System;
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
using System.Text;
using System.Security.Cryptography;

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
                if (files[0].ContentLength > 0)
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
                        return Json(new { result = "SUCCESS", session = storage_session, new_user = false });
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
                            gender = 0
                        };
                        _db.WechatUser.Add(item);
                        await _db.SaveChangesAsync();
                        return Json(new { result = "SUCCESS", session = storage_session, new_user = true });
                    }
                }
                catch (Exception ex)
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
        public async Task<JsonResult> saveVoiceRecord(FormCollection form, string storage_session, int duration)
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
                            var filename = Guid.NewGuid().ToString("N")+".silk";
                            // 保存文件
                            AliOSSUtilities util = new AliOSSUtilities();
                            util.PutObject(files[0].InputStream, "WeChatFiles/" + filename);
                            // 保存到数据库
                            VoiceRecord record = new VoiceRecord()
                            {
                                record_time = DateTime.Now,
                                status = 0,
                                duration = duration,
                                user_id = user.id,
                                voice_path = filename
                            };
                            _db.VoiceRecord.Add(record);
                            await _db.SaveChangesAsync();
                            return Json(new { result = "SUCCESS", localpath = files[0].FileName, filename = filename });
                        }
                    }
                    return Json(new { result = "FAIL", errmsg = "文件无法上传" });
                }
                return Json(new { result = "FAIL", errmsg = "获取用户失败" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "FAIL", errmsg = ex.Message });
            }
        }

        // 实时信息
        [HttpPost]
        public JsonResult getRecordCount(string storage_session)
        {
            try
            {
                var user = getWechatUser(storage_session);
                if(user != null)
                {
                    int own_count = (from m in _db.VoiceRecord
                                   where m.status >= 0 && m.user_id == user.id
                                   select m).Count();
                    if (user.user_status == 1)
                    {
                        int receive_count = (from m in _db.VoiceRecord
                                             where m.receiver_mobile == user.mobile && m.status >= 1
                                             select m).Count();
                        return Json(new { result = "SUCCESS", own_count = own_count, receive_count = receive_count });
                    }
                    else
                    {
                        //return Json(new { result = "FAIL", errmsg = "当前用户未绑定" });
                        return Json(new { result = "SUCCESS", own_count = own_count, receive_count = "N/A" });
                    }
                }
                return Json(new { result = "FAIL", errmsg = "无法获取用户信息" });
            }
            catch
            {
                return Json(new { result = "FAIL", errmsg = "无法获取列表" });
            }
        }

        // 读取语音
        public async Task<ActionResult> getVoiceRecord(int id, string storage_session)
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
                        // 如果是收件人，更新状态
                        if(record.receiver_mobile == user.mobile)
                        {
                            record.status = 2;
                            _db.Entry(record).State = System.Data.Entity.EntityState.Modified;
                            await _db.SaveChangesAsync();
                        }
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
                if (user != null)
                {
                    var list = from m in _db.VoiceRecord
                               where m.user_id == user.id && m.status >= 0
                               orderby m.record_time descending
                               select new { id = m.id, title = m.voice_title, path = m.voice_path, status = getVoiceStatus( m.status), duration = m.duration, record_time = m.record_time };
                    return Json(new { result = "SUCCESS", info = list });
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
                                   where m.receiver_mobile == user.mobile && m.status >= 0
                                   orderby m.record_time descending
                                   select new { id = m.id, title = m.voice_title, path = m.voice_path, status = getVoiceStatus(m.status), duration = m.duration, send_time = m.send_time };
                        return Json(new { result = "SUCCESS", info = list });
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

        // 获取用户资料
        [HttpPost]
        public JsonResult getUserAuthorizedInfo(string storage_session)
        {
            try
            {
                var user = getWechatUser(storage_session);
                if (user != null)
                {
                    if (user.authorize)
                    {
                        return Json(new
                        {
                            result = "SUCCESS",
                            info = new
                            {
                                authorize = user.authorize,
                                mobile = user.mobile,
                                user_status = user.user_status,
                                nickname = user.nickname,
                                province = user.province,
                                city = user.city,
                                avatar_url = user.avatar_url,
                                gender = user.gender
                            }
                        });
                    }
                    return Json(new { result = "SUCCESS", info = new { authorize = user.authorize, mobile = user.mobile, user_status = user.user_status } });
                }
                else
                {
                    return Json(new { result = "FAIL", errmsg = "当前用户不存在" });
                }
            }
            catch
            {
                return Json(new { result = "FAIL", errmsg = "发生错误" });
            }
        }

        // 更新用户资料
        [HttpPost]
        public async Task<JsonResult> updateUserAuthorizedInfo(string storage_session, string encryptedData, string iv)
        {
            try
            {
                var user = getWechatUser(storage_session);
                if (user != null)
                {
                    string rawData = DecodeUserInfo(encryptedData, iv, user.session_key);
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    var userinfo = serializer.Deserialize<UserInfoViewModel>(rawData);
                    // 验证是否同一个用户
                    if (userinfo.openId == user.open_id)
                    {
                        user.authorize = true;
                        user.avatar_url = userinfo.avatarUrl;
                        user.nickname = userinfo.nickName;
                        user.province = userinfo.province;
                        user.country = userinfo.country;
                        user.city = userinfo.city;
                        user.gender = userinfo.gender;
                        user.union_id = userinfo.unionId;
                        _db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                        await _db.SaveChangesAsync();
                        return Json(new { result = "SUCCESS", info = new { avatar_url = user.avatar_url, nickname = user.nickname, gender = user.gender, authorize = user.authorize, user_status = user.user_status } });
                    }
                    return Json(new { result = "FAIL", errmsg = "用户校验错误" });
                }
                return Json(new { result = "FAIL", errmsg = "无法找到用户" });
            }
            catch
            {
                return Json(new { result = "FAIL", errmsg = "异常错误" });
            }
        }

        // 获取手机验证码
        [HttpPost]
        public async Task<JsonResult> sendSmsValidate(string mobile)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(mobile, "1[3|4|5|7|8|][0-9]{9}"))
            {
                string validateCode = CommonUtilities.generateDigits(6);
                SmsValidate validatecode = new SmsValidate()
                {
                    mobile = mobile,
                    validate_code = validateCode,
                    send_time = DateTime.Now,
                    status = 1
                };
                try
                {
                    _db.SmsValidate.Add(validatecode);
                    string message = Send_Sms_VerifyCode(mobile, validateCode);
                    await _db.SaveChangesAsync();
                    return Json(new { result = "SUCCESS", info = message });
                }
                catch
                {
                    return Json(new { result = "FAIL", errmsg = "未知错误" });
                }
            }
            else
            {
                return Json(new { result = "FAIL", errmsg = "电话号码格式错误" });
            }
        }

        // 提交手机绑定
        [HttpPost]
        public async Task<JsonResult> submitMobileBind(string storage_session, string mobile, string vcode)
        {
            try
            {
                // 检查验证码
                var sms = (from m in _db.SmsValidate
                           where m.mobile == mobile && m.status == 1
                           orderby m.send_time descending
                           select m).FirstOrDefault();
                if (sms == null)
                {
                    return Json(new { result = "FAIL", errmsg = "手机绑定错误" });
                }
                if (sms.validate_code == vcode)
                {
                    if (sms.send_time.AddSeconds(1800) <= DateTime.Now)
                    {
                        return Json(new { result = "FAIL", errmsg = "验证码超时，请重试" });
                    }
                    var user = getWechatUser(storage_session);
                    if (user != null)
                    {
                        user.user_status = 1;
                        user.mobile = mobile;
                        _db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                        await _db.SaveChangesAsync();
                        return Json(new { result = "SUCCESS" });
                    }
                    else
                    {
                        return Json(new { result = "FAIL", errmsg = "无法找到用户" });
                    }
                }
                else
                {
                    return Json(new { result = "FAIL", errmsg = "验证码错误" });
                }
            }
            catch
            {
                return Json(new { result = "FAIL", errmsg = "未知错误" });
            }
        }

        // 绑定发送信息
        [HttpPost]
        public async Task<JsonResult> submitSendInfo(string storage_session, string filename, string title, string wish, string mobile)
        {
            try
            {
                // 检查用户
                var user = getWechatUser(storage_session);
                if (user != null)
                {
                    // 检查文件
                    var record = _db.VoiceRecord.SingleOrDefault(m => m.voice_path == filename && m.user_id == user.id);
                    if (record != null)
                    {
                        // 保存发送记录
                        record.voice_title = title;
                        record.voice_wish = wish;
                        record.send_time = DateTime.Now;
                        record.receiver_mobile = mobile;
                        record.status = 1;
                        _db.Entry(record).State = System.Data.Entity.EntityState.Modified;
                        await _db.SaveChangesAsync();
                        return Json(new { result = "SUCCESS", errmsg = "" });
                    }
                    else
                    {
                        return Json(new { result = "FAIL", errmsg = "待发送文件不存在" });
                    }
                }
                else
                {
                    return Json(new { result = "FAIL", errmsg = "当前用户不存在" });
                }
            }
            catch
            {
                return Json(new { result = "FAIL", errmsg = "发生错误" });
            }
        }

        // 获取语音资料详细信息
        [HttpPost]
        public JsonResult getVoiceRecordDetails(int id, string storage_session)
        {
            try
            {
                var user = getWechatUser(storage_session);
                if (user != null)
                {
                    var record = _db.VoiceRecord.SingleOrDefault(m => m.id == id);
                    if (record != null)
                    {
                        if (record.user_id == user.id || record.receiver_mobile == user.mobile)
                        {
                            return Json(new
                            {
                                result = "SUCCESS",
                                info = new
                                {
                                    id = record.id,
                                    title = record.voice_title,
                                    wish = record.voice_wish,
                                    path = record.voice_path,
                                    username = record.WechatUser.nickname,
                                    avatarurl = record.WechatUser.avatar_url
                                }
                            });
                        }
                        else
                        {
                            return Json(new { result = "FAIL", errmsg = "用户无法读取指定语音" });
                        }
                    }
                    return Json(new { result = "FAIL", errmsg = "无法获取语音" });
                }
                else
                {
                    return Json(new { result = "FAIL", errmsg = "当前用户不存在" });
                }
            }
            catch
            {
                return Json(new { result = "FAIL", errmsg = "发生错误" });
            }
        }

        // 删除语音资料
        [HttpPost]
        public async Task<JsonResult> deleteVoiceRecord(int id, string storage_session)
        {
            try
            {
                var user = getWechatUser(storage_session);
                if (user != null)
                {
                    var record = _db.VoiceRecord.SingleOrDefault(m => m.id == id);
                    if (record != null)
                    {
                        if (record.user_id == user.id || record.receiver_mobile == user.mobile)
                        {
                            record.status = -1;
                            _db.Entry(record).State = System.Data.Entity.EntityState.Modified;
                            await _db.SaveChangesAsync();
                            return Json(new { result = "SUCCESS", errmsg = "" });
                        }
                        else
                        {
                            return Json(new { result = "FAIL", errmsg = "用户无法删除指定语音" });
                        }
                    }
                    return Json(new { result = "FAIL", errmsg = "无法获取语音" });
                }
                else
                {
                    return Json(new { result = "FAIL", errmsg = "当前用户不存在" });
                }
            }
            catch
            {
                return Json(new { result = "FAIL", errmsg = "发生错误" });
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

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        private string Send_Sms_VerifyCode(string mobile, string code)
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

        /// <summary>
        /// 解密用户信息
        /// </summary>
        /// <param name="encryptedData"></param>
        /// <param name="iv"></param>
        /// <param name="session_key"></param>
        /// <returns></returns>
        private static string DecodeUserInfo(string encryptedData, string iv, string session_key)
        {
            byte[] iv2 = Convert.FromBase64String(iv);

            if (string.IsNullOrEmpty(encryptedData)) return "";
            Byte[] toEncryptArray = Convert.FromBase64String(encryptedData);

            System.Security.Cryptography.RijndaelManaged rm = new System.Security.Cryptography.RijndaelManaged
            {
                Key = Convert.FromBase64String(session_key),
                IV = iv2,
                Mode = System.Security.Cryptography.CipherMode.CBC,
                Padding = System.Security.Cryptography.PaddingMode.PKCS7
            };
            System.Security.Cryptography.ICryptoTransform cTransform = rm.CreateDecryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Encoding.UTF8.GetString(resultArray);
        }
        #endregion

        private static string getVoiceStatus(int status)
        {
            switch (status)
            {
                case -1:
                    return "已删除";
                case 0:
                    return "已保存";
                case 1:
                    return "已发送";
                case 2:
                    return "已收听";
                default:
                    return "未知";
            }
        }
    }
}