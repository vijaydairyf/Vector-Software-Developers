﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugBusiness.ExtensionMethods
{
    public static class DataConversion
    {
        public static byte[] sbyteToByteArray(sbyte[] input)
        {
            byte[] temp = new byte[input.Length];
            Buffer.BlockCopy(input, 0, temp, 0, input.Length);
            return temp;
        }

        public static byte[] ToByteArray(this Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }

    }
}
