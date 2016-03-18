#!/bin/bash
curr=`pwd`
cd ../output/Debug
lldb -- mono --debug CLI.exe
cd $curr
