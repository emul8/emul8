cur=`pwd`
cd bin/Debug
mono --debug CLI.exe "$@"
cd $cur
