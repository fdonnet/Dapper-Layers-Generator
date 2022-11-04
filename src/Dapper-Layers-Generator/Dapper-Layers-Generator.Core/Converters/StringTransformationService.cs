using Dapper_Layers_Generator.Core.Generators;
using Dapper_Layers_Generator.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core.Converters
{
    public class StringTransformationService
    {
        private readonly SettingsGlobal _settingsGlobal;

        public string IndentString
        {
            get
            {
                switch (_settingsGlobal.IndentStringInGeneratedCode)
                {
                    case "double space":
                        return "  ";
                    case "space":
                        return " ";
                    case "quadruple space":
                        return "    ";
                    case "tab":
                        return "\t";
                    default:
                        return "  ";
                }
            }
        }

        private bool _isPascalCaseEnable
        {
            get
            {
                return _settingsGlobal.UsePascalTransform;
            }
        }

        private string _indentStringInGeneratedCode
        {
            get
            {
                return _settingsGlobal.IndentStringInGeneratedCode;
            }
        }

        private bool _isSingularizeEnable
        {
            get
            {
                return _settingsGlobal.UseSingularizeTransform;
            }
        }

        public StringTransformationService(SettingsGlobal settingsGlobal)
        {
            _settingsGlobal = settingsGlobal;
        }
 
        /// <summary>
        /// Old method for ref
        /// </summary>
        /// <param name="theString"></param>
        /// <returns></returns>
        public string? PascalCaseOld(string? theString)
        {
            // If there are 0 or 1 characters, just return the string.
            if (theString == null) return theString;

            if (_isPascalCaseEnable)
            {
                if (theString.Length < 2) return theString.ToUpper();

                // Split the string into words.
                string[] words = theString.Split(
                    new char[] { '_' },
                    StringSplitOptions.RemoveEmptyEntries);

                // Combine the words.
                string result = "";
                foreach (string word in words)
                {
                    result +=
                        word[..1].ToUpper() +
                        word[1..];
                }

                return result;
            }
            else
                return theString;
        }

        public string? PascalCase(string? theString)
        {
            return theString == null
                ? theString
                : _isPascalCaseEnable
                    ? Humanizer.InflectorExtensions.Pascalize(theString)
                    : theString;
        }

        public string? Singularize(string? theString)
        {
            if (theString == null)
                return null;

            if(_isSingularizeEnable)
            {
                var theStringSplit = theString.Split('_');
                var resultString = string.Empty;

                foreach(var str in theStringSplit)
                {
                    resultString += Humanizer.InflectorExtensions.Singularize(str) + '_';
                }
                return resultString[..^1];
                
            }
            return theString;
        }

        public string? SingularizeAndPascalCase(string? theString)
        {
            return theString == null
                ? null
                : theString == null
                    ? theString
                    : PascalCase(Singularize(theString));
        }

        public string? ApplyConfigTransform(string? theString)
        {
            return SingularizeAndPascalCase(theString);
        }

    }
}
