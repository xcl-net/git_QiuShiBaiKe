using QiuShiBaiKe.BLL;
using QiuShiBaiKe.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QiuShiBaiKe.Model;

namespace QuiShiBaiKe.Web.Controllers
{
    public class UserController : Controller
    {
        //
        // GET: /User/

        public ActionResult Index()
        {
            return View();
        }

        #region 检查用户，是否登录
        /// <summary>
        /// 检查用户，是否登录
        /// </summary>
        /// <returns>Json信息，返回用户名和用户id</returns>
        public ActionResult CheckLoginState()
        {
            //1. 获取用户model
            var userModel = WebHelper.GetUserModelBySession(HttpContext);
            //2. 检查用户是否存在
            if (userModel == null)
            {
                return WebHelper.GetJsonResult(false, "");
            }
            else
            {

                //已登录
                return WebHelper.GetJsonResult(true, "", "", new { UserId = userModel.Id, UserName = userModel.UserName });
            }

        }
        #endregion

        #region 1. 加载登录界面
        [HttpGet]
        public ActionResult ShowLogin()
        {
            return View();
        }
        #endregion

        #region 2. 处理登录提交的信息
        [HttpPost]
        public ActionResult Login(FormCollection fm)
        {


            var userName = fm["userName"];
            var password = fm["password"];
            var validCode = fm["validCode"];
            if (string.IsNullOrWhiteSpace(userName))
            {
                //return WebHelper.GetJsonResult(false, "用户名不能为空！");
                return WebHelper.ReturnValidMsg_Empty("用户名");
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                return WebHelper.ReturnValidMsg_Empty("密码");
            }
            //if (string.IsNullOrWhiteSpace(validCode))
            //{
            //    return WebHelper.ReturnValidMsg_Empty("验证码");
            //}
            //if (validCode != CommonHelper.GetValidCodeInSession(HttpContext))
            //{
            //    //验证码不对，要"重置"验证码,视图中的验证码并没有变，只是服务器端的已经变了；
            //    CommonHelper.ResetValidCode(HttpContext);
            //    return WebHelper.GetJsonResult(false, "验证码错误！");
            //}
            //用户名和密码检查
            //1. 实例Bll对象
            //使用同一个用户请求使用唯一的数据，上下文对象
            UserBll userBll = new UserBll(WebHelper.DbContext);
            bool isPermit = userBll.PermitLogin(userName, password);
            if (isPermit)
            {
                //1. 用户名，密码存到cookie
                WebHelper.Remember(HttpContext, userName, password);

                //2. 登录成功就只是把用户model存到session中
                QiuShiBaiKe.Model.User userModel = userBll.GetByUserName(userName);
                WebHelper.SaveLoginedUserModel(HttpContext, userModel);
            }
            //从session中获取url地址；
            string url = WebHelper.GetLoginedDestinationUrl(HttpContext);
            return WebHelper.GetJsonResult(isPermit, "用户名或者密码错误", url);
        }
        #endregion

        #region 1. 加载注册界面

        public ActionResult ShowRegister()
        {
            return View();
        }
        #endregion

        #region 2. 处理注册提交的信息
        public ActionResult Register(FormCollection fm)
        {
            string userName = fm["UserName"];
            string password = fm["Password"];
            string validCode = fm["ValidCode"];
            if (string.IsNullOrWhiteSpace(userName))
            {
                return WebHelper.ReturnValidMsg_Empty("用户名");
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                return WebHelper.ReturnValidMsg_Empty("密码");
            }
            if (string.IsNullOrWhiteSpace(validCode))
            {
                return WebHelper.ReturnValidMsg_Empty("验证码");
            }
            if (!WebHelper.IsOKValidCode(HttpContext, validCode))
            {
                CommonHelper.ResetValidCode(HttpContext);
                return WebHelper.GetJsonResult(false, "验证码不正确");
            }
            //1. 实例化，Bll对象
            UserBll userBll = new UserBll(WebHelper.DbContext);
            //2. 根据逻辑层的返回结果做判断
            AddResult addResult = userBll.Add(userName, password);
            switch (addResult)
            {
                case AddResult.OK:
                    return WebHelper.GetJsonResult(true, "注册成功！", "/Home/Index");
                case AddResult.UserNameForbiden:
                    return WebHelper.GetJsonResult(false, "用户名中包含禁用词，请修改后再注册");
                case AddResult.UserNameExist:
                    return WebHelper.GetJsonResult(false, "您注册的用户名已经被占用，请修改后再注册");
                case AddResult.UserNameLengthInValid:
                    return WebHelper.GetJsonResult(false, "用户名的长度必须在3至15之间");
                default:
                    return WebHelper.GetJsonResult(false, "未知错误");
            }


        }
        #endregion


        //[HttpGet]
        //public ActionResult LoginOut(HttpContextBase context)
        //{
        //    return View();
        //}
        #region  点击“退出”，退出session和cookie

        //public void LoginOut()
        //{ 

        //}
        public ActionResult LoginOut()
        {
            //退出登录，逻辑代码还是写在WebHelper中
            HttpContext.Session.Abandon();//销毁Session
            Session.RemoveAll();
            WebHelper.Forget(HttpContext);//销毁Cookie

            //return WebHelper.GetJsonResult(true, "");
            return RedirectToAction("Index", "Home");

        }
        #endregion

        #region 生成验证码
        public ActionResult GenerateValidCode()
        {
            string hanziStr;
            byte[] buffer = CommonHelper.GenerateValidCodeImg(out hanziStr);//out适合用在需要retrun多个返回值的地方，需要先对out值在方法中初始化

            CommonHelper.StoreValidCodeInSession(HttpContext, hanziStr);

            return File(buffer, "image/jpeg");
        }
        #endregion
    }
}
