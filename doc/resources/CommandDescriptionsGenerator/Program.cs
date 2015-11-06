//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.UserInterface;
using Emul8.UserInterface.Commands;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;

namespace Emul8.CommandDescriptionsGenerator
{
    class MainClass
    {
        public static Dictionary<string,string[]> additionalExplanations = new Dictionary<string, string[]>
        {
            {"load",new []{
                    "**load** <filename>",
                    "   Loads state from file."
                }
            },
            {"save",new []{
                    "**save** <filename>",
                    "   Saves current state to file."
                }
            },
            {"logLevel",new []{
                    "===== =======",
                    "Level Name",
                    "===== =======",
                    "-1    NOISY",
                    "0     DEBUG",
                    "1     INFO",
                    "2     WARNING",
                    "3     ERROR",
                    "===== ======="
                }
            },
            {"path", new []{
                    "**path set** <PATH>",
                    "   Set ``PATH`` to the given value.",
                    "",
                    "**path add** <PATH>",
                    "   Append the given value to ``PATH``.",
                    "",
                    "**path reset**",
                    "   Reset ``PATH`` to it's default value."
                }
            },
            {"mach",new []{
                    "**mach set** <name>",
                    "   Enable the given machine.",
                    "",
                    "**mach add** <name>",
                    "   Create a new machine with the given name.",
                    "",
                    "**mach rem** <name>",
                    "   Remove a machine.",
                    "",
                    "**mach create**",
                    "   Create a new machine with a generic name and switch to it.",
                    "",
                    "**mach clear**",
                    "   Clear the current selection."
                }
            },
            {"numbersMode",new []{
                    "Options:",
                    "",
                    "* Hexadecimal",
                    "* Decimal",
                    "* Both"
                }
            },
            {"start", new []{
                    "**start <PATH>**",
                    "   just like :term:`include \\<PATH\\> <include>`, but also start all machines created in the script."
                }
            },
            {"using", new []{
                    "**using -**",
                    "   Clear all previous **using** calls",
                    "",
                    "Example: ``using sysbus.gpioPortA``"
                }
            }
        };

        public static void Main(string[] args)
        {
            var monitor = new Monitor();
            var prop = typeof(Monitor).GetProperty("Commands", BindingFlags.NonPublic | BindingFlags.Instance);
            var commands = (HashSet<Command>)prop.GetValue(monitor, null);
            using(var writer = new StreamWriter(@"doc/source/command_descriptions.rst"))
            {
                writer.WriteLine(".. glossary::");
                writer.WriteLine();
                foreach(var command in commands.OrderBy(x => x.Name))
                {
                    writer.WriteLine("   {0}", command.Name);
                    writer.WriteLine("      {0}", command.Description);
                    if(command.AlternativeNames.Count() > 0)
                    {
                        writer.WriteLine();
                        writer.WriteLine("      short: **{0}**", command.AlternativeNames.Aggregate((x,y) => x + ", " + y));
                    }
                    if(additionalExplanations.ContainsKey(command.Name))
                    {
                        writer.WriteLine();
                        var lines = additionalExplanations[command.Name];
                        foreach (var line in lines) {
                            writer.WriteLine("{0}{1}", line.Length > 0 ? "      " : "",  line);
                        }
                    }
                    writer.WriteLine();
                }
            }
        }
    }
}
