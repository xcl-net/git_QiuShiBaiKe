using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QiuShiBaiKe.Model;
using QiuShiBaiKe.DAL;
using QiuShiBaiKe.Commons;

namespace QiuShiBaiKe.BLL
{
    public class UserBll
    {
        //1. 声明私有变量
        private QSBKDBEntities entity;
        private UserDal userDal;
        //2. 使用构造函数，初始化类
        public UserBll(QSBKDBEntities entity_Input)
        {
            this.entity = entity_Input;
            //3. Dal层使用同一个数据上下文对象
            userDal = new UserDal(entity_Input);
        }

        //4. 检查用户是否可以登录
        public bool PermitLogin(string name, string password)
        {
            var userModel = GetByUserName(name);
            if (userModel == null)//数据库中没有记录
            {
                return false;
            }
            string pwd = CommonHelper.CalcMD5(password);
            return pwd == userModel.PassWordHash;
        }
        //5. 根据用户名查出用户的model
        public User GetByUserName(string name)
        {
            return userDal.GetByUserName(name);
        }

        //一. Add方法实现
        public AddResult Add(string username, string password)
        {
            //1.检查用户名是否有违禁词汇
            string[] forbidenWords = { "打倒", "管理员", "毛泽东", "毛主席", "admin" };
            bool isIncludeForbidenWords = false;
            foreach (var item in forbidenWords)
            {
                if (username.Contains(item))
                {
                    isIncludeForbidenWords = true;
                    break;
                }
            }
            if (isIncludeForbidenWords)
            {
                return AddResult.UserNameForbiden;
            }
            //2. 检查用户名长度
            if (username.Length < 3 || username.Length > 30)
            {
                return AddResult.UserNameLengthInValid;
            }
            //3. 对密码进行md5加密处理
            string passwordHash = CommonHelper.CalcMD5(password);
            //3. 检查用户名是否存在,使用插入方法中的检查
            bool isExite = userDal.Insert(username, passwordHash);
            return isExite ? AddResult.OK : AddResult.UserNameExist;
        }

    }
    public enum AddResult
    {
        OK,
        UserNameExist,//用户名已经存在
        UserNameForbiden,//用户名中含有禁用词
        UserNameLengthInValid//用户名长度不符合要求
    }

}
