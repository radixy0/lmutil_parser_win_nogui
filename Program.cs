using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;

namespace lmutil_parser_win_nogui
{
    class Program
    {
        static void Main(string[] args)
        {
            Lmutil_location loc = null;
            string launchoptions = "lmstat - c \" % SE_LICENSE_SERVER % \" -A";
            //check for lmutil location
            loc = deserializeLmutil();
            //ask to re-enter if needed
            if (loc == null)
            {
                Console.WriteLine("lmutil.exe not found");
                Console.WriteLine("please enter full path of lmutil.exe");
                Console.WriteLine("NOTE: you will only have to do this once!");
                string input = Console.ReadLine();
                loc = new Lmutil_location();
                loc.Location = input;
                serializeLmutil(loc);
            }
            Console.WriteLine("lmutil location: " + loc.Location);
            Console.WriteLine("to change this location, delete krong.bin and launch again");
            //ask for launch options
            Console.WriteLine("Launch options: " + launchoptions);
            Console.WriteLine("Change launch options? (input wanted options, hit enter to skip)");
            string launchoptions_temp = Console.ReadLine();
            if(launchoptions_temp != "")
            {
                launchoptions = launchoptions_temp;
            }
            //launch


            StreamWriter outStream = new StreamWriter("log.txt");
            Process process = new Process();
            process.StartInfo.FileName = loc.Location;
            process.StartInfo.Arguments = launchoptions;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (!String.IsNullOrEmpty(e.Data))
                {
                    outStream.WriteLine(e.Data);
                }
            });

            process.Start();

            process.BeginOutputReadLine();

            process.WaitForExit();
            process.Close();

            outStream.Close();

            Console.WriteLine("complete lmutil output can be found in log.txt");

            StreamReader inStream = new StreamReader("log.txt");
            String line;
            while ((line = inStream.ReadLine()) != null)
            {
                Console.Write(processLine(line));
            }

        }

        public static String processLine(String input)
        {
            String output = "";
            //return input; //-- DEBUG
            //pattern 1
            String pattern = @"\bUsers of (\w+): .*";
            MatchCollection matches = Regex.Matches(input, pattern);
            if (matches.Count() > 0)
            {
                output += "\n\n Users of: " + matches.First().Groups[1];
                return output;
            }

            //pattern 2
            pattern = @"\s*(\w+) (\w+) (\S|\.)* \(.*\) \(.*\)";
            matches = Regex.Matches(input, pattern);
            if (matches.Count() > 0)
            {
                output += "\n   User: " + matches.First().Groups[1] + " Machine: " + matches.First().Groups[2];
            }

            return output;
        }

        public static void serializeLmutil(Lmutil_location toSerialize)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("krong.bin", FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, toSerialize);
            stream.Close();
        }

        public static Lmutil_location deserializeLmutil()
        {
            if (!File.Exists(@"krong.bin"))
            {
                return null;
            }
            IFormatter formatter2 = new BinaryFormatter();
            Stream stream2 = new FileStream("krong.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
            Lmutil_location lmutil = (Lmutil_location)formatter2.Deserialize(stream2);
            stream2.Close();
            return lmutil;
        }
    }

    [Serializable]
    public class Lmutil_location
    {
        private String location = "";

        public string Location    // the Name property
        {
            get => location;
            set => location = value;
        }
    }
}
