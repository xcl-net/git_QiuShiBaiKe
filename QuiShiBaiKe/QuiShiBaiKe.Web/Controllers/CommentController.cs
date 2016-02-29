using QiuShiBaiKe.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuiShiBaiKe.Web.Controllers
{
    public class CommentController : Controller
    {
        //
        // GET: /Comment/

        //public RedirectResult PostComment(FormCollection fm)
        public ActionResult PostComment()
        {
            //1. 糗事id
            string _idQiuShi = Request["comment[article_id]"];
            long idQiuShi = Convert.ToInt64(_idQiuShi);
            //2. 评论内容
            string comment = Request["comment[content]"];
            //3. 获取用户的id
            var userModel = WebHelper.GetUserModelBySession(HttpContext);
            long userId = userModel.Id;

            MessageCommentBll commentBll = new MessageCommentBll(WebHelper.DbContext);
            long newComment = commentBll.Add(userId, comment, idQiuShi);
            return WebHelper.GetJsonResult(true, "");
        }

    }
}
