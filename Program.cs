using System.Text.RegularExpressions;
using Microsoft.Win32;

// Author https://github.com/naumenkoff
// Автор https://github.com/naumenkoff

// Works only on 64-bit systems with Windows 10 and above
// Работает только на 64-битных системах с Windows 10 и выше

// The path where the Steam client registry key is located - HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam
// Путь, по которому располагается ключ реестра клиента Steam - HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam

string? GetSteamFolder()
{
    try
    {
        // Using the "using" operator, we can forget about RegistryKey.Close();
        // Используя оператор "using" мы можем забыть про RegistryKey.Close();

        using var registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

        // Если клиент установлен - то мы что то да получим, а иначе - ничего, ведь Wow6432Node - стандартный раздел реестра Windows
        // If the Steam Client is installed, then we'll get something, otherwise nothing, because "Wow6432Node" is a standard Windows registry key

        using var steamKey = registryKey.OpenSubKey(@"SOFTWARE\Wow6432Node\Valve\Steam");

        // Путь до папки с клиентом стима, который вы выбирали при первичной установке, имеет имя значения "InstallPath"
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

    // Здесь мы читаем файл "libraryfolders.vdf" - в изначальной папке стима он содержит все остальные локации стима
    // Here we read the file "libraryfolders.vdf" - in the original Steam folder, it contains all the other Steam folders
    var libraryFoldersPath = Path.Combine(steamFolder, "steamapps", "libraryfolders.vdf");
    var libraryFoldersContent = File.ReadAllText(libraryFoldersPath);

    // Для тех, кто не разбирается в регулярных выражениях, объясню
    // Изначально строчка выглядит так - '		"path"		"C:\\Program Files (x86)\\Steam"'
    // Мы ищем совпадения по "path", и просто добавляем эту строчку в коллекцию, предварительно убрав сам "path", заменив \\ на \ и убрав пробелы

    // For those who dont understand regular expressions, i'll explain
    // Initially the string looks like this - '		"path"		"C:\\Program Files (x86)\\Steam"'
    // We are looking for matches along the "path" and simply add this string to the collection, first deleting the "path" itself, replacing \\ with \ and removing spaces
    return Regex.Matches(libraryFoldersContent, "\"path\".+?\"(.+?)\"").Cast<Match>()
        .Select(x => x.Groups[1].Value.Replace(@"\\", @"\")).ToList();
}
var steamLocations = GetSteamLocations();

steamLocations?.ForEach(Console.WriteLine);
