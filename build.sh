#!/bin/bash
#
# Emulator build script
#
# Copyright (c) Antmicro
# Copyright (c) Realtime Embedded
#

set -e

TARGET="./target/Emul8.sln"

export CLEAN=false
export INSTALL=false
export DEBUG=false
export VERBOSE=false

while getopts ":cidv" opt; do
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
    \?)
      echo "Invalid option: -$OPTARG" >&2
      ;;
  esac
done

if [ ! -f "target/Emul8.sln" ]
then
    ./bootstrap.sh
fi

if ! $CLEAN
then
  pushd Tools/scripts > /dev/null
  ./check_weak_implementations.sh
  popd > /dev/null
fi

# Build CCTask in Release configuration
xbuild /p:Configuration=Release External/cctask/CCTask.sln > /dev/null

if $CLEAN
then
    xbuild /t:Clean /p:Configuration=Debug $TARGET
    xbuild /t:Clean /p:Configuration=Release $TARGET
    rm -fr $ROOT_PATH/output
    exit 0
fi

CONFIGURATION=""
if $DEBUG
then
    CONFIGURATION=" /p:Configuration=Debug"
else
    CONFIGURATION=" /p:Configuration=Release"
fi

if $VERBOSE
then
    CONFIGURATION="$CONFIGURATION /verbosity:detailed"
fi

retries=5
while [ \( ${result_code:-134} -eq 134 \) -a \( $retries -ne 0 \) ]
do
    set +e
    xbuild $CONFIGURATION $TARGET
    result_code=$?
    set -e
    retries=$((retries-1))
done

if $INSTALL
then
    INSTALLATION_PATH="/usr/local/bin/emul8"
    echo "Installing Emul8 in: $INSTALLATION_PATH"
    sudo ln -sf $PWD/run.sh $INSTALLATION_PATH
fi

exit $result_code
