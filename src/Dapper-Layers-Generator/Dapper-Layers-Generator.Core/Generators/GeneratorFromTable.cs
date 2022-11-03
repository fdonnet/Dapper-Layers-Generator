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
    public interface IGeneratorFromTable
    {
        void SetTable(string tableName);
    }

    public abstract class GeneratorFromTable : Generator, IGeneratorFromTable
    {
        //used by factory only can force null not set in constructor
        public ITable Table { get; private set; } = null!;

        public GeneratorFromTable(SettingsGlobal settingsGlobal, IReaderDBDefinitionService data) : base(settingsGlobal, data)
        {

        }
        public void SetTable(string tableName)
        {
            var table = _currentSchema.Tables?.Where(t => t.Name == tableName).SingleOrDefault();

            if (table == null)
                throw new NullReferenceException("Table not found in the DB repository");

            Table = table;
        }

        public override abstract string Generate();

    }
}
