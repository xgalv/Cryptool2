﻿/*
   Copyright 2009 Sören Rinne, Ruhr-Universität Bochum, Germany

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
using System.IO;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using System.Windows.Controls;
using Cryptool.PluginBase.Miscellaneous;
using System.Security.Cryptography;

namespace Cryptool.TEA
{
    [Author("Soeren Rinne", "soeren.rinne@cryptool.de", "Ruhr-Universitaet Bochum, Chair for Embedded Security (EmSec)", "http://www.crypto.ruhr-uni-bochum.de/")]
    [PluginInfo("TEA.Properties.Resources", "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "TEA/Images/tea.png", "TEA/Images/encrypt.png", "TEA/Images/decrypt.png", "TEA/Images/encryptX.png", "TEA/Images/decryptX.png")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class TEA : ICrypComponent
    {
        #region IPlugin Members

        private TEASettings settings;
        private CStreamWriter outputStream;
        private byte[] inputKey;
        private bool stop = false;

        #endregion

        public TEA()
        {
            this.settings = new TEASettings();
            //((TEASettings)(this.settings)).LogMessage += TEA_LogMessage;
        }

        public ISettings Settings
        {
            get { return (ISettings)this.settings; }
            set { this.settings = (TEASettings)value; }
        }

        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputStreamTooltip", true)]
        public ICryptoolStream InputStream
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyTooltip", true)]
        public byte[] InputKey
        {
            get { return this.inputKey; }
            set
            {
                this.inputKey = value;
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", true)]
        public ICryptoolStream OutputStream
        {
            get
            {
                return outputStream;
            }
            set
            {
            }
        }

        public void Dispose()
        {
            Reset();
        }

        private void Reset()
        {
            try
            {
                stop = false;
                inputKey = null;
                outputStream = null;

                if (outputStream != null)
                {
                    outputStream.Flush();
                    outputStream.Close();
                    outputStream.Dispose();
                    outputStream = null;
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

        public void Execute()
        {
            process(settings.Action, settings.Padding);
        }

        private void process(int action, int padding)
        {
            //Encrypt/Decrypt Stream
            try
            {
                if (InputStream == null || InputStream.Length == 0)
                {
                    GuiLogMessage("No input data, aborting now", NotificationLevel.Error);
                    return;
                }

                using (CStreamReader reader = InputStream.CreateReader())
                {
                    reader.WaitEof(); // lazy reading, wait until writer has finished.

                    outputStream = new CStreamWriter();

                    long inputbytes = reader.Length;
                    GuiLogMessage("inputStream length [bytes]: " + inputbytes.ToString(), NotificationLevel.Debug);

                    int bytesRead = 0;
                    int blocksRead = 0;
                    int position;
                    int blocks;

                    // get number of blocks
                    if (((int)inputbytes % 8) == 0)
                    {
                        blocks = (int)inputbytes / 8;
                    }
                    else
                    {
                        blocks = (int)Math.Round(inputbytes / 8 + 0.4, 0) + 1;
                    }

                    byte[] inputbuffer = new byte[8 * blocks];
                    byte[] outputbuffer = new byte[4];
                    GuiLogMessage("# of blocks: " + blocks.ToString(), NotificationLevel.Debug);

                    //read input
                    //GuiLogMessage("Current position: " + inputStream.Position.ToString(), NotificationLevel.Debug);
                    for (blocksRead = 0; blocksRead <= blocks - 1; blocksRead++)
                    {
                        for (position = bytesRead; position <= (blocksRead * 8 + 7); position++)
                        {
                            // no padding to do
                            if (position < inputbytes)
                            {
                                inputbuffer[position] = (byte)reader.ReadByte();
                            }
                            else // padding to do!
                            {
                                if (padding == 0)
                                {
                                    // padding with zeros
                                    inputbuffer[position] = 48;
                                }
                                else if (padding == 2)
                                {
                                    // padding with PKCS7
                                    int temp = 8 - (int)inputbytes % 8 + 48;
                                    inputbuffer[position] = (byte)temp;
                                }
                                else
                                {
                                    // no padding
                                    inputbuffer[position] = (byte)reader.ReadByte();
                                    GuiLogMessage("Byte is: " + inputbuffer[position].ToString(), NotificationLevel.Info);
                                }
                            }
                            bytesRead++;
                            //GuiLogMessage("Current position: " + inputStream.Position.ToString(), NotificationLevel.Debug);
                            //GuiLogMessage("Content of buffer[" + position + "]: " + buffer[position].ToString(), NotificationLevel.Debug);
                        }
                    }

                    //GuiLogMessage("vector[0] before coding: " + vector[0].ToString(), NotificationLevel.Debug);
                    //GuiLogMessage("vector[1] before coding: " + vector[1].ToString(), NotificationLevel.Debug);

                    uint[] key = new uint[4];
                    long[] longKey = new long[4];
                    long keybytes = inputKey.Length;
                    GuiLogMessage("inputKey length [byte]: " + keybytes.ToString(), NotificationLevel.Debug);

                    if (keybytes != 16)
                    {
                        GuiLogMessage("Given key has false length. Please provide a key with 128 Bits length. Aborting now.", NotificationLevel.Error);
                        return;
                    }
                    else
                    {
                        if (settings.Version != 2)
                        {
                            key[0] = BitConverter.ToUInt32(inputKey, 0);
                            key[1] = BitConverter.ToUInt32(inputKey, 4);
                            key[2] = BitConverter.ToUInt32(inputKey, 8);
                            key[3] = BitConverter.ToUInt32(inputKey, 12);
                        }
                        else
                        {
                            longKey[0] = (long)BitConverter.ToUInt32(inputKey, 0);
                            longKey[1] = (long)BitConverter.ToUInt32(inputKey, 4);
                            longKey[2] = (long)BitConverter.ToUInt32(inputKey, 8);
                            longKey[3] = (long)BitConverter.ToUInt32(inputKey, 12);
                        }
                    }

                    //encryption or decryption
                    //GuiLogMessage("Action is: " + action, NotificationLevel.Debug);
                    DateTime startTime = DateTime.Now;

                    uint[] vector = new uint[2];
                    long[] longVector = new long[2];

                    if (action == 0)
                    {
                        GuiLogMessage("Starting encryption [Keysize=128 Bits, Blocksize=64 Bits]", NotificationLevel.Info);
                        for (int i = 0; i <= blocks - 1; i++)
                        {
                            vector[0] = BitConverter.ToUInt32(inputbuffer, (i * 8));
                            vector[1] = BitConverter.ToUInt32(inputbuffer, (i * 8 + 4));

                            // see in settings which version of TEA to use
                            if (settings.Version == 0)
                            {
                                encode_tea(vector, key);
                                StatusChanged((int)TEAImage.Encode);
                            }
                            else if (settings.Version == 1)
                            {
                                encode_xtea((uint)settings.Rounds, vector, key);
                                StatusChanged((int)TEAImage.EncodeX);
                            }
                            else if (settings.Version == 2)
                            {
                                btea(vector, 2, key);
                                StatusChanged((int)TEAImage.EncodeX);
                            }

                            //write buffer to output stream
                            outputbuffer = BitConverter.GetBytes(vector[0]);
                            outputStream.Write(outputbuffer, 0, 4);
                            outputbuffer = BitConverter.GetBytes(vector[1]);
                            outputStream.Write(outputbuffer, 0, 4);
                        }
                    }
                    else if (action == 1)
                    {
                        GuiLogMessage("Starting decryption [Keysize=128 Bits, Blocksize=64 Bits]", NotificationLevel.Info);
                        for (int i = 0; i <= blocks - 1; i++)
                        {
                            vector[0] = BitConverter.ToUInt32(inputbuffer, i * 8);
                            vector[1] = BitConverter.ToUInt32(inputbuffer, i * 8 + 4);

                            // see in settings which version of TEA to use
                            if (settings.Version == 0)
                            {
                                decode_tea(vector, key);
                                StatusChanged((int)TEAImage.Decode);
                            }
                            else if (settings.Version == 1)
                            {
                                decode_xtea((uint)settings.Rounds, vector, key);
                                StatusChanged((int)TEAImage.DecodeX);
                            }
                            else if (settings.Version == 2)
                            {
                                btea(vector, -2, key);
                                StatusChanged((int)TEAImage.DecodeX);
                            }

                            //write buffer to output stream
                            outputbuffer = BitConverter.GetBytes(vector[0]);
                            outputStream.Write(outputbuffer, 0, 4);
                            outputbuffer = BitConverter.GetBytes(vector[1]);
                            outputStream.Write(outputbuffer, 0, 4);
                        }
                    }

                    //GuiLogMessage("vector[0] after coding: " + vector[0], NotificationLevel.Debug);
                    //GuiLogMessage("vector[1] after coding: " + vector[1], NotificationLevel.Debug);

                    /*while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0 && !stop)
                    {
                        outputStream.Write(buffer, 0, bytesRead);
                        if ((int)(inputStream.Position * 100 / inputStream.Length) > position)
                        {
                            position = (int)(inputStream.Position * 100 / inputStream.Length);
                            //ProgressChanged(inputStream.Position, inputStream.Length);
                        }
                    }*/

                    long outbytes = outputStream.Length;
                    DateTime stopTime = DateTime.Now;
                    TimeSpan duration = stopTime - startTime;

                    if (!stop)
                    {
                        if (action == 0)
                        {
                            GuiLogMessage("Encryption complete! (in: " + inputbytes + " bytes, out: " + outbytes.ToString() + " bytes)", NotificationLevel.Info);
                        }
                        else
                        {
                            GuiLogMessage("Decryption complete! (in: " + inputbytes + " bytes, out: " + outbytes.ToString() + " bytes)", NotificationLevel.Info);
                        }
                        GuiLogMessage("Time used: " + duration.ToString(), NotificationLevel.Debug);
                        outputStream.Close();
                        OnPropertyChanged("OutputStream");
                    }

                    if (stop)
                    {
                        outputStream.Close();
                        GuiLogMessage("Aborted!", NotificationLevel.Info);
                    }
                }
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
            finally
            {
                ProgressChanged(1, 1);
            }
        }

        private void encode_tea(uint[] v, uint[] k)
        {
            uint y = v[0];
            uint z = v[1];

            uint k0 = k[0], k1 = k[1], k2 = k[2], k3 = k[3];

            uint sum = 0;
            uint delta = 0x9e3779b9;
            uint n = 64;

            while (n-- > 0)
            {
                /*
                 sum += delta;
                 v0 += ((v1<<4) + k0) ^ (v1 + sum) ^ ((v1>>5) + k1);
                 v1 += ((v0<<4) + k2) ^ (v0 + sum) ^ ((v0>>5) + k3);
                */
                sum += delta;
                y += ((z << 4) + k0) ^ (z + sum) ^ ((z >> 5) + k1);
                z += ((y << 4) + k2) ^ (y + sum) ^ ((y >> 5) + k3);
            }

            v[0] = y;
            v[1] = z;
        }

        private void decode_tea(uint[] v, uint[] k)
        {
            uint n = 64;
            uint sum = 0x8DDE6E40; // for 64 rounds, for 32 rounds it would be 0xC6EF3720

            uint k0 = k[0], k1 = k[1], k2 = k[2], k3 = k[3];
            uint y = v[0];
            uint z = v[1];
            uint delta = 0x9e3779b9;

            while (n-- > 0)
            {
                /*
                 v1 -= ((v0<<4) + k2) ^ (v0 + sum) ^ ((v0>>5) + k3);
                 v0 -= ((v1<<4) + k0) ^ (v1 + sum) ^ ((v1>>5) + k1);
                 sum -= delta;
                */
                z -= ((y << 4) + k2) ^ (y + sum) ^ ((y >> 5) + k3);
                y -= ((z << 4) + k0) ^ (z + sum) ^ ((z >> 5) + k1);
                sum -= delta;
            }

            v[0] = y;
            v[1] = z;
        }

        private void encode_xtea(uint rounds, uint[] v, uint[] k)
        {
            uint y = v[0];
            uint z = v[1];

            uint sum = 0;
            uint delta = 0x9e3779b9;
            uint n = rounds;

            while (n-- > 0)
            {
                y += (z << 4 ^ z >> 5) + z ^ sum + k[sum & 3];
                sum += delta;
                z += (y << 4 ^ y >> 5) + y ^ sum + k[sum >> 11 & 3];
            }

            v[0] = y;
            v[1] = z;
        }

        private void decode_xtea(uint rounds, uint[] v, uint[] k)
        {
            uint n = rounds;
            uint sum;
            uint y = v[0];
            uint z = v[1];
            uint delta = 0x9e3779b9;

            sum = delta * n;

            while (n-- > 0)
            {
                z -= (y << 4 ^ y >> 5) + y ^ sum + k[sum >> 11 & 3];
                sum -= delta;
                y -= (z << 4 ^ z >> 5) + z ^ sum + k[sum & 3];
            }

            v[0] = y;
            v[1] = z;
        }

        private uint btea(uint[] v, int n, uint[] k)
        {
            int m = n;
            if (n < -1) m = -n;
            uint z = v[m - 1], y = v[0], sum = 0, e, DELTA = 0x9e3779b9;

            int p, q;

            uint MX;

            if (n > 1)
            {          /* Coding Part */
                q = 6 + 52 / n;
                while (q-- > 0)
                {
                    sum += DELTA;
                    e = (sum >> 2) & 3;
                    for (p = 0; p < n - 1; p++)
                    {
                        y = v[p + 1];
                        MX = (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (k[p & 3 ^ e] ^ z);
                        z = v[p] += MX;
                    }
                    y = v[0];
                    GuiLogMessage("y: " + y.ToString("X"), NotificationLevel.Info);
                    MX = (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (k[p & 3 ^ e] ^ z);
                    z = v[n - 1] += MX;
                    GuiLogMessage("z: " + z.ToString("X"), NotificationLevel.Info);
                }

                GuiLogMessage("v[n-1]: " + v[n - 1].ToString("X"), NotificationLevel.Info);
                GuiLogMessage("v[0]: " + v[0].ToString("X"), NotificationLevel.Info);

                return 0;
            }
            else if (n < -1)
            {  /* Decoding Part */
                n = -n;
                q = 6 + 52 / n;
                sum = (uint)q * DELTA;
                while (sum != 0)
                {
                    e = (sum >> 2) & 3;
                    for (p = n - 1; p > 0; p--)
                    {
                        z = v[p - 1];
                        MX = (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (k[p & 3 ^ e] ^ z);
                        y = v[p] -= MX;
                    }
                    z = v[n - 1];
                    MX = (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (k[p & 3 ^ e] ^ z);
                    y = v[0] -= MX;
                    sum -= DELTA;
                }
                return 0;
            }
            return 1;
        }


        public void Encrypt()
        {
            //Encrypt Stream
            process(0, settings.Padding);
        }

        public void Decrypt()
        {
            //Decrypt Stream
            process(1, settings.Padding);
        }

        public void Initialize()
        {
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public void PostExecution()
        {
            Reset();
        }

        public void PreExecution()
        {
            Reset();
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public void Stop()
        {
            this.stop = true;
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
            /*if (PropertyChanged != null)
            {
              PropertyChanged(this, new PropertyChangedEventArgs(name));
            }*/
        }

        private void StatusChanged(int imageIndex)
        {
            EventsHelper.StatusChanged(OnPluginStatusChanged, this, new StatusEventArgs(StatusChangedMode.ImageUpdate, imageIndex));
        }

        #endregion
    }

    enum TEAImage
    {
        Default,
        Encode,
        Decode,
        EncodeX,
        DecodeX
    }
}
