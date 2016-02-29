using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuiShiBaiKe.Web.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()//访问主页默认跳转到列表的第一页
        {
            return RedirectToAction("Page", "Message", new { pageIndex = 1 });//设置pageNum参数默认为1
        }


    }
}
