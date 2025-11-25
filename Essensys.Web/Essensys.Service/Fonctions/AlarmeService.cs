using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Essensys.Repository.DTO;
using Essensys.Service.Transaction;
using System.Security.Cryptography;
using System.IO;
using Essensys.Common;
using Essensys.Service.Security;

namespace Essensys.Service.Fonctions
{
    public static class AlarmeService
    {
        public static void RegisterAction(EsUser u, EsMachine m, bool activate, string alresp, string codealarme)
        {
            string alarmeorder = "ALARME";
            if (activate)
                alarmeorder += "ON";
            else
                alarmeorder += "OFF";
            alarmeorder += "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            if (alresp == "")
            {
                throw new Exception("La saisie via code est obsolète");
                //string action = "Alarme via code :";
                //Dictionary<Tbb_Donnees_Index, string> indexes = new Dictionary<Tbb_Donnees_Index, string>();

                //if (activate)
                //{
                //    action += " ON";
                //    indexes.Add(Tbb_Donnees_Index.Alarme_Commande, "1");
                //    indexes.Add(Tbb_Donnees_Index.Alarme_CodeSaisiLSB, codealarme.Substring(0, 2));
                //    indexes.Add(Tbb_Donnees_Index.Alarme_CodeSaisiMSB, codealarme.Substring(2, 2));
                //}
                //else
                //{
                //    action += " OFF";
                //    indexes.Add(Tbb_Donnees_Index.Alarme_Commande, "0");
                //    indexes.Add(Tbb_Donnees_Index.Alarme_CodeSaisiLSB, codealarme.Substring(0, 2));
                //    indexes.Add(Tbb_Donnees_Index.Alarme_CodeSaisiMSB, codealarme.Substring(2, 2));
                //}
                //new ActionService().RegisterAction(m, EsActionType.ALARME, action, indexes);
            }
            else
            {
                LogManager.LogTrace("Test question alarme", null);
                if (new UserService().TestQuestion(u, alresp))
                {
                    LogManager.LogTrace("Register alarme", null);
                    new ActionService().RegisterAction(m, EsActionType.ALARME, EncryptString(alarmeorder, m.Pkey));
                }
            }
        }

        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");
            byte[] encrypted;
            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
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

        private static string EncryptString(string alarmeorder, string strKey)
        {
            string strIv = HashHelper.GetHash(strKey, HashHelper.HashType.MD5).Substring(0, 16);
            Console.WriteLine(strIv);
            // Place le texte à chiffrer dans un tableau d'octets
            byte[] plainText2 = Encoding.UTF8.GetBytes(alarmeorder);
            byte[] plainText = new byte[16];
            plainText[0] = plainText2[0];
            plainText[1] = plainText2[1];
            // Place la clé de chiffrement dans un tableau d'octets
            byte[] key = Encoding.UTF8.GetBytes(strKey);
            Console.WriteLine(plainText.Length);
            // Place le vecteur d'initialisation dans un tableau d'octets
            byte[] iv = Encoding.UTF8.GetBytes(strIv);


            AesManaged rijndael = new AesManaged();

            // Définit le mode utilisé
            rijndael.Mode = CipherMode.CBC;
            rijndael.BlockSize = 128;
            rijndael.KeySize = 256;


            // Crée le chiffreur AES - Rijndael
            ICryptoTransform aesEncryptor = rijndael.CreateEncryptor(key, iv);
            MemoryStream ms = new MemoryStream();

            // Ecris les données chiffrées dans le MemoryStream
            CryptoStream cs = new CryptoStream(ms, aesEncryptor, CryptoStreamMode.Write);
            cs.Write(plainText2, 0, plainText2.Length);
            cs.FlushFinalBlock();

            // Place les données chiffrées dans un tableau d'octet
            byte[] CipherBytes = ms.ToArray();

            ms.Close();
            cs.Close();

            // Place les données chiffrées dans une chaine
            string res = "";
            foreach (byte b in CipherBytes)
            {
                res += b.ToString() + ";";
            }
            return res.Substring(0, res.Length - 1);
        }
    }
}
