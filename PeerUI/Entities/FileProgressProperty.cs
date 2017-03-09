using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PeerUI.Entities {

    /// <summary>
    /// File progress property.
    /// used by the UI in the library tab, holds progress information for each transfer.
    /// </summary>
    public class FileProgressProperty : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        private double progress;
        private string speed;
        private long elapsedTime;

        public int TransferId {
            get; set;
        }

        public string Type {
            get; set;
        }

        public string Name {
            get; set;
        }

        public long Size {
            get; set;
        }

        public double Progress {
            get {
                return progress;
            }
            set {
                progress = value;
                OnPropertyChanged();
            }
        }

        public string Speed {
            get {
                return speed;
            }
            set {
                speed = value;
                OnPropertyChanged();
            }
        }

        public long ElapsedTime {
            get {
                return elapsedTime;
            }
            set {
                elapsedTime = value / 1000;
                OnPropertyChanged();
            }
        }

        public FileProgressProperty(int TransferId, string Type, string Name, long Size, double progress, string speed) {
            this.TransferId = TransferId;
            this.Type = Type;
            this.Name = Name;
            this.Size = Size;
            this.progress = progress;
            this.speed = speed;
        }

        public void OnPropertyChanged([CallerMemberName] String propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public enum TransferType {
        Download, Upload
    }
}
