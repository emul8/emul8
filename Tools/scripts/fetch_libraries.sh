#!/bin/bash

set -e
set -u

REMOTE=https://github.com/antmicro/emul8-libraries.git
DIR=emul8-libraries

#go to the current directory
cd "${0%/*}"

cd ../../External
if [ -e .emul8_libs_fetched ]
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
        echo "Required Emul8 libraries already downloaded. To repeat the process remove External/.emul8_libs_fetched file."
        exit
    fi
    echo "Required Emul8 libraries are available in a new version. The libraries will be redownloaded..."
fi

rm -rf $DIR Lib Tools ../Emulator/LLVMDisassembler/Resources/

mkdir -p ../Emulator/LLVMDisassembler/Resources/
git clone $REMOTE
ln -s $DIR/Lib Lib
ln -s $DIR/Tools Tools
pushd ../Emulator/LLVMDisassembler/Resources >/dev/null
ln -s ../../../External/$DIR/llvm/* .
popd >/dev/null

touch .emul8_libs_fetched
