﻿//////////////////////////////////////////////////////////////////////////////////////////////////
// CrypTool V2
// © 2008 - Gerhard Junker
// Apache License see http://www.apache.org/licenses/
//
// $HeadURL$
//////////////////////////////////////////////////////////////////////////////////////////////////
// $Revision::                                                                                $://
// $Author::                                                                                  $://
// $Date::                                                                                    $://
//////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
  /// <summary>
  /// Testclass for Twofish cypher
  /// </summary>
  [TestClass]
  public class TwofishTest
  {
    public TwofishTest()
    {
      // nothing to do
    }

    private TestContext testContextInstance;
    public TestContext TestContext
    {
      get
      {
        return testContextInstance;
      }
      set
      {
        testContextInstance = value;
      }
    }

    /// <summary>
    /// Converts the hex to byte.
    /// </summary>
    /// <param name="str">The STR.</param>
    /// <returns></returns>
    private byte[] ConvertHexToByte(string str)
    {
      int len = str.Length / 2;
      byte[] hex = new byte[len];
      for (int j = 0; j < len; j++)
      {
        hex[j] = byte.Parse(str.Substring(0, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
        str = str.Substring(2);
      }

      return hex;
    }

    /// <summary>
    /// Converts the byte to hex.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <returns></returns>
    private string ConvertByteToHex(byte[] bytes)
    {
      string tmp = "";
      foreach (byte b in bytes)
      {
        if (b < 0x10)
          tmp += "0";
        tmp += b.ToString("X");
      }
      return tmp;
    }

    [TestMethod]
    public void TwofishTestMethod()
    {
      // test vectors from 
      // http://www.schneier.com/plain/ecb_ival.txt
      //
      string[] source = 
      { 
        "00000000000000000000000000000000",
        "00000000000000000000000000000000",
        "00000000000000000000000000000000"
      };
      
      string[] result = 
      { 
        "9F589F5CF6122C32B6BFEC2F2AE8C35A",
        "CFD1D2E5A9BE9CDF501F13B892BD2248",
        "37527BE0052334B89F0CFCCAE87CFA20"
      };

      string[] key = 
      {
        "00000000000000000000000000000000",
        "0123456789ABCDEFFEDCBA98765432100011223344556677",
        "0123456789ABCDEFFEDCBA987654321000112233445566778899AABBCCDDEEFF",
      };


      ASCIIEncoding enc = new ASCIIEncoding();

      for (int i = 0; i < source.Length; i++)
      {
        byte[] iv = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        
        testContextInstance.WriteLine(" Test " + i.ToString());
        testContextInstance.WriteLine(" data = " + source[i]);

        TwofishManaged tf = TwofishManaged.Create();

        testContextInstance.WriteLine(" key  = " + key[i]);

        byte[] myKey = ConvertHexToByte(key[i]);
        ICryptoTransform encrypt =  tf.CreateEncryptor(myKey, iv);

        byte[] plain = ConvertHexToByte(source[i]);
        
        byte[] code = encrypt.TransformFinalBlock(plain, 0, plain.Length);

        string tmp = ConvertByteToHex(code);
        testContextInstance.WriteLine(" expected   = " + result[i]);
        testContextInstance.WriteLine(" calculated = " + tmp);

        Assert.AreEqual(tmp, result[i]);

        ICryptoTransform decrypt =  tf.CreateDecryptor(myKey, iv);

        byte[] plain2 = decrypt.TransformFinalBlock(code, 0, code.Length);
        string source2 = ConvertByteToHex(plain2);
        testContextInstance.WriteLine(" expected   = " + source[i]);
        testContextInstance.WriteLine(" calculated = " + source2);

        Assert.AreEqual(source[i], source2);
      }
    }

  }
}