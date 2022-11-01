using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_Layers_Generator.Console.Helpers
{
    internal static class ProgramHelper
    {
        internal static void MainTitle()
        {
            AnsiConsole.Write(
            new FigletText("Dapper Layers Generator")
            .Centered());
        }
    }
}
