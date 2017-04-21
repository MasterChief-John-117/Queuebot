@echo off
echo Starting build
dotnet publish
copy changelog.txt bin\Debug\netcoreapp1.1\publish\changelog.txt
echo Build complete, starting run
cd bin\Debug\netcoreapp1.1\publish
dotnet QueueBot.dll

