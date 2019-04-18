using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Threading;

namespace IM.Emoje.Controls
{
    public class GifImageExceptionRoutedEventArgs : RoutedEventArgs
    {
        public Exception ErrorException;

        public GifImageExceptionRoutedEventArgs(RoutedEvent routedEvent, object obj)
            : base(routedEvent, obj)
        {
        }
    }

    class WebReadState
    {
        public WebRequest webRequest;
        public MemoryStream memoryStream;
        public Stream readStream;
        public byte[] buffer;
    }

    public class ImageFrame
    {
        public int Delay { get; set; }
        public string Path { get; set; }
        public BitmapSource BSource { get; set; }

        public Stream Stream { get; set; }

    }

    internal class GifImageDeal
    {

        public string Source;

        public GifImageDeal(string source)
        {
            //System.Drawing.ImageAnimator im = new System.Drawing.ImageAnimator();

            this.Source = source;
            this.CreateFromSourceString(source);
        }


        public List<ImageFrame> Frames;

        public event Action<List<ImageFrame>> FramesLoaded;

        private void SetImageFrames(MemoryStream memoryStream)
        {
            Frames = new List<ImageFrame>();

            System.Drawing.Image gif = System.Drawing.Image.FromStream(memoryStream);

            FrameDimension fd = new FrameDimension(gif.FrameDimensionsList[0]);
            int count = gif.GetFrameCount(fd);

            byte[] delayByte = new byte[4];//延迟时间，以1/100秒为单位

            for (int i = 0; i < count; i++)
            {
                gif.SelectActiveFrame(fd, i);
                MemoryStream ms = new MemoryStream();
                {
                    gif.Save(ms, ImageFormat.Png);

                    BitmapImage imgSource = new BitmapImage();
                    imgSource.BeginInit();
                    imgSource.StreamSource = ms;
                    imgSource.EndInit();

                    imgSource.Freeze();
                    //BitmapImage imgSource = new BitmapImage(new Uri(path));
                    ImageFrame frame = new ImageFrame() { BSource = imgSource, Stream = ms };
                    Frames.Add(frame);

                    for (int j = 0; j < gif.PropertyIdList.Length; j++)//遍历帧属性
                    {
                        if ((int)gif.PropertyIdList.GetValue(j) == 0x5100)//.如果是延迟时间
                        {
                            PropertyItem pItem = (PropertyItem)gif.PropertyItems.GetValue(j);//获取延迟时间属性

                            delayByte[0] = pItem.Value[i * 4];
                            delayByte[1] = pItem.Value[1 + i * 4];
                            delayByte[2] = pItem.Value[2 + i * 4];
                            delayByte[3] = pItem.Value[3 + i * 4];
                            int delay = BitConverter.ToInt32(delayByte, 0) * 10;
                            frame.Delay = delay <= 0 ? 100 : delay;
                            break;
                        }
                    }
                }
            }

            gif = null;
            fd = null;
            delayByte = null;

            FramesLoaded?.Invoke(Frames);
        }

        //private void SetImageFrames1(MemoryStream memoryStream)
        //{
        //    frames.Clear();

        //    using (memoryStream)
        //    {
        //        System.Drawing.Image gif = System.Drawing.Image.FromStream(memoryStream);
        //        FrameDimension fd = new FrameDimension(gif.FrameDimensionsList[0]);
        //        //int count = gif.GetFrameCount(fd);
        //        byte[] gifData = memoryStream.GetBuffer();
        //        Uri rui = new Uri(this._source);
        //        GifBitmapDecoder decoder = new GifBitmapDecoder(memoryStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
        //        int count= decoder.Frames.Count;

        //        byte[] delayByte = new byte[4];//延迟时间，以1/100秒为单位

        //        for (int i = 0; i < count; i++)
        //        {
        //            var source = decoder.Frames[i];

        //            source.Freeze();
        //            ImageFrame frame = new ImageFrame() { BSource = source ,Delay=1000};
        //            frames.Add(frame);


        //            //gif.SelectActiveFrame(fd, i);



        //            //for (int j = 0; j < gif.PropertyIdList.Length; j++)//遍历帧属性
        //            //{
        //            //    if ((int)gif.PropertyIdList.GetValue(j) == 0x5100)//.如果是延迟时间
        //            //    {
        //            //        PropertyItem pItem = (PropertyItem)gif.PropertyItems.GetValue(j);//获取延迟时间属性

        //            //        delayByte[0] = pItem.Value[i * 4];
        //            //        delayByte[1] = pItem.Value[1 + i * 4];
        //            //        delayByte[2] = pItem.Value[2 + i * 4];
        //            //        delayByte[3] = pItem.Value[3 + i * 4];
        //            //        int delay = BitConverter.ToInt32(delayByte, 0) * 10; //乘以10，获取到毫秒 
        //            //        frame.Delay = delay;
        //            //        break;
        //            //    }
        //            //}
        //        }
        //    }
        //}

