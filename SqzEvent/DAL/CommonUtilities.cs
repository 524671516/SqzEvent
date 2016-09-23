using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SqzEvent.DAL
{
    public class CommonUtilities
    {
        private static char[] Chars = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'R', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
                                          'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
                                          '1','2','3','4','5','6','7','8','9','0' };
        private static char[] Digits = { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };

        #region 获取当前时间戳
        /// <summary>
        /// 获取当前时间戳
        /// </summary>
        /// <returns>long</returns>
        public static long generateTimeStamp()
        {
            TimeSpan ts = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }
        #endregion


        #region 生成随机字符串(24个字符)
        /// <summary>
        /// 生成随机字符串(24个字符)
        /// </summary>
        /// <returns>随机字符串</returns>
        public static string generateNonce()
        {

            return generateNonce(24);
        }
        #endregion


        #region 生成随机字符串
        /// <summary>
        /// 生成随机字符串
        /// </summary>
        /// <param name="num">制定位数</param>
        /// <returns>随机字符串</returns>
        public static string generateNonce(int num)
        {
            StringBuilder sb = new StringBuilder();
            Random random = new Random((int)generateTimeStamp());
            string code = "";
            random = new Random();
            for (int j = 0; j < num; j++)
            {
                code = code + Chars[random.Next(0, Chars.Length)];
            }
            return code;
        }
        #endregion


        public static string generateDigits(int num)
        {
            StringBuilder sb = new StringBuilder();
            Random random = new Random((int)generateTimeStamp());
            string code = "";
            random = new Random();
            for (int j = 0; j < num; j++)
            {
                code = code + Digits[random.Next(0, Digits.Length)];
            }
            return code;
        }

        #region SHA1加密算法
        /// <summary>
        /// SHA1加密算法
        /// </summary>
        /// <param name="source_data">原文</param>
        /// <returns>密文</returns>
        public static string encrypt_SHA1(string source_data)
        {
            byte[] StrRes = Encoding.Default.GetBytes(source_data);
            HashAlgorithm iSHA = new SHA1CryptoServiceProvider();
            StrRes = iSHA.ComputeHash(StrRes);
            StringBuilder EnText = new StringBuilder();
            foreach (byte iByte in StrRes)
            {
                EnText.AppendFormat("{0:x2}", iByte);
            }
            return EnText.ToString();
        }
        #endregion

        public static void writeLog(string s)
        {
            StreamWriter log = new StreamWriter("d:\\ceshi.txt", true);
            log.WriteLine("time:" + s);
            log.Close();
        }

        #region MD5加密算法
        /// <summary>
        /// MD5加密算法
        /// </summary>
        /// <param name="source_data">原文</param>
        /// <returns>密文</returns>
        public static string encrypt_MD5(String source_data)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(source_data);
            byte[] md5data = md5.ComputeHash(data);
            md5.Clear();
            string str = "";
            for (int i = 0; i < md5data.Length; i++)
            {
                str += md5data[i].ToString("x").PadLeft(2, '0');
            }
            return str;
        }
        #endregion

        

        public static string getFirst(string photos)
        {
            if (photos != "")
            {
                var list = photos.Split(',');
                return list[0];
            }
            return "";
        }
    }

    #region 网页参数格式化帮助类
    /// <summary>
    /// 网页参数格式化帮助类
    /// </summary>
    public class QueryParameter
    {
        /// <summary>
        /// 参数名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 参数值
        /// </summary>
        public string Value { get; set; }
        public QueryParameter(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
        /// <summary>
        /// 格式化参数列表
        /// </summary>
        /// <param name="parameters">参数列表</param>
        /// <returns>格式化后的参数列表</returns>
        public static string NormalizeRequestParameters(IList<QueryParameter> parameters)
        {
            var list = parameters.OrderBy(m => m.Name).ToList();
            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (var item in list)
            {
                sb.AppendFormat("{0}={1}", item.Name, item.Value);
                if (i < list.Count - 1)
                {
                    sb.Append("&");
                }
                i++;
            }
            return sb.ToString();
        }
        public static string NormalizeKDTParameters(IList<QueryParameter> parameters)
        {
            var list = parameters.OrderBy(m => m.Name).ToList();
            StringBuilder sb = new StringBuilder();
            foreach (var item in list)
            {
                sb.AppendFormat("{0}{1}", item.Name, item.Value);
            }
            return sb.ToString();
        }
    }
    public class QueryParameterComparer : IComparer<QueryParameter>
    {
        public int Compare(QueryParameter x, QueryParameter y)
        {
            if (x.Name == y.Name)
            {
                return string.Compare(x.Value, y.Value);
            }
            else
            {
                return string.Compare(x.Name, y.Name);
            }
        }
    }
    #endregion


    public class ValidateCode
    {
        public ValidateCode()
        {
        }
        /// <summary>
        /// 验证码的最大长度
        /// </summary>
        public int MaxLength
        {
            get { return 10; }
        }
        /// <summary>
        /// 验证码的最小长度
        /// </summary>
        public int MinLength
        {
            get { return 1; }
        }
        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="length">指定验证码的长度</param>
        /// <returns></returns>
        public string CreateValidateCode(int length)
        {
            int[] randMembers = new int[length];
            int[] validateNums = new int[length];
            string validateNumberStr = "";
            //生成起始序列值
            int seekSeek = unchecked((int)DateTime.Now.Ticks);
            Random seekRand = new Random(seekSeek);
            int beginSeek = (int)seekRand.Next(0, Int32.MaxValue - length * 10000);
            int[] seeks = new int[length];
            for (int i = 0; i < length; i++)
            {
                beginSeek += 10000;
                seeks[i] = beginSeek;
            }
            //生成随机数字
            for (int i = 0; i < length; i++)
            {
                Random rand = new Random(seeks[i]);
                int pownum = 1 * (int)Math.Pow(10, length);
                randMembers[i] = rand.Next(pownum, Int32.MaxValue);
            }
            //抽取随机数字
            for (int i = 0; i < length; i++)
            {
                string numStr = randMembers[i].ToString();
                int numLength = numStr.Length;
                Random rand = new Random();
                int numPosition = rand.Next(0, numLength - 1);
                validateNums[i] = Int32.Parse(numStr.Substring(numPosition, 1));
            }
            //生成验证码
            for (int i = 0; i < length; i++)
            {
                validateNumberStr += validateNums[i].ToString();
            }
            return validateNumberStr;
        }

        //C# MVC 升级版
        /// <summary>
        /// 创建验证码的图片
        /// </summary>
        /// <param name="containsPage">要输出到的page对象</param>
        /// <param name="validateNum">验证码</param>
        public byte[] CreateValidateGraphic(string validateCode)
        {
            Bitmap image = new Bitmap((int)Math.Ceiling(validateCode.Length * 16.0), 30);
            Graphics g = Graphics.FromImage(image);
            try
            {
                //生成随机生成器
                Random random = new Random();
                //清空图片背景色
                g.Clear(Color.White);
                //画图片的干扰线
                for (int i = 0; i < 25; i++)
                {
                    int x1 = random.Next(image.Width);
                    int x2 = random.Next(image.Width);
                    int y1 = random.Next(image.Height);
                    int y2 = random.Next(image.Height);
                    g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
                }
                Font font = new Font("Arial", 16, (FontStyle.Bold | FontStyle.Italic));
                LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height),
                 Color.Blue, Color.DarkRed, 1.2f, true);
                g.DrawString(validateCode, font, brush, 3, 2);
                //画图片的前景干扰点
                for (int i = 0; i < 20; i++)
                {
                    int x = random.Next(image.Width);
                    int y = random.Next(image.Height);
                    image.SetPixel(x, y, Color.FromArgb(random.Next()));
                }
                //画图片的边框线
                g.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);
                //保存图片数据
                MemoryStream stream = new MemoryStream();
                image.Save(stream, ImageFormat.Jpeg);
                //输出图片流
                return stream.ToArray();
            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
        }

        /// <summary>
        /// 得到验证码图片的长度
        /// </summary>
        /// <param name="validateNumLength">验证码的长度</param>
        /// <returns></returns>
        public static int GetImageWidth(int validateNumLength)
        {
            return (int)(validateNumLength * 16.0);
        }
        /// <summary>
        /// 得到验证码的高度
        /// </summary>
        /// <returns></returns>
        public static double GetImageHeight()
        {
            return 30;
        }

    }

}