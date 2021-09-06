using Dicom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ScreenShotSaveTool
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Dictionary<DicomTag, dynamic> keyValuePairs = new Dictionary<DicomTag, dynamic>();
            keyValuePairs.Add(DicomTag.SeriesInstanceUID, GetSeriesInstanceUID());
            //  keyValuePairs.Add(DicomTag.StudyID, "fb2fcfec-a86c-4a81-a59f-bfccd4532f2b");

            SaveHelper.StartScreenshot(new System.Windows.Rect(100, 20, 500, 812), @"E:\Data\Study_0d841500-4279-428c-bce9-aa73e09b2a32\Series_6a36d811-bc2f-44c0-9aeb-860d7203ea6f\1_1_time_20210122-13-32-32-268.dcm",
          keyValuePairs, @"E:\文档\WeChat Files\zhangyu9823\FileStorage\File\2021-08\pdf\ScreenShotSaveTool\File", "fb2fcfec-a86c-4a81-a59f-bfccd4532f2b");
            Console.ReadKey();
        }

        public static string GetSeriesInstanceUID()
        {
            return "1.2.250.1.59.0.8569" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".141";
        }
    }

}
