using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using HtmlAgilityPack;
using System.Data;
using System.Web;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Reflection;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices;
using System.Security.Cryptography;

namespace TestApp
{
    public partial class MainWindow : Window
    {
        int count;
        string uName;
        string pwd;
        bool galSearch;

        bool cliArguements;

        public MainWindow()
        {
            InitializeComponent();
            emailsDG.ItemsSource = EmailCollection;

            try
            {
                ReadUserData();
            }
            catch
            {
            }

            string email = uName;
            string password = pwd;

            if (email == null || email == string.Empty || password == null || password == string.Empty)
            {
                var settingW = new Settings();
                settingW.Show();
            }
            cliArguements = false;
        }

        public MainWindow(List<string> emailsToSearch)
        {
            InitializeComponent();
            emailsDG.ItemsSource = EmailCollection;

            try
            {
                ReadUserData();
            }
            catch
            {
            }

            string email = uName;
            string password = pwd;

            if (email == null || email == string.Empty || password == null || password == string.Empty)
            {
                var settingW = new Settings();
                settingW.Show();
            }

            foreach (string emailToSearch in emailsToSearch)
            {
                inputTB.Text += emailToSearch + Environment.NewLine;
            }
            cliArguements = true;
            executeAllThis();
        }

        ObservableCollection<Email> EmailCollection = new ObservableCollection<Email>();

        public class Email
        {
            public string Sender { get; set; }
            public string Recipient { get; set; }

            public string Company { get; set; }
            public bool UserFound { get; set; }

            public string MessageID { get; set; }
            public string MessageSize { get; set; }

            public string Received { get; set; }
            public string Filtered { get; set; }
            public string FirstDAttempt { get; set; }
            public string FinalDAttempt { get; set; }

            public string FromIP { get; set; }
            public string ToIP { get; set; }

            public string FilterR { get; set; }
            public string DeliveryR { get; set; }
        }

        private void ReadUserData()
        {
            // create an isolated storage stream...
            IsolatedStorageFileStream userDataFile = new IsolatedStorageFileStream("UserData.dat", FileMode.Open);

            // create a reader to the stream...
            StreamReader readStream = new StreamReader(userDataFile);

            // write strings to the Isolated Storage file...
            uName = readStream.ReadLine();
            pwd = Crypto.DecryptStringAES(readStream.ReadLine());
            galSearch = Convert.ToBoolean(readStream.ReadLine());

            // Tidy up by closing the streams...
            readStream.Close();
            userDataFile.Close();
        }

        private void executeButton_Click(object sender, RoutedEventArgs e)
        {
            executeAllThis();
        }

