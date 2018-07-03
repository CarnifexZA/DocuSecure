using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Security.Cryptography;
using System.IO;

namespace DocuSecure
{
    public partial class Form1 : Form
    {

        string mySourceFile = "";
        string myEncryptedFile = "";
        string myOutputFile = "";

        
        public Form1()
        {
            InitializeComponent();
        }

        private void WriteToFeedback(string msg, bool newline)
        {
            try
            {
                if (newline)
                {
                    txtFeedback.AppendText(string.Format("{0}\n", msg));
                }
                else
                {
                    txtFeedback.AppendText(string.Format("{0}", msg));
                }
            }
            catch (Exception ex)
            {

            }
        }



        #region AES ENCRYPT / DECRYPT
        // HELP MATERIAL - https://www.codeproject.com/Articles/769741/Csharp-AES-bits-Encryption-Library-with-Salt

        private byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        private byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }

        private string EncryptText(string input, string password)
        {
            // Get the bytes of the string
            byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            // Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

            string result = Convert.ToBase64String(bytesEncrypted);

            return result;
        }

        private string DecryptText(string input, string password)
        {
            // Get the bytes of the string
            byte[] bytesToBeDecrypted = Convert.FromBase64String(input);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

            string result = Encoding.UTF8.GetString(bytesDecrypted);

            return result;
        }

        private void EncryptFile()
        {
            try
            {
                string password = "abcd1234";

                byte[] bytesToBeEncrypted = File.ReadAllBytes(mySourceFile);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

                // Hash the password with SHA256
                passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

                //string fileEncrypted = "C:\\SampleFileEncrypted.DLL";

                File.WriteAllBytes(myOutputFile, bytesEncrypted);

                WriteToFeedback(string.Format("Encrypted File saved : {0}", myOutputFile), true);
                WriteToFeedback("", true);
            }
            catch (Exception ex)
            {
                WriteToFeedback(string.Format("EXCEPTION : {0}", ex.ToString()), true);
                WriteToFeedback("", true);
            }
        }

        private void DecryptFile()
        {
            try
            {
                //string fileEncrypted = "C:\\SampleFileEncrypted.DLL";
                string password = "abcd1234";

                byte[] bytesToBeDecrypted = File.ReadAllBytes(myEncryptedFile);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

                //string file = "C:\\SampleFile.DLL";
                File.WriteAllBytes(myOutputFile, bytesDecrypted);
                WriteToFeedback(string.Format("Decrypted File saved : {0}", myOutputFile), true);
                WriteToFeedback("", true);
            }
            catch (Exception ex)
            {
                WriteToFeedback(string.Format("EXCEPTION : {0}", ex.ToString()), true);
                WriteToFeedback("", true);
            }
        }

        #endregion



        

        private void btnBrowseSourceFile_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog opfd = new OpenFileDialog();
                opfd.Filter = "Adobe PDF files (*.pdf)|*.pdf";
                DialogResult result = opfd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    mySourceFile = opfd.FileName.ToString();
                    WriteToFeedback(string.Format("File Selected : {0}", mySourceFile), true);
                    WriteToFeedback("", true);
                }
                else
                {
                    WriteToFeedback("No file selected", true);
                    WriteToFeedback("", true);
                }
            }
            catch (Exception ex)
            {
                WriteToFeedback(string.Format("EXCEPTION : {0}", ex.ToString()), true);
                WriteToFeedback("", true);
            }
        }

        private void btnBrowseOutputDir_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    FolderBrowserDialog fbdl = new FolderBrowserDialog();
            //    DialogResult result = fbdl.ShowDialog();

            //    if (result == System.Windows.Forms.DialogResult.OK)
            //    {
            //        myOutputDir = fbdl.SelectedPath.ToString();
            //        WriteToFeedback(string.Format("Output Directory : {0}", myOutputDir), true);
            //        WriteToFeedback("", true);
            //    }
            //    else
            //    {
            //        WriteToFeedback("No Directory selected", true);
            //        WriteToFeedback("", true);
            //    }
            //}
            //catch (Exception ex)
            //{

            //}

            try
            {
                SaveFileDialog sfdl = new SaveFileDialog();
                sfdl.Filter = "Adobe PDF files (*.pdf)|*.pdf";
                DialogResult result = sfdl.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    myOutputFile = sfdl.FileName.ToString();
                    WriteToFeedback(string.Format("Output file selected : {0}", myOutputFile), true);
                    WriteToFeedback("", true);
                }
                else
                {
                    WriteToFeedback("No Directory selected", true);
                    WriteToFeedback("", true);
                }
            }
            catch (Exception ex)
            {
                WriteToFeedback(string.Format("EXCEPTION : {0}", ex.ToString()), true);
                WriteToFeedback("", true);
            }
        }

        private void btnBrowseEncryptedFile_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog opfd = new OpenFileDialog();
                opfd.Filter = "Adobe PDF files (*.pdf)|*.pdf";
                DialogResult result = opfd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    myEncryptedFile = opfd.FileName.ToString();
                    WriteToFeedback(string.Format("Encrypted File Selected : {0}", myEncryptedFile), true);
                    WriteToFeedback("", true);
                }
                else
                {
                    WriteToFeedback("No Encrypted file selected", true);
                    WriteToFeedback("", true);
                }
            }
            catch (Exception ex)
            {
                WriteToFeedback(string.Format("EXCEPTION : {0}", ex.ToString()), true);
                WriteToFeedback("", true);
            }
        }


        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            EncryptFile();
        }

        private void btnDecrypt_Click(object sender, EventArgs e)
        {
            DecryptFile();
        }

        
    }
}
