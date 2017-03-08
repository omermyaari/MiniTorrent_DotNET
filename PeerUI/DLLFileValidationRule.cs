using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PeerUI {
    class DLLFileValidationRule : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            if (File.Exists(value.ToString())) {
                string str = value as string;
                var dll = (str.Split(new char[] { '.' })).Last()[0];
                if (dll.Equals("dll"))
                    return ValidationResult.ValidResult;
                return new ValidationResult(false, "Value is not a valid dll file path.");

            } 
            return new ValidationResult(false, "Value is not a valid dll file path.");
        }
    }
}
