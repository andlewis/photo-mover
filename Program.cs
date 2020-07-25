using ExifLib;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace photo_mover
{
    class Program
    {
        static int Main(string[] args)
        {
            if(args==null || args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
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
                    )
            };

            rootCommand.Description = "Photo Mover";

            // Note that the parameters of the handler method are matched according to the names of the options
            rootCommand.Handler = CommandHandler.Create<string, string, bool, bool>((source, dest, copy, overwrite) =>
            {
                MoveIt(source, dest, copy, overwrite);
            });

            // Parse the incoming args and invoke the handler
            return rootCommand.InvokeAsync(args).Result;

          
        }

        static void MoveIt(string sourceFolder, string destinationFolder, bool copyOnly = false, bool overwrite = true)
        {
            var exit = false;


            if (!string.IsNullOrWhiteSpace(sourceFolder) && !System.IO.Directory.Exists(sourceFolder))
            {
                Console.WriteLine($"Source Folder does not exist: {sourceFolder}");
                exit = true;
            }

            if (!string.IsNullOrWhiteSpace(destinationFolder) && !System.IO.Directory.Exists(destinationFolder))
            {
                Console.WriteLine($"Destination Folder does not exist: {destinationFolder}");
                exit = true;
            }

            if (exit)
            {
                return;
            }

            var moveCount = 0;
            var stayCount = 0;
            var files = System.IO.Directory.GetFiles(sourceFolder, "*.*", System.IO.SearchOption.AllDirectories);
            foreach (var path in files)
            {
                using (var reader = new ExifReader(path))
                {
                    if (reader.GetTagValue<DateTime>(ExifTags.DateTimeDigitized, out var date))
                    {
                        reader.Dispose();

                        var file = new System.IO.FileInfo(path);
                        var newPath = System.IO.Path.Combine(destinationFolder, date.Year.ToString("D4"), $"{date.Year.ToString("D4")}_{date.Month.ToString("D2")}", $"{date.Year.ToString("D4")}_{date.Month.ToString("D2")}_{date.Day.ToString("D2")}");

                        if (!System.IO.Directory.Exists(newPath))
                            System.IO.Directory.CreateDirectory(newPath);

                        var movePath = System.IO.Path.Combine(newPath, file.Name);
                        if (copyOnly)
                        {
                            System.IO.File.Copy(path, movePath, overwrite);
                        }
                        else
                        {
                            System.IO.File.Move(path, movePath, overwrite);
                        }
                        moveCount++;
                        Console.WriteLine($"Moved: {date} - {path} to {movePath}");
                    }
                    else
                    {
                        stayCount++;
                        Console.WriteLine($"EXIF Date Not Found: {path}");
                    }
                }
            }

            Console.WriteLine($"Complete. Moved: {moveCount}. Not Moved: {stayCount}");
        }
    }
}
