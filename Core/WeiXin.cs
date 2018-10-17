using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SiteServer.Plugin;
using SS.Magazine.Model;
using WxPayAPI;

namespace SS.Magazine.Core
{
    public static class WeiXinConfig
    {
        public const string AppID = "wx7d306eb1fef3c5eb";
        public const string MchID = "1512892151";
        public const string APIKey = "qUpaKcqjU5IeMJCF0q1A3AN5DKOaFMyG";
    }

    public static class WeiXinPayController
    {
        //public static object ApiArticles(IRequest context)
        //{
        //    var siteId = context.GetPostInt("siteId");
        //    var contentId = context.GetPostInt("contentId");
        //    var articleId = context.GetPostInt("articleId");

        //    var list = new List<ArticleInfo>();
        //    if (articleId == 0)
        //    {
        //        list = Main.ArticleDao.GetArticleInfoList(siteId, contentId);
        //    }
        //    else
        //    {
        //        var articleInfo = Main.ArticleDao.GetArticleInfo(articleId);
        //        if (articleInfo != null)
        //        {
        //            list.Add(articleInfo);
        //        }
        //    }

        //    return new
        //    {
        //        Articles = list,
        //        IsPurchased = Main.Dao.IsPurchased(siteId, contentId, context.UserName)
        //    };
        //}

        public const string RouteResource = "WeiXinPay";

        public static string PayUrl => $"{Context.PluginApi.GetPluginApiUrl(Main.PluginId)}/{RouteResource}"; //支付处理页面

        public static object Pay(IRequest context)
        {
            var amount = context.GetPostString("amount");
            var detail = context.GetPostString("detail");
            var guid = context.GetPostString("guid");

            var requestXml = WeiXinUtil.BuildRequest(amount, detail, guid);

            var resultXml = WeiXinUtil.Post("https://api.mch.weixin.qq.com/pay/unifiedorder", requestXml);

            var dic = WeiXinUtil.FromXml(resultXml);

            string returnCode;
            dic.TryGetValue("return_code", out returnCode);

            if (returnCode == "SUCCESS")
            {
                var json = new JObject();

                var prepay_id = WeiXinUtil.GetValueFromDic<string>(dic, "prepay_id");
                if (!string.IsNullOrEmpty(prepay_id))
                {
                    var payInfo = JsonConvert.DeserializeObject<WxPayModel>(WeiXinUtil.BuildAppPay(prepay_id));

                    json.Add(new JProperty("appid", payInfo.appid));
                    json.Add(new JProperty("partnerid", payInfo.partnerid));
                    json.Add(new JProperty("prepayid", payInfo.prepayid));
                    json.Add(new JProperty("packagestr", payInfo.package));
                    json.Add(new JProperty("noncestr", payInfo.noncestr));
                    json.Add(new JProperty("timestamp", payInfo.timestamp));
                    json.Add(new JProperty("sign", payInfo.sign));
                    json.Add(new JProperty("code", 0));
                    json.Add(new JProperty("msg", "成功"));
                    return json;
                }
                else
                {
                    json.Add(new JProperty("code", 40028));
                    json.Add(new JProperty("msg", "支付错误:" + WeiXinUtil.GetValueFromDic<string>(dic, "err_code_des")));
                    return json;
                }
            }

            return new Exception("error");
        }
    }

    public static class WeiXinNotifyController
    {
        public const string RouteResource = "WeiXinNotify";

        public static string GetNotifyUrl(string guid)
        {
            return $"http://cms.chinacampus.org/api/plugins/ss.magazine/WeiXinNotify/{guid}";
        }

        public static object Notify(string guid)
        {
            var response = new HttpResponseMessage();

            string requestXml;
            bool isPaied;
            string responseXml;
            NotifyByWeixin(HttpContext.Current.Request, out requestXml, out isPaied, out responseXml);

            var filePath = Path.Combine(Context.PhysicalApplicationPath, "notify-log.txt");
            File.WriteAllText(filePath, $@"
-------------------------
{guid}

{requestXml}

{response}
-------------------------
");
            if (isPaied)
            {
                Main.OrderDao.UpdateIsPaied(guid);
            }

            response.Content = new StringContent(responseXml);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
            response.StatusCode = HttpStatusCode.OK;

            return response;
        }

