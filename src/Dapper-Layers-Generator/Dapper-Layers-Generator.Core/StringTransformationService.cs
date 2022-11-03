using Dapper_Layers_Generator.Core.Generators;
using Dapper_Layers_Generator.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Core
{
    public class StringTransformationService
    {
        private readonly bool _isPascalCaseEnable = false;
        private readonly bool _isSingularizeEnable = false;

        public StringTransformationService(SettingsGlobal settingsGlobal)
        {
            _isPascalCaseEnable = settingsGlobal.UsePascalTransform;
            _isSingularizeEnable = settingsGlobal.UseSingularizeTransform;
        }

        public string? PascalCase(string? theString)
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

        public string? Singularize(string? theString)
        {
            return theString == null 
                ? theString 
                : _isSingularizeEnable 
                    ? Humanizer.InflectorExtensions.Singularize(theString) 
                    : theString;
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
