#!/bin/bash

# Create AssemblyVersionInfo.cs but only when the file does not exists or has different git commit
CURRENT_VERSION="`git rev-parse --short=8 HEAD`"
if ! grep "$CURRENT_VERSION" AssemblyVersionInfo.cs > /dev/null 2>/dev/null
then
    sed -e "s;%VERSION%;$CURRENT_VERSION-`date +%Y%m%d%H%M`;" AssemblyVersionInfo.template > AssemblyVersionInfo.cs
fi

