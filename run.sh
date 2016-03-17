#!/bin/bash
set -e
set -u

if [ -z "${ROOT_PATH:-}" -a -x "$(command -v realpath)" ]; then
    # this is to support running emul8 from external directory
    ROOT_PATH="`dirname \`realpath $0\``"
fi

LAUNCHER_PATH=${ROOT_PATH:=.}/Tools/Emul8-Launcher
LAUNCHER_BIN_PATH=$LAUNCHER_PATH/Emul8-Launcher/bin/Release/Emul8-Launcher.exe

# build launcher tool ...
xbuild /p:Configuration=Release /nologo /verbosity:quiet $LAUNCHER_PATH/Emul8-Launcher.sln

# ...and run it
mono $LAUNCHER_BIN_PATH --root-path "$ROOT_PATH/output" $@
