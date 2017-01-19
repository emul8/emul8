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
    popd >/dev/null
    if [ $top_ref == $cur_ref ]
    then
        echo "Required libraries already downloaded. To repeat the process remove External/.emul8_libs_fetched file."
        exit
    else
        echo "Required libraries are available in a new version. The libraries will be redownloaded..."
    fi
fi

rm -rf $DIR Lib Tools ../Emulator/LLVMDisassembler/Resources/
touch .emul8_libs_fetched

mkdir -p ../Emulator/LLVMDisassembler/Resources/
git clone $REMOTE
ln -s $DIR/Lib Lib
ln -s $DIR/Tools Tools
cd ../Emulator/LLVMDisassembler/Resources
ln -s ../../../External/$DIR/llvm/* .
