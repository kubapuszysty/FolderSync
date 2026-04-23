using System.Security.Cryptography;

class Program
{
    static void Main(string[] args)
    {
        if(args.Length < 4)
        {
            Console.WriteLine("Usage: <sourcePath> <replicaPath> <intervalSeconds> <logFilePath>");
            return;
        }

        string sourcePath = args[0];
        string replicaPath = args[1];
        int interval = int.Parse(args[2]);
        string logPath = args[3];

        if(!Directory.Exists(sourcePath))
        {
            Console.WriteLine("Source directory does not exist.");
            return;
        }

        if(!Directory.Exists(replicaPath))
        {
            Directory.CreateDirectory(replicaPath);
        }

        while(true)
        {
            try
            {
                SyncFolders(sourcePath, replicaPath, logPath);
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}", logPath);
            }

            Thread.Sleep(interval * 1000);
        }
    }

    static void SyncFolders(string source, string replica, string logPath)
    {
        foreach(var dir in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(source, dir);
            string destDir = Path.Combine(replica, relativePath);

            if(!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
                Log($"Directory created: {relativePath}", logPath);
            }
        }

        foreach(var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(source, file);
            string destFile = Path.Combine(replica, relativePath);

            string? destDir = Path.GetDirectoryName(destFile);
            if(!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir!);
                Log($"Directory created: {Path.GetRelativePath(replica, destDir!)}", logPath);
            }

            if(!File.Exists(destFile))
            {
                File.Copy(file, destFile);
                Log($"File created: {relativePath}", logPath);
            }
            else
            {
                string sourceHash = GetFileHash(file);
                string destHash = GetFileHash(destFile);

                if(sourceHash != destHash)
                {
                    File.Copy(file, destFile, true);
                    Log($"Updated (MD5 changed): {relativePath}", logPath);
                }
            }
        }

        foreach(var file in Directory.GetFiles(replica, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(replica, file);
            string sourceFile = Path.Combine(source, relativePath);

            if(!File.Exists(sourceFile))
            {
                File.Delete(file);
                Log($"Deleted: {relativePath}", logPath);
            }
        }

        foreach(var dir in Directory.GetDirectories(replica, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(replica,dir);
            string sourceDir = Path.Combine(source, relativePath);

            if(!Directory.Exists(sourceDir))
            {
                Directory.Delete(dir, true);
                Log($"Deleted folder: {relativePath}", logPath);
            }
        }
    }

    static string GetFileHash(string path)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(path);
        var hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
    static void Log(string message, string logPath)
    {
        string fullMessage = $"{DateTime.Now}: {message}";
        Console.WriteLine(fullMessage);

        try
        {
            File.AppendAllText(logPath, fullMessage + Environment.NewLine);
        }
        catch
        {
            Console.WriteLine("Failed to write to log file.");
        }
    }
}