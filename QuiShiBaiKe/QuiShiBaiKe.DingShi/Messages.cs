
using System;
namespace QuiShiBaiKe.DingShi
{
    /// <summary>
    /// Messages:实体类(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [Serializable]
    public partial class Messages
    {

        #region Model
        private long _id;
        private string _msg;
        private long _userid;
        private bool _isanonymous;
        private string _imageurl;
        private DateTime _createdatetime;
        private long _supportednum;
        private long _opposednum;
        /// <summary>
        /// 
        /// </summary>
        public long Id
        {
            set { _id = value; }
            get { return _id; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string Msg
        {
            set { _msg = value; }
            get { return _msg; }
        }
        /// <summary>
        /// 
        /// </summary>
        public long UserId
        {
            set { _userid = value; }
            get { return _userid; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsAnonymous
        {
            set { _isanonymous = value; }
            get { return _isanonymous; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string ImageUrl
        {
            set { _imageurl = value; }
            get { return _imageurl; }
        }
        /// <summary>
        /// 
        /// </summary>
        public DateTime CreateDateTime
        {
            set { _createdatetime = value; }
            get { return _createdatetime; }
        }
        /// <summary>
        /// 
        /// </summary>
        public long SupportedNum
        {
            set { _supportednum = value; }
            get { return _supportednum; }
        }
        /// <summary>
        /// 
        /// </summary>
        public long OpposedNum
        {
            set { _opposednum = value; }
            get { return _opposednum; }
        }
        #endregion Model

    }
}

