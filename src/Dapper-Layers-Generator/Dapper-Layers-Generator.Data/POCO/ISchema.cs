using Dapper.FluentMap.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Data.POCO
{
    public interface ISchema
    {
        public string Name { get; set; }
        public IEnumerable<ITable>? Tables { get; set; }
    }

    public interface ISchemaMap : IEntityMap<ISchema>
    {

    }
}
