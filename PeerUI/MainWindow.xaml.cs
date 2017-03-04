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

namespace PeerUI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private string username;
        private string password;

        private string serverIP;
        private int serverPort;
        private int localPort;
        private string sharedFolderPath;
        private string downloadFolderPath;
        private UploadManager uploadManager = new UploadManager();
        private Thread uploadManagerThread;
        private bool configExists;
        private User user;
        private XmlSerializer SerializerObj = new XmlSerializer(typeof(User));
        private WCFClient wcfClient;

        public MainWindow() {
            InitializeComponent();
            listViewSearch.DataContext = tabControl;
            listViewSearch.SizeChanged += ListView_SizeChanged;
            listViewLibrary.SizeChanged += ListView_SizeChanged;
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
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
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

        private void buttonDownload_Click(object sender, RoutedEventArgs e) {
            List <User> usersList = new List<User>();
            User tempUser = new User {
                UserIP = "192.168.43.124",
                Name = "Vit",
                Password = "Os",
                ServerIP = "10.0.0.1",
                ServerPort = 8888,
                LocalPort = 4080,
                DownloadFolderPath = @"C:\temp\download1",
                SharedFolderPath = @"C:\temp\share1"
            };
            User tempUser2 = new User {
                UserIP = "192.168.43.124",
                Name = "Vit",
                Password = "Os",
                ServerIP = "10.0.0.1",
                ServerPort = 8888,
                LocalPort = 4081,
                DownloadFolderPath = @"C:\temp\download2",
                SharedFolderPath = @"C:\temp\share2"
            };
            User tempUser3 = new User {
                UserIP = "192.168.43.124",
                Name = "Vit",
                Password = "Os",
                ServerIP = "10.0.0.1",
                ServerPort = 8888,
                LocalPort = 4082,
                DownloadFolderPath = @"C:\temp\download2",
                SharedFolderPath = @"C:\temp\share2"
            };
            usersList.Add(tempUser);
            usersList.Add(tempUser2);
            usersList.Add(tempUser3);
            new Thread(() => new DownloadManager(new DataFile("DSC_8319.jpg", 7806444, usersList), user.DownloadFolderPath)).Start();
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

        //  Searches the main server for files shared by all connected peers.
        private void buttonSearch_Click(object sender, RoutedEventArgs e) {
            List<ServiceDataFile> searchResults = wcfClient.FileRequest(textboxSearch.Text);
            List<SearchFileProperty> items = new List<SearchFileProperty>();
            foreach (ServiceDataFile sdf in searchResults) {
                items.Add(new SearchFileProperty(sdf.Name, sdf.Size, sdf.PeerList.Count));
            }
            listViewSearch.ItemsSource = items;
        }
    }
}
