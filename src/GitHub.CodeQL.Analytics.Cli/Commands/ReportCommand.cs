using System.Threading.Tasks;
using Typin;
using Typin.Attributes;
using Typin.Console;
using Typin.Exceptions;

namespace GitHub.CodeQL.Analytics.Cli.Commands; 

[Command("report", Description = "Utilities to generate reports")]
public class ReportCommand : ICommand {
    public ValueTask ExecuteAsync(IConsole console) {
        throw new CommandException("report <command> not specified", showHelp: true);
    }
}