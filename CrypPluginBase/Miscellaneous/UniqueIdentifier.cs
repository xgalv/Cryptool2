﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Management;

namespace Cryptool.PluginBase.Miscellaneous
{
    public class UniqueIdentifier
    {
        /// <summary>
        /// Returns a globally unique identifier for a user on a computer.
        /// </summary>
        /// <returns></returns>
        public static Int64 GetID()
        {
            string username = WindowsIdentity.GetCurrent().Name;
            //string mac = GetMacIdentifier();

            ManagementClass man = new ManagementClass("win32_processor");
            ManagementObjectCollection moc = man.GetInstances();
            string cpuids = "";
            foreach (ManagementObject mob in moc)
            {
                var cpuid = mob.Properties["processorID"].Value;
                if (cpuid != null)
                {
                    cpuids += cpuid.ToString();
                }
            }


            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] idBytes = md5.ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(username + cpuids));
            long ID = BitConverter.ToInt64(idBytes, 0);
            ID = Math.Abs(ID);

            return ID;
        }

        /// <summary>
        /// Returns a globally unique identifier for an external client connected
        /// to this computer.
        /// </summary>
        /// <param name="externalClient"></param>
        /// <returns></returns>
        public static Int64 GetID(String externalClient)
        {
            Int64 localId = GetID();

            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] idBytes = md5.ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(externalClient + localId));
            long ID = BitConverter.ToInt64(idBytes, 0);
            ID = Math.Abs(ID);

            return ID;
        }

        /// <summary>
        /// Returns an identifier that depends on the MAC addresses of this system
        /// </summary>        
        private static string GetMacIdentifier()
        {
            string MacID = "";
            ManagementClass MC = new ManagementClass("Win32_NetworkAdapter");
            ManagementObjectCollection MOCol = MC.GetInstances();
            foreach (ManagementObject MO in MOCol)
                if (MO != null)
                    if (MO["MacAddress"] != null)
                        MacID += MO["MACAddress"].ToString();
            return MacID;
        }

        /// <summary>
        /// Returns the (not unique) host name of this computer.
        /// </summary>
        /// <returns></returns>
        public static string GetHostName()
        {
            return System.Net.Dns.GetHostName();
        }
    }
}
