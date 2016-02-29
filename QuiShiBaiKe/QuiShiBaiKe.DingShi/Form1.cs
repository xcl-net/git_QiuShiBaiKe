using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QiuShiBaiKe.Model;
using System.Web;

namespace QuiShiBaiKe.DingShi
{
    public partial class Form1 : Form
    {
        public static IScheduler sched;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //如果写在项目中，“定时代码”要放在Application_Start()方法中；

            #region 定时发送，修改数据库中的 “赞” 的数值(不用定时了)

            /*
            ISchedulerFactory sf = new StdSchedulerFactory(); //创建一个计划工厂
            sched = sf.GetScheduler();     //创建一个计划着
            JobDetail job = new JobDetail("job1", "group1", typeof(increaseNum));//MyJog为实现了IJob接口的类
            DateTime ts = TriggerUtils.GetNextGivenSecondDate(null, 1);//1秒后开始
            TimeSpan interval = TimeSpan.FromSeconds(1);//开始间隔10秒，依次发送   //创建定时

            Trigger trigger = new SimpleTrigger("trigger1", "group1", "job1", "group1", ts, null,
                                                    SimpleTrigger.RepeatIndefinitely, interval);//每若干小时运行一次，小时间隔由appsettings中的IndexIntervalHour参数指定

            //Timer：更加精准。
            sched.AddJob(job, true);
            sched.ScheduleJob(trigger);
            sched.Start();
             *   */
            #endregion

            increaseNum.Start();//点赞或拍类

            MsgIndexer.Start();//糗事索引类

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //窗口关闭后， 做的操作
            //1. 停止while循环操作
            increaseNum.IsRuning = false;
        }


    }
}
