using QiuShiBaiKe.BLL;
using QiuShiBaiKe.Commons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QiuShiBaiKe.Model;
using Qiniu.Conf;
using Qiniu.RS;
using Qiniu.IO;
using Qiniu.IO.Resumable;
using Qiniu.RPC;
using Qiniu.Util;
using log4net;
using ServiceStack.Redis;
using System.Web.Script.Serialization;


namespace QuiShiBaiKe.Web.Controllers
{
    public class MessageController : Controller
    {

        //记录日志
        private static ILog logger = LogManager.GetLogger(typeof(MessageController));


        /// <summary>
        /// 展示一条糗事的具体信息
        /// </summary>
        /// <param name="idQiuShi"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index(long? id)//mvc自动解析为int类型
        {
            if (id == null)//从请求中获取
            {
                //这里为什么要加上强制类型（object），为了匹配成View(string viewName, object model);方便共用一个错误页；
                return View("Error", (object)"id不能为空！");
            }

            //实例化，messageBll对象
            MessageBll messageBll = new MessageBll(WebHelper.DbContext);
            //实例户，messageCommentBll评论对象
            MessageCommentBll messageCommentBll = new MessageCommentBll(WebHelper.DbContext);
            //获取当前糗事
            Message messageModel = messageBll.GetMessageById((long)id);
            //获取糗事的评论总数
            long countComments = messageBll.GetCountComments((long)id);
            //获取糗事评论的集合
            var commentsListCurrent = messageCommentBll.GetCommentList((long)id);

            ViewBag.messageModel = messageModel;
            ViewBag.countComments = countComments;
            ViewBag.commentsListCurrent = commentsListCurrent;
            //如果用户要从这个页面登录之前，把当前的url地址，存到session中
            WebHelper.SetLoginedDestinationUrl(HttpContext);
            return View();
        }
        /// <summary>
        /// 加载发表一条糗事的发表页面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ShowPostMsg()
        {
            //1.检查用户已经登录
            var userModel = WebHelper.GetUserModelBySession(HttpContext);
            if (userModel == null)
            {
                //2. 设定登录后的url地址
                WebHelper.SetLoginedDestinationUrl(HttpContext, "/Message/ShowPostMsg");
                return View("Error", (object)"请先<a href='/User/ShowLogin'>登陆</a>");
            }
            return View();
        }
        /// <summary>
        /// 糗事发表提交地址
        /// </summary>
        /// <param name="fm"></param>
        /// <returns></returns>
        public ActionResult PostMsg(FormCollection fm)
        {
            //1.检查用户已经登录
            var userModel = WebHelper.GetUserModelBySession(HttpContext);
            if (userModel == null)
            {
                //2. 设定登录后的url地址
                WebHelper.SetLoginedDestinationUrl(HttpContext, "/Message/ShowPostMsg");
                return View("Error", (object)"请先<a href='/User/ShowLogin'>登陆</a>");
            }
            //3. 糗事是否为空
            string txtQiuShi = fm["msg"];
            if (string.IsNullOrEmpty(txtQiuShi))
            {
                return View("Error", (object)"请填写要发表的文字");
            }
            //4. 是否匿名
            bool isAnonymous = (fm["isAnonymous"] == "on");
            //5. 图片处理
            var imgFile = Request.Files["imgFile"];

            //byte[] uploadImageBytes = new byte[imgFile.ContentLength];

            //返回图片的url地址
            string imageUrl = null;
            if (imgFile.ContentLength > 0)//表示有图片
            {
                //1-- 检查图片大小
                if (imgFile.ContentLength > 1 * 1024 * 1024)
                {
                    return View("Error", (object)"图片最大允许 1 MB");
                }
                //2-- 检查图片的扩展名
                string imgFileExt = Path.GetExtension(imgFile.FileName).ToLower();
                if (imgFileExt != ".jpg" && imgFileExt != ".png" && imgFileExt != ".jpeg" && imgFileExt != ".gif")
                {
                    return View("Error", (object)"只允许如下格式图片：jpg,jpeg,png,gif");
                }

                #region 1. 上传图片首先保存到网站后台Upload文件中
                //3--  图片保存的文件夹路径
                DateTime now = DateTime.Now;
                //计算文件的保存文件夹
                //用年、月、日做文件的目录，这样避免一个文件夹下文件过多导致，检索变慢问题
                //用文件的MD5值做文件名，这样避免多次上传一副图片，导致磁盘浪费问题
                string imgKeyInQiNiu = CommonHelper.CalcMD5(imgFile.InputStream) + imgFileExt;//存在七牛中的key值
                string imgPathKey = now.Year + "/" + now.Month + "/" + now.Day + "/" + imgKeyInQiNiu;
                string imgPath = "/Upload/" + imgPathKey;
                imgFile.InputStream.Position = 0;//一定要设置这个，不然上传就有可能为大小为0，因为SDK内部只上传Position后面的数据

                //映射到服务器物理路径
                string imgFileFullPath = Server.MapPath("~" + imgPath);

                //检查文件目录是否存在，不存在，则创建
                Directory.CreateDirectory(Path.GetDirectoryName(imgFileFullPath));//文件夹不存在，则创建文件夹
                imgFile.SaveAs(imgFileFullPath);

                #endregion

                #region 2. 服务器Upload中的图片再保存在“七牛云”云存储中
                try
                {
                    Qiniu.Conf.Config.ACCESS_KEY = "eWIvLfvSzffzYAlM5WecZYPqEJlZSSCMAHQUR0AS";
                    Qiniu.Conf.Config.SECRET_KEY = "DBWXeoFjhJUPv0wuRaNy7UFyeZE5i-8sHH38afcm";

                    //上传到七牛云的空间名
                    string bucketName = "xclnet";


                    CallRet callret = ResumablePutFile(bucketName, imgFileFullPath, imgKeyInQiNiu);

                    if (callret.OK)//上传成功
                    {
                        //返回上传成功的图片url地址
                        imageUrl = "http://7xo5l5.com1.z0.glb.clouddn.com/" + imgKeyInQiNiu;//设置图片链接为七牛云访问
                        //imageUrl="端口号"+imgPath；//设置图片链接为本地访问

                    }
                    else
                    {
                        logger.Error("上传到七牛云失败");
                        throw new Exception("上传到云失败");
                    }

                }
                catch (Exception e)
                {
                    logger.Error("上传到七牛云失败，发生异常：\r\n" + e + "\r\n\r\n");
                    //throw new Exception(e.ToString());
                    return View("Error", (object)"可能您的网络不稳定,发表失败,<a href='/Message/ShowPostMsg'>点此重新发送</a>");
                }

                #endregion

            }
            //新增到数据库
            MessageBll messageBll = new MessageBll(WebHelper.DbContext);
            //新增糗事，并获得新增糗事的id
            long newId = messageBll.Add(userModel.Id, txtQiuShi, isAnonymous, imageUrl);

            #region 存入消息队列中，用DingShi服务器，存到索引库中
            AddToMsgIndex(newId, txtQiuShi, isAnonymous, imageUrl);
            #endregion
            return RedirectToAction("Index", "Message", new { id = newId });
        }
        /// <summary>
        /// 上传文件到七牛云空间
        /// </summary>
        /// <param name="bucketName">空间名字</param>
        /// <param name="imgFileFullPath">文件在服务器端的绝对路径</param>
        /// <param name="imgPathKey">存在云空间的key</param>
        /// <returns></returns>
        private static CallRet ResumablePutFile(string bucketName, string imgFileFullPath, string imgPathKey)
        {
            PutPolicy policy = new PutPolicy(bucketName, 3600);
            string upToken = policy.Token();//上传标记
            Settings setting = new Settings();
            ResumablePutExtra extra = new ResumablePutExtra();
            ResumablePut client = new ResumablePut(setting, extra);
            CallRet callret = client.PutFile(upToken, imgFileFullPath, imgPathKey);//客户端上传文件
            return callret;
        }

