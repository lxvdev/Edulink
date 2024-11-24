using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Edulink.TCPHelper.Classes
{
    public class EdulinkCommand
    {
        public string Command { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
        public byte[] Content { get; set; }

        public string ProtocolName => "EDULINK";
        public string ProtocolVersion => "1.0";

        //public EdulinkCommand(string command, Dictionary<string, string> parameters = default, byte[] content = null)
        //{
        //    Command = command;
        //    Parameters = parameters;
        //    Content = content;
        //}

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

                // If theres a first line then its a header :)
                if ((line = reader.ReadLine()) != null)
                {
                    string[] headerParts = line.Split(new[] { ' ' }, 2, StringSplitOptions.None);
                    if (headerParts.Length > 0)
                    {
                        Command = headerParts[0];

                        string[] protocolInfo = headerParts[1].Split(new[] { "/" }, 2, StringSplitOptions.None);
                        if (protocolInfo.Length > 0)
                        {
                            // Soon
                        }
                    }
                }
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("= "))
                    {
                        string[] paramParts = line.Split(new[] { "= " }, 2, StringSplitOptions.None);
                        if (paramParts.Length > 0)
                        {
                            string key = paramParts[0];
                            string value = paramParts[1];
                            Parameters[key] = value;
                        }
                    }
                    else
                    {
                        // Hope this works because im tired of this content thing
                        if (!string.IsNullOrEmpty(line.Trim()))
                        {
                            Content = Convert.FromBase64String(line.Trim());
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            StringBuilder commandBuilder = new StringBuilder();
            commandBuilder.AppendLine($"{Command} {ProtocolName}/{ProtocolVersion}");

            if (Parameters != null)
            {
                foreach (var parameter in Parameters)
                {
                    commandBuilder.AppendLine($"{parameter.Key}= {parameter.Value}");
                }
            }

            if (Content != null && Content.Length > 0)
            {
                commandBuilder.AppendLine();
                commandBuilder.AppendLine(Convert.ToBase64String(Content));
            }

            return commandBuilder.ToString();
        }
    }
}
