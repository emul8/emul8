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
TARGET=`get_path "$PWD/target/Emul8.sln"`
CONFIGURATION="Release"
PARAMS=("")

CLEAN=false
PACKAGES=false

while getopts ":cdvpt:o:" opt; do
  case $opt in
    c)
      CLEAN=true
      ;;
    d)
      CONFIGURATION="Debug"
      ;;
    v)
      PARAMS+=(/verbosity:detailed)
      ;;
    p)
      PACKAGES=true
      ;;
    t)
      TARGET="$OPTARG"
      ;;
    o)
      OUTPUT="$OPTARG"
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
# this property should be set automatically by xbuild, but it's not...
PARAMS+=(/p:SolutionDir=`dirname $TARGET`)

if $CLEAN
then
    for conf in Debug Release
    do
        $CS_COMPILER /t:Clean /p:Configuration=$conf $TARGET ${PARAMS[@]}
        rm -fr ${OUTPUT:=`get_path $PWD/output`}/$conf
    done
    exit 0
fi

pushd ${ROOT_PATH:=.}/Tools/scripts > /dev/null
./check_weak_implementations.sh
popd > /dev/null

# Build CCTask in Release configuration
$CS_COMPILER /p:Configuration=Release `get_path $ROOT_PATH/External/cctask/CCTask.sln` > /dev/null

PARAMS+=( /p:Configuration=$CONFIGURATION)

retries=5
while [ \( ${result_code:-134} -eq 134 \) -a \( $retries -ne 0 \) ]
do
    set +e
    $CS_COMPILER /p:OutputPath=$OUTPUT/$CONFIGURATION ${PARAMS[@]} $TARGET
    result_code=$?
    set -e
    retries=$((retries-1))
done

if $PACKAGES
then
    if $ON_WINDOWS
    then
        echo "Creating packages is not supported on Windows."
        exit 1
    fi
    params="$VERSION -n"
    if [ $CONFIGURATION == "Debug" ]
    then
        params="$params -d"
    fi
    if $ON_LINUX
    then
      $ROOT_PATH/Tools/packaging/make_linux_packages.sh $params
    elif $ON_OSX
    then
      $ROOT_PATH/Tools/packaging/make_macos_packages.sh $params
    fi
fi

exit $result_code
