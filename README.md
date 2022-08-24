== Introduction

This repository contains source code for the CodeQL test harness.


== Assumptions

The CodeQL test harness is packaged as a container image.

These applications are required to be installe don the host:

- [Docker Engine](https://docs.docker.com/engine/)
- [CodeQL Bundle](https://github.com/github/codeql-action/releases/latest)

== Prerequisites

The contents of the `create.sql` is:

```tsql
if not exists(select * from sys.databases where name = 'd_codeql_analytics') begin
    create database d_codeql_analytics;
end
go

use d_codeql_analytics;

if not exists(select principal_id from sys.server_principals where name = 'u_codeql_analytics') begin
    create login u_codeql_analytics with password = 'P@ssw0rd'
end

if not exists(select principal_id from sys.database_principals where name = 'u_codeql_analytics') begin
    create user u_codeql_analytics for login u_codeql_analytics
    alter role db_owner add member u_codeql_analytics;
end
go
```

The contents of the `drop.sql` is:

```tsql
use d_codeql_analytics;
if exists(select principal_id from sys.database_principals where name = 'u_codeql_analytics') begin
    drop user u_codeql_analytics;
end

if exists(select principal_id from sys.server_principals where name = 'u_codeql_analytics') begin
    drop login u_codeql_analytics;
end
go

use master;
if exists(select * from sys.databases where name = 'd_codeql_analytics') begin
    drop database d_codeql_analytics;
end
go
```

To create the database in Microsoft SQL Server:

```bash
sqlcmd -U sa -P P@ssw0rd -i create.sql
```

To drop the database in Microsoft SQL Server:

```bash
sqlcmd -U sq -P P@ssw0rd -i drop.sql
```

== Build

```bash
docker build . -t github/codeql-analytics
```

== Execution

```bash
docker run --rm -it github/codeql-analytics --help
```
