//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Emul8.Bootstrap
{
    public class PathHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AntBootstrap.PathHelper"/> class.
        /// It calculates common prefix path of all directories provided as an argument.
        /// </summary>
        public PathHelper(IEnumerable<string> workingDirs)
        {
            var segmentedWorkingDirs = workingDirs.Select(x => x.Split(new [] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)).ToArray();
            var segmentsCount = segmentedWorkingDirs.Min(x => x.Length);
            
            var prefixSegments = new List<string>();
            for (int i = 0; i < segmentsCount; i++)
            {
                if(segmentedWorkingDirs.Select(x => x[i]).Distinct().Count() != 1)
                {
                    break;
                }
                
                prefixSegments.Add(segmentedWorkingDirs[0][i]);
            }
            
            workingDirectory = Path.GetFullPath(Path.DirectorySeparatorChar + string.Join(Path.DirectorySeparatorChar.ToString(), prefixSegments)) + Path.DirectorySeparatorChar;
        }

        /// <summary>
        /// Calculates path relative to working directory calculated in constructor.
        /// </summary>
        public string GetRelativePath(string filespec)
        {
            return Uri.UnescapeDataString(new Uri(workingDirectory).MakeRelativeUri(new Uri(filespec)).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        /// <summary>
        /// Checks if two paths are the same assuming they are relative to working directory calculated in constructor.
        /// </summary>
        public bool AreSame(string patha, string pathb)
        {
            return (Path.IsPathRooted(patha) ? patha : Path.Combine(workingDirectory, patha)) ==
            (Path.IsPathRooted(pathb) ? pathb : Path.Combine(workingDirectory, pathb));
        }
        
        private readonly string workingDirectory;
    }
}

