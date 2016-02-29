using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace QuiShiBaiKe.DingShi
{
    public class SqlHelper
    {
        //连接字符创
        //private static readonly string connstr = "Data Source=.;Initial Catalog=Persons;Persist Security Info=True;User ID=sa;Password=123";
        private static readonly string connstr = "Data Source=.;Initial Catalog=QSBKDB;User ID=sa;Password=123";
        //执行查询，返回一张二维表  -----------(查)
        public static DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connstr))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = sql;
                cmd.Parameters.AddRange(parameters);//连续添加
                DataTable dt = new DataTable();//实例化一个表
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);//查询完一条记录就加载到实例化的表dt中
                    return dt;
                }
            }
        }
        //返回受影响的行数，引用外部的，一个连接，和传进来的参数 ------------（增。删。改）
        public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connstr))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = sql;
                cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteNonQuery();//返回该语句受影响的行数
            }

        }
        public static object ExecuteScalar(string sql, params SqlParameter[] paramerters)
        {
            using (SqlConnection conn = new SqlConnection(connstr))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = sql;
                cmd.Parameters.AddRange(paramerters);
                return cmd.ExecuteScalar();//返回该语句查找的对象
            }
        }

    }
}