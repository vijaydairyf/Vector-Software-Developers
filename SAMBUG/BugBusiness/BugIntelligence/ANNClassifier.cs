﻿using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.ML;
using Emgu.CV.ML.Structure;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using Emgu.CV.Util;

namespace BugBusiness.BugIntelligence
{
    public sealed class ANNClassifier
    {
        private class ANNConfig
        {
            public ANNConfig(int[] _layers, double _alpha, double _beta)
            {
                layers = _layers;
                alpha = _alpha;
                beta = _beta;
            }

            public int[] layers { get; set; }
            public double alpha { get; set; }
            public double beta { get; set; }
        }

        private Matrix<float> dictionary;
        private List<string> class_labels;
        private Emgu.CV.ML.ANN_MLP network;

        private string dictionary_file_name;
        private string network_file_name;
        private string classes_file_name;
        private string ann_config_file_name;

        private static volatile ANNClassifier instance;
        private static object syncRoot = new Object();
       
        private Matrix<float> ConcatDescriptors(IList<Matrix<float>> descriptors)
        {
            int cols = descriptors[0].Cols;
            int rows = descriptors.Sum(a => a.Rows);

            float[,] concatedDescs = new float[rows, cols];

            int offset = 0;

            foreach (var descriptor in descriptors)
            {
                // append new descriptors
                Buffer.BlockCopy(descriptor.ManagedArray, 0, concatedDescs, offset, sizeof(float) * descriptor.ManagedArray.Length);
                offset += sizeof(float) * descriptor.ManagedArray.Length;
            }

            return new Matrix<float>(concatedDescs);
        }

        private Image<Bgr, byte> doGrabCut(Image<Bgr, byte> image)
        {
            //1. Convert the image to grayscale.
            int numberOfIterations = 5;
            Image<Gray, byte> grayImage = image.Convert<Gray, Byte>();

            //2. Threshold it using otsu.
            grayImage = getThresholdedImage(grayImage);
          
            grayImage._Not();

            //3. Extract the contours.
            Rectangle ROI = new Rectangle(0, 0, 0, 0);
            #region Extracting the Contours
            using (MemStorage storage = new MemStorage())
            {
                for (Contour<Point> contours = grayImage.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_TREE, storage); contours != null; contours = contours.HNext)
                {

                    Contour<Point> currentContour = contours.ApproxPoly(contours.Perimeter * 0.015, storage);
                    if (currentContour.BoundingRectangle.Width > ROI.Width)
                    {
                        //CvInvoke.cvDrawContours(color, contours, new MCvScalar(255), new MCvScalar(255), -1, 2, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED, new Point(0, 0));
                        //color.Draw(currentContour.BoundingRectangle, new Bgr(0, 255, 0), 1);
                        //To crop the image around the Region of Interest
                        ROI = currentContour.BoundingRectangle;

                    }

                }

            }
            #endregion
            //Emgu.CV.UI.ImageViewer.Show(image);
            //4. Setting the results to the output variables.

            #region Asigning output

            Image<Gray, byte> mask = image.GrabCut(ROI, numberOfIterations);

            mask = mask.ThresholdBinary(new Gray(2), new Gray(255));

            Image<Bgr, byte> result = image.Copy(mask);

            if (ROI.Width < ROI.Height)
                ROI.Width = ROI.Height;
            else if (ROI.Height < ROI.Width)
                ROI.Height = ROI.Width;

            result.ROI = ROI;
            result = result.Resize(256, 256, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            for (int x = 0; x < result.Rows; x++)
                for (int y = 0; y < result.Cols; y++)
                    if (result.Data[x, y, 0] == 0 && result.Data[x, y, 1] == 0 && result.Data[x, y, 2] == 0)
                    {
                        result.Data[x, y, 0] = 255;
                        result.Data[x, y, 1] = 255;
                        result.Data[x, y, 2] = 255;
                    }



            #endregion
            return result;
        }

