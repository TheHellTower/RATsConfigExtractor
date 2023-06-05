using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Policy;
using YaraSharp;

namespace RATsConfigExtractor
{
    internal class Program
    {
        private static string yaraRulesFolder = "TYaraHRulesT", filePath = string.Empty;
        private static void Initialization()
        {
            var extractPath = AppDomain.CurrentDomain.BaseDirectory;
            var downloadPath = $"{extractPath}\\{yaraRulesFolder}";
            if (!Directory.Exists(downloadPath))
            {
                Console.WriteLine($"It seems that {yaraRulesFolder} folder is missing ! Downloading it for you...");
                Directory.CreateDirectory(yaraRulesFolder);
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        client.DownloadFile("https://raw.githubusercontent.com/TheHellTower/RATsConfigExtractor/rules/TYaraHRulesT.zip", $"{downloadPath}.zip");
                        Console.WriteLine("File downloaded successfully.");
                        using (ZipArchive archive = ZipFile.OpenRead($"{downloadPath}.zip"))
                        {
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                try
                                {
                                    string entryPath = $"{extractPath}{entry.FullName.Replace("/", "\\")}";
                                    entry.ExtractToFile(entryPath, true);
                                }
                                catch { }
                            }

                            Console.WriteLine("Extraction completed successfully.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred: {ex.Message}");
                    }
                }
            }
        }
        private static void YaraAndExtract()
        {
            if (filePath.Contains("\\"))
            {
                YSInstance YSInstance = new YSInstance();

                Dictionary<string, object> externals = new Dictionary<string, object>() { { "filename", string.Empty }, { "filepath", string.Empty }, { "extension", string.Empty } };

                List<string> ruleFilenames = Directory.GetFiles($".\\{yaraRulesFolder}", "*.yar", SearchOption.AllDirectories).ToList();

                using (YSContext context = new YSContext())
                {
                    using (YSCompiler compiler = YSInstance.CompileFromFiles(ruleFilenames, externals))
                    {
                        YSRules rules = compiler.GetRules();

                        YSReport errors = compiler.GetErrors();

                        YSReport warnings = compiler.GetWarnings();

                        List<YSMatches> Matches = YSInstance.ScanFile(filePath, rules, externals, 0);

                        foreach (YSMatches Match in Matches)
                        {
                            Console.WriteLine($"{Match.Rule.Identifier} Detected !");

                            Type classType = Type.GetType($"{Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title}.Extractors." + Match.Rule.Identifier);
                            if (classType == null) return;
                            object instance = Activator.CreateInstance(classType);

                            MethodInfo executeMethod = classType.GetMethod("Execute");
                            if (executeMethod == null) return;
                            executeMethod.Invoke(instance, new string[] { filePath });

                        }
                    }
                    //Log errors
                }
            } else
            {
                Console.WriteLine("Invalid File Path.");
            }
        }
        static void Main(string[] args)
        {
            Initialization();
            filePath = args[0] ?? string.Empty;
            while (args.Length == 0 && !File.Exists(filePath))
            {
                Console.WriteLine("File Path: ");
                filePath = Console.ReadLine().Replace("\"", "");
                Console.Clear();
            }
            YaraAndExtract();

            Console.ReadLine();
        }
    }
}