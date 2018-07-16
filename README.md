# WOPI Validator

This project contains the core logic of the [WOPI validator][] as well as a command-line interface to it.

[WOPI validator]: https://wopi.readthedocs.io/en/latest/build_test_ship/validator.html

## Building the project

The project can be built in Visual Studio 2017, Visual Studio Code, or using `dotnet build`:

`dotnet build -c Release`

The resulting build will be output to `src\WopiValidator\bin\Release\netcoreapp2.0\`. Omit the `-c Release`
portion if you want to build the debug version instead.

### Building self-contained package

To build a [self-contained package][1] for Linux or macOS, use the `dotnet publish` command:

```text
dotnet publish -c Release -r linux-x64
dotnet publish -c Release -r osx-x64
dotnet publish -c Release -r win-x64
```

Note: you may see an error on build like this:

```text
error MSB3030: Could not copy the file "obj\Release\netcoreapp2.0\win-x64\Microsoft.Office.WopiValidator.dll"
because it was not found.
```

If you see this error, you should re-build the app using the same `-r` option used in the publish command. For example,
if you were trying to package the Linux self-contained package, first build the app using
`dotnet build -c Release -r linux-x64`, then publish it using `dotnet publish -c Release -r linux-x64`.

The output will be placed in `src/WopiValidator/bin/Release/netcoreapp2.0/linux-x64/publish` (replace `linux-x64`
with other platforms as needed).

[1]: https://docs.microsoft.com/en-us/dotnet/core/deploying/deploy-with-cli

## Running tests

Basic unit tests can be run using the `dotnet test` command:

`dotnet test ./test/WopiValidator.Core.Tests/WopiValidator.Core.Tests.csproj -c Release`

## Packing the NuGet package

From the root of the project, use the following command:

`dotnet pack -c Release`

The package will be output to `src\WopiValidator\bin\Release\Microsoft.Office.WopiValidator.1.0.0.nupkg`

Note: if you see any errors, you may need to build the project first, as described above.

## Usage

There are several ways to run the validator.

### Option 1: `dotnet`

After building the projects as described above, you can run the resulting `Microsoft.Office.WopiValidator.dll`
using the `dotnet` command. For example:

`dotnet Microsoft.Office.WopiValidator.dll --token MyAccessToken --token_ttl 0 -wopisrc http://localhost:5000/wopi/files/1 --testcategory OfficeOnline --ignore-skipped`

Note: the Microsoft.Office.WopiValidator.dll file can be found in `src\WopiValidator\bin\Release\netcoreapp2.0\`.

### Option 2: `dotnet run --project`

You can also use the `dotnet run` command, passing the path to the `WopiValidator.csproj` file using the `--project`
option. Arguments to the validator itself can be passed in by separating them from the `dotnet run` arguments with
a `--`. For example:

`dotnet run --project ./src/WopiValidator/WopiValidator.csproj -- -t MyAccessToken -l 0 -w http://localhost:5000/wopi/files/1 -e OfficeOnline -s`

### Option 3: self-contained package

Another option is to build a self-contained package for your OS (see above) and execute the resulting executable
file, which be called `Microsoft.Office.Validator.exe` on Windows and `Microsoft.Office.Validator` on Linux and macOS.
Arguments to the validator can be passed in directly. For example:

`Microsoft.Office.Validator.exe -t MyAccessToken -l 0 -w http://localhost:5000/wopi/files/1 -e OfficeOnline -s`

### Full usage options

```text
Microsoft.Office.WopiValidator 1.0.0
Copyright (C) 2018 Microsoft

  -w, --wopisrc           Required. WopiSrc URL for a wopitest file

  -t, --token             Required. WOPI access token

  -l, --token_ttl         Required. WOPI access token ttl

  -c, --config            (Default: runConfig.xml) Path to XML file with test definitions

  -g, --testgroup         Run only the tests in the specified group (cannot be used with testname)

  -n, --testname          Run only the test specified (cannot be used with testgroup)

  -e, --testcategory      (Default: All) Run only the tests in the specified category

  -s, --ignore-skipped    Don't output any info about skipped tests.

  --help                  Display this help screen.

  --version               Display version information.
```

## Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit <https://cla.microsoft.com>.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
