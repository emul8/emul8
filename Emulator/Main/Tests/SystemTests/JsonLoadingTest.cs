//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using NUnit.Framework;
using Emul8.Utilities;
using System.IO;
using System.Linq;
using Emul8.Config.Devices;
using Emul8.Core;
using System.Collections.Generic;

namespace Emul8.SystemTests
{
    [TestFixture]
    public class JsonLoadingTest
    {
        [Test, TestCaseSource("GetJsons")]
        public void LoadAllJsons(string json)
        {
            var machine = new Machine();
            new DevicesConfig(ReadFileContents(json), machine);
        }

        private string ReadFileContents(string filename)
        {
            if(!File.Exists(filename))
            {
                throw new ArgumentException(string.Format(
                    "Cannot load devices configuration from file {0} as it does not exist.",
                    filename
                )
                );
            }
            
            string text = "";
            using(TextReader tr = File.OpenText(filename))
            {
                text = tr.ReadToEnd();
            }
	    return text;
        }

        private static IEnumerable<string> GetJsons()
        {
            string emul8Dir;
            if(!Misc.TryGetEmul8Directory(out emul8Dir))
            {
                throw new ArgumentException("Couldn't get Emul8 directory.");
            }
            TypeManager.Instance.Scan(emul8Dir);

            return Directory.GetFiles(emul8Dir, "*.json", SearchOption.AllDirectories).Where(x => x.Contains(Path.Combine("platforms", "cpus")));
        }
    }
}

