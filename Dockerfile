FROM mcr.microsoft.com/dotnet/core/runtime:2.2

COPY src/WopiValidator/bin/Release/netcoreapp2.2/publish/ /app/
WORKDIR /app/

ENTRYPOINT ["dotnet", "Microsoft.Office.WopiValidator.dll"]
