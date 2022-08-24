using System;
using GitHub.CodeQL.Analytics.Data.Models;
using GitHub.CodeQL.Analytics.Data.Models.Animals;
using Microsoft.EntityFrameworkCore;

namespace GitHub.CodeQL.Analytics.Data.Services; 

public class AnalyticsContext : DbContext {

    public DbSet<Person> PersonSet { get; init; }
    public DbSet<Animal> AnimalSet { get; init; }

    public AnalyticsContext(DbContextOptions options) : base(options) {
    }

    protected override void OnModelCreating(ModelBuilder model) {
        model.Entity<Animal>()
            .HasDiscriminator<string>("animal_type")
            .HasValue<Cat>("cat")
            .HasValue<Dog>("dog");

        base.OnModelCreating(model);
    }
}