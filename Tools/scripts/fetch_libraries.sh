#!/bin/bash

set -e
set -u

#go to the current directory
cd "${0%/*}"

cd ../../External
if [ -e .emul8_libs_fetched ]
then
    echo "Already downloaded. To repeat the process remove External/.emul8_libs_fetched file."
    exit
fi

rm -rf emul8-libraries Lib Tools ../Emulator/Cores/disas-llvm/{32,64}_libLLVM*
touch .emul8_libs_fetched

git clone https://github.com/antmicro/emul8-libraries.git
ln -s emul8-libraries/Lib Lib
ln -s emul8-libraries/Tools Tools
cd ../Emulator/Cores/disas-llvm
ln -s ../../../External/emul8-libraries/llvm/* .
