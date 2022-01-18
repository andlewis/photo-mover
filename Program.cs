
if (args == null || args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
{
    args = new string[] { "--help" };
}

// Create a root command with some options
var rootCommand = new RootCommand
            {
                new Option<string>(
                    "--source",
                    description: "Source Folder"),
                new Option<string>(
                    "--dest",
                    description:"Destination  folder"),
                new Option<bool>(
                    "--copy",
                    getDefaultValue: ()=>false,
                    "Copy Only?"),
                new Option<bool>(
                    "--overwrite",
                    getDefaultValue: ()=>true,
                    "Overwrite Files?"
                    ),
                new Option<bool>(
                    "--includeSubfolders",
                    getDefaultValue: ()=>true,
                    "Include Subfolders?"
                    )
            };

rootCommand.Description = "Photo Mover";

// Note that the parameters of the handler method are matched according to the names of the options
rootCommand.Handler = CommandHandler.Create<string, string, bool, bool, bool>((source, dest, copy, overwrite, includeSubfolders) =>
{
    PhotoMover.Utils.MoveIt(source, dest, copy, overwrite, includeSubfolders);
});

// Parse the incoming args and invoke the handler
return rootCommand.InvokeAsync(args).Result;

