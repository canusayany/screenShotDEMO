using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dicom.IO.Buffer;
using Dicom;
using Dicom.Imaging;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using O2S.Components.PDFRender4NET;

namespace pdf
{
    class Program
    {
        private enum Definition
        {
            One = 1, Two = 2, Three = 3, Four = 4, Five = 5, Six = 6, Seven = 7, Eight = 8, Nine = 9, Ten = 10
        }
        /// <summary>
        /// 将PDF文档转换为图片的方法
        /// </summary>
        /// <param name="pdfInputPath">PDF文件路径</param>
        /// <param name="imageOutputPath">图片输出路径</param>
        /// <param name="imageName">生成图片的名字</param>
        /// <param name="startPageNum">从PDF文档的第几页开始转换</param>
        /// <param name="endPageNum">从PDF文档的第几页开始停止转换</param>
        /// <param name="imageFormat">设置所需图片格式</param>
        /// <param name="definition">设置图片的清晰度，数字越大越清晰</param>
        private static void ConvertPDF2Image(string pdfInputPath, string imageOutputPath,
            string imageName, int startPageNum, int endPageNum, ImageFormat imageFormat, Definition definition)
        {
            PDFFile pdfFile = PDFFile.Open(pdfInputPath);

            if (!Directory.Exists(imageOutputPath))
            {
                Directory.CreateDirectory(imageOutputPath);
            }
            // validate pageNum
            if (startPageNum <= 0)
            {
                startPageNum = 1;
            }

            if (endPageNum > pdfFile.PageCount)
            {
                endPageNum = pdfFile.PageCount;
            }

            if (startPageNum > endPageNum)
            {
                int tempPageNum = startPageNum;
                startPageNum = endPageNum;
                endPageNum = startPageNum;
            }
            // start to convert each page
            for (int i = startPageNum; i <= endPageNum; i++)
            {
                Bitmap pageImage = pdfFile.GetPageImage(i - 1, 100 * (int)definition);
                pageImage.Save(imageOutputPath + "\\" + imageName + "." + imageFormat.ToString(), imageFormat);
                pageImage.Dispose();
            }
            pdfFile.Dispose();
        }

