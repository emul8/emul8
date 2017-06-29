//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Emul8.Exceptions;
using Emul8.Utilities;

namespace Emul8.UserInterface
{
    public class MonitorPath
    {
        private List<string> pathEntries = new List<string>();
        private List<string> defaultPath = new List<string>();
        private readonly char[] pathSeparator = new []{';'};
        private Stack<string> workingDirectory = new Stack<string>();

        public String CurrentWorkingDirectory
        {
            get{ return workingDirectory.Peek();}
        }

        public void PushWorkingDirectory(string path)
        {
            Environment.CurrentDirectory = System.IO.Path.Combine(CurrentWorkingDirectory, path);
            workingDirectory.Push(Environment.CurrentDirectory);
        }

        public string PopWorkingDirectory()
        {
            Environment.CurrentDirectory = workingDirectory.Pop();
            return Environment.CurrentDirectory;
        }

        public IEnumerable<string> PathElements
        {
            get{ return pathEntries;}
        }

        private List<string> GetDirEntries(string path)
        {
            var split = path.Split(pathSeparator, StringSplitOptions.RemoveEmptyEntries).Distinct();
            var current = new List<string>();
            foreach (string entry in split)
            {
                var curentry = entry;//System.IO.Path.Combine(Environment.CurrentDirectory, entry);
                if (curentry.StartsWith("./"))
                {
                    curentry = curentry.Length > 2 ? curentry.Substring(2) : ".";
                }
                if (!Directory.Exists(curentry))
                {
                    throw new RecoverableException(String.Format("Entry {0} does not exist or is not a directory.", curentry));
                }
                current.Add(curentry);
            }
            return current;
        }

        public String Path
        {
            get
            {
                return pathEntries.Aggregate((x,y) => x + ';' + y);
            }
            set
            {
                pathEntries = GetDirEntries(value);
            }
        }

        public String DefaultPath
        {
            get{ return defaultPath.Aggregate((x,y) => x + ';' + y);}
            private set
            {
                defaultPath = GetDirEntries(value);
            }
        }

        public void Append(string path)
        {
            pathEntries.AddRange(GetDirEntries(path));
        }

        public void Reset()
        {
            Path = DefaultPath;
        }

        public MonitorPath()
        {
            string rootDirectory;
            string rootFileLocation;
            if(Misc.TryGetEmul8Directory(out rootDirectory, out rootFileLocation))
            {
                defaultPath = new List<string> { rootFileLocation, rootDirectory };
            }
            else
            {
                DefaultPath = ".";
            }
            Path = DefaultPath;
            workingDirectory.Push(Environment.CurrentDirectory);
        }


    }
}

