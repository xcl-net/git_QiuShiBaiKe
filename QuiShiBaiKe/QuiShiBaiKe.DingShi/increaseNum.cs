using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using QiuShiBaiKe.Commons;
using QuiShiBaiKe.Web;
using QiuShiBaiKe.Model;
using System.Threading;
using QiuShiBaiKe.DingShi;



namespace QuiShiBaiKe.DingShi
{
    public class increaseNum
    {

        #region 使用子线程，防止form被卡死
        //1-----添加静态字段
        private static Thread thread;
        //2-----添加字段表示是否在运行
        public static bool IsRuning { get; set; }

        public static void Start()
        {


            IsRuning = true; //程序启动的时候，设置IsRuning 为 true
            thread = new Thread(RunReadRedis);//线程就是“委托”，RunReadRedis()方法交给线程来做
            thread.IsBackground = true; //设置为后台运行
            thread.Start();

        }

        //这个方法，会交给子线程完成
        private static void RunReadRedis()
        {
            while (IsRuning)  //为真：就执行：从赞（拍）对列中取数据
            {

                Execute();
            }
        }
        #endregion
        public static void Execute()
        {

            //执行工作；
            using (var client = RedisManager.ClientManager.GetClient())
            {
                //声明一个list集合
                List<string> infos = new List<string>();
                //每次向用户点赞的时候，将存入list集合中的所有的用户点赞总数记录到数据库Message中；
                //修改完毕，结束Job

                //使用一个while循环完成，不断的从队列中取 “点赞” 消息
                while (true)
                {
                    //取出来，一条用户的消息；
                    string info = client.DequeueItemFromList("supportNum");


                    if (info == null)//从消息队列中取出为空，就开始修改数据库中的“赞”字段
                    {
                        //如果，从队列中，不能取数据了；
                        //则开始修改数据库；

                        //用户点赞，从list集合中，取出用户全部点赞的数据，统计个数；
                        if (infos.Count != 0)
                        {
                            //一次队列中内容： 12|1 、12|1、12|0、12|0、13|1、13|0、15|1、15|0
                            //统计出“赞”（“拍”）的总数  
                            //糗事的id
                            string[] msgIds = infos.ToArray();
                            List<string> msgIdHasUpdated = new List<string>();//设定一个集合存放本次队列已经修该过“赞”字段的糗事id
                            long num = 0;//统计本次队列本条糗事“赞”（“拍”）总数

                            //遍历每一个字符串
                            for (int i = 0; i < msgIds.Length; i++)
                            {
                                if (msgIdHasUpdated.Contains(msgIds[i]))//如果在遍历的时候，发现已经统计过的糗事id则跳过，进行下一个糗事id的遍历
                                {
                                    continue;
                                }
                                //统计，本次队列中，相同糗事id，的个数；
                                for (int j = 0; j < msgIds.Length; j++)
                                {
                                    if (msgIds[i].Equals(msgIds[j]))//如果相同
                                    {
                                        num++;//如果相同就自增一次
                                    }
                                }
                                //num就是这一次队列中，这条糗事被点赞（点拍）的总数

                                msgIdHasUpdated.Add(msgIds[i]);//这个糗事id，存到list集合中，表示已经统计了这个糗事id的个数了；个数就是num，即点赞（拍）数；

                                Judge(msgIds[i], num);//判断是赞还是拍，并修改数据库
                            }
                            infos.Clear(); //取出完毕，清除；
                        }
                        else
                        {
                            Thread.Sleep(500);
                            return;  //如果从list中发现也是为空的就直接返回方法到Execute（），结束Job；       
                        }
                    }
                    else//队列不为空，就存到list中，直到队列为空，就停止存入list；
                    {
                        infos.Add(info);
                    }
                }
            }
        }

        /// <summary>
        /// 根据id更新糗事内容
        /// </summary>
        /// <param name="idQiuShi">本次队列中本条糗事id</param>
        /// <param name="num">本次队列中本条糗事点 “赞” 的总数</param>
        public static void UpdateSupportNum(long idQiuShi, long num)
        {
            UpdateMsg upOP = new UpdateMsg();
            Messages msgModel = upOP.GetMessageById(idQiuShi);
            msgModel.Id = idQiuShi;
            msgModel.SupportedNum += num;

            upOP.UpdateSupportedNum(msgModel);
        }

        /// <summary>
        /// 根据id更新糗事内容
        /// </summary>
        /// <param name="idQiuShi">本次队列中本条糗事id</param>
        /// <param name="num">本次队列中本条糗事点 “拍” 的总数</param>
        public static void UpdateOpposedNum(long idQiuShi, long num)
        {
            UpdateMsg upOP = new UpdateMsg();
            Messages msgModel = upOP.GetMessageById(idQiuShi);
            msgModel.Id = idQiuShi;
            msgModel.OpposedNum += num;
            upOP.UpdateOpposedNum(msgModel);

        }
        /// <summary>
        /// 判断这条糗事是赞还是拍
        /// </summary>
        /// <param name="str"></param>
        /// <param name="num">赞（拍）的总数</param>
        public static void Judge(string str, long num)
        {

            //创建数据上下文对象---本次请求数据库开始时候


            string[] idStute = str.Split(new char[] { '|' });
            //糗事id
            long idQiuShi = Convert.ToInt64(idStute[0]);
            if (idStute[1] == "1")//表赞
            {
                UpdateSupportNum(idQiuShi, num);
            }
            else if (idStute[1] == "0")//表拍
            {
                UpdateOpposedNum(idQiuShi, num);
            }
            else
            {
                throw new Exception("系统异常");
            }
            //销毁数据上下文对象---本次请求数据库结束时候

        }


    }
}