        public static void NotifyByWeixin(HttpRequest request, out string requestXml, out bool isPaied, out string responseXml)
        {
            isPaied = false;

            WxPayConfig.APPID = WeiXinConfig.AppID;
            WxPayConfig.MCHID = WeiXinConfig.MchID;
            WxPayConfig.KEY = WeiXinConfig.APIKey;
            //WxPayConfig.APPSECRET = config.WeixinAppSecret;

            //=======【商户系统后台机器IP】===================================== 
            /* 此参数可手动配置也可在程序中自动获取
            */
            WxPayConfig.IP = "8.8.8.8";


            //=======【代理服务器设置】===================================
            /* 默认IP和端口号分别为0.0.0.0和0，此时不开启代理（如有需要才设置）
            */
            WxPayConfig.PROXY_URL = "http://10.152.18.220:8080";

            //=======【上报信息配置】===================================
            /* 测速上报等级，0.关闭上报; 1.仅错误时上报; 2.全量上报
            */
            WxPayConfig.REPORT_LEVENL = 1;

            //=======【日志级别】===================================
            /* 日志等级，0.不输出日志；1.只输出错误信息; 2.输出错误和正常信息; 3.输出错误信息、正常信息和调试信息
            */
            WxPayConfig.LOG_LEVENL = 0;

            //接收从微信后台POST过来的数据
            System.IO.Stream s = request.InputStream;
            int count;
            byte[] buffer = new byte[1024];
            StringBuilder builder = new StringBuilder();
            while ((count = s.Read(buffer, 0, 1024)) > 0)
            {
                builder.Append(Encoding.UTF8.GetString(buffer, 0, count));
            }
            s.Flush();
            s.Close();
            s.Dispose();

            requestXml = builder.ToString();

            //Log.Info(GetType().ToString(), "NotifyByWeixin : " + builder);

            //转换数据格式并验证签名
            WxPayData notifyData = new WxPayData();
            try
            {
                notifyData.FromXml(builder.ToString());
            }
            catch (WxPayException ex)
            {
                //若签名错误，则立即返回结果给微信支付后台
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", ex.Message);
                //Log.Error(GetType().ToString(), "Sign check error : " + res.ToXml());
                responseXml = res.ToXml();
                return;
            }

            if (!notifyData.IsSet("return_code") || notifyData.GetValue("return_code").ToString() != "SUCCESS")
            {
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "回调数据异常");
                //Log.Info(GetType().ToString(), "The data WeChat post is error : " + res.ToXml());
                responseXml = res.ToXml();
                return;
            }

            //统一下单成功,则返回成功结果给微信支付后台
            WxPayData data = new WxPayData();
            data.SetValue("return_code", "SUCCESS");
            data.SetValue("return_msg", "OK");

            //Log.Info(GetType().ToString(), "UnifiedOrder success , send data to WeChat : " + data.ToXml());
            isPaied = true;
            responseXml = data.ToXml();
        }

        //public static object Notify(string guid)
        //{
        //    Main.OrderDao.UpdateIsPaied(guid);

        //    //return new
        //    //{
        //    //    Status = "SUCCESS"
        //    //};

        //    var request = HttpContext.Current.Request;

        //    var verifyResult = false;
        //    var requestXml = WeiXinUtil.GetRequestXmlData(request);
        //    var dic = WeiXinUtil.FromXml(requestXml);
        //    var returnCode = WeiXinUtil.GetValueFromDic<string>(dic, "return_code");

        //    if (!string.IsNullOrEmpty(returnCode) && returnCode == "SUCCESS")//通讯成功
        //    {
        //        var result = WeiXinUtil.WePayNotifyValidation(dic);
        //        if (result)
        //        {
        //            var transactionid = WeiXinUtil.GetValueFromDic<string>(dic, "transaction_id");