        private void executeAllThis()
        {
            try
            {
                ReadUserData();
            }
            catch
            {
            }

            string email = uName;
            string password = pwd;

            if (email == null || email == string.Empty || password == null || password == string.Empty)
            {
                MessageBox.Show("Please enter FOPE credentials");
            }
            else
            {
                inputTB.IsEnabled = false;
                Normal.IsEnabled = false;
                Reverse.IsEnabled = false;
                executeButton.IsEnabled = false;
                bool loginAttempted = false;
                bool loginFailed = false;

                DateTime timeNow = DateTime.Now;
                TimeZone zone = TimeZone.CurrentTimeZone;
                TimeSpan offset = zone.GetUtcOffset(DateTime.Now);

                // IF searchign by message ID, please note it must be a fully valid msgID
                //string msgID = "";

                string[] myArray = inputTB.Text.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                List<string> URLList = new List<string>();
                ListBoxItem lbItem = new ListBoxItem();
                if (Normal.IsChecked == true)
                {
                    foreach (string emailA in myArray)
                    {
                        foreach (string domain in Domains)
                        {
                            // IF searching by message ID, add the following to the end of the next line:  + "&i=" + Uri.EscapeUriString(msgID)
                            URLList.Add("https://admin.messaging.microsoft.com/TraceMessage.mvc/AsyncMessageList/309489?&s=" + Uri.EscapeUriString(emailA) + "&r=" + domain + "&d=" + Uri.EscapeUriString(timeNow.AddDays(-29).ToString()) + "&e=" + Uri.EscapeUriString(timeNow.ToString()) + "&a=" + Uri.EscapeUriString(offset.Hours.ToString()));
                        }
                    }
                }
                else
                {
                    foreach (string emailA in myArray)
                    {
                        foreach (string domain in Domains)
                        {
                            URLList.Add("https://admin.messaging.microsoft.com/TraceMessage.mvc/AsyncMessageList/309489?&r=" + Uri.EscapeUriString(emailA) + "&s=" + domain + "&d=" + Uri.EscapeUriString(timeNow.AddDays(-29).ToString()) + "&e=" + Uri.EscapeUriString(timeNow.ToString()) + "&a=" + Uri.EscapeUriString(offset.Hours.ToString()));
                        }
                    }
                }
                var domainQueue = new Queue<string>(URLList);

                Action navigateQueue = () =>
                {
                    if (domainQueue.Count != 0)
                    {
                        this.wbControl.Navigate(domainQueue.Dequeue());
                    }
                    else
                    {
                        currentRLabel.Visibility = System.Windows.Visibility.Collapsed;
                        currentR.Visibility = System.Windows.Visibility.Collapsed;
                        currentSLabel.Visibility = System.Windows.Visibility.Collapsed;
                        currentS.Visibility = System.Windows.Visibility.Collapsed;

                        currentStatLabel.Visibility = System.Windows.Visibility.Visible;
                        currentStat.Visibility = System.Windows.Visibility.Visible;
                        currentStat.Content = "Completed searching";
                    }
                };

                this.wbControl.LoadCompleted += (o, e0) =>
                {
                    if (this.wbControl.IsLoaded == true)
                    {
                        dynamic doc = this.wbControl.Document;

                        try
                        {
                            if (loginAttempted == false)
                            {
                                doc.GetElementById("email").SetAttribute("value", email);
                                doc.GetElementById("Password").SetAttribute("value", password);
                                doc.GetElementById("submit_signin").Click();
                                loginAttempted = true;
                            }
                        }
                        catch
                        {
                        }

                        if (e0.Uri.AbsolutePath.Contains("AsyncMessageList"))
                        {
                            List<string> DetailsList = new List<string>();

                            string sVal = HttpUtility.ParseQueryString(e0.Uri.OriginalString.ToString()).Get("s");
                            string rVal = HttpUtility.ParseQueryString(e0.Uri.OriginalString.ToString()).Get("r");

                            currentS.Content = sVal;
                            currentR.Content = rVal;

                            DetailsList.AddRange(ExtractAllAHrefTags(doc));

                            count += DetailsList.Count;
                            emailFoundCount.Content = count.ToString();
                            foreach (string href in DetailsList)
                            {
                                domainQueue.Enqueue(href);
                            }
                            navigateQueue();
                        }
                        else if (e0.Uri.AbsolutePath.Contains("Details"))
                        {
                            currentRLabel.Visibility = System.Windows.Visibility.Collapsed;
                            currentR.Visibility = System.Windows.Visibility.Collapsed;
                            currentSLabel.Visibility = System.Windows.Visibility.Collapsed;
                            currentS.Visibility = System.Windows.Visibility.Collapsed;

                            currentStatLabel.Visibility = System.Windows.Visibility.Visible;
                            currentStat.Visibility = System.Windows.Visibility.Visible;
                            currentStat.Content = "Parsing individual records";

                            ParseEntries(doc);
                            navigateQueue();
                        }
                        else
                        {

                        }

                    }
                };
                if (loginAttempted == true && loginFailed == true)
                {
                    MessageBox.Show("You have entered invalid credentials. Please re-enter them and try again.", "Invalid Credentials", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
                else
                {
                    navigateQueue();
                }
            }
        }

        private void ParseEntries(dynamic inputDoc)
        {
            HtmlAgilityPack.HtmlDocument docHAP = new HtmlAgilityPack.HtmlDocument();
            docHAP.LoadHtml(inputDoc.Body.InnerHtml.ToString());

            Email tmpEmail = new Email();
            int i = 1;

            foreach (HtmlNode emNode in docHAP.DocumentNode.SelectNodes("//em"))
            {
                if (emNode.Attributes["class"] == null)
                {
                    if (i == 1) tmpEmail.Sender = emNode.InnerText.ToString();
                    else if (i == 2) tmpEmail.Recipient = emNode.InnerText.ToString();
                    else if (i == 3) tmpEmail.MessageID = emNode.InnerText.ToString();
                    else if (i == 4) tmpEmail.MessageSize = emNode.InnerText.ToString();
                    else if (i == 5) tmpEmail.Received = emNode.InnerText.ToString();
                    else if (i == 6) tmpEmail.Filtered = emNode.InnerText.ToString();
                    else if (i == 7) tmpEmail.FirstDAttempt = emNode.InnerText.ToString();
                    else if (i == 8) tmpEmail.FinalDAttempt = emNode.InnerText.ToString();
                    else if (i == 9) tmpEmail.FromIP = emNode.InnerText.ToString();
                    else if (i == 10) tmpEmail.ToIP = emNode.InnerText.ToString();
                    else if (i == 11) tmpEmail.FilterR = emNode.InnerText.ToString();
                    else if (i == 12) tmpEmail.DeliveryR = emNode.InnerText.ToString();

                    i++;
                }
            }

            if (galSearch == true)
            {
                try
                {
                    var galEntry = GALLookup(tmpEmail.Recipient);
                    if (galEntry.Mail != null)
                    {
                        tmpEmail.UserFound = true;
                        tmpEmail.Company = GALLookup(tmpEmail.Recipient).Company;
                    }
                }
                catch
                {
                }
            }

            EmailCollection.Add(tmpEmail);
        }

        private List<string> ExtractAllAHrefTags(dynamic inputDoc)
        {

            HtmlAgilityPack.HtmlDocument docHAP = new HtmlAgilityPack.HtmlDocument();
            docHAP.LoadHtml(inputDoc.Body.InnerHtml.ToString());

            List<string> hrefTags = new List<string>();
            try
            {
                foreach (HtmlNode link in docHAP.DocumentNode.SelectNodes("//a[@href]"))
                {
                    HtmlAttribute att = link.Attributes["href"];
                    hrefTags.Add("https://" + this.wbControl.Source.Host.ToString() + System.Web.HttpUtility.HtmlDecode(att.Value));
                }
            }
            catch
            {
            }

            return hrefTags;
        }

        private List<string> Domains
        {
            get
            {
                List<string> currentDomains = new List<string>();
                currentDomains.Add("example.com");
                return currentDomains;
            }
        }

        private void lpButton_Click(object sender, RoutedEventArgs e)
        {
            var settingW = new Settings();
            settingW.Show();
        }

        private void exportDataB_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            //saveFileDialog1.DefaultExt = ".xlsx";
            saveFileDialog1.Filter = "Excel Workbook|*.xlsx";
            //saveFileDialog1.Filter = "CSV|*.csv|Excel Workbook|*.xlsx";
            string format = "yyyyMMMd-HHmm";
            saveFileDialog1.FileName = "FOPEResults-" + DateTime.Now.ToString(format);
            saveFileDialog1.Title = "Export As";
            saveFileDialog1.ShowDialog();

            var tbl = IEnumerableExt.Ext_ToDataTable<Email>(EmailCollection);

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();

                using (ExcelPackage pck = new ExcelPackage())
                {
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add(saveFileDialog1.FileName);
                    ws.Cells["A1"].LoadFromDataTable(tbl, true);
                    ws.Cells[ws.Dimension.Address].AutoFilter = true;
                    ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    ws.View.FreezePanes(2, 1);

                    using (var rng = ws.Cells["A1:Z1"])
                    {
                        rng.Style.Font.Bold = true;
                        rng.Style.WrapText = false;
                        rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }
                    pck.SaveAs(fs);
                }
                fs.Close();
            }
        }
        private LDAPInformation GALLookup(string email)
        {
            try
            {
                var currentForest = Forest.GetCurrentForest();
                var globalCatalog = currentForest.FindGlobalCatalog();

                using (var searcher = globalCatalog.GetDirectorySearcher())
                {
                    searcher.ReferralChasing = ReferralChasingOption.All;

                    using (var entry = new DirectoryEntry(searcher.SearchRoot.Path))
                    {

                        //searcher.Filter = "(&(mailnickname=*)(objectCategory=user)(mail=" + email + "))";
                        searcher.Filter = "(&(mailnickname=*)(objectCategory=user)(proxyAddresses=SMTP:" + email + "))";

                        searcher.PropertyNamesOnly = true;
                        searcher.SearchScope = SearchScope.Subtree;
                        searcher.Sort.Direction = SortDirection.Ascending;
                        searcher.Sort.PropertyName = "displayName";

                        LDAPInformation[] allEntries = (searcher.FindAll().Cast<SearchResult>().Select(result => new LDAPInformation(result.GetDirectoryEntry())).ToArray());
                        if (allEntries.Length == 0)
                        {
                            searcher.Filter = "(&(mailnickname=*)(objectCategory=user)(proxyAddresses=smtp:" + email + "))";
                            allEntries = (searcher.FindAll().Cast<SearchResult>().Select(result => new LDAPInformation(result.GetDirectoryEntry())).ToArray());
                        }
                        if (allEntries.Length == 0)
                        {
                            searcher.Filter = "(&(mailnickname=*)(objectCategory=user)(mail=smtp:" + email + "))";
                            allEntries = (searcher.FindAll().Cast<SearchResult>().Select(result => new LDAPInformation(result.GetDirectoryEntry())).ToArray());
                        }

                        LDAPInformation temp = null;
                        foreach (LDAPInformation singleEntry in allEntries)
                        {
                            temp = singleEntry;
                        }
                        return temp;
                    }
                }
            }
            catch (ActiveDirectoryOperationException e)
            {
            }
            return null;
        }
    }

