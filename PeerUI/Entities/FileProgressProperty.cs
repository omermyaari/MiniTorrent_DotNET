using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerUI.Entities {
    public class FileProgressProperty : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        private string progress;
        private string speed;
        private long elapsedTime;

        public string Type {
            get; set;
        }

        public string Name {
            get; set;
        }

        public long Size {
            get; set;
        }

        public string Progress {
            get {
                return progress;
            }
            set {
                progress = value;
                this.OnPropertyChanged("Progress");
            }
        }

        public string Speed {
            get {
                return speed;
            }
            set {
                speed = value;
                this.OnPropertyChanged("Speed");
            }
        }

        public long ElapsedTime {
            get {
                return elapsedTime;
            }
            set {
                elapsedTime = value / 1000;
                this.OnPropertyChanged("ElapsedTime");
            }
        }

        public FileProgressProperty(string Type, string Name, long Size, string progress, string speed) {
            this.Type = Type;
            this.Name = Name;
            this.Size = Size;
            this.progress = progress;
            this.speed = speed;
        }


        public override int GetHashCode() {
            if (Name == null)
                return 0;
            return Name.GetHashCode();
        }

        public override bool Equals(object obj) {
            FileProgressProperty other = obj as FileProgressProperty;
            return other != null && other.Name == this.Name && other.Size == this.Size;
        }

        protected void OnPropertyChanged(string propertyName) {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null) {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public enum TransferType {
        Download, Upload
    }
}
