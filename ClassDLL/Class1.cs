using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassDLL {
    public class Class1 {

        public string str;
        private int integer;
        
        public float Float {
            get; set;
        }

        public Class1() {

        }

        public Class1(string str, int integer, float Float) {
            this.str = str;
            this.integer = integer;
            this.Float = Float;
        }

        public float AddIntFloat() {
            return integer + Float;
        }

        private string PrintIntFloatToStrPrivate() {
            return Convert.ToString(integer + Float);
        }
    }
}
