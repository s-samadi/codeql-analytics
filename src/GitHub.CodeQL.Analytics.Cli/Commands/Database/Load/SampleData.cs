using System.Data;
using System.Threading.Tasks;
using GitHub.CodeQL.Analytics.Cli.Services;
using GitHub.CodeQL.Analytics.Data.Models;
using GitHub.CodeQL.Analytics.Data.Models.Animals;
using GitHub.CodeQL.Analytics.Data.Services;
using Microsoft.Extensions.Logging;
using Typin;
using Typin.Attributes;
using Typin.Console;

namespace GitHub.CodeQL.Analytics.Cli.Commands.Database.Load; 

[Command("db load sample-data")]
public class SampleData : ICommand {

    private readonly ILogger<SampleData> _log;
    private readonly AnalyticsContext _db;
    private readonly SecretService _secretService;

    public SampleData(
        ILogger<SampleData> log,
        AnalyticsContext db, 
        SecretService secretService) {
        _log = log;
        _db = db;
        _secretService = secretService;
    }
    
    public async ValueTask ExecuteAsync(IConsole console) {
        await CreatePeople();
        await _db.SaveChangesAsync();
    }

    private async Task CreatePeople() {
        var personDetailList = new[] {
            new {
                Name = "John Doe",
                Pets = new[] {
                    new {
                        Name = "Rover",
                        AnimalType = "Dog"
                    },
                    new {
                        Name = "Felix",
                        AnimalType = "Cat"
                    }
                }
            },
            new {
                Name = "Alice Smith",
                Pets = new [] {
                    new {
                        Name = "Rex",
                        AnimalType = "Dog"
                    }
                }
            }
        };
        
        foreach (var personDetail in personDetailList) {
            var person = new Person() {
                Name = personDetail.Name
            };

            foreach (var petDetail in personDetail.Pets) {
                Animal animal = petDetail.AnimalType.ToLower() switch {
                    "dog" => new Dog {
                        Name = petDetail.Name,
                        Person = person
                    },
                    "cat" => new Cat {
                        Name = petDetail.Name,
                        Person = person
                    },
                    _ => throw new DataException($"Unknown animal type: {petDetail.AnimalType}")
                };
                _db.AnimalSet.Add(animal);
            }
            
            _db.PersonSet.Add(person);
            await _db.SaveChangesAsync();
            _log.LogInformation("Added person: {name}", person.Name);
        }
    }
}