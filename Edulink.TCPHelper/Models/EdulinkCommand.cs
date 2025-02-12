using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Edulink.TCPHelper.Models
{
    public class EdulinkCommand
    {
        public string Command { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
        public byte[] Content { get; set; } = Array.Empty<byte>();

        public string ProtocolName => "EDULINK";
        public string ProtocolVersion => "1.0";

        public EdulinkCommand() { }

        public EdulinkCommand(string input)
        {
            Parse(input);
        }

        public void Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Input cannot be null or empty.");

            using (StringReader reader = new StringReader(input))
            {
                string line;

                if ((line = reader.ReadLine()) != null)
                {
                    string[] headerParts = line.Split(new[] { ' ' }, 2, StringSplitOptions.None);
                    if (headerParts.Length > 0)
                    {
                        Command = headerParts[0];

                        string[] protocolInfo = headerParts[1].Split(new[] { "/" }, 2, StringSplitOptions.None);
                        // TODO: Check if the protocol info is valid
                    }
                }
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("= "))
                    {
                        string[] paramParts = line.Split(new[] { "= " }, 2, StringSplitOptions.None);
                        Parameters[paramParts[0]] = paramParts[1];
                    }
                    else if (!string.IsNullOrWhiteSpace(line))
                    {
                        Content = Convert.FromBase64String(line.Trim());
                    }
                }
            }
        }

        public override string ToString()
        {
            StringBuilder commandBuilder = new StringBuilder();
            commandBuilder.AppendLine($"{Command} {ProtocolName}/{ProtocolVersion}");

            foreach (KeyValuePair<string, string> parameter in Parameters)
            {
                commandBuilder.AppendLine($"{parameter.Key}= {parameter.Value}");
            }

            if (Content.Length > 0)
            {
                commandBuilder.AppendLine();
                commandBuilder.AppendLine(Convert.ToBase64String(Content));
            }

            return commandBuilder.ToString();
        }
    }
}
