create schema QL;
GO


create table QL.Analyses
(
    AnalysisId  nvarchar(36) primary key,
    TotalAnalysisTime   nvarchar(50),
    PackVersion nvarchar(10),
    AnalysisDate DateTime, 
    CodeQLVersion nvarchar(10),
    AnalysisType nvarchar(50),
    LanguageAnalysed nvarchar(50),
    CodeQLDatabaseName nvarchar(MAX),
    QueryPack nvarchar(MAX)
);

create table QL.Results 
(
   ResultId nvarchar(36) primary key, 
   RuleId nvarchar(36),
   Id nvarchar(MAX),
   AlertMessage nvarchar(MAX),
   ArtefactFileLocation nvarchar(MAX), 
   AnalysisId nvarchar(36)
);

create table QL.Rules
(
   RuleId nvarchar(36) primary key,
   Id nvarchar(MAX),
   RuleName nvarchar(MAX), 
   ShortDescription nvarchar(MAX),
   FullDescription nvarchar(MAX),
   Kind nvarchar(MAX), 
   RulePrecision nvarchar(MAX),
   ProblemSeverity nvarchar(MAX), 
   PackVersion nvarchar(36),
   AnalysisId nvarchar(36)
);

create table QL.Tags 
(
   TagId nvarchar(36) primary key, 
   RuleId nvarchar(36), 
   TagName nvarchar(MAX) 
);

create table QL.EvaluationTime 
(

  EvaluationId nvarchar(36) primary key, 
  PredicateName nvarchar(MAX), 
  QueryCausingWork nvarchar(MAX), 
  EvaluationStrategy nvarchar(MAX),
  EvaluationTimeMilliseconds float, 
  AnalysisId nvarchar(36),
  ResultSize float
);

 
