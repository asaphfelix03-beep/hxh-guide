namespace TaskFlow;

public static class AppInfo
{
    /// <summary>Version définie dans TaskFlow.csproj (balise &lt;Version&gt;).</summary>
    public static string Version { get; } = typeof(AppInfo).Assembly.GetName().Version?.ToString(3) ?? "inconnue";
}
