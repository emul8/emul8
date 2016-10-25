//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Runtime.InteropServices;
using System.Linq;
using ELFSharp;
using System.Collections.Generic;
using ELFSharp.ELF;
using ELFSharp.ELF.Sections;
using System.IO;
using System.Text;
using System.ComponentModel;

namespace Emul8.Utilities
{
    public static class SharedLibraries
    {
        /// <summary>
        /// Loads the given library to memory.
        /// </summary>
        /// <returns>
        /// The address of the loaded library.
        /// </returns>
        /// <param name='path'>
        /// Path to the library file.
        /// </param>
        /// <param name='relocation'>
        /// Whether relocation should be done immediately after loading or being deferred (lazy).
        /// The default option is to relocate immediately.
        /// </param>
        public static IntPtr LoadLibrary(string path, Relocation relocation = Relocation.Now)
        {
            IntPtr address;
            if (!TryLoadLibrary(path, out address, relocation))
            {
                HandleError("opening");
            }
            return address;
        }

        public static bool TryLoadLibrary(string path, out IntPtr address, Relocation relocation = Relocation.Now)
        {
#if EMUL8_PLATFORM_WINDOWS
            address = WindowsLoadLibrary(path);
#else
            //HACK: returns 0 on first call, somehow
            dlerror();
            address = dlopen(path, (int)relocation);
#endif
            return address != IntPtr.Zero;
        }

        /// <summary>
        /// Unloads the library and frees memory taken by it.
        /// </summary>
        /// <param name='address'>
        /// Address of the library, returned by the <see cref="LoadLibrary" /> function.
        /// </param>
        public static void UnloadLibrary(IntPtr address)
        {
#if EMUL8_PLATFORM_WINDOWS
            var result = WindowsCloseLibrary(address);
            if (!result)
            {
                HandleError("unloading");
            }
#else
            var result = dlclose(address);
            if (result != 0)
            {
                HandleError("unloading");
            }
#endif
        }

        /// <summary>
        /// Gets all exported symbol names for a given library.
        /// </summary>
        /// <returns>
        /// Exported symbol names.
        /// </returns>
        /// <param name='path'>
        /// Path to a library file.
        /// </param>
        /// <remarks>
        /// Currently it works only with ELF files.
        /// </remarks>
        public static IEnumerable<string> GetAllSymbols(string path)
        {
            ELFSharp.MachO.MachO machO;
            if(ELFSharp.MachO.MachOReader.TryLoad(path, out machO) == ELFSharp.MachO.MachOResult.OK)
            {
                var machoSymtab = machO.GetCommandsOfType<ELFSharp.MachO.SymbolTable>().Single();
                return machoSymtab.Symbols.Select(x => x.Name.TrimStart('_'));
            }
            ELFSharp.PE.PE pe;
            if(ELFSharp.PE.PEReader.TryLoad(path, out pe))
            {
                return pe.GetExportedSymbols();
            }
            var elf = ELFReader.Load(path);
            var symtab = (ISymbolTable)elf.GetSection(".symtab");
            return symtab.Entries.Select(x => x.Name);
        }

        /// <summary>
        /// Gets the address of the symbol with a given name.
        /// </summary>
        /// <returns>
        /// The address of the symbol in memory.
        /// </returns>
        /// <param name='libraryAddress'>
        /// Address to library returned by the <see cref="LoadLibrary" /> function.
        /// </param>
        /// <param name='name'>
        /// Name of the symbol to retrieve.
        /// </param>
        public static IntPtr GetSymbolAddress(IntPtr libraryAddress, string name)
        {
#if EMUL8_PLATFORM_WINDOWS
            var address = WindowsGetSymbolAddress(libraryAddress, name);
#else
            var address = dlsym(libraryAddress, name);
#endif
            if (address == IntPtr.Zero)
            {
                HandleError("getting symbol from");
            }
            return address;
        }

        /// <summary>
        /// Verifies the existance of the shared libraries in the application
        /// base directory.
        /// </summary>
        /// <returns>
        /// True if the library was found.
        /// <returns>
        /// <param name='name'>
        /// Name of the shared library.
        /// </param>
        public static bool Exists(string name)
        {
            IntPtr lib;
            if(TryLoadLibrary(AppDomain.CurrentDomain.BaseDirectory + name, out lib, Relocation.Lazy))
            {
                UnloadLibrary(lib);
                return true;
            }
            return false;
        }

        private static void HandleError(string operation)
        {
            string message;
#if EMUL8_PLATFORM_WINDOWS
            message = new Win32Exception(Marshal.GetLastWin32Error()).Message;
#else
            var messagePtr = dlerror();
            if (messagePtr == IntPtr.Zero)
            {
                message = "unknown error";
            }
            else
            {
                message = Marshal.PtrToStringAnsi(messagePtr);
            }
#endif
            throw new InvalidOperationException(string.Format("Error while {1} dynamic library: {0}", message, operation));
        }

#if EMUL8_PLATFORM_WINDOWS
        [DllImport("kernel32", SetLastError=true, CharSet = CharSet.Ansi, EntryPoint="LoadLibrary")]
        static extern IntPtr WindowsLoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

        [DllImport("kernel32.dll", EntryPoint="GetProcAddress")]
        public static extern IntPtr WindowsGetSymbolAddress(IntPtr hModule, string symbolName);

        [DllImport("kernel32.dll", EntryPoint="FreeLibrary")]
        public static extern bool WindowsCloseLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", EntryPoint="GetLastError")]
        public static extern UInt32 WindowsGetLastError();
#else
        [DllImport("dl")]
        private static extern IntPtr dlopen(string file, int mode);

        [DllImport("dl")]
        private static extern IntPtr dlerror();

        [DllImport("dl")]
        private static extern IntPtr dlsym(IntPtr handle, string name);

        [DllImport("dl")]
        private static extern int dlclose(IntPtr handle);
#endif
    }

    public enum Relocation
    {
        Lazy = 1,
        Now = 2
    }
}

