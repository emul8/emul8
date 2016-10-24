#!/bin/bash
set -e
set -u

if [ -z "${ROOT_PATH:-}" -a -x "$(command -v realpath)" ]; then
    # this is to support running emul8 from external directory
    ROOT_PATH="`dirname \`realpath $0\``"
fi

. ${ROOT_PATH}/Tools/common.sh

LAUNCHER_PATH=${ROOT_PATH:=.}/Tools/Emul8-Launcher
LAUNCHER_BIN_PATH=$LAUNCHER_PATH/Emul8-Launcher/bin/Release/Emul8-Launcher.exe

# build launcher tool ...
$CS_COMPILER /p:Configuration=Release /nologo /verbosity:quiet `get_path $LAUNCHER_PATH/Emul8-Launcher.sln`

# ...and run it
if $ON_WINDOWS
then
	$LAUNCHER_BIN_PATH --root-path `get_path "$ROOT_PATH/output"` "$@"
else
	$LAUNCHER `get_path $LAUNCHER_BIN_PATH` --root-path `get_path "$ROOT_PATH/output"` "$@"
fi
