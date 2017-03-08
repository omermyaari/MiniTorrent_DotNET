using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassDLL {

    /// <summary>
    /// Class used for DLL analyzing.
    /// </summary>
    public class Class1 {

        public string str;
        private int integer;

        public int Integer {
            get {
                return integer;
            }

            set {
                integer = value;
            }
        }

        public Class1(string str, int integer) {
            this.str = str;
            this.integer = integer;
        }

        public void DoubleInt() {
            integer *= 2;
        }

        public override string ToString() {
            return "String: " + str + ", Integer: " + Integer;
        }
    }
}
