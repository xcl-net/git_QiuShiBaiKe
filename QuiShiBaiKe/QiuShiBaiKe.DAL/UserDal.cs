using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QiuShiBaiKe.Model;
using System.Data.Entity;

namespace QiuShiBaiKe.DAL
{
    public class UserDal
    {
        //1. 声明私有变量
        public QSBKDBEntities entity;
        //2. 使用构造函数，初始化类
        public UserDal(QSBKDBEntities entity_Input)
        {
            this.entity = entity_Input;
        }
        //3. 根据用户名查出用户的model
        public User GetByUserName(string name)
        {
            var userModel = entity.Users.Where(i => i.UserName == name).FirstOrDefault();
            return userModel;
        }

        //4. 检查用户是否存在，不存在则新增用户
        public bool Insert(string name, string pwd)
        {
            bool isExist = entity.Users.Where(i => i.UserName == name).Any();
            if (isExist)
            {
                return false;
            }
            else
            {
                User user = new User { UserName = name, PassWordHash = pwd };
                entity.Users.Add(user);
                entity.Entry(user).State = EntityState.Added;
                entity.SaveChanges();
                return true;
            }
        }
    }
}
