using System.Windows;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.IO;
using System;

namespace File_Encrypter_and_Decrypter
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        String Encryptpath;
        String Decryptpath;
        private string Password = "EnterRandomPasswordHere";

        [DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr Destination, int Length);
        public static byte[] GenerateRandomSalt()
        {
            byte[] Data = new byte[32];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                for (int i = 0; i < 10; i++)
                {
                    rng.GetBytes(Data);
                }
            }

            return Data;
        }
        private void FileEncrypt(string InputFile, string Password)
        {
            byte[] Troep = GenerateRandomSalt();
            FileStream fsCrypt = new FileStream(InputFile + ".aes", FileMode.Create);
            byte[] PasswordBytes = System.Text.Encoding.UTF8.GetBytes(Password);
            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            AES.Padding = PaddingMode.PKCS7;
            var key = new Rfc2898DeriveBytes(PasswordBytes, Troep, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            AES.Mode = CipherMode.CFB;
            fsCrypt.Write(Troep, 0, Troep.Length);
            CryptoStream CS = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);
            FileStream fsIn = new FileStream(InputFile, FileMode.Open);
            byte[] buffer = new byte[1048576];
            int read;
            try
            {
                while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                {
                    CS.Write(buffer, 0, read);
                }
                fsIn.Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CS.Close();
                fsCrypt.Close();
            }
        }
        private void FileDecrypt(string InputFile, string OutputFile, string Password)
        {
            byte[] PasswordBytes = System.Text.Encoding.UTF8.GetBytes(Password);
            byte[] Troep = new byte[32];
            FileStream fsCrypt = new FileStream(InputFile, FileMode.Open);
            fsCrypt.Read(Troep, 0, Troep.Length);
            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            var key = new Rfc2898DeriveBytes(PasswordBytes, Troep, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            AES.Padding = PaddingMode.PKCS7;
            AES.Mode = CipherMode.CFB;
            CryptoStream CS = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);
            OutputFile = OutputFile.Replace(".aes", "");
            FileStream fsOut = new FileStream(OutputFile, FileMode.Create);
            int read;
            byte[] buffer = new byte[1048576];
            try
            {
                while ((read = CS.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fsOut.Write(buffer, 0, read);
                }
            }
            catch (CryptographicException ex_CryptographicException)
            {
                System.Windows.MessageBox.Show(ex_CryptographicException.Message, "Error",  MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                CS.Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CS.Close();
                fsOut.Close();
                fsCrypt.Close();
            }
        }

        private void Encrypt_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(EncryptFilepath.Text))
            {
                System.Windows.MessageBox.Show("The Filepath must not be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                GCHandle gch = GCHandle.Alloc(Password, GCHandleType.Pinned);
                Encryptpath = EncryptFilepath.Text;
                Path.GetFullPath(Encryptpath).Replace(@"\", @"\\");
                FileEncrypt(Encryptpath, Password);
                ZeroMemory(gch.AddrOfPinnedObject(), Password.Length * 2);
                gch.Free();
            }
        }

        private void EncryptBrowse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Title = "Select your file to Encrypt:";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
               string filename = dlg.FileName;
               EncryptFilepath.Text = filename;
            }
        }

        private void DecryptBrowse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".aes";
            dlg.Filter = "AES Files (*.aes)|*.aes";
            Nullable<bool> result = dlg.ShowDialog(); 
            if (result == true)
            {
                string filename = dlg.FileName;
                DecryptFilepath.Text = filename;
            }
        }

        private void Decrypt_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(DecryptFilepath.Text))
            {
                System.Windows.MessageBox.Show("The Filepath must not be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                GCHandle gch = GCHandle.Alloc(Password, GCHandleType.Pinned);
                Decryptpath = DecryptFilepath.Text;
                Path.GetFullPath(Decryptpath).Replace(@"\", @"\\");
                FileDecrypt(Decryptpath , Decryptpath, Password);
                ZeroMemory(gch.AddrOfPinnedObject(), Password.Length * 2);
                gch.Free();
            }
        }
    }
}
