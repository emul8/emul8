#!/bin/bash

set -e
set -u

#go to the current directory
cd "${0%/*}"

cd ../../External
if [ -e .emul8_libs_fetched ]
then
    top_ref=`git ls-remote -h https://github.com/antmicro/emul8-libraries.git master | cut -f1`
    pushd emul8-libraries >/dev/null
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

rm -rf emul8-libraries Lib Tools ../Emulator/LLVMDisassembler/Resources/
touch .emul8_libs_fetched

mkdir -p ../Emulator/LLVMDisassembler/Resources/
git clone https://github.com/antmicro/emul8-libraries.git
ln -s emul8-libraries/Lib Lib
ln -s emul8-libraries/Tools Tools
cd ../Emulator/LLVMDisassembler/Resources
ln -s ../../../External/emul8-libraries/llvm/* .
