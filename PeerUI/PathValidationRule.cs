using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PeerUI {

    /// <summary>
    /// Directory path validation rule.
    /// </summary>
    class PathValidationRule : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            if(Directory.Exists(value.ToString())) {
                return ValidationResult.ValidResult;
            }
            return new ValidationResult(false, "Value is not a valid directory path.");
        }
    }
}
