using QiuShiBaiKe.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QiuShiBaiKe.DAL;

namespace QiuShiBaiKe.BLL
{
    public class MessageBll
    {
        private QSBKDBEntities entity;
        MessageDal messageDal;
        public MessageBll(QSBKDBEntities entity_Input)
        {
            this.entity = entity_Input;
            messageDal = new MessageDal(entity_Input);
        }
        /// <summary>
        /// 新增的糗事
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="msg">内容</param>
        /// <param name="isAnonymous">匿名</param>
        /// <param name="imageUrl">图片链接</param>
        /// <returns>新增糗事id</returns>
        public long Add(long userId, string msg, bool isAnonymous, string imageUrl)
        {
            return messageDal.Add(userId, msg, isAnonymous, imageUrl);
        }
        /// <summary>
        /// 根据糗事的id获得糗事model
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Message GetMessageById(long messageId)
        {
            return messageDal.GetMessageById(messageId);
        }
        /// <summary>
        /// 根据糗事id，查出糗事有几条评论
        /// </summary>
        /// <param name="idQiuShi"></param>
        /// <returns></returns>
        public long GetCountComments(long messageId)
        {
            return messageDal.GetCountComments(messageId);
        }

        /// <summary>
        /// 获取糗事的总条数
        /// </summary>
        /// <returns></returns>
        public long GetCountMessages()
        {
            return messageDal.GetCountMessages();
        }
        /// <summary>
        /// 获得第pageIndex页的数据，每一页显示pageSize条
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<Message> GetMessage(int pageIndex, int pageSize)
        {
            return messageDal.GetMessage(pageIndex, pageSize);
        }


        /// <summary>
        /// 更新糗事model
        /// </summary>
        /// <param name="model"></param>
        public void Update(Message model)
        {
            messageDal.Update(model);
        }
    }
}