    class LDAPInformation
    {
        internal LDAPInformation(DirectoryEntry entry)
        {
            //Section: HASH
            this.sAMAccountName = (string)entry.Properties["sAMAccountName"].Value;

            //Section: Email
            this.Mail = (string)entry.Properties["mail"].Value;
            foreach (string proxyAddr in entry.Properties["proxyAddresses"])
            {
                //Make it 'case-insensative'
                if (proxyAddr.ToLower().StartsWith("smtp:"))
                    //Get the email string from AD
                    this.ProxyAddresses += proxyAddr.Substring(5) + "|";
            }
            if (this.ProxyAddresses != null)
            {
                this.ProxyAddresses = this.ProxyAddresses.Remove(this.ProxyAddresses.Length - 1);
            }

            //Section: Organziation
            this.Description = (string)entry.Properties["description"].Value;
            this.Company = (string)entry.Properties["company"].Value;
            this.Title = (string)entry.Properties["title"].Value;
            this.Department = (string)entry.Properties["department"].Value;

            //Section: Name
            this.DisplayName = (string)entry.Properties["displayName"].Value;
            this.FirstName = (string)entry.Properties["firstName"].Value;
            this.MiddleName = (string)entry.Properties["middleName"].Value;
            this.LastName = (string)entry.Properties["lastName"].Value;

            //Section: Address
            this.StreetAddress = (string)entry.Properties["streetAddress"].Value;
            this.City = (string)entry.Properties["city"].Value;
            this.State = (string)entry.Properties["state"].Value;
            this.PostalCode = (string)entry.Properties["postalCode"].Value;
            this.TelephoneNumber = (string)entry.Properties["telephoneNumber"].Value;

            //Section: Administrative
            if (entry.Properties["userAccountControl"].Value != null)
            {
                UserAccountControlFlags userAccountControl = (UserAccountControlFlags)entry.Properties["userAccountControl"].Value;
                this.AccountFlags = userAccountControl.ToString();
            }
            this.LastLogon = (string)entry.Properties["lastLogon"].Value;

            foreach (string memOf in entry.Properties["memberOf"])
            {
                //this.MemberOf += memOf.ToString() + "|";
                string[] t0 = memOf.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string[] t1 = t0[0].Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                this.MemberOf += t1[1].ToString() + "|";
            }
            if (this.MemberOf != null)
            {
                this.MemberOf = this.MemberOf.Remove(this.MemberOf.Length - 1);
            }
        }

