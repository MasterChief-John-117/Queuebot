@echo off
::Goes to working directory
cd bin\Debug\netcoreapp1.1\publish
::Sets the title so the kill works in BotControlPanel
title QueueBot
::runs QueueBot with the dotnet core
dotnet QueueBot.dll

::Pastebin of this file: https://pastebin.com/mapcc9Rp