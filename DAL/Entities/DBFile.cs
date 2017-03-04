using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities {

    public class DBFile {
        public long ID { get; private set; }
        public string Name { get; set; }
        public long Size { get; set; }

        public DBFile(string Name, long Size) {
            this.Name = Name;
            this.Size = Size;
        }

        public override int GetHashCode() {
            if (Name == null)
                return 0;
            return Name.GetHashCode();
        }

        public override bool Equals(object obj) {
            DBFile other = obj as DBFile;
            return other != null && other.Name == this.Name && other.Size == this.Size;
        }
    }
}
