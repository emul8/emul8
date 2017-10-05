#!/bin/bash

set -e
set -u

REMOTE=https://github.com/antmicro/emul8-libraries.git
CURRENT_PATH="`dirname \"\`realpath "$0"\`\"`"
GUARD=`realpath --relative-to="$ROOT_PATH" "$CURRENT_PATH/../../External/.emul8_libs_fetched"`
EMUL8_LIBRARIES_DIR="$CURRENT_PATH/../../External/emul8-libraries"

source "$CURRENT_PATH/../common.sh"

if [ -e "$GUARD" ]
then
    top_ref=`git ls-remote -h $REMOTE master | cut -f1`
    pushd "$EMUL8_LIBRARIES_DIR" >/dev/null
    cur_ref=`git rev-parse HEAD`
    master_ref=`git rev-parse master`
    if [ $master_ref != $cur_ref ]
    then
        echo "The Emul8 libraries repository is not on the local master branch. This situation should be handled manually."
        exit
    fi
    popd >/dev/null
    if [ $top_ref == $cur_ref ]
    then
        echo "Required Emul8 libraries already downloaded. To repeat the process remove $GUARD file."
        exit
    fi
    echo "Required Emul8 libraries are available in a new version. The libraries will be redownloaded..."
fi

rm -rf "$EMUL8_LIBRARIES_DIR" "$CURRENT_PATH"/../../External/{Lib,Tools} "$CURRENT_PATH/../../Emulator/LLVMDisassembler/Resources/"

git clone $REMOTE "`realpath --relative-to="$PWD" "$EMUL8_LIBRARIES_DIR"`"

TOOLS_TARGET_DIR="$CURRENT_PATH/../../External/Tools"
LIB_TARGET_DIR="$CURRENT_PATH/../../External/Lib"

if $ON_LINUX || $ON_OSX
then
    ln -s "$EMUL8_LIBRARIES_DIR/Lib" "$LIB_TARGET_DIR"
    ln -s "$EMUL8_LIBRARIES_DIR/Tools" "$TOOLS_TARGET_DIR"
elif $ON_WINDOWS
then
    FIRST=$(sed 's:/:\\:g' <<< `cygpath -mw "$TOOLS_TARGET_DIR"`)
    SECOND=$(sed 's:/:\\:g' <<< `cygpath -mw "$EMUL8_LIBRARIES_DIR/Tools"`)
    cmd /C mklink /J "$FIRST" "$SECOND" >/dev/null

    THIRD=$(sed 's:/:\\:g' <<< `cygpath -mw "$LIB_TARGET_DIR"`)
    FOURTH=$(sed 's:/:\\:g' <<< `cygpath -mw "$EMUL8_LIBRARIES_DIR/Lib"`)
    cmd /C mklink /J "$THIRD" "$FOURTH" >/dev/null
fi

mkdir -p "$CURRENT_PATH/../../Emulator/LLVMDisassembler/Resources/"
pushd "$CURRENT_PATH/../../Emulator/LLVMDisassembler/Resources" >/dev/null
LLVM_BINARIES_DIR=`realpath --relative-to="$PWD" "$EMUL8_LIBRARIES_DIR/llvm"`
for f in `ls "$LLVM_BINARIES_DIR"`
do
    if $ON_LINUX || $ON_OSX
    then
        ln -s "$LLVM_BINARIES_DIR/$f"
    elif $ON_WINDOWS
    then
        cmd /C mklink "$f" "$(sed 's:/:\\:g' <<< `cygpath -mw "$LLVM_BINARIES_DIR/$f"`)" >/dev/null
    fi
done

popd >/dev/null

touch "$GUARD"
