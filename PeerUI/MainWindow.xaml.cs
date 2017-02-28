using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Threading;

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
        XmlSerializer SerializerObj = new XmlSerializer(typeof(User));

        public MainWindow() {
            InitializeComponent();
            listViewFiles.DataContext = tabControl;
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
                Username = username,
                Password = password,
                ServerIP = serverIP,
                ServerPort = serverPort,
                LocalPort = localPort,
                SharedFolderPath = sharedFolderPath,
                DownloadFolderPath = downloadFolderPath
            };
            saveConfigToXml(user);
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
        private void getDetailsFromFields()
        {
            username = textboxUsername.Text;
            password = passwordboxPassword.Password;
            sharedFolderPath = textboxSharedFolder.Text;
            downloadFolderPath = textboxDownloadFolder.Text;
            serverIP = textboxServerIP.Text;
            Int32.TryParse(textboxServerPort.Text, out serverPort);
            Int32.TryParse(textboxLocalPort.Text, out localPort);
        }

        private void loadDetailsFromUser()
        {
            textboxUsername.Text = user.Username;
            passwordboxPassword.Password = user.Password;
            textboxSharedFolder.Text = user.SharedFolderPath;
            textboxDownloadFolder.Text = user.DownloadFolderPath;
            textboxServerIP.Text = user.ServerIP;
            textboxServerPort.Text = Convert.ToString(user.ServerPort);
            textboxLocalPort.Text = Convert.ToString(user.LocalPort);
        }

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

        private void saveConfigToXml(User user) {
            TextWriter WriteFileStream = new StreamWriter("MyConfig.xml");
            SerializerObj.Serialize(WriteFileStream, user);
            WriteFileStream.Close();
        }

        private void loadConfigFromXml()////////////////////////////
        {
            var reader = new StreamReader("MyConfig.xml");
            user = (User)SerializerObj.Deserialize(reader);
            reader.Close();
            loadDetailsFromUser();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            uploadManager.StopListening();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (configExists = File.Exists("MyConfig.xml"))
            {
                loadConfigFromXml();
                //  TODO config xml to settings
                uploadManagerThread = new Thread(()=> uploadManager.StartListening(user.LocalPort, user.SharedFolderPath));
                uploadManagerThread.Start();
            }
            else
            {
                MessageBox.Show("Config file does not exist, please fill the settings.", "Error");
                settingsTab.IsSelected = true;
                //  TODO change tab to settings and bring up a message to user
            }
        }

        private void buttonDownload_Click(object sender, RoutedEventArgs e) {
            List <User> usersList = new List<User>();
            User tempUser = new User {
                UserIP = "192.168.43.124",
                Username = "Vit",
                Password = "Os",
                ServerIP = "10.0.0.1",
                ServerPort = 8888,
                LocalPort = 4080,
                DownloadFolderPath = @"C:\temp\download1",
                SharedFolderPath = @"C:\temp\share1"
            };
            User tempUser2 = new User {
                UserIP = "192.168.43.124",
                Username = "Vit",
                Password = "Os",
                ServerIP = "10.0.0.1",
                ServerPort = 8888,
                LocalPort = 4081,
                DownloadFolderPath = @"C:\temp\download2",
                SharedFolderPath = @"C:\temp\share2"
            };
            usersList.Add(tempUser);
            usersList.Add(tempUser2);
            new Thread(() => new DownloadManager(new DataFile("DSC_8319.jpg", 7806444, usersList), user.DownloadFolderPath)).Start();
        }
    }
}
