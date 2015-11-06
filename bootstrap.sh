#!/bin/bash
#
# ==========================
# Emul8 bootstrapping script
# ==========================
#
# This script is used to create the <<Emul8.sln>> file
# with references to the selected modules:
#
# * UI module
# * extension libraries with peripherals
# * plugins
# * tests
# * other projects

set -e

BATCH_MODE=0
while getopts "ad:h" opt
do
    case "$opt" in
        a)
            BATCH_MODE=1
            ;;
        d)
            DIRECTORY="$OPTARG"
            ;;
        h)
            echo "Emul8 bootstrapping script"
            echo "=========================="
            echo "Usage: $0 [-a] [-d directory] [-h]"
            echo "  -a            batch mode, generates the 'All projects' solution without"
            echo "                any interaction with the user"
            echo "  -d directory  location of the base directory"
            echo "  -h            prints this help"
            exit 0
    esac
done

# Check mono version
MONO_VERSION=`mono --version | sed -n -r 's/.* version ([0-9.]+) \(.*/\1/p'`
MONO_VERSION_MAJOR=`echo $MONO_VERSION | sed -n 's/[^0-9]*\([0-9]*\).*/\1/p'`
if [ $MONO_VERSION_MAJOR -lt 4 -a $MONO_VERSION != "3.99.0" ]
then
    echo "Wrong mono version detected: $MONO_VERSION. Exiting!"
    exit 1
fi

git submodule update --init --recursive

# Create Sandbox project
pushd Misc/Sandbox > /dev/null
if [ ! -e Sandbox.csproj ]
then
  echo " >> Creating Sandbox.csproj..."
  cp Sandbox.csproj-template Sandbox.csproj
  cp SandboxMain.cs-template SandboxMain.cs
fi
popd > /dev/null

BOOTSTRAPER_DIR=./Tools/Bootstrap
BOOTSTRAPER_BIN=$BOOTSTRAPER_DIR/bin/Release/Bootstrap.exe

CCTASK_DIR=./External/cctask
CCTASK_BIN=$CCTASK_DIR/CCTask/bin/Release/CCTask.dll

# We build bootstrap/cctask every time in order to have the newest versions at every bootstrapping.
xbuild $BOOTSTRAPER_DIR/Bootstrap.csproj /p:Configuration=Release /nologo /verbosity:quiet || (echo "There was an error during Bootstrap compilation!" && exit 1)
xbuild $CCTASK_DIR/CCTask.sln /p:Configuration=Release /nologo /verbosity:quiet            || (echo "There was an error during CCTask compilation!"       && exit 1)

if [ $BATCH_MODE -eq 1 ]
then
    mono $BOOTSTRAPER_BIN GenerateAll --directories "${DIRECTORY:-.}"
else
    set +e
    mono $BOOTSTRAPER_BIN --interactive --directories "$DIRECTORY"
    result=$?
    set -e
    clear
    case $result in
        0) echo "Solution file generated in target/Emul8.sln. Now you can run ./build.sh" ;;
        1) echo "Solution file generation cancelled." ;;
        2) echo "There was an error while generating the solution file." ;;
        3) echo "Bootstrap setup cleaned." ;;
    esac
    exit $result
fi
