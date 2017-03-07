using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PeerUI {
    public class IPValidationRule : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            IPAddress ipAddress;
            try {
                ipAddress = IPAddress.Parse(value.ToString());
            }
            catch (FormatException) {
                return new ValidationResult(false, "Value is not a valid IP address.");
            }
            return ValidationResult.ValidResult;
        }
    }
}
