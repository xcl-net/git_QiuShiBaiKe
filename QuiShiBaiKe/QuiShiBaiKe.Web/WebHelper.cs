using QiuShiBaiKe.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QiuShiBaiKe.Commons;
using System.Text;

namespace QuiShiBaiKe.Web
{
    /// <summary>
    /// 完成，控制器和视图中方法复杂的逻辑实现，提高代码可读性
    /// </summary>
    public class WebHelper
    {
        //1. 使用常量
        public const string HASLOGINEDUSER = "HasLoginedUser";
        private const string DBCONTEXT = "DbContext";
        public const string LOGIN_DESTINATION_URL = "LoginDestinationUrl";
        //用于cookies
        private const string USERNAME = "UserName";
        private const string PASSWORD = "PassWord";

        #region 2. 把用户Model存入到session中
        /// <summary>
        /// 把用户Model存入到session中
        /// </summary>
        /// <param name="context"></param>
        /// <param name="hasLoginedUser"></param>
        public static void SaveLoginedUserModel(HttpContextBase context, User hasLoginedUser)
        {
            context.Session[HASLOGINEDUSER] = hasLoginedUser;
        }
        #endregion

        #region 3. 从session中读取用户的信息
        /// <summary>
        /// 3. 从session中读取用户的信息
        /// </summary>
        /// <param name="context"></param>
        /// <returns>当前登陆用户对象，如果没有登录，则为null</returns>
        public static User GetUserModelBySession(HttpContextBase context)
        {
            return (User)context.Session[HASLOGINEDUSER];
        }
        #endregion

        #region 4. 得到一个包含Json信息的JsonResult
        /// <summary>
        /// 4. 得到一个包含Json信息的JsonResult
        /// </summary>
        /// <param name="isOk">服务器处理是否成功</param>
        /// <param name="msg">提示消息</param>
        /// <param name="nextUrl">下个要访问的地址</param>
        /// <param name="data">携带的额外信息</param>
        /// <returns></returns>
        public static JsonResult GetJsonResult(bool isOk, string msg, string nextUrl = null, object data = null)
        {
            //1. 封转到一个匿名对象中
            var jsonObj = new { isOk = isOk, msg = msg, nextUrl = nextUrl, data = data };
            //2. 序列化匿名对象
            return new JsonResult { Data = jsonObj };

        }
        #endregion

        #region 5. 封装必填字段为空的方法
        public static JsonResult ReturnValidMsg_Empty(string name)
        {
            return GetJsonResult(false, name + "不能为空！");
        }
        #endregion

        #region 6. 检查用户输入的验证码--与服务器端Session中保存的验证码是否一致
        public static bool IsOKValidCode(HttpContextBase context, string validCode)
        {
            //1. 从session中获得验证码
            string validCodeInSession = (string)CommonHelper.GetValidCodeInSession(context);
            //2. 比较
            return string.Equals(validCode, validCodeInSession, StringComparison.CurrentCultureIgnoreCase);//忽略大小写
        }
        #endregion

        #region 7. 设置登陆后要跳转到的页面
        public static void SetUrlInSession(HttpContextBase context, string url)
        {
            context.Session[LOGIN_DESTINATION_URL] = url;
        }
        #endregion

        #region 8. 把当前请求的Url设置为“登陆后要跳转到的页面”
        public static void SetLoginedDestinationUrl(HttpContextBase context)
        {
            string url = context.Request.RawUrl;
            SetUrlInSession(context, url);
        }
        #endregion
        /// <summary>
        /// 设置登陆后要跳转到的页面
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="url"></param>
        public static void SetLoginedDestinationUrl(HttpContextBase ctx, string url)
        {
            ctx.Session[LOGIN_DESTINATION_URL] = url;
        }

        #region 9. 获取存储在session中 URL地址
        public static string GetLoginedDestinationUrl(HttpContextBase context)
        {
            string url = (string)context.Session[LOGIN_DESTINATION_URL];
            return url ?? "/Home/Index";//如果为空，则跳转到首页/Home/Index
        }
        #endregion

