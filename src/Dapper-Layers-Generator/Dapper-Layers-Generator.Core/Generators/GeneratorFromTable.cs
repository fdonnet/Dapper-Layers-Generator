using Dapper_Layers_Generator.Core.Settings;
using Dapper_Layers_Generator.Data.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dapper_Layers_Generator.Core.Generators
{
    public abstract class GeneratorFromTable : Generator
    {
        public ITable? Table { get; private set; }

        public GeneratorFromTable(SettingsGlobal settingsGlobal, IReaderDBDefinitionService data) : base(settingsGlobal, data)
        {

        }
        public void SetTable(string tableName)
        {
            Table = _currentSchema.Tables?.Where(t => t.Name == tableName).SingleOrDefault();

            if (Table == null)
                throw new NullReferenceException("Table not found in the DB repository");
        }

        public override abstract string Generate();

    }
}
