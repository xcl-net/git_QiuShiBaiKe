using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QiuShiBaiKe.Model;
using System.Data.Entity;

namespace QiuShiBaiKe.DAL
{
    public class MessageDal
    {
        private QSBKDBEntities entity;
        public MessageDal(QSBKDBEntities entity_Input)
        {
            this.entity = entity_Input;
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
            Message message = new Message();
            message.ImageUrl = imageUrl;
            message.IsAnonymous = isAnonymous;
            message.Msg = msg;
            message.UserId = userId;
            message.CreateDateTime = DateTime.Now;
            entity.Messages.Add(message);
            entity.Entry(message).State = EntityState.Added;
            entity.SaveChanges();
            return message.Id;//EF可以返回新增的条目id是多少，很方便
            //保存后，自动增长字段就会通过id属性取到了
        }
        /// <summary>
        /// 根据糗事的id获得糗事model
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Message GetMessageById(long messageId)
        {
            return entity.Messages.Where(i => i.Id == messageId).FirstOrDefault();

        }
        /// <summary>
        /// 根据糗事id，查出糗事有几条评论
        /// </summary>
        /// <param name="idQiuShi"></param>
        /// <returns></returns>
        public long GetCountComments(long messageId)
        {
            return entity.MessageComments.Where(i => i.MessageId == messageId).LongCount();
        }
        /// <summary>
        /// 获取糗事的总条数
        /// </summary>
        /// <returns></returns>
        public long GetCountMessages()
        {
            return entity.Messages.LongCount();
        }
        /// <summary>
        /// 获得第pageIndex页的数据，每一页显示pageSize条
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<Message> GetMessage(int pageIndex, int pageSize)
        {
            return entity.Messages.OrderByDescending(i => i.CreateDateTime)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
        }

        /// <summary>
        /// 更新糗事model
        /// </summary>
        /// <param name="model"></param>
        public void Update(Message model)
        {
            entity.Entry(model).State = EntityState.Modified;
        }
    }
}
