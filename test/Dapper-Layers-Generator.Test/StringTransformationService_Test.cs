using Dapper_Layers_Generator.Core.Converters;
using Dapper_Layers_Generator.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Test
{
    public class StringTransformationService_Test
    {
        private StringTransformationService _transform = null!;
        private SettingsGlobal _settings = null!;

        public StringTransformationService_Test()
        {
            _settings = new SettingsGlobal();
            _transform = new StringTransformationService(_settings);
        }

        [Fact]
        public void IndentString_DoubleSpace_Test()
        {
            _settings.IndentStringInGeneratedCode = "double space";

            var result = _transform.IndentString;

            Assert.Equal("  ", result);
        }

        [Fact]
        public void IndentString_Tab_Test()
        {
            _settings.IndentStringInGeneratedCode = "tab";

            var result = _transform.IndentString;

            Assert.Equal("\t", result);
        }

        [Fact]
        public void IndentString_Space_Test()
        {
            _settings.IndentStringInGeneratedCode = "space";

            var result = _transform.IndentString;

            Assert.Equal(" ", result);
        }

        [Fact]
        public void IndentString_QuadrupleSpace_Test()
        {
            _settings.IndentStringInGeneratedCode = "quadruple space";

            var result = _transform.IndentString;

            Assert.Equal("    ", result);
        }

        [Fact]
        public void PascalCase_Enabled_Test()
        {
            _settings.UsePascalTransform = true;

            var result = _transform.PascalCase("test_pascal");

            Assert.Equal("TestPascal", result);
        }

        [Fact]
        public void PascalCase_Disabled_Test()
        {
            _settings.UsePascalTransform = false;

            var result = _transform.PascalCase("test_pascal");

            Assert.Equal("test_pascal", result);
        }

        [Fact]
        public void PascalCase_NullValue_Test()
        {
            _settings.UsePascalTransform = true;

            var result = _transform.PascalCase(null);

            Assert.Null(result);
        }

        [Fact]
        public void Singularize_Enabled_Test()
        {
            _settings.UseSingularizeTransform = true;

            var result = _transform.Singularize("tests");

            Assert.Equal("test",result);
        }

        [Fact]
        public void Singularize_Disabled_Test()
        {
            _settings.UseSingularizeTransform = false;

            var result = _transform.Singularize("tests");

            Assert.Equal("tests", result);
        }

        [Fact]
        public void Singularize_NullValue_Test()
        {
            _settings.UseSingularizeTransform = true;

            var result = _transform.Singularize(null);

            Assert.Null(result);
        }

        [Fact]
        public void PluralizeToLower_Test()
        {
            var result = _transform.PluralizeToLower("Customer");

            Assert.Equal("customers",result);
        }

        [Fact]
        public void ApplyConfigTransformClass_PascalEnabled_SingularizeEnabled_Test()
        {
            _settings.UsePascalTransform = true;
            _settings.UseSingularizeTransform = true;
            var result = _transform.ApplyConfigTransformClass("customers_actions");

            Assert.Equal("CustomerAction", result);
        }

        [Fact]
        public void ApplyConfigTransformClass_PascalDisabled_SingularizeEnabled_Test()
        {
            _settings.UsePascalTransform = false;
            _settings.UseSingularizeTransform = true;
            var result = _transform.ApplyConfigTransformClass("customers_actions");

            Assert.Equal("customer_action", result);
        }

        [Fact]
        public void ApplyConfigTransformClass_PascalEnabled_SingularizeDisabled_Test()
        {
            _settings.UsePascalTransform = true;
            _settings.UseSingularizeTransform = false;
            var result = _transform.ApplyConfigTransformClass("customers_actions");

            Assert.Equal("CustomersActions", result);
        }

        [Fact]
        public void ApplyConfigTransformClass_PascalDisabled_SingularizeDisabled_Test()
        {
            _settings.UsePascalTransform = false;
            _settings.UseSingularizeTransform = false;
            var result = _transform.ApplyConfigTransformClass("customers_actions");

            Assert.Equal("customers_actions", result);
        }

        [Fact]
        public void ApplyConfigTransformClass_NullValue_Test()
        {
            _settings.UsePascalTransform = true;
            _settings.UseSingularizeTransform = true;
            var result = _transform.ApplyConfigTransformClass(null);

            Assert.Null(result);
        }

        [Fact]
        public void ApplyConfigTransformMember_PascalEnabled_Test()
        {
            _settings.UsePascalTransform = true;
            var result = _transform.ApplyConfigTransformMember("customer_action");

            Assert.Equal("customerAction", result);
        }

        [Fact]
        public void ApplyConfigTransformMember_PascalDisabled_Test()
        {
            _settings.UsePascalTransform = false;
            var result = _transform.ApplyConfigTransformMember("customer_action");

            Assert.Equal("customer_action", result);
        }

        [Fact]
        public void ApplyConfigTransformMember_NullValue_Test()
        {
            _settings.UsePascalTransform = true;
            var result = _transform.ApplyConfigTransformMember(null);

            Assert.Null(result);
        }

        [Fact]
        public void FirstCharToLower_Test()
        {
            var result = _transform.FirstCharToLower("Test");

            Assert.Equal("test", result);
        }

        [Fact]
        public void FirstCharToLower_NullValue_Test()
        {
            var result = _transform.FirstCharToLower(null);

            Assert.Null(result);
        }

    }
}
