#!/bin/bash

set -e
set -u

REMOTE=https://github.com/antmicro/emul8-libraries.git
ROOT_PATH="`dirname \`realpath $0\``"
DIR=$ROOT_PATH/../../External/emul8-libraries
GUARD=`realpath --relative-to=$PWD $ROOT_PATH/../../External/.emul8_libs_fetched`

if [ -e $GUARD ]
then
    top_ref=`git ls-remote -h $REMOTE master | cut -f1`
    pushd $DIR >/dev/null
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

rm -rf $DIR $ROOT_PATH/../../External/{Lib,Tools} $ROOT_PATH/../../Emulator/LLVMDisassembler/Resources/

mkdir -p $ROOT_PATH/../../Emulator/LLVMDisassembler/Resources/
git clone $REMOTE $DIR
ln -s $DIR/Lib $ROOT_PATH/../../External/Lib
ln -s $DIR/Tools $ROOT_PATH/../../External/Tools
pushd $ROOT_PATH/../../Emulator/LLVMDisassembler/Resources >/dev/null
ln -s ../../../External/$DIR/llvm/* .
popd >/dev/null

touch $GUARD
