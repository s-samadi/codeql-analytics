== Introduction

This repository contains source code for the CodeQL test harness.


== Assumptions

The CodeQL test harness is packaged as a container image.

These applications are required to be installed on the host:

- [Docker Engine](https://docs.docker.com/engine/)
- [CodeQL Bundle](https://github.com/github/codeql-action/releases/latest)

== Build

```bash
docker build . -t github/codeql-analytics
```

== Execution

```bash
docker run --rm -it github/codeql-analytics --help
```
