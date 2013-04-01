using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TestApp
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        private string uName;
        private string pwd;
        private bool galSearch;
        
        public Settings()
        {
            InitializeComponent();

            try
            {

                ReadUserData();
                userNameTB.Text = uName;
                passWordTB.Password = pwd;
                galSeachCB.IsChecked = galSearch;
            }
            catch
            {
            }
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        private void WriteUserData()
        {
            // create an isolated storage stream...
            IsolatedStorageFileStream userDataFile = new IsolatedStorageFileStream("UserData.dat", FileMode.Create);

            // create a writer to the stream...
            StreamWriter writeStream = new StreamWriter(userDataFile);
            
            // write strings to the Isolated Storage file...
            writeStream.WriteLine(userNameTB.Text);
            string tString = Crypto.EncryptStringAES(passWordTB.Password);
            writeStream.WriteLine(tString);
            writeStream.WriteLine(galSeachCB.IsChecked.Value.ToString());

            // Tidy up by flushing the stream buffer and then closing
            // the streams...
            writeStream.Flush();
            writeStream.Close();
            userDataFile.Close();
        }

        private void ReadUserData()
        {
            // create an isolated storage stream...
            IsolatedStorageFileStream userDataFile = new IsolatedStorageFileStream("UserData.dat", FileMode.Open);

            // create a reader to the stream...
            StreamReader readStream = new StreamReader(userDataFile);

            // write strings to the Isolated Storage file...
            uName = readStream.ReadLine();
            string tString = readStream.ReadLine();
            pwd = Crypto.DecryptStringAES(tString);
            galSearch = Convert.ToBoolean(readStream.ReadLine());

            // Tidy up by closing the streams...
            readStream.Close();
            userDataFile.Close();
        }

        private void saveB_Click(object sender, RoutedEventArgs e)
        {
            WriteUserData();
            this.Close();
        }

        private void closeB_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
