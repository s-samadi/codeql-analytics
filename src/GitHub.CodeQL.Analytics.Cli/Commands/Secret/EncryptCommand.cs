using System.Threading.Tasks;
using GitHub.CodeQL.Analytics.Cli.Services;
using Typin;
using Typin.Attributes;
using Typin.Console;

namespace GitHub.CodeQL.Analytics.Cli.Commands.Secret; 

[Command("secret encrypt", Description = "Encrypt plain text data")]
public class EncryptCommand : ICommand {
    
    private readonly SecretService _secretService;

    public EncryptCommand(SecretService secretService) {
        _secretService = secretService;
    }
    
    [CommandParameter(1, Description = "Text to encrypt", Name = "text")]
    public string PlainText { get; set; }

    public ValueTask ExecuteAsync(IConsole console) {
        var cipherText = _secretService.Encrypt(PlainText);
        console.Output.WriteLine(cipherText);
        return ValueTask.CompletedTask;       
    }
}