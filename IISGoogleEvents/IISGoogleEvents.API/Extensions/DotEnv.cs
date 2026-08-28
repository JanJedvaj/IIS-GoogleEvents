namespace IISGoogleEvents.API.Extensions;

public static class DotEnv
{
    public static void Load(string fileName = ".env")
    {
        var path = FindUpwards(fileName);
        if (path == null)
            return;

        foreach (var rawLine in File.ReadAllLines(path))
        {
            var line = rawLine.Trim();

            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            var separator = line.IndexOf('=');
            if (separator <= 0)
                continue;

            var key = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim().Trim('"');

            if (Environment.GetEnvironmentVariable(key) == null)
                Environment.SetEnvironmentVariable(key, value);
        }
    }

    private static string? FindUpwards(string fileName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null)
        {
            var candidate = Path.Combine(directory.FullName, fileName);
            if (File.Exists(candidate))
                return candidate;

            var isRepositoryRoot = Directory.Exists(Path.Combine(directory.FullName, ".git"));
            if (isRepositoryRoot)
                return null;

            directory = directory.Parent;
        }

        return null;
    }
}
