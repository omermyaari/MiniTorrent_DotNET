using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using System.IO;
using System.Threading;
using PeerUI.Entities;
using PeerUI.Communication;
using TorrentWcfServiceLibrary;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Reflection;

namespace PeerUI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public delegate void TransferProgressDelegate(int transferId, string fileName, long fileSize, long position, long time, TransferType type);
    public delegate void StopAllDownloading();
    public delegate void WcfMessageDelegate(bool error, string message);
    public delegate List<ServiceDataFile> WcfFileRequestDelegate(string fileName);
    public partial class MainWindow : Window {

        private string username, password, serverIP, sharedFolderPath, downloadFolderPath, dllFilePath = "";
        private int serverPort, localPort;
        private bool configExists;
        private UploadManager uploadManager;
        private Thread uploadManagerThread;
        private User user;
        private XmlSerializer SerializerObj = new XmlSerializer(typeof(User));
        private WCFClient wcfClient;
        private List<ServiceDataFile> searchResults;
        private List<FileProgressProperty> libraryFiles = new List<FileProgressProperty>();
        private ObservableCollection<FileProgressProperty> observableLibraryFile = new ObservableCollection<FileProgressProperty>();
        
        public static event StopAllDownloading stopDownloadingEvent;


        public MainWindow() {
            InitializeComponent();
            listViewSearch.SizeChanged += ListView_SizeChanged;
            listViewLibrary.SizeChanged += ListView_SizeChanged;
        }

        public void updateDownloadProgress(int transferId, string fileName, long fileSize, long position, long time, TransferType type) {
            try {
                FileProgressProperty tempFileProgressProperty;
                if ((tempFileProgressProperty = observableLibraryFile.Where(x => x.TransferId == transferId).FirstOrDefault()) == null) {
                    Dispatcher.Invoke((Action)delegate {
                        var fileProgressProperty = new FileProgressProperty(transferId, type.ToString(), fileName, fileSize, (((float)position / fileSize) * 100), (position / ((float)time / 1000)) / 1024 + "KB/s");
                        observableLibraryFile.Add(fileProgressProperty);
                    });
                }
                else {
                    Dispatcher.Invoke((Action)delegate {
                        if (position != 0) {
                            tempFileProgressProperty.Speed = (position / ((float)time / 1000)) / 1024 + "KB/s";
                            tempFileProgressProperty.Progress = (((float)position / fileSize) * 100);
                            tempFileProgressProperty.ElapsedTime = time;
                            //Console.WriteLine("PROGRESS = " + tempFileProgressProperty.Progress);
                        }
                        else {
                            tempFileProgressProperty.Progress = 100;
                        }

                    });
                }
            }
            catch (TaskCanceledException) {
            }
        }

        private void buttonSharedFolder_Click(object sender, RoutedEventArgs e) {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog()) {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                textboxSharedFolder.Text = sharedFolderPath = dialog.SelectedPath;
            }
        }

        private void buttonDownloadFolder_Click(object sender, RoutedEventArgs e) {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog()) {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                textboxDownloadFolder.Text = downloadFolderPath = dialog.SelectedPath;
            }
        }

        //  Saves the current input settings.
        private void buttonApply_Click(object sender, RoutedEventArgs e) {
            getDetailsFromFields();
            user = new User
            {
                Name = username,
                Password = password,
                ServerIP = serverIP,
                ServerPort = serverPort,
                LocalPort = localPort,
                SharedFolderPath = sharedFolderPath,
                DownloadFolderPath = downloadFolderPath
            };
            DataContext = user;
            saveConfigToXml(user);
            new Thread(() => {
                wcfClient.CloseConnection();
                wcfClient.UpdateConfig(user);
            }).Start();
            if (uploadManagerThread.IsAlive)
                uploadManager.StopListening();
            uploadManagerThread = new Thread(() => uploadManager.StartListening(localPort, sharedFolderPath));
            uploadManagerThread.Start();
        }

        //  Clears the config settings.
        private void buttonClear_Click(object sender, RoutedEventArgs e) {
            clearSettings();
        }

        //  TODO validator?
        private void getDetailsFromFields() {
            username = textboxUsername.Text;
            password = textboxPassword.Text;
            sharedFolderPath = textboxSharedFolder.Text;
            downloadFolderPath = textboxDownloadFolder.Text;
            serverIP = textboxServerIP.Text;
            if (Int32.TryParse(textboxServerPort.Text, out serverPort))
                serverPort = 9876;
            if (Int32.TryParse(textboxLocalPort.Text, out localPort))
                localPort = 9876;
        }

        private void loadDetailsFromUser() {
            textboxUsername.Text = user.Name;
            textboxPassword.Text = user.Password;
            textboxSharedFolder.Text = user.SharedFolderPath;
            textboxDownloadFolder.Text = user.DownloadFolderPath;
            textboxServerIP.Text = user.ServerIP;
            textboxServerPort.Text = Convert.ToString(user.ServerPort);
            textboxLocalPort.Text = Convert.ToString(user.LocalPort);
        }

        //  Clears the settings in the settings tab.
        private void clearSettings() {
            foreach (var item in gridConfig.Children) {
                if (item is TextBox) {
                    TextBox tb = (TextBox)item;
                    tb.Clear();
                }
            }
            textboxDownloadFolder.Clear();
            textboxSharedFolder.Clear();
        }

        //  Saves settings to the configuration xml file.
        private void saveConfigToXml(User user) {
            TextWriter WriteFileStream = new StreamWriter(Properties.Resources.configFileName);
            SerializerObj.Serialize(WriteFileStream, user);
            WriteFileStream.Close();
        }

        //  Loads settings from the configuration xml file.
        private void loadConfigFromXml() {
            try {
                var reader = new StreamReader(Properties.Resources.configFileName);
                user = (User)SerializerObj.Deserialize(reader);
                DataContext = user;
                reader.Close();
                loadDetailsFromUser();
            }
            catch (InvalidOperationException ex) {
                throw ex;
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            uploadManager.StopListening();
            if (wcfClient != null)
                new Thread(() => wcfClient.CloseConnection()).Start();
            if (stopDownloadingEvent != null)
                stopDownloadingEvent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            buttonDownload.IsEnabled = false;
            listViewLibrary.ItemsSource = observableLibraryFile;
            uploadManager = new UploadManager(new TransferProgressDelegate(updateDownloadProgress));
            try {
                if (configExists = File.Exists(Properties.Resources.configFileName)) {
                    loadConfigFromXml();
                    wcfClient = new WCFClient();
                    wcfClient.WcfMessageEvent += displayWcfMessage;
                    new Thread(() => wcfClient.UpdateConfig(user)).Start();
                    uploadManagerThread = new Thread(() => uploadManager.StartListening(user.LocalPort, user.SharedFolderPath));
                    uploadManagerThread.Start();
                }
                else {
                    textblockStatus.Foreground = Brushes.Red;
                    textblockStatus.Text = Properties.Resources.errorConfigFileNotExist;
                    settingsTab.IsSelected = true;
                }
            }
            catch (InvalidOperationException) {
                displayWcfMessage(true, Properties.Resources.errorConfigFileBad);
            }
        }

        //  Event to change the ListView's columns width when the ListView's size changes.
        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e) {
            ListView listView = sender as ListView;
            GridView gridView = listView.View as GridView;
            var newWidth = listView.ActualWidth / gridView.Columns.Count;
            foreach (var column in gridView.Columns) {
                column.Width = newWidth;
            }
        }



        //  Event to handle column minimum size.
        private void HandleColumnHeaderSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs) {
            if (sizeChangedEventArgs.NewSize.Width <= 60) {
                sizeChangedEventArgs.Handled = true;
                ((GridViewColumnHeader)sender).Column.Width = 60;
            }
        }

        private void buttonDownload_Click(object sender, RoutedEventArgs e) {
            SearchFileProperty searchFileProperty = (SearchFileProperty)listViewSearch.SelectedItem;
            foreach (ServiceDataFile sdf in searchResults) {
                if (sdf.Name == searchFileProperty.Name && sdf.Size == searchFileProperty.Size)
                    new Thread(() => new DownloadManager(sdf, user.DownloadFolderPath, 
                        new TransferProgressDelegate(updateDownloadProgress),
                        displayWcfMessage)).Start();
            }
        }

        //  Searches the main server for files shared by all connected peers.
        private void buttonSearch_Click(object sender, RoutedEventArgs e) {
            WcfFileRequestDelegate dlgt = new WcfFileRequestDelegate(wcfClient.FileRequest);
            IAsyncResult ar = dlgt.BeginInvoke(textboxSearch.Text,
            new AsyncCallback(FileSearchCallback), dlgt);

        }

        private void FileSearchCallback(IAsyncResult ar) {
            WcfFileRequestDelegate dlgt = (WcfFileRequestDelegate)ar.AsyncState;
            searchResults = dlgt.EndInvoke(ar);
            if (searchResults == null)
                return;
            List<SearchFileProperty> items = new List<SearchFileProperty>();
            foreach (ServiceDataFile sdf in searchResults) {
                items.Add(new SearchFileProperty(sdf.Name, sdf.Size, sdf.PeerList.Count));
            }
            Dispatcher.Invoke((Action)delegate {
                listViewSearch.ItemsSource = items;
                if (items.Count > 0)
                    buttonDownload.IsEnabled = true;
                else
                    buttonDownload.IsEnabled = false;
            });
        }

        //  Displays error messages called by the ErrorMessageDelegate event
        private void displayWcfMessage(bool error, string message) {
            try {
                Dispatcher.Invoke((Action)delegate {
                    textblockStatus.Foreground = (error) ? Brushes.Red : Brushes.Green;
                    textblockStatus.Text = message;
                });
            }
            catch (TaskCanceledException) {

            }
        }

        private void buttonSetDLLPath_Click(object sender, RoutedEventArgs e) {
            using (var dialog = new System.Windows.Forms.OpenFileDialog()) {
                dialog.DefaultExt = ".dll";
                dialog.Filter = "DLL Files (*.dll)|*.dll";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    textboxDLLPath.Text = dllFilePath = dialog.FileName;
            }
        }

        private void buttonAnalyzeDLL_Click(object sender, RoutedEventArgs e) {
            string dllClassName = textboxDLLClassName.Text;
            if (File.Exists(dllFilePath) && !String.IsNullOrEmpty(dllClassName)) {
                Assembly assembly = Assembly.LoadFrom(dllFilePath);
                Console.WriteLine(assembly.GetName());
                Type t = assembly.GetType(dllClassName);
                if (t != null) {
                    textblockDLLDetails.Text = "Name: " + t.Name + "\n" +
                        "Namespace: " + t.Namespace + "\n" +
                        "IsClass: " + t.IsClass + "\n" +
                        "IsAbstract: " + t.IsAbstract + "\n" +
                        "IsSealed: " + t.IsSealed + "\n" +
                        "IsPublic: " + t.IsPublic + "\n";

                    //  Constructors
                    textblockDLLDetails.Text += "\nConstructors:\n";
                    var ctors = t.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
                    ConcatDLLMembers(ctors);

                    //  Methods
                    textblockDLLDetails.Text += "\nMethods:\n";
                    var methods = t.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic |
                    BindingFlags.Public);
                    ConcatDLLMembers(methods);

                    //  Fields
                    textblockDLLDetails.Text += "\nFields:\n";
                    var fields = t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic |
                    BindingFlags.Public);
                    ConcatDLLMembers(fields);

                    //  Properties
                    textblockDLLDetails.Text += "\nProperties:\n";
                    var props = t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic |
                    BindingFlags.Public);
                    ConcatDLLMembers(props);

                    //  Calling constructor
                    object[] parametersArray = new Object[2];
                    parametersArray[0] = "Hello";
                    parametersArray[1] = 3;
                    object obj = Activator.CreateInstance(t, parametersArray);

                    //  Calling ToString method
                    MethodInfo mi = t.GetMethod("ToString");
                    string s = (string)mi.Invoke(obj, null);
                    textblockDLLDetails.Text += "\nToString():\n" + s + "\n";

                    //  Setting the Integer property to 7
                    PropertyInfo pi = t.GetProperty("Integer");
                    pi.SetValue(obj, 7, null);

                    //  Calling the method that doubles the Integer property
                    MethodInfo mi2 = t.GetMethod("DoubleInt");
                    mi2.Invoke(obj, null);

                    //  Calling ToString method again
                    s = (string)mi.Invoke(obj, null);
                    textblockDLLDetails.Text += "\nToString() after setting the integer " +
                        "property to 7 and using the DoubleInt method:\n" + s + "\n";
                }
                else {
                    textblockStatus.Foreground = Brushes.Red;
                    textblockStatus.Text = Properties.Resources.errorDLLReading;
                }
            }
            else {
                textblockStatus.Foreground = Brushes.Red;
                textblockStatus.Text = Properties.Resources.errorDLLFileError;
            }
        }

        //
        public void ConcatDLLMembers(MemberInfo[] members) {
            foreach (MemberInfo memberInfo in members) {
                textblockDLLDetails.Text += memberInfo + "\n";
            }
        }
    }
}
