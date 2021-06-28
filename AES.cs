using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;

public class Userlogin
    {
        public string User { get; set; }
        public string Pass { get; set; }
    }
    public class ApiTestCryptoController : ApiController
    {
        [Route("api/ApiTestCryptoController/Test")]
        [HttpPost]
        [ResponseType(typeof(ResponceMessage))]
        public ResponceMessage Details(RequestMessage Req)
        {
            ResponceMessage Resp = new ResponceMessage();
            Resp.Header.RespCode = Req.Header.ReqCode;
            Resp.Header.DeviceId = Req.Header.DeviceId;
            Resp.Header.FunCode = Req.Header.FunCode;
            var Logdb = new Sys_Api_Logs();
            var apiLog = bool.Parse(ConfigurationManager.AppSettings["ApiLog"]);
            var refM = new RefMetho();
            var dt = new Store();
            try
            {
                var bodyConvert = JsonConvert.DeserializeObject<Userlogin>(JsonConvert.SerializeObject(Req.Body)) as Userlogin;
                Req.Body = bodyConvert;
                //> Write log input
                if (apiLog)
                {
                    refM = ApiHelpDta.WriteLog(Req, Resp, ref Logdb, MethodAction.Create);
                    if (refM.eCode != RefCodeType.Success)
                    {
                        Resp.Header.RespStatus = refM.eCode.ToString();
                        Resp.Header.RespSms = refM.eString;
                        return Resp;
                    }
                }

                var username = DecryptStringAES(bodyConvert.User);
                var password = DecryptStringAES(bodyConvert.Pass);

                var result = EncryptStringAES("Hello word!");
                var result2 = DecryptStringAES(result);
                Resp.Body = result;
                Resp.Header.RespTime = DateTime.Now.ToString("yyyyMMddhhmmss");
                Resp.Header.RespStatus = refM.eCode.ToString();
                Resp.Header.RespSms = refM.eString;
                Logdb.Process_Log = refM.LogCode;
                //> Write log out
                if (apiLog)
                {
                    var rt = ApiHelpDta.WriteLog(Req, Resp, ref Logdb, MethodAction.Update);
                    if (rt.eCode != RefCodeType.Success)
                    {
                        refM.eString += "WriteLog error: " + rt.eString;
                    }
                }
            }
            catch (Exception ex)
            {
                string sms;
                if (ex.InnerException != null)
                {
                    sms = ex.InnerException.Message;
                }
                else
                {
                    sms = ex.Message;
                }
                Resp.Header.RespTime = DateTime.Now.ToString("yyyyMMddhhmmss");
                Resp.Header.RespStatus = RefCodeType.ExceptionError.ToString();
                Resp.Header.RespSms = "Exception " + sms;
                Logdb.Process_Log = "ApiStore_Details > Ex > 96";
                //> Write log out
                if (apiLog)
                {
                    var rt = ApiHelpDta.WriteLog(Req, Resp, ref Logdb, MethodAction.Update);
                    if (rt.eCode != RefCodeType.Success)
                    {
                        Resp.Header.RespStatus = rt.eCode.ToString();
                        Resp.Header.RespSms = "Exception " + rt.eString;
                        return Resp;
                    }
                }
            }

            return Resp;
        }

        private static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            // Check arguments.  
            if (cipherText == null || cipherText.Length <= 0)
            {
                throw new ArgumentNullException("cipherText");
            }
            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }
            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }

            // Declare the string used to hold  
            // the decrypted text.  
            string plaintext = null;

            // Create an RijndaelManaged object  
            // with the specified key and IV.  
            using (var rijAlg = new RijndaelManaged())
            {
                //Settings  
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;

                rijAlg.Key = key;
                rijAlg.IV = iv;

                // Create a decrytor to perform the stream transform.  
                var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                try
                {
                    // Create the streams used for decryption.  
                    using (var msDecrypt = new MemoryStream(cipherText))
                    {
                        using(var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {

                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                // Read the decrypted bytes from the decrypting stream  
                                // and place them in a string.  
                                plaintext = srDecrypt.ReadToEnd();

                            }

                        }
                    }
                }
                catch
                {
                    plaintext = "keyError";
                }
            }

            return plaintext;
        }

        private static byte[] EncryptStringToBytes(string plainText, byte[] key, byte[] iv)
        {
            // Check arguments.  
            if (plainText == null || plainText.Length <= 0)
            {
                throw new ArgumentNullException("plainText");
            }
            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }
            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }
            byte[] encrypted;
            // Create a RijndaelManaged object  
            // with the specified key and IV.  
            using (var rijAlg = new RijndaelManaged())
            {
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;

                rijAlg.Key = key;
                rijAlg.IV = iv;

                // Create a decrytor to perform the stream transform.  
                var encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption.  
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.  
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.  
            return encrypted;
        }

        public static string DecryptStringAES(string cipherText)
        {
            var keybytes = Encoding.UTF8.GetBytes("8080808080808080");
            var iv = Encoding.UTF8.GetBytes("8080808080808080");

            var encrypted = Convert.FromBase64String(cipherText);
            var decriptedFromJavascript = DecryptStringFromBytes(encrypted, keybytes, iv);
            return string.Format(decriptedFromJavascript);
        }

        public static string EncryptStringAES(string clearText)
        {
            var keybytes = Encoding.UTF8.GetBytes("8080808080808080");
            var iv = Encoding.UTF8.GetBytes("8080808080808080");

            var decriptedFromJavascript = EncryptStringToBytes(clearText, keybytes, iv);
            return Convert.ToBase64String(decriptedFromJavascript);
        }
    }