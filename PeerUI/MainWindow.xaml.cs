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

        //  User settings and the DLL file path.
        private string username, password, serverIP, sharedFolderPath, downloadFolderPath, dllFilePath = "";
        private int serverPort, localPort;
        //  UploadManager instance.
        private UploadManager uploadManager;
        //  UploadManager's thread.
        private Thread uploadManagerThread;
        //  Current user settings.
        private User user;
        //  XML serializer used to serialize and deserialize the config xml file.
        private XmlSerializer SerializerObj = new XmlSerializer(typeof(User));
        //  The WCF client, used to send and receive messages to and from the main WCF server.
        private WCFClient wcfClient;
        //  Holds the current search's results.
        private List<ServiceDataFile> searchResults;
        //  Holds the current transfers from and to the user.
        private List<FileProgressProperty> libraryFiles = new List<FileProgressProperty>();
        private ObservableCollection<FileProgressProperty> observableLibraryFile = new ObservableCollection<FileProgressProperty>();
        //  Event used by the UI to signal the DownloadManager to stop all downloading.
        public static event StopAllDownloading stopDownloadingEvent;


        public MainWindow() {
            InitializeComponent();
            listViewSearch.SizeChanged += ListView_SizeChanged;
            listViewLibrary.SizeChanged += ListView_SizeChanged;
        }

        /// <summary>
        /// Used by the UploadManager and the DownloadManager to update the UI with the transfers progress.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="fileName"></param>
        /// <param name="fileSize"></param>
        /// <param name="position"></param>
        /// <param name="time"></param>
        /// <param name="type"></param>
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
                        //  If the transfer hasn't finished yet.
                        if (position != 0) {
                            tempFileProgressProperty.Speed = (position / ((float)time / 1000)) / 1024 + "KB/s";
                            tempFileProgressProperty.Progress = (((float)position / fileSize) * 100);
                            tempFileProgressProperty.ElapsedTime = time;
                        }
                        //  If the transfer has finished.
                        else {
                            tempFileProgressProperty.Progress = 100;
                        }

                    });
                }
            }
            catch (TaskCanceledException) {
            }
        }

        /// <summary>
        /// Executes when the "Set" button next to the share folder settings is clicked.
        /// opens a folder browse dialog, to choose a new shared folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSharedFolder_Click(object sender, RoutedEventArgs e) {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog()) {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                textboxSharedFolder.Text = sharedFolderPath = dialog.SelectedPath;
            }
        }

        /// <summary>
        /// Executes when the "Set" button next to the download folder settings is clicked.
        /// opens a folder browse dialog, to choose a new incoming downloads folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonDownloadFolder_Click(object sender, RoutedEventArgs e) {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog()) {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                textboxDownloadFolder.Text = downloadFolderPath = dialog.SelectedPath;
            }
        }

        /// <summary>
        /// Saves the current input settings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonApply_Click(object sender, RoutedEventArgs e) {
            //  Retrieves the input settings given by the user.
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
            //  Saves the settings to the config xml file.
            saveConfigToXml(user);
            //  Creates a connection with the information supplied by the user.
            new Thread(() => { wcfClient.UpdateConfig(user);
            }).Start();
            //  Stop listening for new upload requests.
            if (uploadManagerThread.IsAlive)
                uploadManager.StopListening();
            //  Start listening for new upload requests, using the new settings.
            uploadManagerThread = new Thread(() => uploadManager.StartListening(localPort, sharedFolderPath));
            uploadManagerThread.Start();
        }

        /// <summary>
        /// Clears the settings in the settings tab.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClear_Click(object sender, RoutedEventArgs e) {
            clearSettings();
        }

        /// <summary>
        /// Retrieves the settings from the settings fields.
        /// </summary>
        private void getDetailsFromFields() {
            username = textboxUsername.Text;
            password = textboxPassword.Text;
            sharedFolderPath = textboxSharedFolder.Text;
            downloadFolderPath = textboxDownloadFolder.Text;
            serverIP = textboxServerIP.Text;
            if (!Int32.TryParse(textboxServerPort.Text, out serverPort))
                serverPort = 9876;
            if (!Int32.TryParse(textboxLocalPort.Text, out localPort))
                localPort = 9876;
        }

        /// <summary>
        /// Displays the config file's settings in the settings tab.
        /// </summary>
        private void loadDetailsFromUser() {
            textboxUsername.Text = user.Name;
            textboxPassword.Text = user.Password;
            textboxSharedFolder.Text = user.SharedFolderPath;
            textboxDownloadFolder.Text = user.DownloadFolderPath;
            textboxServerIP.Text = user.ServerIP;
            textboxServerPort.Text = Convert.ToString(user.ServerPort);
            textboxLocalPort.Text = Convert.ToString(user.LocalPort);
        }

        /// <summary>
        /// Clears the settings in the settings tab.
        /// </summary>
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

        /// <summary>
        /// Saves settings to the configuration xml file.
        /// </summary>
        /// <param name="user"></param>
        private void saveConfigToXml(User user) {
            TextWriter WriteFileStream = new StreamWriter(Properties.Resources.configFileName);
            SerializerObj.Serialize(WriteFileStream, user);
            WriteFileStream.Close();
        }

        /// <summary>
        /// Loads settings from the configuration xml file.
        /// </summary>
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

        /// <summary>
        /// This method executes when the window is closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            //  Stop accepting upload requests.
            uploadManager.StopListening();
            if (wcfClient != null)
                //  Close the connection to the main WCF server.
                if (wcfClient.userConnected)
                    new Thread(() => wcfClient.CloseConnection()).Start();
            if (stopDownloadingEvent != null)
                //  Stop all active downloads.
                stopDownloadingEvent();
        }

        /// <summary>
        /// This method executes when the window loads.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            buttonDownload.IsEnabled = false;
            listViewLibrary.ItemsSource = observableLibraryFile;
            uploadManager = new UploadManager(new TransferProgressDelegate(updateDownloadProgress), displayWcfMessage);
            try {
                //  If a configuration file exists.
                if (File.Exists(Properties.Resources.configFileName)) {
                    //  Load the config file.
                    loadConfigFromXml();
                    wcfClient = new WCFClient();
                    wcfClient.WcfMessageEvent += displayWcfMessage;
                    //  Update the WCF client with the config file.
                    new Thread(() => wcfClient.UpdateConfig(user)).Start();
                    //  Start listening for incoming upload requests.
                    uploadManagerThread = new Thread(() => uploadManager.StartListening(user.LocalPort, user.SharedFolderPath));
                    uploadManagerThread.Start();
                }
                //  If a configuration file doesn't exist, display an error message.
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

        /// <summary>
        /// Event to change the ListView's columns width when the ListView's size changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e) {
            ListView listView = sender as ListView;
            GridView gridView = listView.View as GridView;
            var newWidth = listView.ActualWidth / gridView.Columns.Count;
            foreach (var column in gridView.Columns) {
                column.Width = newWidth;
            }
        }



        /// <summary>
        /// Event to handle column minimum size.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="sizeChangedEventArgs"></param>
        private void HandleColumnHeaderSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs) {
            if (sizeChangedEventArgs.NewSize.Width <= 60) {
                sizeChangedEventArgs.Handled = true;
                ((GridViewColumnHeader)sender).Column.Width = 60;
            }
        }

        /// <summary>
        /// Downloads the selected file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonDownload_Click(object sender, RoutedEventArgs e) {
            SearchFileProperty searchFileProperty = (SearchFileProperty)listViewSearch.SelectedItem;
            foreach (ServiceDataFile sdf in searchResults) {
                if (sdf.Name == searchFileProperty.Name && sdf.Size == searchFileProperty.Size)
                    new Thread(() => new DownloadManager(sdf, user.DownloadFolderPath, 
                        new TransferProgressDelegate(updateDownloadProgress),
                        displayWcfMessage)).Start();
            }
        }

        /// <summary>
        /// Sends a request to the WCF main server to search a given file name.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSearch_Click(object sender, RoutedEventArgs e) {
            WcfFileRequestDelegate dlgt = new WcfFileRequestDelegate(wcfClient.FileRequest);
            IAsyncResult ar = dlgt.BeginInvoke(textboxSearch.Text,
            new AsyncCallback(FileSearchCallback), dlgt);

        }

        /// <summary>
        /// The file search callback, refreshes the search tab with the files found at the WCF main server.
        /// </summary>
        /// <param name="ar"></param>
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

        /// <summary>
        /// Displays error messages called by the ErrorMessageDelegate event
        /// </summary>
        /// <param name="error"></param>
        /// <param name="message"></param>
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

        /// <summary>
        /// Open a file dialog for the user to choose a DLL file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSetDLLPath_Click(object sender, RoutedEventArgs e) {
            using (var dialog = new System.Windows.Forms.OpenFileDialog()) {
                dialog.DefaultExt = ".dll";
                dialog.Filter = "DLL Files (*.dll)|*.dll";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    textboxDLLPath.Text = dllFilePath = dialog.FileName;
            }
        }

        /// <summary>
        /// Analyzes a given DLL file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonAnalyzeDLL_Click(object sender, RoutedEventArgs e) {
            string dllClassName = textboxDLLClassName.Text;
            
            //  If the DLL file exists, and the given class name isn't null or empty.
            if (File.Exists(dllFilePath) && !String.IsNullOrEmpty(dllClassName)) {
                Assembly assembly = Assembly.LoadFrom(dllFilePath);
                Console.WriteLine(assembly.GetName());
                Type t = assembly.GetType(dllClassName);
                //  If the type is indeed the given class's name.
                if (t != null) {
                    textblockDLLDetails.Text = "Name: " + t.Name + "\n" +
                        "Namespace: " + t.Namespace + "\n" +
                        "IsClass: " + t.IsClass + "\n" +
                        "IsAbstract: " + t.IsAbstract + "\n" +
                        "IsSealed: " + t.IsSealed + "\n" +
                        "IsPublic: " + t.IsPublic + "\n";

                    //  Display constructors.
                    textblockDLLDetails.Text += "\nConstructors:\n";
                    var ctors = t.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
                    ConcatDLLMembers(ctors);

                    //  Display methods.
                    textblockDLLDetails.Text += "\nMethods:\n";
                    var methods = t.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic |
                    BindingFlags.Public);
                    ConcatDLLMembers(methods);

                    //  Display fields.
                    textblockDLLDetails.Text += "\nFields:\n";
                    var fields = t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic |
                    BindingFlags.Public);
                    ConcatDLLMembers(fields);

                    //  Dispplay properties.
                    textblockDLLDetails.Text += "\nProperties:\n";
                    var props = t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic |
                    BindingFlags.Public);
                    ConcatDLLMembers(props);

                    //  Calling constructor with two parameters: string and integer.
                    object[] parametersArray = new Object[2];
                    parametersArray[0] = "Hello";
                    parametersArray[1] = 3;
                    object obj = Activator.CreateInstance(t, parametersArray);

                    //  Calling ToString method
                    MethodInfo mi = t.GetMethod("ToString");
                    string s = (string)mi.Invoke(obj, null);
                    textblockDLLDetails.Text += "\nToString():\n" + s + "\n";

                    //  Setting the integer property to 7
                    PropertyInfo pi = t.GetProperty("Integer");
                    pi.SetValue(obj, 7, null);

                    //  Calling the method that doubles the integer property
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

        /// <summary>
        /// Concats the DLL file's members to a single string.
        /// </summary>
        /// <param name="members"></param>
        public void ConcatDLLMembers(MemberInfo[] members) {
            foreach (MemberInfo memberInfo in members) {
                textblockDLLDetails.Text += memberInfo + "\n";
            }
        }
    }
}
