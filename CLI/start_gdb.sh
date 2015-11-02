#!/bin/bash
curr=`pwd`
cd ../output/Debug
gdb -ex "handle SIGXCPU SIG33 SIG35 SIGPWR nostop noprint" --args mono --debug CLI.exe
cd $curr
