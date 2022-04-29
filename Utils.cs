namespace PhotoMover;
public static class Utils
{

    public static void MoveIt(string sourceFolder, string destinationFolder, bool copyOnly = false, bool overwrite = true, bool includeSubfolders = true)
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
        var files = System.IO.Directory.GetFiles(sourceFolder, "*.*", includeSubfolders
            ? System.IO.SearchOption.AllDirectories
            : System.IO.SearchOption.TopDirectoryOnly);
        var validExtensions = new string[] { ".jpg", ".heic", ".mov", ".mp4", ".png", ".gif", ".webp" };
        foreach (var path in files)
        {
            if (validExtensions.Any(m => m == new System.IO.FileInfo(path).Extension.ToLower()))
            {
                var rawDate = GetDate(path);
                if (rawDate != null)
                {
                    var date = rawDate.Value;
                    var file = new System.IO.FileInfo(path);
                    var newPath = System.IO.Path.Combine(destinationFolder,
                        date.Year.ToString("D4"),
                        $"{date.Year:D4}_{date.Month:D2}",
                        $"{date.Year:D4}_{date.Month:D2}_{date.Day:D2}");

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
            else
            {
                Console.WriteLine($"Invalid extension ({new System.IO.FileInfo(path).Extension}): {path}");
            }
        }

        Console.WriteLine($"Complete. Moved: {moveCount}. Not Moved: {stayCount}");
    }

    private static DateTime? GetDate(string path)
    {
        var directories = ImageMetadataReader.ReadMetadata(path);
        string dateTime;
        foreach (var dir in directories)
        {
            dateTime = dir?.GetDescription(ExifDirectoryBase.TagDateTime)
                ?? dir.GetDescription(ExifDirectoryBase.TagDateTimeOriginal)
                ?? dir.Tags.FirstOrDefault(m => m.Name == "Created")?.Description;


            if (!string.IsNullOrWhiteSpace(dateTime))
            {
                if (DateTime.TryParse(dateTime, out var date))
                {
                    return date;
                }
                else if (DateTime.TryParseExact(dateTime,
                    new string[] { "ddd. MMM. dd HH:mm:ss yyyy", "yyyy:MM:dd HH:mm:ss" },
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var date2))
                {
                    return date2;
                }


            }
            else if (new System.IO.FileInfo(path).Name.Length > 18
                    && DateTime.TryParseExact(new System.IO.FileInfo(path).Name[..19],
                    new string[] { "yyyyMMdd_HHmmssfff_", "yyyy-MM-dd HH.mm.ss" },
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var date3))
            {
                return date3;
            }
        }

        return null;

    }

}
