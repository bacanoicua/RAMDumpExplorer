using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class Program
{
    private static async Task FindMatchesAsync(string filePath, string[] regexStrings)
    {
        RegexOptions regexOptions = RegexOptions.Compiled | RegexOptions.None;
        Regex[] regexes = Array.ConvertAll(regexStrings, regexString => new Regex(regexString, regexOptions));

        using (FileStream fileStream = File.OpenRead(filePath))
        using (StreamReader streamReader = new StreamReader(fileStream))
        {
            int bufferSize = 512 * 1024;
            char[] buffer = new char[bufferSize];
            int read;
            HashSet<string> matchedValues = new HashSet<string>();
            while ((read = await streamReader.ReadAsync(buffer, 0, bufferSize)) > 0)
            {
                string bufferString = new string(buffer, 0, read);
                foreach (Regex regex in regexes)
                {
                    MatchCollection matches = regex.Matches(bufferString);

                    foreach (Match match in matches)
                    {
                        string matchValue = match.Value;
                        if (matchedValues.Contains(matchValue))
                        {
                            continue;
                        }

                        matchedValues.Add(matchValue);
                        Console.WriteLine("{0}", matchValue);
                    }
                }
            }
        }
    }

    static async Task Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: program <path_to_file>");
            return;
        }

        string filePath = args[0];
        string[] regexStrings;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("RAMDumpExplorer\nCreated By Bacanoicua\nVersion: 1.0\n");
        Console.ResetColor();
        Console.WriteLine("CMD:\n");

        CultureInfo currentCulture = CultureInfo.CurrentCulture;
        if (currentCulture.TwoLetterISOLanguageName == "pt")
        {
            regexStrings = new[] { @"Prompt de Comando - ([^.]+).{4}" };
        }
        else if (currentCulture.TwoLetterISOLanguageName == "en")
        {
            regexStrings = new[] { @"Command Prompt - ([^.]+).{4}", @"Administrator: C:\Windows\System32\cmd.exe - ([^.]+).{4}" };
        }
        else
        {
            regexStrings = new[] { @"Símbolo del sistema - ([^.]+).{4}", @"Administrador: C:\Windows\System32\cmd.exe - ([^.]+).{4}" };
        }

        try
        {
            await FindMatchesAsync(filePath, regexStrings);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: {0}", ex.Message);
        }

        Console.WriteLine("\nFiles and rundll32/regsvr32:\n");

        regexStrings = new[]
        {
    @"\\Device\\HarddiskVolume[\w\s\\.-]*\.(?<!rundll32|regsvr32)[a-zA-Z0-9\s]{3}",
    @"\\\\Device\\\\HarddiskVolume[\\w\\s\\\\.-]*(rundll32|regsvr32)\\.[a-zA-Z0-9\\s]{3}(\\.[\\w\\s-]{3})?"
};

        try
        {
            await FindMatchesAsync(filePath, regexStrings);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: {0}", ex.Message);
        }
    }
}