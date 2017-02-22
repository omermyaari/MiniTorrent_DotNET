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
            getDetails();
            var user = new User {
                Username = username,
                Password = password,
                ServerIP = serverIP,
                ServerPort = serverPort,
                LocalPort = localPort,
                SharedFolderPath = sharedFolderPath,
                DownloadFolderPath = downloadFolderPath
            };
            saveConfigToXml(user);
        }

        //  Clears the config settings.
        private void buttonClear_Click(object sender, RoutedEventArgs e) {
            clearSettings();
        }

        //  TODO validator?
        private void getDetails() {
            username = textboxUsername.Text;
            password = textboxPassword.Text;
            sharedFolderPath = textboxSharedFolder.Text;
            downloadFolderPath = textboxDownloadFolder.Text;
            serverIP = textboxServerIP.Text;
            //IPAddress.TryParse(textboxServerIP.Text, out serverIP);
            Int32.TryParse(textboxServerPort.Text, out serverPort);
            Int32.TryParse(textboxLocalPort.Text, out localPort);
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
            XmlSerializer SerializerObj = new XmlSerializer(typeof(User));
            TextWriter WriteFileStream = new StreamWriter("MyConfig.xml");
            SerializerObj.Serialize(WriteFileStream, user);
            WriteFileStream.Close();
            /*using (XmlWriter writer = XmlWriter.Create("MyConfig.xml")) {
                writer.WriteStartDocument();
                writer.WriteStartElement("Settings");

                for
            }*/
        }
    }
}
