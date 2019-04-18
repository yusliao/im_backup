using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMModels
{
    public class PersonCardModel:BaseModel
    {
        private int _userId;
        /// <summary>
        /// 名片用户Id
        /// </summary>
        public int UserId
        {
            get { return _userId; }
            set { _userId = value; this.OnPropertyChanged(); }
        }

        private string _name;
        /// <summary>
        /// 名片用户名字
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value;this.OnPropertyChanged(); }
        }

        private string _photoImg;
        /// <summary>
        /// 名片用户图片
        /// </summary>
        public string PhotoImg
        {
            get { return _photoImg; }
            set { _photoImg = value;this.OnPropertyChanged(); }
        }

        private string _phoneNumber;
        /// <summary>
        /// 名片用户的电话号码
        /// </summary>
        public string PhoneNumber
        {
            get { return _phoneNumber; }
            set { _phoneNumber = value; }
        }
            
    }
}
