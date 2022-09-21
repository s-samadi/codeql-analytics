using System.Threading.Tasks;
using Typin;
using Typin.Attributes;
using Typin.Console;
using Typin.Exceptions;

namespace GitHub.CodeQL.Analytics.Cli.Commands.Database;


/// <summary>
/// 
/// </summary>
/// <returns></returns>
/// <exception cref="DataException"></exception

[Command("db load", Description = "Utilities to populate test data")]
public class LoadCommand : ICommand {
    public ValueTask ExecuteAsync(IConsole console) {
        throw new CommandException("db load <command> not specified", showHelp: true);
    }
}

