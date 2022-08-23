using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub.CodeQL.Analytics.Data.Models.Animals;
using GitHub.CodeQL.Analytics.Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Typin;
using Typin.Attributes;
using Typin.Console;

namespace GitHub.CodeQL.Analytics.Cli.Commands.Reports; 

[Command("report simple")]
public class SimpleCommand : ICommand {
    
    private readonly ILogger<SimpleCommand> _log;
    private readonly AnalyticsContext _ctx;

    public SimpleCommand(ILogger<SimpleCommand> log, AnalyticsContext ctx) {
        _log = log;
        _ctx = ctx;
    }
    
    public async ValueTask ExecuteAsync(IConsole console) {
        await DisplayCatPeople();
        await LetThePetsTalk();
    }

    private async Task DisplayCatPeople() {
        var people = await _ctx.PersonSet
            .Where(x => x.PetList.Any(y => y is Cat))
            .ToListAsync();

        var msg = new StringBuilder();
        msg.AppendLine("Cat people:");
        foreach (var person in people) {
            msg.AppendLine($"\t- {person.Name}");
        }
        _log.LogInformation(msg.ToString());
    }

    private async Task LetThePetsTalk() {
        var people = await _ctx.PersonSet
            .ToListAsync();

        foreach (var person in people) {
            var msg = new StringBuilder();
            msg.AppendLine($"{person.Name}'s pets say:");
            foreach (var pet in person.PetList) {
                msg.AppendLine($"\t- {pet.Name} says '{pet.Speak()}!'");
            }
            _log.LogInformation(msg.ToString());
        }
    }
}