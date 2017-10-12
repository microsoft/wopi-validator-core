# WOPI Validator

This project contains the core logic of the [WOPI validator][] as well as a command-line interface to it.

[WOPI validator]: https://wopi.readthedocs.io/en/latest/build_test_ship/validator.html

## Building the project

The project can be built in Visual Studio 2017, or using `dotnet build`:

`dotnet build -c Release`

Omit the `-c Release` portion if you want to build the debug version instead.

### Building self-contained package

To build a [self-contained package][1] for Linux or OS X, use the `dotnet publish` command:

```text
dotnet publish -c Release -r linux-x64
dotnet publish -c Release -r osx-x64
dotnet publish -c Release -r win-x64
```

The output will be placed in `\src\WopiValidator\bin\Release\netcoreapp2.0\linux-x64\publish` (replace `linux-x64`
with other platforms as needed).

[1]: https://docs.microsoft.com/en-us/dotnet/core/deploying/deploy-with-cli

## Packing the NuGet package

From the root of the project, use the following command:

`dotnet pack -c Release`

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
