//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Nini.Config;
using System.IO;
using System;
using Emul8.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Emul8.Utilities
{
    public sealed class ConfigurationManager
    {
        public static ConfigurationManager Instance { get; private set; }

        static ConfigurationManager()
        {
            Instance = new ConfigurationManager();
        }

        private ConfigurationManager()
        {
            Config = new ConfigSource();
        }

        public T Get<T>(string group, string name, T defaultValue)
        {
            T result;
            if(!TryFindInCache(group, name, out result))
            {
                var config = VerifyValue(group, name, defaultValue);
                if(typeof(T) == typeof(int))
                {
                    result = (T)(object)config.GetInt(name);
                }
                else if(typeof(T) == typeof(string))
                {
                    result = (T)(object)config.GetString(name);
                }
                else if(typeof(T) == typeof(bool))
                {
                    result = (T)(object)config.GetBoolean(name);
                }
                else if(typeof(T).IsEnum)
                {
                    var value = Get<string>(group, name, defaultValue.ToString());
                    if(!Enum.IsDefined(typeof(T), value))
                    {
                        throw new ArgumentException(String.Format("Could not apply value {0} for type {1}. Verify your configuration file {5} in section {2}->{3}. Available options are: {4}.", 
                                    value, typeof(T).Name, group, name, Enum.GetNames(typeof(T)).Aggregate((x, y) => x + ", " + y), Config.FileName));
                    }
                    result = (T)Enum.Parse(typeof(T), value);
                }
                else
                {
                    throw new ArgumentException("Unsupported type: " + typeof(T));
                }
                AddToCache(group, name, result);
            }
            return result;
        }

        public void SetNonPersistent<T>(string group, string name, T value)
        {
            AddToCache(group, name, value);
        }

        public void Set<T>(string group, string name, T value)
        {
            var config = VerifyValue(group, name, value);
            AddToCache(group, name, value);
            config.Set(name, value);
        }

        private IConfig VerifyValue(string group, string name, object defaultValue)
        {
            if(defaultValue == null)
            {
                throw new ArgumentException("Default value cannot be null", "defaultValue");
            }
            var config = VerifyGroup(group);
            if(!config.Contains(name))
            {
                config.Set(name, defaultValue);
            }
            return config;
        }

        private IConfig VerifyGroup(string group)
        {
            var config = Config.Source.Configs[group];
            return config ?? Config.Source.AddConfig(group);
        }

        private void AddToCache<T>(string group, string name, T value)
        {
            cachedValues[Tuple.Create(group, name)] = value;
        }

        private bool TryFindInCache<T>(string group, string name, out T value)
        {
            value = default(T);
            object obj;
            var result = cachedValues.TryGetValue(Tuple.Create(group, name), out obj);
            if(result)
            {
                value = (T)obj;
            }
            return result;
        }

        private readonly Dictionary<Tuple<string, string>, object> cachedValues = new Dictionary<Tuple<string, string>, object>();
    
        private readonly ConfigSource Config;
    }

    public class ConfigSource
    {
        public IConfigSource Source
        {
            get
            {
                if(source == null)
                {
                    if(File.Exists(FileName))
                    {
                        try
                        {
                            source = new IniConfigSource(FileName);
                        }
                        catch(Exception)
                        {
                            Logger.Log(LogLevel.Warning, "Configuration file {0} exists, but it cannot be read.", FileName);
                        }
                    }
                    else
                    {
                        source = new IniConfigSource();
                        source.Save(FileName);
                    }
                }
                source.AutoSave = true;
                return source;
            }
        }

        public string FileName
        {
            get
            {
                return Path.Combine(Misc.GetUserDirectory(), "config");
            }
        }

        private IniConfigSource source;
    }
}

