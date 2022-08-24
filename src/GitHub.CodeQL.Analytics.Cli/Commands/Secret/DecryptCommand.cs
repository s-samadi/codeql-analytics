using System.Threading.Tasks;
using GitHub.CodeQL.Analytics.Cli.Services;
using Typin;
using Typin.Attributes;
using Typin.Console;

namespace GitHub.CodeQL.Analytics.Cli.Commands.Secret;

[Command("secret decrypt", Description = "Decrypt base64 encoded string containing encrypted data")]
public class DecryptCommand : ICommand {
    private readonly SecretService _secretService;

    public DecryptCommand(SecretService secretService) {
        _secretService = secretService;
    }

    [CommandParameter(1, Description = "data to decrypt", Name = "data")]
    public string CipherText { get; set; }

    public ValueTask ExecuteAsync(IConsole console) {
        var plainText = _secretService.Decrypt(CipherText);
        console.Output.WriteLine(plainText);
        return ValueTask.CompletedTask;
    }
}
