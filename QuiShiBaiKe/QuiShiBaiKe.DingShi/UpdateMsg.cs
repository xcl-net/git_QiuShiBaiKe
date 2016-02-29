
using QuiShiBaiKe.DingShi;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QiuShiBaiKe.DingShi
{
    public class UpdateMsg
    {

        /// <summary>
        /// 更新糗事model赞
        /// </summary>
        /// <param name="model"></param>
        public int UpdateSupportedNum(Messages msgModel)
        {
            string sql = "update Messages set SupportedNum=@num where Id=@id ";
            SqlParameter[] parameters = new SqlParameter[] { 
                new SqlParameter("@num", msgModel.SupportedNum), 
                new SqlParameter("@id", msgModel.Id) };
            return SqlHelper.ExecuteNonQuery(sql, parameters);

        }

        /// <summary>
        /// 更新糗事model拍
        /// </summary>
        /// <param name="msgModel"></param>
        public int UpdateOpposedNum(Messages msgModel)
        {
            string sql = "update Messages set OpposedNum=@num where Id=@id ";
            SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@num", msgModel.OpposedNum),
                new SqlParameter("@id", msgModel.Id) };
            return SqlHelper.ExecuteNonQuery(sql, parameters);
        }


        /// <summary>
        /// 根据糗事的id获得糗事model
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Messages GetMessageById(long messageId)
        {
            string cmdText = "select Id, Msg, UserId, IsAnonymous, ImageUrl, CreateDateTime, SupportedNum, OpposedNum from Messages where Id=@id ";
            SqlParameter param = new SqlParameter("@id", messageId);
            Messages msgModel = new Messages();
            DataTable record = SqlHelper.ExecuteQuery(cmdText, param);//一条条记录的查询
            DataRow row = record.Rows[0];
            if (record.Rows.Count != 0)
            {
                msgModel.Id = Convert.ToInt64(row["Id"]);
                msgModel.OpposedNum = Convert.ToInt64(row["OpposedNum"]);
                msgModel.SupportedNum = Convert.ToInt64(row["SupportedNum"]);
                msgModel.Msg = Convert.ToString(row["Msg"]);
                msgModel.UserId = Convert.ToInt64(row["UserId"]);
                if (row["IsAnonymous"].ToString().ToLower() == "true")//不为空
                {
                    msgModel.IsAnonymous = true;
                }
                else
                {
                    msgModel.IsAnonymous = false;
                }
                msgModel.ImageUrl = Convert.IsDBNull(row["ImageUrl"]) ? null : row["ImageUrl"].ToString();
                msgModel.CreateDateTime = DateTime.Parse(row["CreateDateTime"].ToString());
                return msgModel;
            }
            return null;
        }
    }
}
