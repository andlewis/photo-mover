using ExifLib;
using System;

namespace photo_mover
{
    class Program
    {
        static void Main(string[] args)
        {
            var exit = false;

            if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]) || string.IsNullOrWhiteSpace(args[1]))
            {
                Console.WriteLine("usage: photo-mover.exe [source folder] [destination folder]");
                exit = true;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(args[0]) && !System.IO.Directory.Exists(args[0]))
                {
                    Console.WriteLine($"Source Folder does not exist: {args[0]}");
                    exit = true;
                }

                if (!string.IsNullOrWhiteSpace(args[1]) && !System.IO.Directory.Exists(args[1]))
                {
                    Console.WriteLine($"Destination Folder does not exist: {args[1]}");
                    exit = true;
                }
            }

            if (exit)
            {
                return;
            }

            var sourceFolder = args[0];
            var destFolder = args[1];
            var copyOnly = true;
            var overwrite = true;

            var moveCount = 0;
            var stayCount = 0;
            var files = System.IO.Directory.GetFiles(args[0], "*.*", System.IO.SearchOption.AllDirectories);
            foreach (var path in files)
            {
                using (var reader = new ExifReader(path))
                {
                    if (reader.GetTagValue<DateTime>(ExifTags.DateTimeDigitized, out var date))
                    {
                        reader.Dispose();

                        var file = new System.IO.FileInfo(path);
                        var newPath = System.IO.Path.Combine(args[1], date.Year.ToString("D4"), $"{date.Year.ToString("D4")}_{date.Month.ToString("D2")}", $"{date.Year.ToString("D4")}_{date.Month.ToString("D2")}_{date.Day.ToString("D2")}");

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
