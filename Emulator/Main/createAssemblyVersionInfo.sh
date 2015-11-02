#!/bin/bash

# Create AssemblyVersionInfo.cs but only when the file does not exists or has different git commit
CURRENT_VERSION="`git branch | sed -n '/\* /s///p'`-`git log | head -1 | cut -d\  -f2 | head -c 8`"
if ! grep "$CURRENT_VERSION" AssemblyVersionInfo.cs > /dev/null 2>/dev/null
then
    sed -e "s;%VERSION%;`git branch | sed -n '/\* /s///p'`-`git log | head -1 | cut -d\  -f2 | head -c 8`-`date +%Y%m%d%H%M`;" AssemblyVersionInfo.template > AssemblyVersionInfo.cs
fi