        #region 一个请求使用一个DbContext
        #region 1. 创建DbContext，并且保存到HttpContext
        public static void CreateDbContext()
        {
            //创建一个数据上下文对象
            QSBKDBEntities entity = new QSBKDBEntities();
            HttpContext.Current.Items[DBCONTEXT] = entity;
        }
        #endregion

        #region 2. 获取当前请求的唯一的数据上下文对象
        public static QSBKDBEntities DbContext
        {
            get { return (QSBKDBEntities)HttpContext.Current.Items[DBCONTEXT]; }
        }
        #endregion

        #region 3. 销毁数据上下文对象
        public static void FinishDbContext()
        {
            using (DbContext)
            {
                DbContext.SaveChanges();//设置最后一个防线
            }
        }
        #endregion
        #endregion

        #region 记住用户名和密码到Cookie中
        public static void Remember(HttpContextBase context, string username, string password)
        {
            context.Response.SetCookie(new HttpCookie(USERNAME, context.Server.UrlEncode(username)) { Expires = DateTime.Now.AddDays(30) });
            context.Response.SetCookie(new HttpCookie(PASSWORD, CommonHelper.DesEncypt(password)) { Expires = DateTime.Now.AddDays(30) });//密码加密存入cookie

        }
        #endregion

        #region 忘记Cookie中存储的用户名。密码
        public static void Forget(HttpContextBase context)
        {
            //不是删除的同一个，cookie信息，报异常
            HttpCookie cookieUserName = context.Request.Cookies[USERNAME];

            if (cookieUserName != null)
            {
                string usernameOfKey = cookieUserName.Name;
                if (USERNAME != usernameOfKey)
                {
                    throw new Exception("不能删除cookie信息,查看是否是同一个cookie键，请先手动清除浏览器中的cookie信息");
                }
            }
            //过期时间设置为已经过去的时间，就能起到删除cookie的作用
            context.Response.SetCookie(new HttpCookie(USERNAME, "") { Expires = DateTime.Now.AddDays(-1) });
            context.Response.SetCookie(new HttpCookie(PASSWORD, "") { Expires = DateTime.Now.AddDays(-1) });
        }
        #endregion