        /// <summary>
        /// 加载每页显示多少条糗事
        /// </summary>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public ActionResult Page(int pageIndex)
        {
            if (pageIndex <= 0)
            {
                //return View("Error",(object)"pageIndex必须大于0");
                return View("Error", (object)"当前没有任何糗事~");
            }


            //【2】查询数据库中“糗事”总条数，使用“缓存”
            MessageBll msgBll = new MessageBll(WebHelper.DbContext);
            //尝试从缓存中获取总条数，避免频繁的查询总条数，给数据库带来的压力
            //糗事的总条数
            string cahcheKey = "MESSAGE_TOTALCOUNT";
            long? totalCount = (long?)HttpContext.Cache[cahcheKey];//缓存的key

            if (totalCount == null)
            {
                totalCount = msgBll.GetCountMessages();
                HttpContext.Cache.Insert(cahcheKey, totalCount, null, DateTime.Now.AddSeconds(10), TimeSpan.Zero);//缓存10秒
            }
            //【3】每页显示10条糗事
            int pageSize = 10;
            //【4】. 每页显示的糗事的对象集合
            List<Message> msgList = msgBll.GetMessage(pageIndex, pageSize);
            ViewBag.pageIndex = pageIndex;
            ViewBag.pageSize = pageSize;
            ViewBag.totalCount = totalCount;
            return View(msgList);
        }

