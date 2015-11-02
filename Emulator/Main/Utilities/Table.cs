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

namespace Emul8.Utilities
{
    public class Table
    {
        public Table()
        {
            content = new List<string[]>();
        }

        private Table(List<string[]> content)
        {
            this.content = content;
        }

        public Table AddRow(params string[] elements)
        {
            content.Add(elements);
            return this;
        }

        public Table AddRows<TEntry>(IEnumerable<TEntry> source, params Func<TEntry, string>[] selectors)
        {
            foreach(var element in source)
            {
                content.Add(selectors.Select(x => x(element)).ToArray());
            }
            return this;
        }

        public string[,] ToArray()
        {
            var width = content.Max(x => x.Length);
            var height = content.Count;
            var result = new string[height, width];
            for(var i = 0; i < height; i++)
            {
                for(var j = 0; j < width; j++)
                {
                    result[i, j] = content[i].Length > j ? content[i][j] : string.Empty;
                }
            }
            return result;
        }

        private readonly List<string[]> content;
    }
}