        #region 10. 分页组件
        /// <summary>
        /// 生成分页组件
        /// </summary>
        /// <param name="urlFormat">超链接的格式</param>
        /// <param name="totalSize">总的数据条数</param>
        /// <param name="pageSize">每页多少条</param>
        /// <param name="currentPage">当前页的页码</param>
        /// <returns></returns>
        public static HtmlString PagingModule(string urlFormat, long totalSize, long pageSize, long currentPage)
        {
            StringBuilder sb = new StringBuilder();
            //实现的效果：
            //currentPage表示当前页：前边显示5页，后边显示5页：


            //[首页]1,2,3,{4},5,6,7,8,9[尾页]   -----------前边显示不到5个页码，怎么实现这个效果？

            //[首页]3,4,5,6,7,{8},9,10 [尾页]   -----------后边显示不到5个页码，怎么实现这个效果？    

            //[首页]3,4,5,6,7,{8},9,10,11,12,13 [尾页]   --正常显示...

            //计算页码的总页数: 一共51页，每页10条，则需要5.1页，就是取天花板数，6页！
            //                    一共50页，每页10条，则需要5页！
            //                     一共48页，每页10条，则需要5页！

            //                   总结：取得都是，  天花板数

            long totalPageCount = (long)Math.Ceiling((totalSize * 1.0f) / pageSize);//一个是浮点数就行，测试得到。

            //在当期页面前后，各 最多显示 5个，页码

            //计算页码中的第一个页码
            long firstPageNum = Math.Max(currentPage - 5, 1);
            //注释：当是这个情况时候：[首页]1,2,3,{4},5,6,7,8,9[尾页]   4-5=-1   取最大值，那么第一页显示的就是 1  ，也就是第一页
            //      当是这个情况时候：[首页]3,4,5,6,7,{8},9,10 [尾页]    8-5=3   取最大值，那么第一页显示的就是3   ，正好符合

            //计算页码中的最后一个页码
            long lastPageNum = Math.Min(currentPage + 5, totalPageCount);
            //注释：当是这个情况时候：[首页]3,4,5,6,7,{8},9,10 [尾页]  8+5=13  取最小值，那么最后一页显示的就是10   就是尾页

            //拼接出来，分页组件

            //如果当前页是 第1页，就只是显示“尾页”“下一页”;

            if (currentPage == 1)
            {
                if (totalPageCount == 1)
                {
                    #region MyRegion
                    //for (long i = firstPageNum; i <= lastPageNum; i++)
                    //{
                    //    string url = urlFormat.Replace("{pageNum}", i.ToString());
                    //    if (i == currentPage)
                    //    {
                    //        sb.Append("<li class='active'><a>" + i + "</a></li>");
                    //    }
                    //    else
                    //    {
                    //        sb.Append("<li><a href='" + url + "'>" + i + "</a></li>");
                    //    }
                    //}
                    #endregion
                    ReplaceCurrentPage(urlFormat, firstPageNum, lastPageNum, currentPage, sb);
                }
                else
                {
                    ReplaceCurrentPage(urlFormat, firstPageNum, lastPageNum, currentPage, sb);
                    sb.AppendLine("<li><a href='" + urlFormat.Replace("{pageNum}", (currentPage + 1).ToString()) + "'>下一页</a></li>");
                    sb.AppendLine("<li><a href='" + urlFormat.Replace("{pageNum}", totalPageCount.ToString()) + "'>尾页</a></li>");
                }
            }
            //如果当前页是 第最后页，只是就显示“首页”“上一页”;
            else if (currentPage == lastPageNum)
            {
                sb.AppendLine("<li><a href='" + urlFormat.Replace("{pageNum}", "1") + "'>首页</a></li>");
                sb.AppendLine("<li><a href='" + urlFormat.Replace("{pageNum}", (currentPage - 1).ToString()) + "'>上一页</a></li>");
                #region MyRegion
                //for (long i = firstPageNum; i <= lastPageNum; i++)
                //{
                //    string url = urlFormat.Replace("{pageNum}", i.ToString());
                //    if (i == currentPage)
                //    {
                //        sb.Append("<li class='active'><a>" + i + "</a></li>");
                //    }
                //    else
                //    {
                //        sb.Append("<li><a href='" + url + "'>" + i + "</a></li>");
                //    }
                //} 
                #endregion
                ReplaceCurrentPage(urlFormat, firstPageNum, lastPageNum, currentPage, sb);
            }
            //如果当前页不是第一页，也不是最后一页，就显示“首页”，“上一页”，“下一页”，“尾页”;
            else
            {

                sb.AppendLine("<li><a href='" + urlFormat.Replace("{pageNum}", "1") + "'>首页</a></li>");
                sb.AppendLine("<li><a href='" + urlFormat.Replace("{pageNum}", (currentPage - 1).ToString()) + "'>上一页</a></li>");
                #region MyRegion
                //for (long i = firstPageNum; i <= lastPageNum; i++)
                //{
                //    string url = urlFormat.Replace("{pageNum}", i.ToString());
                //    if (i == currentPage)
                //    {
                //        sb.Append("<li class='active'><a>" + i + "</a></li>");
                //    }
                //    else
                //    {
                //        sb.Append("<li><a href='" + url + "'>" + i + "</a></li>");
                //    }
                //} 
                #endregion
                ReplaceCurrentPage(urlFormat, firstPageNum, lastPageNum, currentPage, sb);
                sb.AppendLine("<li><a href='" + urlFormat.Replace("{pageNum}", (currentPage + 1).ToString()) + "'>下一页</a></li>");
                sb.AppendLine("<li><a href='" + urlFormat.Replace("{pageNum}", totalPageCount.ToString()) + "'>尾页</a></li>");
            }
            return new HtmlString(sb.ToString());
        }
        #endregion

        //当前页不是第一页的时候，li 匹配情况；
        public static void ReplaceCurrentPage(string urlFormat, long firstPageNum, long lastPageNum, long currentPage, StringBuilder sb)
        {
            for (long i = firstPageNum; i <= lastPageNum; i++)
            {
                string url = urlFormat.Replace("{pageNum}", i.ToString());
                if (i == currentPage)
                {
                    sb.Append("<li class='active'><a>" + i + "</a></li>");
                }
                else
                {
                    sb.Append("<li><a href='" + url + "'>" + i + "</a></li>");
                }
            }
        }
    }
}