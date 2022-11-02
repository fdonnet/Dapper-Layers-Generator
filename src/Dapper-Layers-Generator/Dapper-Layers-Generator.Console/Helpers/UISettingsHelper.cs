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
        internal static Dictionary<int, SettingsKeyVal> SettingsDic(object settings)
        {
            var dic = new Dictionary<int, SettingsKeyVal>();

            foreach (var curProp in settings.GetType().GetProperties())
            {
                if (curProp.PropertyType == typeof(string) || curProp.PropertyType == typeof(bool))
                {
                    //Get property attributes
                    var attributes = curProp.GetCustomAttribute(typeof(SettingsAttribute), false);
                    if (attributes != null)
                    {
                        SettingsAttribute attr = (SettingsAttribute)attributes;

                        dic.Add(attr.Position, new SettingsKeyVal()
                        {
                            Label = $"{attr.Position}) {attr.Message}",
                            Settings = curProp.GetValue(settings, null)?.ToString() ?? "",
                            PropertyName = curProp.Name,
                            ChildOf = attr.ChildOf,
                            Position = attr.Position,
                            Type = curProp.PropertyType,
                            ColumnModeOnly = attr.OnlyInColumnMode,
                            Group = attr.Group
                        });
                    }
                }
            }
            dic = dic.OrderBy(d => d.Value.Position).ToDictionary(d=>d.Key,d=>d.Value);
            return dic;
        }

       internal static object SetSettingsStringValue(object settings, string propertyName, string newValue)
        {
            PropertyInfo propertyInfo = settings.GetType().GetProperty(propertyName)!;
            propertyInfo.SetValue(settings, Convert.ChangeType(newValue, propertyInfo.PropertyType), null);

            return settings;
        }
    }
}
