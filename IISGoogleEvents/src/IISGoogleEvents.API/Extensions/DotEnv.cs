namespace IISGoogleEvents.API.Extensions;

/// <summary>
/// Loads the repository-root .env into environment variables, so that secrets stay
/// out of appsettings.json and arrive through the standard environment-variable
/// configuration provider (where "__" separates configuration sections).
///
/// Must run before WebApplication.CreateBuilder, which snapshots the environment.
/// </summary>
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

            // Split on the first '=' only: base64 secrets and connection strings
            // legitimately contain '=' in the value.
            var separator = line.IndexOf('=');
            if (separator <= 0)
                continue;

            var key = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim().Trim('"');

            // A variable already present in the real environment wins, so CI and
            // container deployments can override the file without editing it.
            if (Environment.GetEnvironmentVariable(key) == null)
                Environment.SetEnvironmentVariable(key, value);
        }
    }

    /// <summary>
    /// Walks up from the running assembly, which sits several levels down in
    /// bin/Debug/net8.0, and stops at the repository root. Bounding the search there
    /// means an unrelated .env further up the filesystem can never be picked up.
    /// </summary>
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
