using System;
using System.Security.AccessControl;
using GitHub.CodeQL.Analytics.Data.Models;

using Microsoft.EntityFrameworkCore;

namespace GitHub.CodeQL.Analytics.Data.Services; 

public class AnalyticsContext : DbContext {

    //public DbSet<Person> PersonSet { get; init; }
    //public DbSet<Animal> AnimalSet { get; init; }

    public DbSet<Rules> RuleSet { get; init; }
    public DbSet<Results> ResultSet { get; init; }
    public DbSet<Tags> TagSet { get; init; }

    public DbSet<Analyses> AnalysesSet { get; init; }

    public DbSet<EvalutationTime> EvaluationSet { get; init; }

    public AnalyticsContext(DbContextOptions options) : base(options) {
    }
    
    protected override void OnModelCreating(ModelBuilder model) {
        model.Entity<Results>().Property(r => r.ResultId).HasConversion<string>();
        model.Entity<Results>().Property(r => r.AnalysisId).HasConversion<string>();
        model.Entity<Results>().Property(r => r.RuleId).HasConversion<string>();
        model.Entity<Rules>().Property(r => r.RuleId).HasConversion<string>();
        model.Entity<Rules>().Property(r => r.AnalysisId).HasConversion<string>();
        model.Entity<Analyses>().Property(a => a.AnalysisId).HasConversion<string>();
        model.Entity<Tags>().Property(t => t.RuleId).HasConversion<string>();
        model.Entity<Tags>().Property(t => t.TagId).HasConversion<string>();
        model.Entity<EvalutationTime>().Property(e => e.AnalysisId).HasConversion<string>();
        model.Entity<EvalutationTime>().Property(e => e.EvaluationTimeId).HasConversion<string>();

        model.Entity<Results>().HasOne(r => r.Rule).WithMany(a => a.Results).HasForeignKey(r => r.RuleId);
        model.Entity<Rules>().HasMany(r => r.RuleTags).WithOne(t => t.Rule).HasForeignKey(t => t.RuleId);
        model.Entity<Rules>().HasOne(r => r.Analysis).WithMany(a => a.Rules).HasForeignKey(r => r.AnalysisId);
        model.Entity<Analyses>().HasMany(a => a.Results).WithOne(r => r.Analysis).HasForeignKey(r => r.AnalysisId);

        base.OnModelCreating(model);
    }
}