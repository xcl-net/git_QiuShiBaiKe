using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QiuShiBaiKe.Model;
using QiuShiBaiKe.DAL;

namespace QiuShiBaiKe.BLL
{

    public class MessageCommentBll
    {
        private QSBKDBEntities entity;
        MessageCommentDal messageCommentDal;
        public MessageCommentBll(QSBKDBEntities entity_InPut)
        {
            this.entity = entity_InPut;
            messageCommentDal = new MessageCommentDal(entity_InPut);
        }
        /// <summary>
        /// 根据糗事的id查出来评论的集合对象
        /// </summary>
        /// <param name="idQiuShi"></param>
        /// <returns></returns>
        public IQueryable<MessageComment> GetCommentList(long idQiuShi)
        {
            return messageCommentDal.GetCommentList(idQiuShi);
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
            return messageCommentDal.Add(userId, commentTxt, idQiuShi);
        }
    }
}