        //接收点赞方法
        public ActionResult Support()
        {
            string num = Request["supportNum"];// 1 
            string msgId = Request["msgId"];// " 13|1 "  第13条糗事被 “赞” 了

            #region 存入到消息队列中
            using (var client = RedisManager.ClientManager.GetClient())
            {
                string msgKey_Queue = msgId + "|" + num;
                client.EnqueueItemOnList("supportNum", msgKey_Queue);
            }
            #endregion

            //获得第一个人点赞时候，赞的数值，存入到 NoSql数据库中（假设1000人，同时点赞，就是一个人点赞）
            StoreSupportNumInRedis(msgId, num, 1000); //键：msgId + num,缓存时间：50 ms ;

            #region 从NoSql数据库中获取赞的数值，每一次请求要 +1
            using (var client = RedisManager.ClientManager.GetClient())
            {
                string msgStr_ = msgId + num;
                string supportNum = client.Get<string>(msgStr_);
                long supportNumber = Convert.ToInt64(supportNum);
                if (supportNum == null)//如果缓存中的 “赞” 数值是空
                {
                    supportNumber = GetSupportNum(msgId);
                    long s_Number = supportNumber + 1;
                    return WebHelper.GetJsonResult(true, "", "", new { supportNumber = s_Number });
                }
                return WebHelper.GetJsonResult(true, "", "", new { supportNumber = supportNumber });
            }
            #endregion


        }

        //接收点拍方法
        public ActionResult Oppose()
        {
            string num = Request["oppsoeNum"];// 0
            string msgId = Request["msgId"];  //" 13|0 "  第13条糗事被 “拍” 了

            #region //存入点“拍”到消息队列中
            using (var client = RedisManager.ClientManager.GetClient())
            {
                string msgKey_Queue = msgId + "|" + num;  //  " 13 | 0 "  第13条糗事被 “拍” 了
                client.EnqueueItemOnList("supportNum", msgKey_Queue);
            }
            #endregion

            StoreOpposeNumInRedis(msgId, num, 1000);

            #region 从NoSql数据库中获取拍的数值，每一次请求要 +1
            using (var client = RedisManager.ClientManager.GetClient())
            {
                string msgStr_ = msgId + num;
                string opposedNum = client.Get<string>(msgStr_);
                long opposeNumber = Convert.ToInt64(opposedNum);
                if (opposedNum == null)//如果缓存中的 “赞” 数值是空
                {
                    opposeNumber = GetOpposedNum(msgId);
                    long o_Number = opposeNumber + 1;
                    return WebHelper.GetJsonResult(true, "", "", new { opposeNumber = o_Number });
                }
                return WebHelper.GetJsonResult(true, "", "", new { opposeNumber = opposeNumber });
            }
            #endregion
        }

