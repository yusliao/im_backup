using IMModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMClient.ViewModels
{
    public class FeedBackViewModel:ViewModel
    {
        private string _feedBackTitle;
        /// <summary>
        /// 反馈标题
        /// </summary>
        public string FeedBackTitle
        {
            get { return _feedBackTitle; }
            set { _feedBackTitle = value;this.OnPropertyChanged(); }
        }
        private string _feedBackContent;
        /// <summary>
        /// 反馈内容
        /// </summary>
        public string FeedBackContent
        {
            get { return _feedBackContent; }
            set { _feedBackContent = value; this.OnPropertyChanged(); }
        }

        public FeedBackViewModel(FeedBackModel model):base(model)
        {

        }
    }
    public class FeedBackModel:BaseModel
    { }
}
