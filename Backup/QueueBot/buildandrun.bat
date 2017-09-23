@echo off
::Because it's nice to say what's going on
echo Starting build
::Build the bot from source code, also allows not using an IDE
dotnet publish
::copy the changelog
copy changelog.txt bin\Debug\netcoreapp1.1\publish\changelog.txt
echo Build complete, starting run
::move to working directory
cd bin\Debug\netcoreapp1.1\publish
::Title so taskkill from BotControlPanel works
title QueueBot
::runs Queuebot with dotnet core
dotnet QueueBot.dll

::Pastebin for this file https://pastebin.com/1ahCfjxC