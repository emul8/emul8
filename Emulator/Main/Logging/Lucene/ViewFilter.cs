//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using Emul8.Logging;
using System.Linq;
using Emul8.Logging.Backends;

namespace Emul8.Logging.Lucene
{
    public class ViewFilter
    {
        public IEnumerable<LogLevel> LogLevels { get; set; }
        public IEnumerable<string> Sources { get; set; }
        public string CustomFilter { get; set; }

        public string GenerateQueryString()
        {
            var filters = new List<string>();
            if(LogLevels != null && LogLevels.Any())
            {
                var levelsString = String.Join(" OR ", LogLevels.Select(x => string.Format("{0}:{1}", LuceneLoggerBackend.TypeFiledName, x)));
                filters.Add(levelsString);
            }

            if(Sources != null && Sources.Any())
            {
                var sourcesString = String.Join(" OR ", Sources.Select(x => string.Format("{0}:{1}", LuceneLoggerBackend.SourceFieldName, x)));
                filters.Add(sourcesString);
            }

            if(!string.IsNullOrEmpty(CustomFilter))
            {
                filters.Add(CustomFilter);
            }

            return string.Join(" AND ", filters.Select(x => string.Format("({0})", x)));
        }
    }
}

