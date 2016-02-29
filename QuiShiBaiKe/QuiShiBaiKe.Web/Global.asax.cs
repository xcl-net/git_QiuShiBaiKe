using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace QuiShiBaiKe.Web
{
    // 注意: 有关启用 IIS6 或 IIS7 经典模式的说明，
    // 请访问 http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure();//程序启动的时候读取配置
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest()
        {
            //请求开始之前创建本次请求用到的DbContext
            //One DbContext Per Request
            //DbContext不是线程安全的
            WebHelper.CreateDbContext();
        }

        protected void Application_EndRequest()
        {
            //本次请求结束销毁DbContext
            WebHelper.FinishDbContext();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            //一般系统的错误信息，都是在这里记录到日志文件的
            ILog log = LogManager.GetLogger(typeof(MvcApplication));
            log.Error("系统发生了未经处理的异常", Context.Error);//拿到错误对象
        }
    }
}