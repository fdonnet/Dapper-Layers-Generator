using Dapper.FluentMap.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Data.POCO
{
    public interface IColumn
    {
        public string Table { get; set; }
        public string Name { get; set; }
    }
    public interface IColumnMap : IEntityMap<IColumn>
    {

    }

}
