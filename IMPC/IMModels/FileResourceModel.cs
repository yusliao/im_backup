using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMModels
{
    /// <summary>
    /// 文件资源模型
    /// </summary>
    public class FileResourceModel : BaseModel
    {
        public FileResourceModel(string fullName)
        {
            if (File.Exists(fullName))
            {
                FileInfo fileInfo = new FileInfo(fullName);
                this.FullName = fullName;
                this.FileName = fileInfo.Name;
                this.Length = fileInfo.Length;
            }
        }

        public FileResourceModel() : this(null)
        {
        }

        /// <summary>
        /// 引用信息
        /// </summary>
        public object RefInfo { get; set; }

        private string _key;
        /// <summary>
        /// 唯一标识键
        /// </summary>
        public string Key
        {
            get { return _key; }
            set { _key = value; this.OnPropertyChanged(); }
        }

        private string _smallKey;
        /// <summary>
        /// 缩略图标识
        /// </summary>
        public string SmallKey
        {
            get { return _smallKey; }
            set { _smallKey = value; this.OnPropertyChanged(); }
        }

        private object _fileImg;
        /// <summary>
        /// 文件缩略图
        /// </summary>
        public Object FileImg
        {
            get { return _fileImg; }
            set { _fileImg = value; this.OnPropertyChanged(); }
        }

        private string _fileName;
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; this.OnPropertyChanged(); }
        }

        private string _fullName;
        /// <summary>
        /// 文件名（全路径）
        /// </summary>
        public string FullName
        {
            get { return _fullName; }
            set { _fullName = value; this.OnPropertyChanged(); }
        }

        private long _length;
        /// <summary>
        /// 文件大小长度
        /// </summary>
        public long Length
        {
            get { return _length; }
            set { _length = value; this.OnPropertyChanged(); }
        }

        private long _completeLength;
        /// <summary>
        /// 完成（上传/下载）文件大小长度
        /// </summary>
        public long CompleteLength
        {
            get { return _completeLength; }
            set
            {
                _completeLength = value;
                this.OnPropertyChanged();
                if (Length > 0)
                {
                    _progress = Math.Round(Convert.ToDouble(_completeLength) / Convert.ToDouble(Length), 2);
                    this.OnPropertyChanged("Progress");
                }
            }
        }

        private double _progress;
        /// <summary>
        /// 上传、下载进度
        /// </summary>
        public double Progress
        {
            get { return _progress; }
            set { _progress = value; this.OnPropertyChanged(); }
        }

        private int _recordTime;
        /// <summary>
        /// 视频文件时长
        /// </summary>
        public int RecordTime
        {
            get { return _recordTime; }
            set { _recordTime = value; this.OnPropertyChanged(); }
        }

        private string _previewkey;
        /// <summary>
        /// 视频文件预览图标识
        /// </summary>
        public string PreviewKey
        {
            get { return _previewkey; }
            set { _previewkey = value; this.OnPropertyChanged(); }
        }

        private string _previewImagePath;
        /// <summary>
        /// 视频预览图本地全路径
        /// </summary>
        public string PreviewImagePath
        {
            get { return _previewImagePath; }
            set { _previewImagePath = value; this.OnPropertyChanged(); }
        }


        private int _dbState;
        /// <summary>
        /// 数据库中的状态
        /// fileState 0：未开始，1：下载中，2：已完成,3:取消，4：异常
        /// </summary>
        public int DBState
        {
            get { return _dbState; }
            set
            {
                if(value!= _dbState)
                {
                    _dbState = value;
                }

            }

        }

        private FileStates _fileState;
        /// <summary>
        /// 文件状态
        /// </summary>
        public FileStates FileState
        {
            get { return _fileState; }
            set { _fileState = value; this.OnPropertyChanged(); }
        }

    }
}
