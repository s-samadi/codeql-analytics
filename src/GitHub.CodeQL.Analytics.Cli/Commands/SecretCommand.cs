using System.Threading.Tasks;
using Typin;
using Typin.Attributes;
using Typin.Console;
using Typin.Exceptions;

namespace GitHub.CodeQL.Analytics.Cli.Commands;

[Command("secret", Description = "Provide cryptographic capabilities to encrypt/decrypt data")]
public class SecretCommand : ICommand {
    public ValueTask ExecuteAsync(IConsole console) {
        throw new CommandException("secret <command> not specified", showHelp: true);
    }
}