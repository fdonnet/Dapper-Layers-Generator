﻿using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;

namespace Dapper_Layers_Generator.Core.Generators.MySql
{
    public interface IMySqlGeneratorContext : IGenerator
    {
    }
    public class MySqlGeneratorContext : GeneratorContextForProvider, IMySqlGeneratorContext
    {
        protected override string UsingDbProviderSpecific { get; init; }
        protected override string DbProviderString { get; init; }
        protected override string ConnectionStringInject { get; init; }
        protected override string DapperDefaultMapStrat { get; init; }
        protected override string DapperCommandTimeOut { get; init; }
        protected override string ConnectionClassName { get; init; }

        public MySqlGeneratorContext(SettingsGlobal settingsGlobal
            , IReaderDBDefinitionService data
            , StringTransformationService stringTransformationService) : base(settingsGlobal, data, stringTransformationService)
        {
            UsingDbProviderSpecific = "using MySqlConnector;";
            //Allow bulk
            ConnectionStringInject = $@"Connection = new MySqlConnection(_config.GetConnectionString(""{_settings.ConnectionStringName}"") + "";AllowLoadLocalInfile=True"");";
            DapperDefaultMapStrat = "DefaultTypeMap.MatchNamesWithUnderscores = true;";
            DapperCommandTimeOut = "SqlMapper.Settings.CommandTimeout = 60000;";
            DbProviderString = "MySql";
            ConnectionClassName = "MySqlConnection";
    }
    }
}
