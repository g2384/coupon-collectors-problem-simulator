using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Simulator
{
    public class Program
    {
        private static readonly string ConfigFilePath = "config.json";

        static void Main(string[] args)
        {
            Settings settings;
            if (File.Exists(ConfigFilePath))
            {
                var json = File.ReadAllText(ConfigFilePath);
                settings = JsonConvert.DeserializeObject<Settings>(json);
            }
            else
            {
                settings = new Settings
                {
                    CardCount = 20,
                    CardPerPack = 4,
                    SimulationCount = 1_000_000,
                    ReportEveryPercentage = 1
                };
                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(ConfigFilePath, json);
            }
            if (settings == null)
            {
                Console.WriteLine("Bad configuration.");
                return;
            }
            var cardCount = settings.CardCount;
            var cardPerPack = settings.CardPerPack;
            var sampleCount = (decimal)settings.SimulationCount;
            var progressReport = 100 / settings.ReportEveryPercentage;
            var samplePoolVolume = Math.Round(sampleCount / progressReport);

            var numberList = Enumerable.Range(1, cardCount).ToArray();
            var allPacks = Combinations.CombinationsOfK(numberList, cardPerPack).ToArray();
            var actualPackCombo = allPacks.Length;

            var r = new Random();
            var stat = Enumerable.Range(1, cardCount / cardPerPack)
                .Select(e => e * cardPerPack)
                .ToDictionary(e => e, e => 0M);
            stat[cardCount] = 0;

            var currentSample = 0;
            var sw = new Stopwatch();
            sw.Start();
            using (var progress = new ProgressBar())
            {
                for (var i = 0; i < sampleCount; i++)
                {
                    var statCopy = Enumerable.Range(1, cardCount / cardPerPack)
                        .Select(e => e * cardPerPack)
                        .ToDictionary(e => e, e => 0);
                    statCopy[cardCount] = 0;
                    var drawCount = 0;
                    var result = "";
                    var ownedSet = new HashSet<int>();
                    while (ownedSet.Count != cardCount)
                    {
                        drawCount++;
                        var n = r.Next(actualPackCombo);
                        var set = allPacks[n];
                        foreach (var card in set)
                        {
                            ownedSet.Add(card);
                        }

                        result += $"{drawCount}, {ownedSet.Count}\n";

                        var key = (int)Math.Floor(ownedSet.Count / (double)cardPerPack) * cardPerPack;
                        if (statCopy[key] == 0)
                        {
                            statCopy[key] = drawCount;
                        }
                    }

                    statCopy[cardCount] = drawCount;

                    foreach (var k in statCopy)
                    {
                        stat[k.Key] += k.Value;
                    }

                    //Console.WriteLine(i);

                    //File.AppendAllText("result.csv", result);

                    currentSample++;
                    if (currentSample >= samplePoolVolume)
                    {
                        currentSample = 0;
                        var percentage = Math.Round((i + 1) / sampleCount * 100);
                        var ms = sw.ElapsedMilliseconds;
                        progress.Report((double)percentage / 100, $"(elapsed: {GetSeconds(ms)}, remaining: {GetSeconds(ms / (percentage / 100) - ms)})");
                        //Console.WriteLine($"{percentage}% (elapsed: {GetSeconds(ms)}, remaining: {GetSeconds(ms / (percentage / 100) - ms)})");
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine("collect, draw n times");
            foreach (var k in stat)
            {
                Console.WriteLine($"{k.Key.ToString().PadLeft(7, ' ')}, {Math.Round(k.Value / sampleCount, 8)}");
            }

            //Console.WriteLine($"e = {Math.Round((stat[cardCount] / sampleCount) / (cardCount / (decimal)cardPerPack), 8)}");
            Console.ReadKey();
        }

        private static string GetSeconds(decimal ms)
        {
            if (ms < 1000 * 2)
            {
                return Math.Round(ms, 3) + "ms";
            }

            if (ms < 1000 * 60 * 2)
            {
                return Math.Round(ms / 1000, 3) + "s";
            }

            if (ms < 1000 * 60 * 60 * 2)
            {
                return Math.Round(ms / (1000 * 60), 3) + "m";
            }

            return Math.Round(ms / (1000 * 60 * 60), 3) + "h";
        }
    }
}