        public string DisplayName
        {
            get;
            private set;
        }

        public string Mail
        {
            get;
            private set;
        }

        public string ProxyAddresses
        {
            get;
            private set;
        }

        public string sAMAccountName
        {
            get;
            private set;
        }

        public string Description
        {
            get;
            private set;
        }

        public string Company
        {
            get;
            private set;
        }

        public string Title
        {
            get;
            private set;
        }

        public string Department
        {
            get;
            private set;
        }

        public string FirstName
        {
            get;
            private set;
        }

        public string MiddleName
        {
            get;
            private set;
        }

        public string LastName
        {
            get;
            private set;
        }

        public string StreetAddress
        {
            get;
            private set;
        }

        public string City
        {
            get;
            private set;
        }

        public string State
        {
            get;
            private set;
        }

        public string PostalCode
        {
            get;
            private set;
        }

        public string TelephoneNumber
        {
            get;
            private set;
        }

        public string AccountFlags
        {
            get;
            private set;
        }

        public string LastLogon
        {
            get;
            private set;
        }

        public string MemberOf
        {
            get;
            private set;
        }

        [Flags]
        public enum UserAccountControlFlags
        {
            Script = 0x1,
            AccountDisabled = 0x2,
            HomeDirectoryRequired = 0x8,
            AccountLockedOut = 0x10,
            PasswordNotRequired = 0x20,
            PasswordCannotChange = 0x40,
            EncryptedTextPasswordAllowed = 0x80,
            TempDuplicateAccount = 0x100,
            NormalAccount = 0x200,
            InterDomainTrustAccount = 0x800,
            WorkstationTrustAccount = 0x1000,
            ServerTrustAccount = 0x2000,
            PasswordDoesNotExpire = 0x10000,
            MnsLogonAccount = 0x20000,
            SmartCardRequired = 0x40000,
            TrustedForDelegation = 0x80000,
            AccountNotDelegated = 0x100000,
            UseDesKeyOnly = 0x200000,
            DontRequirePreauth = 0x400000,
            PasswordExpired = 0x800000,
            TrustedToAuthenticateForDelegation = 0x1000000,
            NoAuthDataRequired = 0x2000000
        }
    }

