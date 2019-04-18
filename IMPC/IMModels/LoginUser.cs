using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMModels
{
    /// <summary>
    /// 登录用户模型
    /// </summary>
    public class LoginUser:BaseModel
    {
        /// <summary>
        /// 登录对象
        /// </summary>
        /// <param name="user"></param>
        public LoginUser(UserModel user)
        {
            if (user != null)
            {
                this.ID = user.ID;
                this.User = user;
            }
        } 
        /// <summary>
        /// 对应的用户
        /// </summary>
        public UserModel User { get; } 

        private string _password;
        /// <summary>
        /// 登陆密码
        /// </summary>
        public string Password
        {
            get { return _password; }
            set { _password = value; this.OnPropertyChanged(); }
        }

        private bool _isSavePassword;
        /// <summary>
        /// 是否记住密码
        /// </summary>
        public bool IsSavePassword
        {
            get { return _isSavePassword; }
            set { _isSavePassword = value; this.OnPropertyChanged(); }
        }

        
        private bool _isOnline;
        /// <summary>
        /// 是否在线（断线状态）
        /// </summary>
        public bool IsOnline
        {
            get { return _isOnline; }
            set { _isOnline = value; this.OnPropertyChanged(); }
        }

    }
}