        public static byte[] GetPixels(Bitmap bitmap)
        {
           
            byte[] bytes = new byte[bitmap.Width * bitmap.Height * 3];

        

            int wide = bitmap.Width;
            int i = 0;
            int height = bitmap.Height;
            for (int y = 0; y < height; y++)//20210312        
            {
                /*  if (y > height / 5)
                  {
                      break;
                  }*/

                for (int x = 0; x < wide; x++)//20210312
                {
                    var srcColor = bitmap.GetPixel(x, y);
                    //bytes[i] = (byte)(srcColor.R * .299 + srcColor.G * .587 + srcColor.B * .114);//
                    /*  if (srcColor.R == 255)
                      {
                          bytes[i] = 0;
                      }
                      else
                      {
                          if (srcColor.R == 220)
                          {
                              bytes[i] = 255;
                          }
                          else
                          {
                              bytes[i] = srcColor.R;
                          }
                      }*/
                    bytes[i] = srcColor.R;
                    i++;
                    /* if (srcColor.G == 255)
                     {
                         bytes[i] = 0;
                     }
                     else
                     {
                         if (srcColor.G == 220)
                         {
                             bytes[i] = 255;
                         }
                         else
                         {
                             bytes[i] = srcColor.R;
                         }
                     }*/
                    bytes[i] = srcColor.G;
                    i++;
                    /*if (srcColor.B == 255)
                    {
                        bytes[i] = 0;
                    }
                    else
                    {
                        if (srcColor.B == 220)
                        {
                            bytes[i] = 255;
                        }
                        else
                        {
                            bytes[i] = srcColor.R;
                        }
                    }*/
                    bytes[i] = srcColor.B;
                    i++;
                }
            }
           
            //byte[] bytess = new byte[bitmap.Width * bitmap.Height / 5 * 3];//20210311
            byte[] bytess = new byte[bitmap.Width * bitmap.Height * 3];//20210311
           
            for (int j = 0; j < bytess.Length; j++)
            {
                bytess[j] = bytes[j];
            }

          
            return bytess;
        }
        static void Main(string[] args)
        {                    
            string pdfpath = @"C:\Users\Administrator\Desktop\test11\1.pdf";
            string imagefolderpath = @"C:\Users\Administrator\Desktop\test11";
            ConvertPDF2Image(pdfpath, imagefolderpath, "test", 1, 1, ImageFormat.Bmp, Definition.One);

            string file = imagefolderpath + "\\test.bmp";
           
            string SeriesUid = "1111";
            Bitmap bitmap = new Bitmap(file);
                  
            byte[] pixels = GetPixels(bitmap);
          
            MemoryByteBuffer buffer = new MemoryByteBuffer(pixels);
            DicomDataset dataset = new DicomDataset();
          
            dataset.Add(DicomTag.PhotometricInterpretation, PhotometricInterpretation.Rgb.Value);// PhotometricInterpretation.Rgb.Value
                                                                                                 // dataset.Add(DicomTag.Rows, (ushort)(bitmap.Height / 5));
            dataset.Add(DicomTag.Rows, (ushort)(bitmap.Height));
            dataset.Add(DicomTag.Columns, (ushort)bitmap.Width);
            dataset.Add(DicomTag.BitsAllocated, (ushort)8);
            dataset.Add(DicomTag.SOPClassUID, "1.2.840.10008.5.1.4.1.1.2");
            dataset.Add(DicomTag.SOPInstanceUID, "1.2.840.10008.5.1.4.1.1.2.20181120090837121314");
            //  dataset.Add(DicomTag.PatientAge, 10);
            dataset.Add(DicomTag.StudyInstanceUID, "111");
            dataset.Add(DicomTag.StudyDate, DateTime.Now.ToString("yyyyMMdd"));
            dataset.Add(DicomTag.PatientName, "MPR");
            dataset.Add(DicomTag.PatientID, "1010");
            dataset.Add(DicomTag.PatientPosition, "HFS");  
            //dataset.Add(DicomTag.PixelDataAreaOriginRelativeToFOV, DataCommon.PatientInfo.PatientPosition);
            //dataset.Add(DicomTag.PhotometricInterpretation, "MONOCHROME2"); 
            dataset.Add(DicomTag.SamplesPerPixel, "3");

            dataset.Add(DicomTag.WindowCenter, "128");
            dataset.Add(DicomTag.WindowWidth, "256");
            dataset.Add(DicomTag.RescaleIntercept, "0");
            dataset.Add(DicomTag.RescaleSlope, "1");
            dataset.Add(DicomTag.RescaleType, "US");
            dataset.Add(DicomTag.PixelSpacing, "1.0\\1.0");
            dataset.Add(DicomTag.SeriesInstanceUID, SeriesUid);
            dataset.Add(DicomTag.ProtocolName, "MPR");
            dataset.Add(DicomTag.SeriesNumber, "5102");
            dataset.Add(DicomTag.SeriesDescription, "0*0");
            DicomPixelData pixelData = DicomPixelData.Create(dataset, true);
            pixelData.BitsStored = 8;
            //pixelData.BitsAllocated = 8;
            pixelData.SamplesPerPixel = 3;
            pixelData.HighBit = 7;
            pixelData.PixelRepresentation = 0;
            pixelData.PlanarConfiguration = 0;
            pixelData.AddFrame(buffer);
            string dcmFileName;
            DicomFile dicomfile = new DicomFile(dataset);
            bitmap.Dispose();
            dicomfile.Save(file.Replace(".bmp", ".dcm"));
            dcmFileName = file.Replace(".bmp", ".dcm");
           


        }
    }
}
