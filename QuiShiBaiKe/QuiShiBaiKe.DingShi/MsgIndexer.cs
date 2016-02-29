using Lucene.Net.Analysis.PanGu;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using QiuShiBaiKe.Commons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace QuiShiBaiKe.DingShi
{
    /// <summary>
    /// 对新增糗事的队列读取，然后，存入到“卢思银”索引库中
    /// </summary>
    public class MsgIndexer  //采用一次性读取
    {

        #region 使用子线程，防止form界面卡死
        private static Thread thread;  //添加静态字段

        public static bool IsRunning { get; set; }  //表示线程是否运行
        #endregion

        /// <summary>
        /// 开启一个子线程
        /// </summary>
        public static void Start()
        {
            IsRunning = true;  //程序开始的时候，设置IsRunning为True
            thread = new Thread(RunReadFromRedis); //线程就是“委托”，RunReadFromRedis（）方法教给线程来做
            thread.IsBackground = true; //设置为后台运行
            thread.Start();
        }

        private static void RunReadFromRedis()
        {
            while (IsRunning)//为真就执行，从糗事消息队列中取数据，放到索引库中
            {
                //1. 获取消息队列中
                using (var client = RedisManager.ClientManager.GetClient())
                {
                    //2. 怎么从队列中获取数据？怎么存入到索引库效率高；
                    ProcessQueue(client);
                }
            }
        }

        #region 一 、 打开Lucene索引库 ；二 、循环：1. 取（队列）；2. 存（到索引库）；三 、关闭Lucene索引库
        //本方法思路：
        //1. 这里一次打开Lucene 索引库，就不断的While循环:从消息队列中取数据（序列化的msgModel对象），并存到Lucene索引库中，直到从消息队列中取得的数据为空，CPU休息一会儿，接着返回方法；
        //2. 注意：打开的Lucene 索引库，最后记得关闭索引库，释放资源；用 try--finally :(就是出现异常了，最后还是会关闭索引库)；

        private static void ProcessQueue(ServiceStack.Redis.IRedisClient client)
        {
            FSDirectory directory = null;
            IndexWriter writer = null;
            try
            {
                #region 一、 有就打开Lucene索引库，并完成解锁；没索引库就创建索引库
                string indexPath = @"G:/Index";//注意和磁盘上文件夹的大小写一致，否则会报错。
                directory = FSDirectory.Open(new DirectoryInfo(indexPath), new NativeFSLockFactory());
                bool isExist = IndexReader.IndexExists(directory);
                if (isExist) //检查索引库在硬盘中是锁定的状态
                {
                    //如果索引目录被锁定（比如索引过程中程序异常退出），则首先解锁
                    //Lucene.net在索引库写索引数据之前会自动加锁，在close的时候会自动解锁
                    //不能多线程执行，只能处理意外被永远锁定的情况
                    if (IndexWriter.IsLocked(directory))
                    {
                        IndexWriter.Unlock(directory);//强制解锁
                    }
                }
                //调用盘IndexWriter类 在索引库中写， 用盘古分词算法， !isExist：不存索引库，则创建
                writer = new IndexWriter(directory, new PanGuAnalyzer(), !isExist, Lucene.Net.Index.IndexWriter.MaxFieldLength.UNLIMITED);

                #endregion

                #region 二 、while循环：1. 取（队列）；2. 存（到索引库）；
                while (true)
                {
                    //获取存入队列中的内容，糗事id，糗事内容 txtMsg ，是不是匿名 isAnonymous 这个糗事中图片的 imageUrl
                    string oneMsgJson = client.DequeueItemFromList("msgIndex");
                    if (oneMsgJson != null)//从队列中取得新闻不是空，才进行反序列化
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        //object obj = js.DeserializeObject(oneMsgJson);//由于不确定obj是什么类型，假设为Object类型；通过调式发现是字典类型（快速监视，添加ojb.GetType()就知道是什么类型了）
                        Dictionary<string, object> obj = (Dictionary<string, object>)js.DeserializeObject(oneMsgJson);
                        //由于不知道反序列化是long 类型，还是int 类型，所以使用Convert
                        long id = Convert.ToInt64(obj["Id"]); //糗事id
                        string msg = Convert.ToString(obj["Msg"]);//z糗事内容
                        bool isAnonymous = (bool)obj["IsAnonymous"];
                        string imageUrl = Convert.ToString(obj["ImageUrl"]);

                        #region 2. 存到索引库
                        //调用writer对象存到索引库中-----怎么存，这里可以写一个方法
                        WriteToIndex(writer, id, msg, isAnonymous, imageUrl);
                        #endregion

                        //一个小技巧 ！
                        //怎么判断 write对象 将一条糗事写入索引库中 ？ 
                        //用IO记录 或者是log4net 
                        File.AppendAllText("d:/1.txt", "id=" + id + ";msg=" + msg + "\r\n");
                    }
                    else
                    {
                        //为空，不做检查
                        Thread.Sleep(1000);
                        //队列中没有能取的，就退出本次ProcessQueue(处理队列)，当然，finally 中还是会执行的
                        return;
                    }
                }
                #endregion
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close(); //关闭写对象，释放资源；
                }
                if (directory != null)
                {
                    directory.Close();//不要忘了Close，否则索引结果搜不到
                }
            }
        #endregion
        }
        #region 写一个方法：把反序列化的新闻对象，存入到索引库中
        //导入 Lucene.dll 、Pangu.dll 、Pangu.Lucene.Analyze.dll 和盘古分词工具分词需要的字典Dict（设置文件全部较新则复制）
        /// <summary>
        /// 把反序列化的新闻对象，存入到索引库中
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="id"></param>
        /// <param name="msg"></param>
        /// <param name="isAnonymous"></param>
        /// <param name="imageUrl"></param>
        private static void WriteToIndex(IndexWriter writer, long id, string msg, bool isAnonymous, string imageUrl)
        {
            Document oneDoc = new Document();//一条Document相当于一条记录

            //每个Document可以有自己的属性（字段），所有字段名都是自定义的，值都是【 string】类型
            oneDoc.Add(new Field("Id", id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED)); //ANALYZED表示模糊匹配；  //YES 表示存储分词前的原文，必须为YES才行，否则搜索不到
            oneDoc.Add(new Field("Msg", msg, Field.Store.YES, Field.Index.ANALYZED, Lucene.Net.Documents.Field.TermVector.WITH_POSITIONS_OFFSETS)); //TermVector 表示如何保存索引词之间的距离，方便检索一定距离内的词；

            writer.AddDocument(oneDoc); //insert into 插入一条记录，有两个字段：Id Msg
        }
        #endregion
    }
}
