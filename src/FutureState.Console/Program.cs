using ManyConsole;

namespace FutureState.Console
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var consoleCommands = ConsoleCommandDispatcher.FindCommandsInAssembly(typeof(Program).Assembly);

            var result = ConsoleCommandDispatcher.DispatchCommand(consoleCommands, args, System.Console.Out);

            System.Console.WriteLine("Press any key to exit.");
            System.Console.ReadLine();

            return result;
        }
    }
}
