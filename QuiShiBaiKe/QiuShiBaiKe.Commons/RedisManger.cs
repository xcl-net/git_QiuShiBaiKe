using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QiuShiBaiKe.Commons
{
    public class RedisManager
    {

        // public static string Name { set; get; }
        public static PooledRedisClientManager ClientManager { get; private set; }//表示只能在本类才可以设置，外部类都不能对这个属性进行设置；

        //构造函数
        static RedisManager()
        {
            RedisClientManagerConfig redisConfig = new RedisClientManagerConfig();//Redis的配置实例
            redisConfig.MaxWritePoolSize = 128;//涉及到Redis的优化，需要自己研究了
            redisConfig.MaxReadPoolSize = 128;
            //构造函数的参数：
            //读写分离：使服务器哪几台式可以读，哪几台可以写；服务器集群；
            ClientManager = new PooledRedisClientManager(new string[] { "127.0.0.1" }, new string[] { "127.0.0.1" }, redisConfig);//对属性赋值
        }
    }
}
