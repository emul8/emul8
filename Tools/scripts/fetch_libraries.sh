#!/bin/bash

set -e
set -u

REMOTE=https://github.com/antmicro/emul8-libraries.git
ROOT_PATH="`dirname \"\`realpath "$0"\`\"`"
GUARD=`realpath --relative-to="$PWD" "$ROOT_PATH/../../External/.emul8_libs_fetched"`
EMUL8_LIBRARIES_DIR="$ROOT_PATH/../../External/emul8-libraries"

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

rm -rf "$EMUL8_LIBRARIES_DIR" "$ROOT_PATH"/../../External/{Lib,Tools} "$ROOT_PATH/../../Emulator/LLVMDisassembler/Resources/"

git clone $REMOTE "`realpath --relative-to="$PWD" "$EMUL8_LIBRARIES_DIR"`"

ln -s "$EMUL8_LIBRARIES_DIR/Lib" "$ROOT_PATH/../../External/Lib"
ln -s "$EMUL8_LIBRARIES_DIR/Tools" "$ROOT_PATH/../../External/Tools"

mkdir -p "$ROOT_PATH/../../Emulator/LLVMDisassembler/Resources/"
pushd "$ROOT_PATH/../../Emulator/LLVMDisassembler/Resources" >/dev/null
LLVM_BINARIES_DIR=`realpath --relative-to="$PWD" "$EMUL8_LIBRARIES_DIR/llvm"`
for f in `ls "$LLVM_BINARIES_DIR"`
do
    ln -s "$LLVM_BINARIES_DIR/$f"
done
popd >/dev/null

touch "$GUARD"
