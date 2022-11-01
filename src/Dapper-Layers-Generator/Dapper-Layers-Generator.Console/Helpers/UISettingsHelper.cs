using Dapper_Layers_Generator.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Console.Helpers
{
    internal static class UISettingsHelper
    {
        /// <summary>
        /// Dic for all settings (need to have an attribute SettingsAttribute defined)
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        internal static Dictionary<int, SettingsKeyVal> SettingsDic(SettingsGlobal settings)
        {
            var dic = new Dictionary<int, SettingsKeyVal>();

            foreach (var stringProp in settings.GetType().GetProperties())
            {
                if (stringProp.PropertyType == typeof(string))
                {
                    //Get property attributes
                    var attributes = stringProp.GetCustomAttribute(typeof(SettingsAttribute), false);
                    if (attributes != null)
                    {
                        SettingsAttribute attr = (SettingsAttribute)attributes;

                        dic.Add(attr.Position, new SettingsKeyVal()
                        {
                            Label = $"{attr.Position}) {attr.Message}",
                            Settings = stringProp.GetValue(settings, null)?.ToString() ?? "",
                            PropertyName = stringProp.Name,
                            ChildOf = attr.ChildOf,
                            Position = attr.Position
                        });
                    }
                }
            }
            dic = dic.OrderBy(d => d.Value.Position).ToDictionary(d=>d.Key,d=>d.Value);
            return dic;
        }

       internal static SettingsGlobal SetGlobalSettingsStringValue(SettingsGlobal settings, string propertyName, string newValue)
        {
            PropertyInfo propertyInfo = settings.GetType().GetProperty(propertyName)!;
            propertyInfo.SetValue(settings, Convert.ChangeType(newValue, propertyInfo.PropertyType), null);

            return settings;
        }
    }
}
