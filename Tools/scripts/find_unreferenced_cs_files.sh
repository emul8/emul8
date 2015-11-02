#!/bin/bash

CS_FILES=csfiles.tmp
REFERENCED_CS_FILES=refcsfiles.tmp

if [ -f $CS_FILES ]; then
    echo Error: $CS_FILES file already exists. Please remove it manually before running this script.
    exit 1
fi

if [ -f $REFERENCED_CS_FILES ]; then
    echo Error: $REFERENCED_CS_FILES file already exists. Please remove it manually before running this script.
    exit 1
fi

for i in $( find ../../ -name '*.csproj' ); do
    CSPROJ_PATH=$(echo $i | cut -f 2- -d "/" | rev | cut -f 2- -d "/" | rev)
    grep Compile\ Include < $i | cut -d "\"" -f 2 | sed 's,\\,/,g' | sed "s,^,$CSPROJ_PATH/,g" >> $REFERENCED_CS_FILES
done

find -name '*.cs' | cut -f 2- -d "/" | grep -v \\.AssemblyAttribute\\.cs$ > $CS_FILES

UNREFERENCED_FILES=$(grep -vwF -f $REFERENCED_CS_FILES $CS_FILES)

if [ -n "$UNREFERENCED_FILES" ]; then
    echo "Unreferenced .cs files:"
    echo "$UNREFERENCED_FILES"
else
    echo "All .cs files present in this directory are referenced in a csproj file."
fi

rm -f $REFERENCED_CS_FILES $CS_FILES
