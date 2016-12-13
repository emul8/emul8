UNAME=`uname -s`
if [ "$UNAME" == "Linux" ]
then
    ON_WINDOWS=false
    ON_OSX=false
    ON_LINUX=true
    CS_COMPILER=xbuild
    LAUNCHER="mono"
elif [ "$UNAME" == "Darwin" ]
then
    ON_WINDOWS=false
    ON_OSX=true
    ON_LINUX=false
    CS_COMPILER=xbuild
    LAUNCHER="mono"
else
    ON_WINDOWS=true
    ON_OSX=false
    ON_LINUX=false
    CS_COMPILER=msbuild.exe
    LAUNCHER=""
fi

function get_path {
    if $ON_WINDOWS
    then
        echo -n "`cygpath -aw $1`"
    else
        echo -n "$1"
    fi
}

