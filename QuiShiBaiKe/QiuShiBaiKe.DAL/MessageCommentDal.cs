
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QiuShiBaiKe.Model;
using System.Data.Entity;

namespace QiuShiBaiKe.DAL
{
    public class MessageCommentDal
    {
        private QSBKDBEntities entity;
        public MessageCommentDal(QSBKDBEntities entity_InPut)
        {
            this.entity = entity_InPut;
        }
        /// <summary>
        /// 根据糗事的id查出来评论的集合对象
        /// </summary>
        /// <param name="idQiuShi"></param>
        /// <returns></returns>
        public IQueryable<MessageComment> GetCommentList(long idQiuShi)
        {
            IQueryable<MessageComment> comments = entity.MessageComments.Where(i => i.MessageId == idQiuShi);
            return comments;
        }
        /// <summary>
        /// 新增一条评论，返回新增评论的id
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="commentTxt"></param>
        /// <param name="idQiuShi"></param>
        /// <returns></returns>
        public long Add(long userId, string commentTxt, long idQiuShi)
        {
            MessageComment comment = new MessageComment();
            comment.UserId = userId;
            comment.CreateDateTime = DateTime.Now;
            comment.MessageId = idQiuShi;
            comment.Comment = commentTxt;
            entity.MessageComments.Add(comment);
            entity.Entry(comment).State = EntityState.Added;
            entity.SaveChanges();
            return comment.Id;
        }
    }
}
