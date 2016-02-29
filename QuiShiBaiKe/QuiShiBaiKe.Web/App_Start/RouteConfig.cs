using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace QuiShiBaiKe.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //糗事列表分页跳转页面路由
            routes.MapRoute("Paging", "{controller}/Page/{pageIndex}",
                            new { controller = "Message", action = "Page" });

            //糗事搜索分页跳转路由
            routes.MapRoute("SearchPaging", "{controller}/Search/{pageIndex}",
                           new { controller = "Search", action = "Search", pageIndex = UrlParameter.Optional });

            //进入单个糗事主页面路由
            routes.MapRoute("MsgIndex", "{controller}/{action}/{id}",
                           new { controller = "Message", action = "Index" });

            //网站主页路由
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}