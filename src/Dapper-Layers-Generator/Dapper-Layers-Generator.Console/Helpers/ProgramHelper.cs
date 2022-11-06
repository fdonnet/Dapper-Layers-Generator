using Spectre.Console;

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
