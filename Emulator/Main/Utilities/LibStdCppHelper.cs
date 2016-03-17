//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Emul8.Utilities
{
    public static class LibStdCppHelper
    {
        public static string GetLibStdCppPath()
        {
            string result;
            foreach(var gccPath in gccPaths)
            {
                if(TryFindLibStdCppInDir(gccPath, out result))
                {
                    return result;
                }
            }
            return null;
        }

        public static string LibStdCppName
        {
            get
            {
                return "libstdc++.so";
            }
        }

        private static bool TryFindLibStdCppInDir(string gccPath, out string path)
        {
            path = null;
            if(!Directory.Exists(gccPath))
            {
                return false;
            }
            var directory = new DirectoryInfo(gccPath);
            //If the architecture is 64bit, use x86_64* prefix for gcc. Otherwise use any other
            var pointerSizePrefix = Marshal.SizeOf(typeof(IntPtr)) * 8;
            DirectoryInfo[] arch;
            if(pointerSizePrefix == 64)
            {
                arch = directory.GetDirectories(x64Prefix + "*");
            }
            else
            {
                arch = directory.GetDirectories().Where(x => !x.Name.StartsWith(x64Prefix, StringComparison.Ordinal)).ToArray();
            }
            if(arch == null || arch.Length == 0)
            {
                return false;
            }
            var directoriesToSearch = arch.Concat(new []{ arch.SelectMany(x => x.GetDirectories()).OrderByDescending(x => x.Name).FirstOrDefault() }).ToList();
            if(directoriesToSearch == null || directoriesToSearch.Count == 0)
            {
                return false;
            }
            foreach(var currentDirectory in directoriesToSearch)
            {
                var libstd = currentDirectory.GetFiles(LibStdCppName + "*").FirstOrDefault();
                if(libstd != null)
                {
                    path = libstd.FullName;
                    return true;
                }
            }
            return false;
        }

        private static readonly string[] gccPaths = {"/usr/lib/", "/usr/lib/gcc","/usr/lib64/gcc", "/usr/lib32/gcc", "/usr/local/lib/gcc"};
        private const string x64Prefix = "x86_64";

        public delegate string CxaDemangleDelegate(String symbol, IntPtr p1, IntPtr p2, out int result);
    }
}