        private Image<Gray, byte> getThresholdedImage(Image<Gray, byte> Img_Org_Gray)
        {
                Image<Gray, byte> Img_Source_Gray = Img_Org_Gray.Copy();
                Image<Gray, byte> Img_Otsu_Gray = Img_Org_Gray.CopyBlank();
                
                CvInvoke.cvThreshold(Img_Source_Gray.Ptr, Img_Otsu_Gray.Ptr, 0, 255, Emgu.CV.CvEnum.THRESH.CV_THRESH_OTSU);
               // Emgu.CV.UI.ImageViewer.Show(Img_Otsu_Gray);
                return Img_Otsu_Gray;
        }

        private Image<Bgr, byte> preProcessImage(Image<Bgr, byte> image)
        {
            while (image.Width > 1000 || image.Height > 1000)
                image = image.Resize(0.5, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            return doGrabCut(image);
        }

        private List<string> restoreClasses(string fileName)
        {
            return File.ReadAllLines(fileName).ToList<string>();
        }

        private ANNConfig restoreNetworkConfig(string fileName)
        {
            string configJSON = File.ReadAllText(ann_config_file_name);
            return JsonConvert.DeserializeObject<ANNConfig>(configJSON);
        }

        private Emgu.CV.ML.ANN_MLP restoreNetwork(string networkFileName, string configFileName)
        {
            ANNConfig config = restoreNetworkConfig(configFileName);
            ANN_MLP net = new ANN_MLP(new Matrix<int>(config.layers), Emgu.CV.ML.MlEnum.ANN_MLP_ACTIVATION_FUNCTION.SIGMOID_SYM, config.alpha, config.beta);
            net.Load(networkFileName);
            return net;
        }

        private Matrix<float> restoreDictionary(string fileName)
        {
            return Emgu.Util.Toolbox.XmlDeserialize<Matrix<float>>(XDocument.Load(fileName));
        }

        public string classify(byte[] picture)
        {
            Bitmap bmp;
            using (var ms = new MemoryStream(picture))
            {
                bmp = new Bitmap(ms);
            }
        
            return classify(new Image<Bgr,byte>(bmp));
        }

        public string classify(Image<Bgr, byte> image)  //class labels and dict read from XML docs
        {
            Matrix<float> classification_result = new Matrix<float>(1, class_labels.Count);
            SURFDetector detector = new SURFDetector(400, false);
            BruteForceMatcher<float> matcher = new BruteForceMatcher<float>(DistanceType.L2);
            BOWImgDescriptorExtractor<float> bowDE = new BOWImgDescriptorExtractor<float>(detector, matcher);
            //Emgu.CV.UI.ImageViewer.Show(image);
            //Store the vocabulary
            bowDE.SetVocabulary(dictionary);
            image = preProcessImage(image);
            //Emgu.CV.UI.ImageViewer.Show(image);
            Image<Gray, Byte> testImgGray = image.Convert<Gray, byte>();
            VectorOfKeyPoint testKeyPoints = detector.DetectKeyPointsRaw(testImgGray, null);
            Matrix<float> testBOWDescriptor = bowDE.Compute(testImgGray, testKeyPoints);

            network.Predict(testBOWDescriptor, classification_result);

            int predicted_class_index = 0;
            for (int i = 0; i < classification_result.Cols; i++)
                if (classification_result[0, i] > classification_result[0, predicted_class_index])
                    predicted_class_index = i;

            return class_labels[predicted_class_index]+"@"+predicted_class_index;
        }

        private ANNClassifier() //Trains the network
        {
            string app_data_path = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            dictionary_file_name = app_data_path+"\\ANN\\dictionary.xml";
            network_file_name = app_data_path+"\\ANN\\network.stat";
            classes_file_name = app_data_path+"\\ANN\\classes.txt";
            ann_config_file_name = app_data_path+"\\ANN\\config.json";

            dictionary = restoreDictionary(dictionary_file_name);
            class_labels = restoreClasses(classes_file_name);
            network = restoreNetwork(network_file_name, ann_config_file_name);
        }

        public static ANNClassifier getInstance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new ANNClassifier();
                        }
                    }
                }

                return instance;
            }
        }
    }
}
