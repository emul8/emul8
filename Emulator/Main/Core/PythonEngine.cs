//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Microsoft.Scripting.Hosting;
using IronPython.Hosting;
using IronPython.Modules;
using Antmicro.Migrant.Hooks;
using System.Collections.Generic;
using System.Linq;
using IronPython.Runtime;
using System;
using Antmicro.Migrant;

namespace Emul8.Core
{
    public abstract class PythonEngine
    {
        #region Python Engine

        private static readonly ScriptEngine _Engine = Python.CreateEngine();
        protected ScriptEngine Engine { get { return PythonEngine._Engine; } }

        #endregion

        [Transient]
        protected ScriptScope Scope;

        private readonly string[] Imports =
        {   
            "import clr",
            "clr.AddReference('Emulator')",
            "import Emul8",
            "import System",
            "import time",
            "import sys",
            "import Emul8.Logging.Logger",
            "clr.ImportExtensions(Emul8.Logging.Logger)",
            "import Emul8.Logging.LogLevel as LogLevel"
        };

        protected virtual string[] ReservedVariables 
        { 
            get 
            { 
                return new []
                {
                    "__doc__",
                    "__builtins__",
                    "Emul8",
                    "System",
                    "cpu",
                    "clr",
                    "sysbus",
                    "time",
                    "__file__",
                    "__name__",
                    "sys",
                    "LogLevel"
                };
            } 
        }

        protected PythonEngine()
        {
            InnerInit();
        }

        private void InnerInit()
        {
            Scope = Engine.CreateScope();
            PythonTime.localtime();

            var imports = Engine.CreateScriptSourceFromString(Aggregate(Imports));
            imports.Execute(Scope);
        }

        protected virtual void Init()
        {
            InnerInit();
        }

        #region Serialization

        [PreSerialization]
        private void BeforeSerialization()
        {
            var variablesToSerialize = Scope.GetVariableNames().Except(ReservedVariables);
            variables = new Dictionary<string, object>();
            foreach(var variable in variablesToSerialize)
            {
                var value = Scope.GetVariable<object>(variable);
                if(value.GetType() == typeof(PythonModule))
                {
                    continue;
                }
                variables[variable] = value;
            }
        }

        [PostSerialization]
        private void AfterSerialization()
        {
            variables = null;
        }

        [PostDeserialization]
        private void AfterDeserialization()
        {
            Init();
            foreach(var variable in variables)
            {
                Scope.SetVariable(variable.Key, variable.Value);
            }
            variables = null;
        }

        private Dictionary<string, object> variables;

        #endregion

        #region Helper methods

        protected static string Aggregate(string[] array)
        {
            return array.Aggregate((prev, curr) => string.Format("{0}{1}{2}", prev, Environment.NewLine, curr));
        }

        #endregion
    }
}

