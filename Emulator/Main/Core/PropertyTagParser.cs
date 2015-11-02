//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using System.Text;

namespace Emul8.Core
{
    public class PropertyTagParser
    {
        public PropertyTagParser(string[] lines)
        {
            buffer = lines;
            current = 0;
        }

        public Tuple<string, string> GetNextTag()
        {
            while(current < buffer.Length && String.IsNullOrWhiteSpace(buffer[current]))
            {
                current++;
            }

            if(current < buffer.Length && buffer[current].StartsWith(":"))
            {
                string key = string.Empty;
                string value = string.Empty;

                var end = buffer[current].Substring(1).IndexOf(":");
                if(end != -1)
                {
                    key = buffer[current].Substring(1, end);
                    value = buffer[current].Substring(end + 2).Trim();

                    if(String.IsNullOrEmpty(value))
                    {
                        var bldr = new StringBuilder();
                        while (true)
                        {
                            current++;
                            if(!(buffer[current].StartsWith(" ") || buffer[current].StartsWith("\t")))
                            {
                                break;
                            }
                            bldr.Append(buffer[current].Trim()).Append("\n\r");
                        }
                        value = bldr.ToString();
                    }
                }

                current++;
                return Tuple.Create(key, value);
            }

            return null;
        }

        private string[] buffer;
        private int current;
    }
}

