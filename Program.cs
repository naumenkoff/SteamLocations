using System.Text.RegularExpressions;
using Microsoft.Win32;

// Author https://github.com/naumenkoff
// Works only on 64-bit systems with Windows 10 and above
// The path where the Steam client registry key is located - HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam

string? GetSteamFolder()
{
    try
    {
        // Using the "using" operator, we can forget about RegistryKey.Close();
        using var registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

        // If the Steam Client is installed, then we'll get something, otherwise nothing, because "Wow6432Node" is a standard Windows registry key
        using var steamKey = registryKey.OpenSubKey(@"SOFTWARE\Wow6432Node\Valve\Steam");

        // The path to the Steam Client folder that you selected during the first installation has the value "InstallPath"
        return steamKey?.GetValue("InstallPath")?.ToString();
    }
    catch (Exception e)
    {
        Console.WriteLine("[EXCEPTION] " + e);
        return null;
    }
}
var steamFolder = GetSteamFolder();

List<string>? GetSteamLocations()
{
    if (steamFolder == null) return null;

    // Here we read the file "libraryfolders.vdf" - in the original Steam folder, it contains all the other Steam folders
    var libraryFoldersPath = Path.Combine(steamFolder, "steamapps", "libraryfolders.vdf");
    var libraryFoldersContent = File.ReadAllText(libraryFoldersPath);

    // For those who dont understand regular expressions, i'll explain
    // Initially the string looks like this - '		"path"		"C:\\Program Files (x86)\\Steam"'
    // We are looking for matches along the "path" and simply add this string to the collection, first deleting the "path" itself, replacing \\ with \ and removing spaces
    return Regex.Matches(libraryFoldersContent, "\"path\".+?\"(.+?)\"").Select(x => x.Groups[1].Value.Replace(@"\\", @"\")).ToList();
}

// Using example
var steamLocations = GetSteamLocations();

// Output
steamLocations?.ForEach(Console.WriteLine);
