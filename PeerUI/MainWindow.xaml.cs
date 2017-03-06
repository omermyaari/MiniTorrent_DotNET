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

namespace PeerUI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public delegate void TransferProgressDelegate(int transferId, string fileName, long fileSize, long position, long time, TransferType type);
    public partial class MainWindow : Window {

        private string username;
        private string password;

        private string serverIP;
        private int serverPort;
        private int localPort;
        private string sharedFolderPath;
        private string downloadFolderPath;
        private UploadManager uploadManager;
        private Thread uploadManagerThread;
        private bool configExists;
        private User user;
        private XmlSerializer SerializerObj = new XmlSerializer(typeof(User));
        private WCFClient wcfClient;
        private List<ServiceDataFile> searchResults;
        private List<FileProgressProperty> libraryFiles = new List<FileProgressProperty>();
        private ObservableCollection<FileProgressProperty> observableLibraryFile = new ObservableCollection<FileProgressProperty>();

        public MainWindow() {
            InitializeComponent();
            listViewSearch.SizeChanged += ListView_SizeChanged;
            listViewLibrary.SizeChanged += ListView_SizeChanged;
        }

        public void updateDownloadProgress(int transferId, string fileName, long fileSize, long position, long time, TransferType type) {
            try {
                FileProgressProperty tempFileProgressProperty;
                if ((tempFileProgressProperty = observableLibraryFile.Where(x => x.TransferId == transferId).FirstOrDefault()) == null) {
                    this.Dispatcher.Invoke((Action)delegate {
                        var fileProgressProperty = new FileProgressProperty(transferId, type.ToString(), fileName, fileSize, (((float)position / fileSize) * 100), (position / ((float)time / 1000)) / 1024 + "KB/s");
                        observableLibraryFile.Add(fileProgressProperty);
                    });
                }
                else {
                    this.Dispatcher.Invoke((Action)delegate {
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
            catch (TaskCanceledException taskCancelled) {
                Console.WriteLine("Task was cancelled");
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
            saveConfigToXml(user);
            wcfClient.CloseConnection();
            wcfClient.UpdateConfig(user);
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
            password = passwordboxPassword.Password;
            sharedFolderPath = textboxSharedFolder.Text;
            downloadFolderPath = textboxDownloadFolder.Text;
            serverIP = textboxServerIP.Text;
            Int32.TryParse(textboxServerPort.Text, out serverPort);
            Int32.TryParse(textboxLocalPort.Text, out localPort);
        }

        private void loadDetailsFromUser() {
            textboxUsername.Text = user.Name;
            passwordboxPassword.Password = user.Password;
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
            TextWriter WriteFileStream = new StreamWriter("MyConfig.xml");
            SerializerObj.Serialize(WriteFileStream, user);
            WriteFileStream.Close();
        }

        //  Loads settings from the configuration xml file.
        private void loadConfigFromXml() {
            var reader = new StreamReader("MyConfig.xml");
            user = (User)SerializerObj.Deserialize(reader);
            reader.Close();
            loadDetailsFromUser();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            uploadManager.StopListening();
            wcfClient.CloseConnection();
            DownloadManager.stopDownloading = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            buttonDownload.IsEnabled = false;
            listViewLibrary.ItemsSource = observableLibraryFile;
            uploadManager = new UploadManager(new TransferProgressDelegate(updateDownloadProgress));
            if (configExists = File.Exists("MyConfig.xml")) {
                loadConfigFromXml();
                wcfClient = new WCFClient(user);
                uploadManagerThread = new Thread(()=> uploadManager.StartListening(user.LocalPort, user.SharedFolderPath));
                uploadManagerThread.Start();
            }
            else {
                MessageBox.Show("Config file does not exist, please fill the settings.", "Error");
                settingsTab.IsSelected = true;
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
                    new Thread(() => new DownloadManager(sdf, user.DownloadFolderPath, new TransferProgressDelegate(updateDownloadProgress))).Start();
            }
        }

        //  Searches the main server for files shared by all connected peers.
        private void buttonSearch_Click(object sender, RoutedEventArgs e) {
            searchResults = wcfClient.FileRequest(textboxSearch.Text);
            List<SearchFileProperty> items = new List<SearchFileProperty>();
            foreach (ServiceDataFile sdf in searchResults) {
                items.Add(new SearchFileProperty(sdf.Name, sdf.Size, sdf.PeerList.Count));
            }
            listViewSearch.ItemsSource = items;
            if (items.Count > 0)
                buttonDownload.IsEnabled = true;
            else
                buttonDownload.IsEnabled = false;
        }
    }
}
