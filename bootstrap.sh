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

if [ -z "$ROOT_PATH" -a -x "$(command -v realpath)" ]; then
    # this is to support running emul8 from external directory
    ROOT_PATH="`dirname \`realpath $0\``"
fi

. ${ROOT_PATH}/Tools/common.sh

BATCH_MODE=false
KEEP_SUBMODULES=false
OUTPUT_DIRECTORY="target"
BINARIES_DIRECTORY="bin"
VERBOSE=false
EXCLUDE=""
PARAMS=()

while getopts "ad:o:b:s:hkve:" opt
do
    case "$opt" in
        a)
            BATCH_MODE=true
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
        k)
            KEEP_SUBMODULES=true
            ;;
        v)
            PARAMS+=(-v)
            VERBOSE=true
            ;;
        e)
            EXCLUDE="$OPTARG"
            ;;
        h)
            echo "Emul8 bootstrapping script"
            echo "=========================="
            echo "Usage: $0 [-a] [-d directory] [-b directory] [-o directory] [-s csproj_file] [-e exclude] [-v] [-h]"
            echo "  -a              batch mode, generates the 'All projects' solution without"
            echo "                  any interaction with the user"
            echo "  -d directory    location of the base directory to scan"
            echo "  -b directory    location for binaries created from generated project"
            echo "  -o directory    location of generated project files"
            echo "  -s csproj_file  location of the project file"
            echo "  -e exclude      list of projects to exclude from generated solution"
            echo "  -k              keep submodules intact (do not update them)"
            echo "  -v              show diagnostic messages"
            echo "  -h              prints this help"
            exit 0
    esac
done

if ! $ON_WINDOWS
then
	if ! [ -x "$(command -v mono)" ]
	then
	    echo "Mono not found. Please refer to documentation for installation instructions. Exiting!"
	    exit 1
	fi

	if ! [ -x "$(command -v mcs)" ]
	then
	    echo "mcs not found. Please refer to documentation for installation instructions. Exiting!"
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
	MCS_VERSION=`mcs --version | cut -d' ' -f5`
	if [ $MONO_VERSION != ${MCS_VERSION:0:${#MONO_VERSION}} ]
	then
	    echo "Wrong mcs version detected: $MCS_VERSION (mono version is $MONO_VERSION). Please refer to documentation for installation instructions. Exiting!"
	    exit 1
	fi
fi

if ! $KEEP_SUBMODULES
then
    git submodule update --init --recursive
fi

# Update references to Xwt
TERMSHARP_PROJECT=${ROOT_PATH:=.}/External/termsharp/TermSharp.csproj
if [ -e $TERMSHARP_PROJECT ]
then
    sed -i.bak 's/"xwt\\Xwt\\Xwt.csproj"/"..\\xwt\\Xwt\\Xwt.csproj"/'                        $TERMSHARP_PROJECT
    rm $TERMSHARP_PROJECT.bak
fi

${ROOT_PATH}/Tools/scripts/fetch_libraries.sh

BOOTSTRAPER_DIR=$ROOT_PATH/Tools/Bootstrap
BOOTSTRAPER_BIN=$BOOTSTRAPER_DIR/bin/Release/Bootstrap.exe

CCTASK_DIR=$ROOT_PATH/External/cctask

# We build bootstrap/cctask every time in order to have the newest versions at every bootstrapping.
# We need to use get_path helper function in order to resolve paths to projects correctly both on linux and windows
$CS_COMPILER `get_path $BOOTSTRAPER_DIR/Bootstrap.csproj` /p:Configuration=Release /nologo /verbosity:quiet || (echo "There was an error during Bootstrap compilation!" && exit 1)
$CS_COMPILER `get_path $CCTASK_DIR/CCTask.sln`            /p:Configuration=Release /nologo /verbosity:quiet || (echo "There was an error during CCTask compilation!"    && exit 1)

mkdir -p $OUTPUT_DIRECTORY
if $ON_OSX
then
  PROP_FILE=$ROOT_PATH/Emulator/Cores/osx-properties.csproj
elif $ON_LINUX
then
  PROP_FILE=$ROOT_PATH/Emulator/Cores/linux-properties.csproj
else
  PROP_FILE=$ROOT_PATH/Emulator/Cores/windows-properties.csproj
fi
cp $PROP_FILE $OUTPUT_DIRECTORY/properties.csproj

PARAMS+=( --directories `get_path ${DIRECTORY:-.}` --output-directory `get_path $OUTPUT_DIRECTORY` --binaries-directory `get_path $BINARIES_DIRECTORY`)
if [ ! -z $EXCLUDE ]
then
    PARAMS+=( --exclude "$EXCLUDE")
fi

if $BATCH_MODE
then
    $LAUNCHER $BOOTSTRAPER_BIN GenerateAll --generate-entry-project ${PARAMS[@]}
elif [ -n "$SELECTED_PROJECT" ]
then
    $LAUNCHER $BOOTSTRAPER_BIN GenerateSolution --main-project=`get_path $SELECTED_PROJECT` ${PARAMS[@]}
else
    set +e
    $LAUNCHER $BOOTSTRAPER_BIN --interactive --generate-entry-project ${PARAMS[@]}
    result=$?
    set -e
    if ! $VERBOSE
    then
        clear
    fi
    case $result in
        0) echo "Solution file generated in $OUTPUT_DIRECTORY/Emul8.sln. Now you can run ./build.sh" ;;
        1) echo "Solution file generation cancelled." ;;
        2) echo "There was an error while generating the solution file." ;;
        3) echo "Bootstrap setup cleaned." ;;
    esac
    exit $result
fi

