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
OUTPUT_DIRECTORY="target"
BINARIES_DIRECTORY="bin"

while getopts "ad:o:b:s:h" opt
do
    case "$opt" in
        a)
            BATCH_MODE=1
            ;;
        s)
            SELECTED_PROJECT="$OPTARG"
            ;;
        d)
            DIRECTORY="$OPTARG"
            ;;
        o)
            OUTPUT_DIRECTORY="$OPTARG"
            ;;
        b)
            BINARIES_DIRECTORY="$OPTARG"
            ;;
        h)
            echo "Emul8 bootstrapping script"
            echo "=========================="
            echo "Usage: $0 [-a] [-d directory] [-b directory] [-o directory] [-s csproj_file] [-h]"
            echo "  -a              batch mode, generates the 'All projects' solution without"
            echo "                  any interaction with the user"
            echo "  -d directory    location of the base directory to scan"
            echo "  -b directory    location for binaries created from generated project"
            echo "  -o directory    location of generated project files"
            echo "  -s csproj_file  location of the project file"
            echo "  -h              prints this help"
            exit 0
    esac
done

if ! [ -x "$(command -v mono)" ]
then
    echo "Mono not found. Please refer to documentation for installation instructions. Exiting!"
    exit 1
fi

# Check mono version
MONO_VERSION=`mono --version | head -n1 | cut -d' ' -f5`
MONO_VERSION_MAJOR=`echo $MONO_VERSION | cut -d'.' -f1`
if [ $MONO_VERSION_MAJOR -lt 4 -a $MONO_VERSION != "3.99.0" ]
then
    echo "Wrong mono version detected: $MONO_VERSION. Please refer to documentation for installation instructions. Exiting!"
    exit 1
fi

git submodule update --init --recursive

if [ -z "$ROOT_PATH" -a -x "$(command -v realpath)" ]; then
    # this is to support running emul8 from external directory
    ROOT_PATH="`dirname \`realpath $0\``"
fi

# Create Sandbox project
pushd ${ROOT_PATH:=.}/Misc/Sandbox > /dev/null
if [ ! -e Sandbox.csproj ]
then
  echo " >> Creating Sandbox.csproj..."
  cp Sandbox.csproj-template Sandbox.csproj
  cp SandboxMain.cs-template SandboxMain.cs
fi
popd > /dev/null

BOOTSTRAPER_DIR=$ROOT_PATH/Tools/Bootstrap
BOOTSTRAPER_BIN=$BOOTSTRAPER_DIR/bin/Release/Bootstrap.exe

CCTASK_DIR=$ROOT_PATH/External/cctask
CCTASK_BIN=$CCTASK_DIR/CCTask/bin/Release/CCTask.dll

# We build bootstrap/cctask every time in order to have the newest versions at every bootstrapping.
xbuild $BOOTSTRAPER_DIR/Bootstrap.csproj /p:Configuration=Release /nologo /verbosity:quiet || (echo "There was an error during Bootstrap compilation!" && exit 1)
xbuild $CCTASK_DIR/CCTask.sln /p:Configuration=Release /nologo /verbosity:quiet            || (echo "There was an error during CCTask compilation!"    && exit 1)

OS_NAME=`uname`

rm -f $OUTPUT_DIRECTORY/properties.csproj
if [ "$OS_NAME" == "Darwin" ]
then
  mkdir -p $OUTPUT_DIRECTORY
  cp $ROOT_PATH/Emulator/Cores/osx-properties.csproj  $OUTPUT_DIRECTORY/properties.csproj
fi

if [ $BATCH_MODE -eq 1 ]
then
    mono $BOOTSTRAPER_BIN GenerateAll --generate-entry-project --directories "${DIRECTORY:-.}" --output-directory "$OUTPUT_DIRECTORY" --binaries-directory "$BINARIES_DIRECTORY"
elif [ -n "$SELECTED_PROJECT" ]
then
    mono $BOOTSTRAPER_BIN GenerateSolution --directories "${DIRECTORY:-.}" --output-directory "$OUTPUT_DIRECTORY" --binaries-directory "$BINARIES_DIRECTORY" --main-project="$SELECTED_PROJECT"
else
    set +e
    mono $BOOTSTRAPER_BIN --interactive --generate-entry-project --directories "${DIRECTORY:-.}" --output-directory "$OUTPUT_DIRECTORY" --binaries-directory "$BINARIES_DIRECTORY"
    result=$?
    set -e
    clear
    case $result in
        0) echo "Solution file generated in $OUTPUT_DIRECTORY/Emul8.sln. Now you can run ./build.sh" ;;
        1) echo "Solution file generation cancelled." ;;
        2) echo "There was an error while generating the solution file." ;;
        3) echo "Bootstrap setup cleaned." ;;
    esac
    exit $result
fi