        //            if (!string.IsNullOrEmpty(transactionid))
        //            {
        //                var queryXml = WeiXinUtil.BuildQueryRequest(transactionid, dic);
        //                var queryResult = WeiXinUtil.Post("https://api.mch.weixin.qq.com/pay/orderquery", queryXml);
        //                var queryReturnDic = WeiXinUtil.FromXml(queryResult);

        //                if (WeiXinUtil.ValidatonQueryResult(queryReturnDic))//查询成功
        //                {
        //                    verifyResult = true;
        //                    var status = WeiXinUtil.GetValueFromDic<string>(dic, "result_code");

        //                    if (!string.IsNullOrEmpty(status) && status == "SUCCESS")
        //                    {
        //                        Main.OrderDao.UpdateIsPaied(guid);

        //                        WeiXinUtil.BuildReturnXml("OK", "成功");
        //                    }
        //                }
        //                else
        //                    WeiXinUtil.BuildReturnXml("FAIL", "订单查询失败");
        //            }
        //            else
        //                WeiXinUtil.BuildReturnXml("FAIL", "支付结果中微信订单号不存在");
        //        }
        //        else
        //            WeiXinUtil.BuildReturnXml("FAIL", "签名失败");
        //    }
        //    else
        //    {
        //        string returnmsg;
        //        dic.TryGetValue("return_msg", out returnmsg);
        //        throw new Exception("异步通知错误：" + returnmsg);
        //    }

        //    return verifyResult;
        //}
    }

    public class WxPayModel
    {
        public string appid { get; set; }
        public string partnerid { get; set; }
        public string prepayid { get; set; }
        public string package { get; set; }
        public string noncestr { get; set; }
        public string timestamp { get; set; }
        public string sign { get; set; }
    }

    public static class WeiXinUtil
    {
        /// <summary>
        /// Gets the request XML data.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public static string GetRequestXmlData(HttpRequest request)
        {
            var stream = request.InputStream;
            int count;
            var buffer = new byte[1024];
            var builder = new StringBuilder();
            while ((count = stream.Read(buffer, 0, 1024)) > 0)
            {
                builder.Append(Encoding.UTF8.GetString(buffer, 0, count));
            }
            stream.Flush();
            stream.Close();

            return builder.ToString();
        }

        /// <summary>
        /// Wes the pay notify validation.
        /// </summary>
        /// <param name="dic">The dic.</param>
        /// <returns></returns>
        public static bool WePayNotifyValidation(SortedDictionary<string, string> dic)
        {
            var sign = GetValueFromDic<string>(dic, "sign");
            if (dic.ContainsKey("sign"))
            {
                dic.Remove("sign");
            }

            var tradeType = GetValueFromDic<string>(dic, "trade_type");
            var preString = CreateURLParamString(dic);

            if (string.IsNullOrEmpty(tradeType))
            {
                var preSignString = preString + "&key=" + WeiXinConfig.APIKey;
                var signString = Sign(preSignString, "utf-8").ToUpper();
                return signString == sign;
            }
            else
                return false;
        }

        /// <summary>
        /// Builds the query request.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="dic">The dic.</param>
        /// <returns></returns>
        public static string BuildQueryRequest(string transactionId, SortedDictionary<string, string> dic)
        {
            var dicParam = CreateQueryParam(transactionId);
            var signString = CreateURLParamString(dicParam);
            var key = WeiXinConfig.APIKey;
            var preString = signString + "&key=" + key;
            var sign = Sign(preString, "utf-8").ToUpper();
            dicParam.Add("sign", sign);

            return BuildForm(dicParam);
        }

        /// <summary>
        /// Creates the query parameter.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <returns></returns>
        private static SortedDictionary<string, string> CreateQueryParam(string transactionId)
        {
            var dic = new SortedDictionary<string, string>
        {
            {"appid", WeiXinConfig.AppID},//公众账号ID
            {"mch_id", WeiXinConfig.MchID},//商户号
            {"nonce_str", Guid.NewGuid().ToString().Replace("-", "")},//随机字符串
            {"transaction_id", transactionId}//微信订单号
        };
            return dic;
        }

