#!/bin/bash
set -e
set -u

if [ -z "${ROOT_PATH:-}" -a -x "$(command -v realpath)" ]
then
    # this is to support running emul8 from external directory
    ROOT_PATH="`dirname \"\`realpath "$0"\`\"`"
fi

. "${ROOT_PATH}/Tools/common.sh"

PARAMS=()
for ARG in "$@"
do
    if [ "$ARG" == "-d" ]
    then
        DEBUG=true;
    elif [ "$ARG" == "-q" ]
    then
        QUIET=true
    else
        PARAMS+=("$ARG")
    fi
done

if ${DEBUG:-false}
then
    TARGET=Debug
fi

if ${REMOTE:-false}
then
    LAUNCHER="$LAUNCHER --debugger-agent=transport=dt_socket,address=0.0.0.0:${REMOTE_PORT:=9876},server=y"
    echo "Waiting for a debugger at port $REMOTE_PORT..."
fi

if ${QUIET:-false}
then
    exec 2>/dev/null
fi

if $ON_WINDOWS
then
    "${BINARY_LOCATION:-$ROOT_PATH/target/bin}/${TARGET:-Release}/${BINARY_NAME:-CLI.exe}" "${PARAMS[@]:-}"
else
    $LAUNCHER "${BINARY_LOCATION:-$ROOT_PATH/target/bin}/${TARGET:-Release}/${BINARY_NAME:-CLI.exe}" "${PARAMS[@]:-}"
fi

