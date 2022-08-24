using System.Threading.Tasks;
using Typin;
using Typin.Attributes;
using Typin.Console;
using Typin.Exceptions;

namespace GitHub.CodeQL.Analytics.Cli.Commands; 

[Command("db", Description = "Utilities to manage the database")]
public class DatabaseCommand : ICommand {
    public ValueTask ExecuteAsync(IConsole console) {
        throw new CommandException("db <command> not specified", showHelp: true);
    }
}