

<img width="490" alt="Screen Shot 2022-09-21 at 7 26 10 pm" src="https://user-images.githubusercontent.com/68650974/191468356-0b6fe5f9-0b73-4132-9a5e-ca9c56846f63.png">




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