    public static class IEnumerableExt
    {
        public static DataTable Ext_ToDataTable<T>(this IEnumerable<T> varlist)
        {
            DataTable dtReturn = new DataTable();

            // column names 
            PropertyInfo[] oProps = null;
            FieldInfo[] oField = null;
            if (varlist == null) return dtReturn;

            foreach (T rec in varlist)
            {
                // Use reflection to get property names, to create table, Only first time, others will follow 
                if (oProps == null)
                {
                    oProps = ((Type)rec.GetType()).GetProperties();
                    foreach (PropertyInfo pi in oProps)
                    {
                        Type colType = pi.PropertyType;

                        if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                        {
                            colType = colType.GetGenericArguments()[0];
                        }

                        dtReturn.Columns.Add(new DataColumn(pi.Name, colType));
                    }
                    oField = ((Type)rec.GetType()).GetFields();
                    foreach (FieldInfo fieldInfo in oField)
                    {
                        Type colType = fieldInfo.FieldType;

                        if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                        {
                            colType = colType.GetGenericArguments()[0];
                        }

                        dtReturn.Columns.Add(new DataColumn(fieldInfo.Name, colType));
                    }
                }

                DataRow dr = dtReturn.NewRow();

                if (oProps != null)
                {
                    foreach (PropertyInfo pi in oProps)
                    {
                        dr[pi.Name] = pi.GetValue(rec, null) ?? DBNull.Value;
                    }
                }
                if (oField != null)
                {
                    foreach (FieldInfo fieldInfo in oField)
                    {
                        dr[fieldInfo.Name] = fieldInfo.GetValue(rec) ?? DBNull.Value;
                    }
                }
                dtReturn.Rows.Add(dr);
            }
            return dtReturn;
        }
    }
}
