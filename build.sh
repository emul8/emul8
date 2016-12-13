#!/bin/bash
#
# Emulator build script
#
# Copyright (c) Antmicro
# Copyright (c) Realtime Embedded
#

set -e

if [ -z "$ROOT_PATH" -a -x "$(command -v realpath)" ]; then
    # this is to support running emul8 from external directory
    ROOT_PATH="`dirname \`realpath $0\``"
fi

. ${ROOT_PATH}/Tools/common.sh

VERSION=1.0
TARGET=`get_path "./target/Emul8.sln"`

export CLEAN=false
export INSTALL=false
export DEBUG=false
export VERBOSE=false
export PACKAGES=false

while getopts ":cidvp" opt; do
  case $opt in
    c)
      CLEAN=true
      ;;
    i)
      INSTALL=true
      ;;
    d)
      DEBUG=true
      ;;
    v)
      VERBOSE=true
      ;;
    p)
      PACKAGES=true
      ;;
    \?)
      echo "Invalid option: -$OPTARG" >&2
      ;;
  esac
done

if [ ! -f $TARGET ]
then
    ./bootstrap.sh
fi

if ! $CLEAN
then
  pushd ${ROOT_PATH:=.}/Tools/scripts > /dev/null
  ./check_weak_implementations.sh
  popd > /dev/null
fi

# Build CCTask in Release configuration
$CS_COMPILER /p:Configuration=Release `get_path $ROOT_PATH/External/cctask/CCTask.sln` > /dev/null

# Build Termsharp in Release configuration
$CS_COMPILER /p:Configuration=Release `get_path $ROOT_PATH/External/TermsharpConsole/TermsharpConsole.sln`

if $CLEAN
then
    $CS_COMPILER /t:Clean /p:Configuration=Debug $TARGET
    $CS_COMPILER /t:Clean /p:Configuration=Release $TARGET
    rm -fr $ROOT_PATH/output
    exit 0
fi

if $DEBUG
then
    CONFIGURATION="Debug"
else
    CONFIGURATION="Release"
fi

PARAMS=" /p:Configuration=$CONFIGURATION"

if $VERBOSE
then
    PARAMS="$PARAMS /verbosity:detailed"
fi

retries=5
while [ \( ${result_code:-134} -eq 134 \) -a \( $retries -ne 0 \) ]
do
    set +e
    $CS_COMPILER /p:OutputPath=`get_path $PWD/output/$CONFIGURATION` $PARAMS $TARGET $ADDITIONAL_PARAM
    result_code=$?
    set -e
    retries=$((retries-1))
done

if $INSTALL
then
    if $ON_WINDOWS
    then
        echo "Installing using this script is not supported on Windows."
        exit 1
    fi
    INSTALLATION_PATH="/usr/local/bin/emul8"
    echo "Installing Emul8 in: $INSTALLATION_PATH"
    sudo ln -sf $ROOT_PATH/run.sh $INSTALLATION_PATH
fi

if $PACKAGES
then
    params="$VERSION -n"
    if $DEBUG
    then
        $params="$params -d"
    fi
    $ROOT_PATH/Tools/packaging/make_linux_packages.sh $params
fi

exit $result_code