        //private void CreateNonGifAnimationImage()
        //{
        //    this.Source = (System.Windows.Media.ImageSource)(new System.Windows.Media.ImageSourceConverter().ConvertFromString(_source));
        //} 

        private void CreateFromSourceString(string source)
        {
            if (File.Exists(source))
            {
                // 打开文件 
                FileStream fileStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read);
                // 读取文件的 byte[] 
                byte[] bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, bytes.Length);
                fileStream.Close();

                ReadGifStreamSynch(new MemoryStream(bytes));
                return;
            }

            Uri uri;

            try
            {
                uri = new Uri(source, UriKind.RelativeOrAbsolute);
            }
            catch
            {
                return;
            }

            if (source.Trim().ToUpper().EndsWith(".GIF"))
            {
                if (!uri.IsAbsoluteUri)
                {
                    GetGifStreamFromPack(uri);
                }
                else
                {
                    string leftPart = uri.GetLeftPart(UriPartial.Scheme);

                    if (leftPart == "http://" || leftPart == "ftp://" || leftPart == "file://")
                    {
                        GetGifStreamFromHttp(uri);
                    }
                    else if (leftPart == "pack://")
                    {
                        GetGifStreamFromPack(uri);
                    }
                    else
                    {
                        //CreateNonGifAnimationImage();
                    }
                }
            }
            else
            {
                //CreateNonGifAnimationImage();
            }
        }


        private delegate void WebRequestFinishedDelegate(MemoryStream memoryStream);

        private void WebRequestFinished(MemoryStream memoryStream)
        {
            SetImageFrames(memoryStream);
        }

        private void WebResponseCallback(IAsyncResult asyncResult)
        {
            WebReadState webReadState = (WebReadState)asyncResult.AsyncState;
            WebResponse webResponse;
            try
            {
                webResponse = webReadState.webRequest.EndGetResponse(asyncResult);
                webReadState.readStream = webResponse.GetResponseStream();
                webReadState.buffer = new byte[100000];
                webReadState.readStream.BeginRead(webReadState.buffer, 0, webReadState.buffer.Length, new AsyncCallback(WebReadCallback), webReadState);
            }
            catch (WebException exp)
            {
                throw exp;
            }
        }

        private void WebReadCallback(IAsyncResult asyncResult)
        {
            WebReadState webReadState = (WebReadState)asyncResult.AsyncState;
            int count = webReadState.readStream.EndRead(asyncResult);
            if (count > 0)
            {
                webReadState.memoryStream.Write(webReadState.buffer, 0, count);
                try
                {
                    webReadState.readStream.BeginRead(webReadState.buffer, 0, webReadState.buffer.Length, new AsyncCallback(WebReadCallback), webReadState);
                }
                catch (WebException exp)
                {
                    throw exp;
                }
            }
            else
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, new WebRequestFinishedDelegate(WebRequestFinished), webReadState.memoryStream);
            }
        }

        private void GetGifStreamFromHttp(Uri uri)
        {
            try
            {
                WebReadState webReadState = new WebReadState();
                webReadState.memoryStream = new MemoryStream();
                webReadState.webRequest = WebRequest.Create(uri);
                webReadState.webRequest.Timeout = 10000;

                webReadState.webRequest.BeginGetResponse(new AsyncCallback(WebResponseCallback), webReadState);
            }
            catch (System.Security.SecurityException)
            {
                //CreateNonGifAnimationImage();
            }
        }
        private void ReadGifStreamSynch(Stream s)
        {
            byte[] gifData;
            MemoryStream memoryStream;
            using (s)
            {
                memoryStream = new MemoryStream((int)s.Length);
                BinaryReader br = new BinaryReader(s);
                gifData = br.ReadBytes((int)s.Length);
                memoryStream.Write(gifData, 0, (int)s.Length);
                memoryStream.Flush();
            }
            SetImageFrames(memoryStream);
        }

        #region Parse Gif

        private void ParseGifDataStream(byte[] gifData, int offset)
        {
            offset = ParseHeader(gifData, offset);
            offset = ParseLogicalScreen(gifData, offset);
            while (offset != -1)
            {
                offset = ParseBlock(gifData, offset);
            }
        }

        private int ParseHeader(byte[] gifData, int offset)
        {
            string str = System.Text.ASCIIEncoding.ASCII.GetString(gifData, offset, 3);
            if (str != "GIF")
            {
                throw new Exception("Not a proper GIF file: missing GIF header");
            }
            return 6;
        }

        private int ParseLogicalScreen(byte[] gifData, int offset)
        {
            int logicalWidth = BitConverter.ToUInt16(gifData, offset);
            int logicalHeight = BitConverter.ToUInt16(gifData, offset + 2);

            byte packedField = gifData[offset + 4];
            bool hasGlobalColorTable = (int)(packedField & 0x80) > 0 ? true : false;

            int currentIndex = offset + 7;
            if (hasGlobalColorTable)
            {
                int colorTableLength = packedField & 0x07;
                colorTableLength = (int)Math.Pow(2, colorTableLength + 1) * 3;
                currentIndex = currentIndex + colorTableLength;
            }
            return currentIndex;
        }

        private int ParseBlock(byte[] gifData, int offset)
        {
            switch (gifData[offset])
            {
                case 0x21:
                    if (gifData[offset + 1] == 0xF9)
                    {
                        return ParseGraphicControlExtension(gifData, offset);
                    }
                    else
                    {
                        return ParseExtensionBlock(gifData, offset);
                    }
                case 0x2C:
                    offset = ParseGraphicBlock(gifData, offset);
                    return offset;
                case 0x3B:
                    return -1;
                default:
                    throw new Exception("GIF format incorrect: missing graphic block or special-purpose block. ");
            }
        }

        private int ParseGraphicControlExtension(byte[] gifData, int offset)
        {
            int returnOffset = offset;
            int length = gifData[offset + 2];
            returnOffset = offset + length + 2 + 1;

            //byte packedField = gifData[offset + 3];
            //currentParseGifFrame.disposalMethod = (packedField & 0x1C) >> 2;

            int delay = BitConverter.ToUInt16(gifData, offset + 4);
            //currentParseGifFrame.delayTime = delay;
            while (gifData[returnOffset] != 0x00)
            {
                returnOffset = returnOffset + gifData[returnOffset] + 1;
            }

            returnOffset++;

            return returnOffset;
        }

        private int ParseGraphicBlock(byte[] gifData, int offset)
        {
            //currentParseGifFrame.left = BitConverter.ToUInt16(gifData, offset + 1);
            //currentParseGifFrame.top = BitConverter.ToUInt16(gifData, offset + 3);
            //currentParseGifFrame.width = BitConverter.ToUInt16(gifData, offset + 5);
            //currentParseGifFrame.height = BitConverter.ToUInt16(gifData, offset + 7);
            //if (currentParseGifFrame.width > logicalWidth)
            //{
            //    logicalWidth = currentParseGifFrame.width;
            //}
            //if (currentParseGifFrame.height > logicalHeight)
            //{
            //    logicalHeight = currentParseGifFrame.height;
            //}
            byte packedField = gifData[offset + 9];
            bool hasLocalColorTable = (int)(packedField & 0x80) > 0 ? true : false;

            int currentIndex = offset + 9;
            if (hasLocalColorTable)
            {
                int colorTableLength = packedField & 0x07;
                colorTableLength = (int)Math.Pow(2, colorTableLength + 1) * 3;
                currentIndex = currentIndex + colorTableLength;
            }
            currentIndex++;

            currentIndex++;

            while (gifData[currentIndex] != 0x00)
            {
                int length = gifData[currentIndex];
                currentIndex = currentIndex + gifData[currentIndex];
                currentIndex++;
            }
            currentIndex = currentIndex + 1;
            return currentIndex;
        }

        private int ParseExtensionBlock(byte[] gifData, int offset)
        {
            int returnOffset = offset;
            int length = gifData[offset + 2];
            returnOffset = offset + length + 2 + 1;
            if (gifData[offset + 1] == 0xFF && length > 10)
            {
                string netscape = System.Text.ASCIIEncoding.ASCII.GetString(gifData, offset + 3, 8);
                if (netscape == "NETSCAPE")
                {
                    int numberOfLoops = BitConverter.ToUInt16(gifData, offset + 16);
                    if (numberOfLoops > 0)
                    {
                        numberOfLoops++;
                    }
                }
            }
            while (gifData[returnOffset] != 0x00)
            {
                returnOffset = returnOffset + gifData[returnOffset] + 1;
            }

            returnOffset++;

            return returnOffset;
        }

        #endregion

        private void GetGifStreamFromPack(Uri uri)
        {
            try
            {
                StreamResourceInfo streamInfo;

                if (!uri.IsAbsoluteUri)
                {
                    streamInfo = Application.GetContentStream(uri);
                    if (streamInfo == null)
                    {
                        streamInfo = Application.GetResourceStream(uri);
                    }
                }
                else
                {
                    if (uri.GetLeftPart(UriPartial.Authority).Contains("siteoforigin"))
                    {
                        streamInfo = Application.GetRemoteStream(uri);
                    }
                    else
                    {
                        streamInfo = Application.GetContentStream(uri);
                        if (streamInfo == null)
                        {
                            streamInfo = Application.GetResourceStream(uri);
                        }
                    }
                }
                if (streamInfo == null)
                {
                    throw new FileNotFoundException("Resource not found.", uri.ToString());
                }
                ReadGifStreamSynch(streamInfo.Stream);
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }
    }
}
