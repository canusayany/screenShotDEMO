using Dicom;
using Dicom.Imaging;
using Dicom.IO.Buffer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScreenShotSaveTool
{
    public class SaveHelper
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// 保存图片到文件
        /// </summary>
        /// <param name="image">图片数据</param>
        /// <param name="filePath">保存路径</param>
        private void SaveImageToFile(BitmapSource image, string filePath)
        {
            BitmapEncoder encoder = new PngBitmapEncoder(); ;
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(stream);
            }
        }
        public static void SaveBitmapSource(BitmapSource bitmapsource,string path)
        {
            BitmapEncoder encoder = new PngBitmapEncoder(); ;
            encoder.Frames.Add(BitmapFrame.Create(bitmapsource));
            using (var stream = new FileStream(path, FileMode.Create))
            {
                encoder.Save(stream);
            }
        }
        /// <summary>
        /// 获取像素点信息
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static byte[] GetPixels(Bitmap bitmap)
        {
            byte[] bytes = new byte[bitmap.Width * bitmap.Height * 3];
            int wide = bitmap.Width;
            int i = 0;
            int height = bitmap.Height;
            for (int y = 0; y < height; y++)//20210312        
            {

                for (int x = 0; x < wide; x++)//20210312
                {
                    var srcColor = bitmap.GetPixel(x, y);

                    bytes[i] = srcColor.R;
                    i++;
                    bytes[i] = srcColor.G;
                    i++;
                    bytes[i] = srcColor.B;
                    i++;
                }
            }
            byte[] bytess = new byte[bitmap.Width * bitmap.Height * 3];//20210311

            for (int j = 0; j < bytess.Length; j++)
            {
                bytess[j] = bytes[j];
            }
            return bytess;
        }
        /// <summary>
        ///根据传入信息 创建新的dicom对象
        /// </summary>
        /// <param name="bitmap">图像</param>
        /// <param name="dicomFile">要使用的基础dicom</param>
        /// <param name="keyValuePairs">要更新的tag以及值</param>
        /// <returns></returns>
        public static DicomFile CreatDicom(Bitmap bitmap, DicomFile dicomFile, Dictionary<DicomTag, dynamic> keyValuePairs)
        {
            DicomDataset dataset = dicomFile.Dataset;
            dataset.AddOrUpdate(DicomTag.Rows, (ushort)bitmap.Height);
            dataset.AddOrUpdate(DicomTag.Columns, (ushort)bitmap.Width);
            dataset.AddOrUpdate(DicomTag.WindowCenter, "128");
            dataset.AddOrUpdate(DicomTag.WindowWidth, "256");
            dataset.AddOrUpdate(DicomTag.SeriesDate, System.DateTime.Now.Date.ToString("yyyyMMdd"));
            dataset.AddOrUpdate(DicomTag.SeriesTime, System.DateTime.Now.ToString("HHmmss"));
            dataset.AddOrUpdate(DicomTag.PhotometricInterpretation, PhotometricInterpretation.Rgb.Value);
            string speWord = "ss";
            dataset.TryGetValues<string>(DicomTag.SeriesNumber, out string[] seriesNumbers);
            string seriesNumber = "";
            if (seriesNumber != null)
            {
                seriesNumber = seriesNumbers[0] + "01";
            }
            dataset.AddOrUpdate(DicomTag.SeriesNumber, seriesNumber);

            dataset.TryGetValues<string>(DicomTag.ProtocolName, out string[] ProtocolNames);
            string ProtocolName = "";
            if (seriesNumber != null)
            {
                seriesNumber = ProtocolNames[0] + speWord;
            }
            dataset.AddOrUpdate(DicomTag.ProtocolName, ProtocolName);

            dataset.TryGetValues<string>(DicomTag.SeriesDescription, out string[] SeriesDescriptions);
            string SeriesDescription = "";
            if (seriesNumber != null)
            {
                SeriesDescription = SeriesDescriptions[0] + speWord;
            }
            dataset.AddOrUpdate(DicomTag.SeriesDescription, SeriesDescription);


            foreach (var item in keyValuePairs)
            {
                dataset.AddOrUpdate(item.Key, item.Value);
            }
            byte[] pixels = GetPixels(bitmap);
            MemoryByteBuffer buffer = new MemoryByteBuffer(pixels);
            DicomPixelData pixelData = DicomPixelData.Create(dataset, true);
            pixelData.BitsStored = 8;
            //pixelData.BitsAllocated = 8;
            pixelData.SamplesPerPixel = 3;
            pixelData.HighBit = 7;
            pixelData.PixelRepresentation = 0;
            pixelData.PlanarConfiguration = 0;
            pixelData.AddFrame(buffer);
            DicomFile dicomfile = new DicomFile(dataset);
            bitmap.Dispose();
            return dicomfile;
        }
        /// <summary>
        /// 截图 并且放入文件夹png格式的图片,在data放入dicom图
        /// </summary>
        /// <param name="rect">截图范围</param>
        /// <param name="baseDicomPath">基础dicom存放路径 需要一张正在展示的dicom作为基础 使用其tag</param>
        /// <param name="keyValuePairs">需要修改的tag 以及值 例如 Dictionary<DicomTag, dynamic> keyValuePairs = new Dictionary<DicomTag, dynamic>();keyValuePairs.Add(DicomTag.SeriesInstanceUID, GetSeriesInstanceUID());</param>
        /// <param name="iisPath">png存放目录 例如 @"E:\文档\WeChat Files"</param>
        /// <param name="studyID">当前检查的studyID</param>
        /// <returns>成功与否</returns>
        public static bool StartScreenshot(Rect rect, string baseDicomPath, Dictionary<DicomTag, dynamic> keyValuePairs, string iisPath, string studyID)
        {
            try
            {
                //BitmapSource bitmapSource2=     Screenshot.Screenshot.CaptureRegion();
                //SaveBitmapSource(bitmapSource2, iisPath + "\\" + DateTime.Now.ToString("HHmmss.ffff") + ".png");

                BitmapSource bitmapSource = Screenshot.Screenshot.StartScreenshot(rect);
                if (!Directory.Exists(iisPath))
                {
                    Directory.CreateDirectory(iisPath);
                }
                SaveBitmapSource(bitmapSource, iisPath + "\\" + DateTime.Now.ToString("HHmmss") + ".png");


                Bitmap bitmap = new Bitmap(iisPath + "\\" + DateTime.Now.ToString("HHmmss") + ".png");
                DicomFile baseDicomFile = null;
                DicomFile newDicomFile = null;
                if (System.IO.File.Exists(baseDicomPath))
                {
                    baseDicomFile = DicomFile.Open(baseDicomPath);
                }
                else
                {
                    log.Error($"基础dicom找不到,传入参数为{baseDicomPath}");
                    return false;
                }
                newDicomFile = CreatDicom(bitmap, baseDicomFile, keyValuePairs);
                string DataPath = @"E:\Data\";
                string seriesID = Guid.NewGuid().ToString();
                string savedDicomPath = DataPath + "Study_" + studyID + "\\Series_" + seriesID + "\\1_1_time_" + DateTime.Now.ToString("yyyyMMdd-HH-mm-ss-fff") + ".dcm";
                string savedDicomFoldPath = DataPath + "Study_" + studyID + "\\Series_" + seriesID;
                Directory.CreateDirectory(savedDicomFoldPath);
                newDicomFile.Save(savedDicomPath);
                InsertDB(newDicomFile, DataPath, seriesID, studyID);
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"截图错误", ex);
                return false;
            }

        }

        private static void InsertDB(DicomFile newDicomFile, string DataPath, string seriesID,string studyID)
        {
            string savedDicomFoldPath = DataPath + "Study_" + newDicomFile.Dataset.GetSingleValue<string>(DicomTag.StudyID) + "\\Series_" + seriesID;
            DicomDataset dicomDataset = newDicomFile.Dataset;
          //  string StudyID = dicomDataset.GetSingleValue<string>(DicomTag.StudyID);
            string SeriesNumber = dicomDataset.GetSingleValue<string>(DicomTag.SeriesNumber);
            string SeriesDescription = dicomDataset.GetSingleValue<string>(DicomTag.SeriesDescription);
            string SeriesInstanceUID = dicomDataset.GetSingleValue<string>(DicomTag.SeriesInstanceUID);
          
            DateTime date = System.DateTime.ParseExact(dicomDataset.GetSingleValue<string>(DicomTag.SeriesDate), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
            DateTime time = System.DateTime.ParseExact(dicomDataset.GetSingleValue<string>(DicomTag.SeriesTime), "HHmmss", System.Globalization.CultureInfo.CurrentCulture);
            string sql = $"Insert into `t_series` (`ID`, `StudyID`,`SeriesNumber`,`SeriesDate`,`SeriesTime`,`StoreState`,`Window`," +
                $"`SeriesDescription`,`ReportPath`,`SeriesStatus`,`ImageCount`,`SeriesInstanceUID`) values('{seriesID}','{studyID}','{SeriesNumber}','{date.ToString("yyyy-MM-dd HH:mm:ss")}','{time.ToString("yyyy-MM-dd HH:mm:ss")}'" +
                $",'{1}','customer','{SeriesDescription}','{savedDicomFoldPath}',1,1,'{SeriesInstanceUID}'); ";
         int count=   SQLHelper.DbHelper<object>.InsertOrUpdateBysql(sql);
            Console.WriteLine(count);
        }
    }
}