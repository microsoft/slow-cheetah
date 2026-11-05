---
name: testing
description: Provides guidance and instructions for writing, running and filtering tests.
user-invocable: false
---

**IMPORTANT**: This repository uses xUnit v3 with Microsoft.Testing.Platform (MTP v2). Traditional VSTest `--filter` syntax does NOT work. Use the options below instead.

* There should generally be one test project (under the `test` directory) per shipping project (under the `src` directory). Test projects are named after the project being tested with a `.Tests` suffix.
* Tests use `xunit.v3.mtp-v2` and target .NET Framework 4.7.2.

## Running Tests

There *may* be unstable tests that are attributed as such.
Running the [dotnet-test-cloud.ps1](../../../tools/dotnet-test-cloud.ps1) script will execute all previously built tests except the unstable ones.
The commands below will *not* automatically exclude unstable tests. Add `-- --filter-not-trait "FailsInCloudTest=true"` to exclude them.

**Run all tests** (from the root of the repository):
```bash
dotnet test --solution src\SlowCheetah.slnx --no-build -c Release
```

To run a specific test project, change to `test\Microsoft.VisualStudio.SlowCheetah.Tests` or `test\Microsoft.VisualStudio.SlowCheetah.VS.Tests` first.

**Run tests for a specific test project** (within that test project's directory):
```bash
dotnet test --no-build -c Release
```

**Run a single test method**:
```bash
dotnet test --no-build -c Release -- --filter-method "*ClassName.MethodName"
```

**Run all tests in a test class**:
```bash
dotnet test --no-build -c Release -- --filter-class "*ClassName"
```

**Run tests with wildcard matching** (supports wildcards at beginning and/or end):
```bash
dotnet test --no-build -c Release -- --filter-method "*Pattern*"
```

**Run tests with a specific trait**:
```bash
dotnet test --no-build -c Release -- --filter-trait "TraitName=value"
```

**Run tests for a specific framework only**:
```bash
dotnet test --no-build -c Release --framework net472
```

**List all available tests without running them**:
```bash
dotnet run --no-build -c Release --framework net472 -- --list-tests
```

**Key points about test filtering with xUnit v3 / MTP v2**:
- Options after `--` are passed to the test runner, not to `dotnet test`
- Use `--filter-class`, `--filter-method`, `--filter-trait`, and `--filter-not-trait`
- Traditional VSTest `--filter` expressions do NOT work
- Wildcards `*` are supported in class and method filters
- See `--help` for additional runner options
