﻿/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Cryptool.Plugins.VisualEncoder.Model;
using DataMatrix.net;
using VisualEncoder.Properties;

namespace Cryptool.Plugins.VisualEncoder.Encoders
{
    class DataMatrix : DimCodeEncoder
    {
      
        #region legend Strings

        private readonly LegendItem alignmentLegend = new LegendItem
        {
            ColorBlack = Color.Blue,
            ColorWhite = Color.LightBlue, //has no white, just for debuging
            LableValue = Resources.DM_ALIG_LABLE,
            DiscValue = Resources.DM_ALIG_DISC
            
        };

        private readonly LegendItem columnIDLegend = new LegendItem
        {
            ColorBlack = Color.Green,
            ColorWhite = Color.LightGreen,
            LableValue = Resources.DM_COLUMNID_LABLE,
            DiscValue = Resources.DM_COLUMNID_DISC
        };

        #endregion

        public DataMatrix(VisualEncoder caller) : base(caller) {/*empty*/}

        protected override Image GenerateBitmap(byte[] input, VisualEncoderSettings settings)
        {
            // tradeof quality vs rendertime
            int size = 2;
            if (input.Length < 30)
                size = 10;
            else if (input.Length < 100)
                size = 6;
            else if (input.Length < 200)
                size = 5;
            else if (input.Length < 400)
                size = 4;
            else if (input.Length < 600)
                size = 3;

            var encoder = new DmtxImageEncoder();
            var options = new DmtxImageEncoderOptions {ModuleSize = size, MarginSize = 5 };
            var payload = Encoding.ASCII.GetString(input);
            return encoder.EncodeImage(payload, options);
        }

        protected override byte[] EnrichInput(byte[] input, VisualEncoderSettings settings)
        {
            return input;
        }

        protected override bool VerifyInput(byte[] input, VisualEncoderSettings settings)
        {
            return true;
        }

        protected override List<LegendItem> GetLegend(byte[] input, VisualEncoderSettings settings)
        {
            var legend = new List<LegendItem>{alignmentLegend, columnIDLegend};
            return legend;
        }

        protected override Image GeneratePresentationBitmap(Image input, VisualEncoderSettings settings)
        {
            var bitmap = new Bitmap(input);

            #region find elements

            //find the upper left corner
            int x = 0;
            int y = 0;

            var lockBitmap = new LockBitmap(bitmap);
            lockBitmap.LockBits();

            while(lockBitmap.GetPixel(x, y).R != Color.Black.R)
            {
                if(x < lockBitmap.Width/2)
                {
                    x++;
                } else if (y < lockBitmap.Height/2)
                {
                    x = 0;
                    y++;
                }
                else //avoid endless search
                {   //if we found no bar end, we stop here
                    return bitmap;
                }
            }
            int leftX = x;
            int upperY = y;

            //calc barwidth
            while (lockBitmap.GetPixel(x, y+1).R == Color.Black.R)
            {
                if(x < lockBitmap.Width/2)
                {
                    x++;
                } 
                else //avoid endless search
                {   //if we found no bar end, we stop here
                    return bitmap;
                }
            }
            int barWidth = x - leftX - 1;

            lockBitmap.UnlockBits();
            int codeHight = CalcBarHight(bitmap, leftX) - upperY;
            lockBitmap.LockBits();

            //calc codeWidth
            x = leftX;
            y = upperY + codeHight;

            while (lockBitmap.GetPixel(x, y).R == Color.Black.R)
            {
                if (x < lockBitmap.Width)
                {
                    x++;
                }
                else //avoid endless search
                {   //if we found no bar end, we stop here
                    return bitmap;
                }
            }
            int codeWidth = x - leftX - 1;
            lockBitmap.UnlockBits();
            #endregion

            // mark column identificator
            bitmap = FillArea(leftX, leftX + codeWidth, upperY, upperY + barWidth,
                                bitmap, columnIDLegend.ColorBlack, columnIDLegend.ColorWhite);

            bitmap = FillArea(leftX + codeHight - barWidth, leftX + codeWidth, upperY, upperY + codeWidth,
                                bitmap, columnIDLegend.ColorBlack, columnIDLegend.ColorWhite);

             //mark  alignment
            bitmap = FillArea(leftX, leftX + barWidth, upperY, upperY + codeHight,
                                bitmap, alignmentLegend.ColorBlack, alignmentLegend.ColorWhite);

            bitmap = FillArea(leftX + barWidth, leftX + codeWidth, upperY + codeHight - barWidth, upperY + codeHight,
                               bitmap, alignmentLegend.ColorBlack, alignmentLegend.ColorWhite);

            //TODO
            return bitmap;
        }
    }
}