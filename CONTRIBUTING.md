# Contributing to QueueBot
---
Pleasepleaseplease add your changes to `dev`, not `master`, so I can keep track of.. things

Stuff here will change as I have no idea how to do this

## Code Style Guidelines
A few guidelines to follow when adding your own code
---
For Commands, always follow this structure:
```cs
[Command("Name")]
[Alias("InCapsPls")]
[Summary("Use this for the command info, it'll be used for the help command")]
public async Task Command_Name()
```

Log stuff using `QueueBot.Logger`, not `Console.WriteLine`, for anything that's not debugging.
