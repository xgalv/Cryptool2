﻿/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;
using System.Runtime.CompilerServices;
using Cryptool.Plugins.Converter;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Cryptool.Plugins.Converter
{
    public enum OutputTypes { StringType = 0, IntType, ShortType, ByteType, DoubleType, BigIntegerType, IntArrayType, ByteArrayType, CryptoolStreamType };

    [Author("Raoul Falk, Dennis Nolte", "falk@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Converter.Properties.Resources", false, "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "Converter/icons/icon.png", "Converter/icons/tostring.png", "Converter/icons/toint.png", "Converter/icons/toshort.png", "Converter/icons/tobyte.png", "Converter/icons/todouble.png", "Converter/icons/tobig.png", "Converter/icons/tointarray.png", "Converter/icons/tobytearray.png", "Converter/icons/tocryptoolstream.png")]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    class Converter : ICrypComponent
    {
        #region private variables

        private ConverterSettings settings = new ConverterSettings();
        private object inputOne;
        private object output;

        #endregion

        #region public interfaces

        public Converter()
        {
            // this.settings = new ConverterSettings();
            this.settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
        }

        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (ConverterSettings)value; }
        }

        private void Converter_LogMessage(string msg, NotificationLevel loglevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(msg, this, loglevel));
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        [PropertyInfo(Direction.InputData, "InputOneCaption", "InputOneTooltip", "", true, false, QuickWatchFormat.Text, null)]
        public object InputOne
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return inputOne; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (value != inputOne)
                {
                    inputOne = value;
                    OnPropertyChanged("InputOne");
                }
            }
        }



        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip", "", true, false, QuickWatchFormat.Text, null)]
        public object Output
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                if (inputOne == null)
                    return null;

                if (settings.Converter == OutputTypes.CryptoolStreamType)    // CryptoolStream
                {
                    byte[] streamData = null;

                    if (inputOne is byte[])
                        streamData = (byte[])inputOne;
                    else if (inputOne is byte)
                        streamData = new byte[] { (byte)inputOne };
                    else if (inputOne is String)
                        streamData = Encoding.Default.GetBytes((String)inputOne);
                    else if (inputOne is BigInteger)
                    {
                        streamData = ((BigInteger)inputOne).ToByteArray();
                        Array.Reverse(streamData); // Display MSB first
                    }

                    if (streamData != null)
                    {
                        ICryptoolStream cStream = new CStreamWriter(streamData);
                        return cStream;
                    }
                    else
                    {
                        GuiLogMessage("Conversion from " + inputOne.GetType().Name + " to Cryptoolstream is not yet implemented", NotificationLevel.Error);
                        return null;
                    }
                }
                else
                {
                    return output;
                }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                this.output = value;
                OnPropertyChanged("Output");
            }
        }

        #endregion

        #region IPlugin members

        public void Dispose()
        {
        }

        private byte[] CStreamReaderToByteArray(CStreamReader stream)
        {
            stream.WaitEof();
            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            stream.ReadFully(buffer);
            return buffer;
        }

        private byte[] ICryptoolStreamToByteArray(ICryptoolStream stream)
        {
            return CStreamReaderToByteArray(stream.CreateReader());
        }

        private BigInteger ByteArrayToBigInteger(byte[] buffer, bool msb=true)
        {
            if (msb)
            {
                byte[] temp = new byte[buffer.Length];
                buffer.CopyTo(temp, 0);
                Array.Reverse(temp);

                return new BigInteger(temp);
            }
            else
            {
                return new BigInteger(buffer);
            }
        }

        public void Execute()
        {
            if (InputOne != null)
                GuiLogMessage("Laufe! " + InputOne.ToString(), NotificationLevel.Debug);

            if (InputOne is ICryptoolStream)
            {
                #region public ConvertFromICryptoolStream

                switch (this.settings.Converter)
                {
                    case OutputTypes.StringType:
                        {
                            byte[] buffer = ICryptoolStreamToByteArray((ICryptoolStream)InputOne);
                            Output = Encoding.Default.GetString(buffer);
                            break;
                        }

                    case OutputTypes.ByteArrayType:
                        {
                            Output = ICryptoolStreamToByteArray((ICryptoolStream)InputOne);
                            break;
                        }

                    case OutputTypes.BigIntegerType:
                        {
                            byte[] buffer = ICryptoolStreamToByteArray((ICryptoolStream)InputOne);
                            Output = ByteArrayToBigInteger(buffer);
                            break;
                        }

                    default:
                        GuiLogMessage("Conversion from ICryptoolStream to the chosen type is not implemented", NotificationLevel.Error);
                        return;
                }

                ProgressChanged(100, 100);
                return;

                #endregion
            } 
            else if (InputOne is int[])
            {
                #region public ConvertFromIntArray

                GuiLogMessage("Conversion from int[] to the chosen type is not implemented", NotificationLevel.Error);
                return;

                #endregion
            }
            else if (InputOne is byte[])
            {
                #region public ConvertFromByteArray

                switch (this.settings.Converter)
                {
                    case OutputTypes.BigIntegerType: // byte[] to BigInteger
                        {
                            Output = ByteArrayToBigInteger((byte[])InputOne);                                
                            ProgressChanged(100, 100);
                            return;
                        }
                    case OutputTypes.StringType: // byte[] to String
                        {
                            Output = Encoding.Default.GetString((byte[])InputOne);
                            ProgressChanged(100, 100);
                            return;
                        }
                    //default:
                    //    {
                    //        GuiLogMessage("Could not convert from byte[] to chosen type: ", NotificationLevel.Error);
                    //        break;
                    //    }
                }

                #endregion
            }
            else if (InputOne is bool)
            {
                #region public ConvertFromBool

                switch (this.settings.Converter)
                {
                    case OutputTypes.StringType:
                        {
                            Output = InputOne.ToString();
                            break;
                        }
                    case OutputTypes.IntType:
                        {
                            Output = (bool)InputOne ? 1 : 0;
                            break;
                        }
                    case OutputTypes.BigIntegerType: // bool to BigInteger
                        {
                            Output = (BigInteger)((bool)InputOne ? 1 : 0);
                            break;
                        }
                    default:
                        {
                            GuiLogMessage("Could not convert from bool to chosen type: ", NotificationLevel.Error);
                            return;
                        }
                }

                ProgressChanged(100, 100);
                return;

                #endregion
            }

            // the string representation is used for all upcoming operations
            string inpString = Convert.ToString(InputOne);

            #region public ConvertFromString

            switch (this.settings.Converter) // convert to what?
            {
                case OutputTypes.StringType:
                    {
                        Output = inpString;
                        ProgressChanged(100, 100);
                        break;
                    }
                case OutputTypes.IntType:
                    {
                        try // can be read as int?
                        {
                            Output = Convert.ToInt32(inpString);
                            ProgressChanged(100, 100);
                        }
                        catch (Exception e)
                        {
                            GuiLogMessage("Could not convert input to integer: " + e.Message, NotificationLevel.Error);
                        }
                        break;
                    }
                case OutputTypes.ShortType:
                    {
                        try // can be read as short?
                        {
                            short temp = Convert.ToInt16(inpString);

                            Output = temp;
                            ProgressChanged(100, 100);
                        }
                        catch (Exception e)
                        {
                            GuiLogMessage("Could not convert input to short: " + e.Message, NotificationLevel.Error);
                        }
                        break;
                    }
                case OutputTypes.ByteType:
                    {
                        try // can be read as byte?
                        {
                            byte temp = Convert.ToByte(inpString);

                            Output = temp;
                            ProgressChanged(100, 100);
                        }
                        catch (Exception e)
                        {
                            GuiLogMessage("Could not convert input to byte: " + e.Message, NotificationLevel.Error);
                        }
                        break;
                    }
                case OutputTypes.DoubleType:
                    {
                        try // can be read as double?
                        {
                            String cleanInputString = DoubleCleanup(inpString); // apply user settings concerning input format

                            double temp = Convert.ToDouble(cleanInputString);
                            GuiLogMessage("Converting String to double is not safe. Digits may have been cut off  ", NotificationLevel.Warning);
                            Output = temp;
                            ProgressChanged(100, 100);
                        }
                        catch (Exception e)
                        {
                            GuiLogMessage("Could not convert input to double: " + e.Message, NotificationLevel.Error);
                        }
                        break;
                    }
                case OutputTypes.BigIntegerType:
                    {
                        try // can be read as BigInteger?
                        {
                            Match match = Regex.Match(inpString, "^\\s*0[xX]([a-fA-F0-9]+)");

                            if (match.Success)  // is in hex-format?
                            {
                                string result = Regex.Replace(inpString, "0[xX]", "");
                                result = Regex.Replace(result, "[^a-fA-F0-9]", "");
                                match = Regex.Match(result, "^([a-fA-F0-9]+)");
                                Output = BigInteger.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.AllowHexSpecifier);
                            } 
                            else
                                Output = BigIntegerHelper.ParseExpression(inpString);

                            ProgressChanged(100, 100);
                        }
                        catch (Exception e)
                        {
                            GuiLogMessage("Could not convert input to BigInteger: " + e.Message, NotificationLevel.Error);
                        }
                        break;
                    }
                case OutputTypes.IntArrayType:
                    {
                        GuiLogMessage("Conversion to int[] not yet defined: ", NotificationLevel.Error);
                        break;
                    }
                case OutputTypes.ByteArrayType:
                    {
                        inpString = setText(inpString);
                        if (settings.Numeric) // apply user setting concerning numeric interpretation of input (else input is read as string)
                        {
                            try // can be read as int? 
                            {
                                int tempint = Convert.ToInt32(inpString);
                                byte[] temp = new byte[4];
                                temp = BitConverter.GetBytes(tempint);

                                int test = BitConverter.ToInt32(temp, 0);

                                Output = temp;

                                ProgressChanged(100, 100);
                                break;
                            }
                            catch (Exception)
                            {
                            }

                            try // can be read as BigInteger?
                            {
                                BigInteger tempbigint = BigInteger.Parse(inpString);
                                Output = tempbigint;

                                ProgressChanged(100, 100);
                                break;
                            }
                            catch (Exception)
                            {
                            }

                            try // can be read as double
                            {
                                double tempDouble = Convert.ToDouble(DoubleCleanup(inpString));
                                byte[] temp = BitConverter.GetBytes(tempDouble);

                                double test = BitConverter.ToDouble(temp, 0);
                                GuiLogMessage("Converting String to double is not safe. Digits may have been cut off " + test.ToString(), NotificationLevel.Warning);

                                Output = temp;

                                ProgressChanged(100, 100);
                                break;
                            }
                            catch (Exception)
                            {
                            }

                            System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
                            Output = enc.GetBytes(inpString);

                            ProgressChanged(100, 100);
                            break;
                        }
                        else // numeric interpretation NOT selected:
                        {
                            switch (settings.Encoding) //apply user settings concerning encoding
                            {
                                case ConverterSettings.EncodingTypes.Default:
                                    Output = Encoding.Default.GetBytes(inpString.ToCharArray());
                                    break;
                                case ConverterSettings.EncodingTypes.Unicode:
                                    Output = Encoding.Unicode.GetBytes(inpString.ToCharArray());
                                    break;
                                case ConverterSettings.EncodingTypes.UTF7:
                                    Output = Encoding.UTF7.GetBytes(inpString.ToCharArray());
                                    break;
                                case ConverterSettings.EncodingTypes.UTF8:
                                    Output = Encoding.UTF8.GetBytes(inpString.ToCharArray());
                                    break;
                                case ConverterSettings.EncodingTypes.UTF32:
                                    Output = Encoding.UTF32.GetBytes(inpString.ToCharArray());
                                    break;
                                case ConverterSettings.EncodingTypes.ASCII:
                                    Output = Encoding.ASCII.GetBytes(inpString.ToCharArray());
                                    break;
                                case ConverterSettings.EncodingTypes.BigEndianUnicode:
                                    Output = Encoding.BigEndianUnicode.GetBytes(inpString.ToCharArray());
                                    break;
                                default:
                                    Output = Encoding.Default.GetBytes(inpString.ToCharArray());
                                    break;
                            }

                            ProgressChanged(100, 100);
                            break;
                        }
                    }
                case OutputTypes.CryptoolStreamType:
                    {
                        GuiLogMessage("Conversion from " + inputOne.GetType().Name , NotificationLevel.Info);
                        
                        if (inputOne is byte[] || inputOne is byte || inputOne is BigInteger || inputOne is String)
                        {
                            OnPropertyChanged("Output");
                            ProgressChanged(100, 100);
                        }
                        else
                        {
                            GuiLogMessage("Conversion from " + inputOne.GetType().Name + " to CryptoolStream is not yet implemented", NotificationLevel.Error);
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            #endregion
        }

        private String setText(string temp) //apply user selected presentation format
        {
            if (temp != null)
            {

                switch (settings.Presentation)
                {
                    case ConverterSettings.PresentationFormat.Text:
                        // nothin to do here
                        break;
                    case ConverterSettings.PresentationFormat.Hex:
                        byte[] byteValues = Encoding.Default.GetBytes(temp.ToCharArray());
                        temp = BitConverter.ToString(byteValues, 0, byteValues.Length).Replace("-", "");
                        break;
                    case ConverterSettings.PresentationFormat.Base64:
                        temp = Convert.ToBase64String(Encoding.Default.GetBytes(temp.ToCharArray()));
                        break;
                    default:
                        break;
                }
                return temp;
            }
            return temp;
        }

        public String DoubleCleanup(String inpString) //apply user selected input format
        {
            if (this.settings.FormatAmer)
            {
                String temp1 = inpString.Replace(",", "");
                if (!(temp1.IndexOf(".") == temp1.LastIndexOf(".")))
                {
                    String tempXY = temp1.Insert(0, "X");
                    return tempXY;
                }
                if (temp1.Contains(".") && temp1.IndexOf(".") == temp1.LastIndexOf("."))
                {
                    String temp2 = temp1.Replace(".", ",");
                    return temp2;
                }
                else
                {
                    String temp3 = inpString.Replace(".", "");
                    return temp3;
                }
            }
            else
            {
                return inpString;
            }

        }

        public void Initialize()
        {
            settings.UpdateTaskPaneVisibility();
            settings.UpdateIcon();
        }

        public void Pause()
        {

        }

        public void PostExecution()
        {

        }

        public void PreExecution()
        {

        }

        public void Stop()
        {

        }

        #endregion

        #region INotifyPropertyChanged Member

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }
        #endregion

        #region event handling

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(this, args);
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        #endregion
    }
}