        /// <summary>
        /// Builds the return XML.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="returnMsg">The return MSG.</param>
        /// <returns></returns>
        public static string BuildReturnXml(string code, string returnMsg)
        {
            return
                $"<xml><return_code><![CDATA[{code}]]></return_code><return_msg><![CDATA[{returnMsg}]]></return_msg></xml>";
        }

        /// <summary>
        /// Builds the request.
        /// </summary>
        /// <returns></returns>
        public static string BuildRequest(string amount, string detail, string guid)
        {
            var dicParam = CreateParam(amount, detail, guid);

            var signString = CreateURLParamString(dicParam);
            var preString = signString + "&key=" + WeiXinConfig.APIKey;
            var sign = Sign(preString, "utf-8").ToUpper();
            dicParam.Add("sign", sign);

            return BuildForm(dicParam);
        }

        /// <summary>
        /// Generates the out trade no.
        /// </summary>
        /// <returns></returns>
        private static string GenerateOutTradeNo()
        {
            var ran = new Random();
            return $"{WeiXinConfig.MchID}{DateTime.Now:yyyyMMddHHmmss}{ran.Next(999)}";
        }

        /// <summary>
        /// Signs the specified prestr.
        /// </summary>
        /// <param name="prestr">The prestr.</param>
        /// <param name="_input_charset">The input charset.</param>
        /// <returns></returns>
        private static string Sign(string prestr, string _input_charset)
        {
            var sb = new StringBuilder(32);
            MD5 md5 = new MD5CryptoServiceProvider();
            var t = md5.ComputeHash(Encoding.GetEncoding(_input_charset).GetBytes(prestr));
            foreach (var t1 in t)
            {
                sb.Append(t1.ToString("x").PadLeft(2, '0'));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <returns></returns>
        private static SortedDictionary<string, string> CreateParam(string amount, string detail, string guid)
        {
            double dubamount;
            double.TryParse(amount, out dubamount);
            var notify_url = WeiXinNotifyController.GetNotifyUrl(guid); //支付完成后的回调处理页面

            var dic = new SortedDictionary<string, string>
            {
                {"appid", WeiXinConfig.AppID}, //账号ID
                {"mch_id", WeiXinConfig.MchID}, //商户号
                {"nonce_str", Guid.NewGuid().ToString().Replace("-", "")}, //随机字符串
                {"body", detail}, //商品描述
                {"out_trade_no", GenerateOutTradeNo()}, //商户订单号
                {"total_fee", (dubamount * 100).ToString(CultureInfo.InvariantCulture)}, //总金额
                {"spbill_create_ip", HttpContext.Current.Request.UserHostAddress}, //终端IP
                {"notify_url", HttpUtility.UrlEncode(notify_url)}, //通知地址
                {"trade_type", "APP"} //交易类型
            };

            return dic;
        }

        /// <summary>
        /// Creates the URL parameter string.
        /// </summary>
        /// <param name="dicArray">The dic array.</param>
        /// <returns></returns>
        private static string CreateURLParamString(SortedDictionary<string, string> dicArray)
        {
            var prestr = new StringBuilder();
            foreach (var temp in dicArray.OrderBy(o => o.Key))
            {
                prestr.Append(temp.Key + "=" + temp.Value + "&");
            }

            var nLen = prestr.Length;
            prestr.Remove(nLen - 1, 1);
            return prestr.ToString();
        }

        /// <summary>
        /// Builds the form.
        /// </summary>
        /// <param name="dicParam">The dic parameter.</param>
        /// <returns></returns>
        private static string BuildForm(SortedDictionary<string, string> dicParam)
        {
            var sbXML = new StringBuilder();
            sbXML.Append("<xml>");
            foreach (var temp in dicParam)
            {
                sbXML.Append("<" + temp.Key + ">" + temp.Value + "</" + temp.Key + ">");
            }

            sbXML.Append("</xml>");
            return sbXML.ToString();
        }

        /// <summary>
        /// Froms the XML.
        /// </summary>
        /// <param name="xml">The XML.</param>
        /// <returns></returns>
        /// <exception cref="Exception">将空的xml串转换为WxPayData不合法!</exception>
        public static SortedDictionary<string, string> FromXml(string xml)
        {
            var sortDic = new SortedDictionary<string, string>();
            if (string.IsNullOrEmpty(xml))
            {
                throw new Exception("将空的xml串转换为WxPayData不合法!");
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            var xmlNode = xmlDoc.FirstChild;//获取到根节点<xml>
            var nodes = xmlNode.ChildNodes;
            foreach (XmlNode xn in nodes)
            {
                var xe = (XmlElement)xn;

                if (!sortDic.ContainsKey(xe.Name))
                    sortDic.Add(xe.Name, xe.InnerText);
            }
            return sortDic;
        }

        /// <summary>
        /// Posts the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="content">The content.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <returns></returns>
        /// <exception cref="Exception">POST请求错误" + e</exception>
        public static string Post(string url, string content, string contentType = "application/x-www-form-urlencoded")
        {
            string result;
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
                    var stringContent = new StringContent(content, Encoding.UTF8);
                    var response = client.PostAsync(url, stringContent).Result;
                    result = response.Content.ReadAsStringAsync().Result;
                }
            }
            catch (Exception e)
            {
                throw new Exception("POST请求错误" + e);
            }
            return result;
        }

        /// <summary>
        /// Gets the value from dic.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dic">The dic.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static T GetValueFromDic<T>(IDictionary<string, string> dic, string key)
        {
            string val;
            dic.TryGetValue(key, out val);

            var returnVal = default(T);
            if (val != null)
                returnVal = (T)Convert.ChangeType(val, typeof(T));

            return returnVal;
        }

        /// <summary>
        /// Builds the application pay.
        /// </summary>
        /// <param name="prepayid">The prepayid.</param>
        /// <returns></returns>
        public static string BuildAppPay(string prepayid)
        {
            var dicParam = CreateWapAndAppPayParam(prepayid);
            var signString = CreateURLParamString(dicParam);
            var preString = signString + "&key=" + WeiXinConfig.APIKey;

            var sign = Sign(preString, "utf-8").ToUpper();
            dicParam.Add("sign", sign);

            return JsonConvert.SerializeObject(
                new
                {
                    appid = dicParam["appid"],
                    partnerid = dicParam["partnerid"],
                    prepayid = dicParam["prepayid"],
                    package = dicParam["package"],
                    noncestr = dicParam["noncestr"],
                    timestamp = dicParam["timestamp"],
                    sign = dicParam["sign"]
                });
        }

        /// <summary>
        /// Creates the wap and application pay parameter.
        /// </summary>
        /// <param name="prepayId">The prepay identifier.</param>
        /// <returns></returns>
        private static SortedDictionary<string, string> CreateWapAndAppPayParam(string prepayId)
        {
            var dic = new SortedDictionary<string, string>
            {
                {"appid", WeiXinConfig.AppID}, //公众账号ID
                {"partnerid", WeiXinConfig.MchID}, //商户号
                {"prepayid", prepayId}, //预支付交易会话ID
                {"package", "Sign=WXPay"}, //扩展字段
                {"noncestr", Guid.NewGuid().ToString().Replace("-", "")}, //随机字符串
                {
                    "timestamp",
                    (Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds)).ToString()
                } //时间戳
            };

            return dic;
        }

        /// <summary>
        /// Validatons the query result.
        /// </summary>
        /// <param name="dic">The dic.</param>
        /// <returns></returns>
        public static bool ValidatonQueryResult(SortedDictionary<string, string> dic)
        {
            var result = false;

            if (dic.ContainsKey("return_code") && dic.ContainsKey("return_code"))
            {
                if (dic["return_code"] == "SUCCESS" && dic["result_code"] == "SUCCESS")
                    result = true;
            }

            if (result) return true;

            var sb = new StringBuilder();
            foreach (var item in dic.Keys)
            {
                sb.Append(item + ":" + dic[item] + "|");
            }

            return false;
        }
    }
}
