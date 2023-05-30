using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using YaraSharp;

namespace RATsConfigExtractor
{
    internal class Program
    {
        private static string yaraRulesFolder = "TYaraHRulesT", filePath = string.Empty;
        private static void Initialisation()
        {
            if (!Directory.Exists(yaraRulesFolder))
            {
                Console.WriteLine($"It seems that {yaraRulesFolder} folder is missing ! Do you want to download ? (y/any)");
                var key = Console.ReadKey().Key;
                if(key != ConsoleKey.Y) Environment.Exit(0);
                Console.Clear();
                //Download Step
            }
        }
        private static void YaraTest()
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


                    string Filename =                                                                                                                           @"C:\Users\TheHellTower_THT\Downloads\Quasar.v1.4.1\Quasar v1.4.1\Client.exe";

                    List<YSMatches> Matches = YSInstance.ScanFile(Filename, rules,externals, 0);

                    foreach (YSMatches Match in Matches)
                    {
                        Console.WriteLine($"{Match.Rule.Identifier} Detected !");

                        //Console.WriteLine(Encoding.UTF8.GetString(Match.Matches.First().Value.First().Data));

                        Type classType = Type.GetType($"{Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title}.Extractors." + Match.Rule.Identifier);
                        if (classType == null) return;
                        object instance = Activator.CreateInstance(classType);

                        MethodInfo executeMethod = classType.GetMethod("Execute");
                        if (executeMethod == null) return;
                        executeMethod.Invoke(instance, null);

                    }
                }
                //  Log errors
            }
        }
        static void Main(string[] args)
        {
            Initialisation();
            while (args.Length == 0 && !File.Exists(filePath))
            {
                Console.WriteLine("File Path: ");
                filePath = Console.ReadLine().Replace("\"", "");
                Console.Clear();
            }
            YaraTest();

            Console.ReadLine();
        }
    }
}