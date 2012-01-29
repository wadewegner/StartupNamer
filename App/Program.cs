using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;

namespace App
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> adjectives = new List<string>();
            List<string> nouns = new List<string>();
            string outputFile = @"PossibleNames.txt";

            LoadAdjectivesAndNouns(adjectives, nouns);

            List<string> possibleNames = BuildPossibleNames(adjectives, nouns);
            List<string> potentialNames = new List<string>();

            int currentItem = 0;
            foreach (var possibleName in possibleNames)
            {
                currentItem += 1;

                if (!IsRegistered(possibleName))
                {
                    potentialNames.Add(possibleName);
                }

                Console.Write("\rProcessing {0} of {1} possible names; {2} unregistered potentials", currentItem, possibleNames.Count, potentialNames.Count);
            }

            Console.WriteLine();
            Console.WriteLine("Found {0} possible names.", potentialNames.Count);

            WriteAndOpenFile(outputFile, potentialNames);
        }

        private static void WriteAndOpenFile(string outputFile, List<string> potentialNames)
        {
            System.IO.File.Delete(outputFile);
            using (StreamWriter file = new StreamWriter(outputFile, true))
            {
                foreach (var potentialName in potentialNames)
                {
                    file.WriteLine(potentialName);
                }
            }

            ProcessStartInfo startInfo = new ProcessStartInfo();

            startInfo.FileName = "Notepad.exe";
            startInfo.Arguments = outputFile;

            Process.Start(startInfo);
        }

        private static void LoadAdjectivesAndNouns(List<string> adjectives, List<string> nouns)
        {
            System.IO.StreamReader file;
            string line;

            file = new System.IO.StreamReader(Environment.CurrentDirectory + @"\Adjectives.txt");
            while ((line = file.ReadLine()) != null)
            {
                adjectives.Add(line);
            }
            file.Close();
            Console.WriteLine("Loaded {0} adjectives.", adjectives.Count);

            file = new System.IO.StreamReader(Environment.CurrentDirectory + @"\Nouns.txt");
            while ((line = file.ReadLine()) != null)
            {
                nouns.Add(line);
            }
            file.Close();
            Console.WriteLine("Loaded {0} nouns.", nouns.Count);
        }

        private static List<string> BuildPossibleNames(List<string> adjectives, List<string> nouns)
        {
            List<string> possibleNames = new List<string>();
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            foreach (var adjective in adjectives)
            {
                foreach (var noun in nouns)
                {
                    possibleNames.Add(textInfo.ToTitleCase(adjective) + textInfo.ToTitleCase(noun));
                    possibleNames.Add(textInfo.ToTitleCase(adjective) + textInfo.ToTitleCase(noun) + "s");
                    possibleNames.Add(textInfo.ToTitleCase(noun) + textInfo.ToTitleCase(adjective));
                }
            }

            Console.WriteLine("Created {0} potential names.", possibleNames.Count);

            return possibleNames;
        }

        private static bool IsRegistered(string name)
        {
            bool isRegistered = true;

            string response = WhoisResponse(name);

            if (response.IndexOf("No match for", 0) > -1)
            {
                isRegistered = false;
            }

            return isRegistered;
        }

        private static string WhoisResponse(string name)
        {
            using (TcpClient tcpClient = new TcpClient())
            {
                tcpClient.Connect("whois.crsnic.net", 43);

                name += ".com\r\n";
                byte[] domain = Encoding.ASCII.GetBytes(name);

                using (Stream stream = tcpClient.GetStream())
                {
                    stream.Write(domain, 0, name.Length);

                    using (StreamReader streamReader = new StreamReader(tcpClient.GetStream(), Encoding.ASCII))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
        }
    }
}