        /// <summary>
        ///  把第一个人点“赞”的数值，存到 NoSql数据库中，并设置缓存时间
        /// </summary>
        /// <param name="msgId">糗事id</param>
        /// <param name="num">"1"|赞、"0"|拍</param>
        /// <param name="timeSpan">设置缓存时间</param>
        public static void StoreSupportNumInRedis(string msgId, string num, long timeSpan)
        {

            MessageBll msgBll = new MessageBll(WebHelper.DbContext);
            Message msgModel = msgBll.GetMessageById(Convert.ToInt64(msgId));
            long supportNum = msgModel.SupportedNum;
            using (IRedisClient client = RedisManager.ClientManager.GetClient())
            {
                //将第一次点赞数值，添加到 redis 缓存中，key为了防止冲突，加上前缀，和用户名
                client.Set<string>(msgId + num, supportNum.ToString(), DateTime.Now.AddMilliseconds(timeSpan));//单位 ms
            }
        }
        /// <summary>
        ///  把第一个人点“拍”的数值，存到 NoSql数据库中，并设置缓存时间
        /// </summary>
        /// <param name="msgId">糗事id</param>
        /// <param name="num">"1"|赞、"0"|拍</param>
        /// <param name="timeSpan">设置缓存时间</param>
        public static void StoreOpposeNumInRedis(string msgId, string num, long timeSpan)
        {

            MessageBll msgBll = new MessageBll(WebHelper.DbContext);
            Message msgModel = msgBll.GetMessageById(Convert.ToInt64(msgId));
            long opposeNum = msgModel.OpposedNum;
            using (IRedisClient client = RedisManager.ClientManager.GetClient())
            {
                //将第一次点赞数值，添加到 redis 缓存中，key为了防止冲突，加上前缀，和用户名
                client.Set<string>(msgId + num, opposeNum.ToString(), DateTime.Now.AddMilliseconds(timeSpan));//单位 ms
            }
        }
        /// <summary>
        /// 根据糗事id获取点赞的总数
        /// </summary>
        /// <param name="msgId"></param>
        /// <returns></returns>
        public static long GetSupportNum(string msgId)
        {
            MessageBll msgBll = new MessageBll(WebHelper.DbContext);
            Message msgModel = msgBll.GetMessageById(Convert.ToInt64(msgId));
            return msgModel.SupportedNum;
        }

        /// <summary>
        /// 根据糗事id获取点拍的总数
        /// </summary>
        /// <param name="msgId"></param>
        /// <returns></returns>
        public static long GetOpposedNum(string msgId)
        {
            MessageBll msgBll = new MessageBll(WebHelper.DbContext);
            Message msgModel = msgBll.GetMessageById(Convert.ToInt64(msgId));
            return msgModel.OpposedNum;
        }

        /// <summary>
        /// 新增的糗事存入到消息队列中
        /// </summary>
        /// <param name="idQiuShi"></param>
        /// <param name="txtQiuShi"></param>
        /// <param name="isAnonymous"></param>
        /// <param name="imageUrl"></param>
        static void AddToMsgIndex(long idQiuShi, string txtQiuShi, bool isAnonymous, string imageUrl)
        {
            Message msg = new Message();
            msg.Id = idQiuShi;
            msg.Msg = txtQiuShi;
            msg.IsAnonymous = isAnonymous;
            msg.ImageUrl = imageUrl;
            //把糗事对象序列化
            string msgJson = new JavaScriptSerializer().Serialize(msg);
            //写入索引消息队列中
            using (var client = RedisManager.ClientManager.GetClient())
            {
                client.EnqueueItemOnList("msgIndex", msgJson);
            }
        }
    }
}
