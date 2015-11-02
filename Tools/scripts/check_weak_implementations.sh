#!/bin/bash
set -u
set -e

function first()
{
  echo $1
}

function next()
{
  shift
  echo $*
}

pushd ../../Emulator/Cores/ > /dev/null

WEAKS=weaks.tmp
IMPLEMENTATIONS=implementations.tmp
ARCHS="arm ppc sparc"
PATHS_WEAK="tlib/callbacks.c tlib/arch/arm/arch_callbacks.c tlib/arch/ppc/arch_callbacks.c tlib/arch/sparc/arch_callbacks.c"
PATHS_IMPLEMENTATION="emul8/emul8_callbacks.c emul8/arch/arm/emul8_arm_callbacks.c emul8/arch/ppc/emul8_ppc_callbacks.c emul8/arch/sparc/emul8_sparc_callbacks.c"

RESULT=0

for PATH_WEAK in $PATHS_WEAK
do
  PATH_IMPLEMENTATION=`first $PATHS_IMPLEMENTATION`
  PATHS_IMPLEMENTATION=`next $PATHS_IMPLEMENTATION`
  gcc -I tlib/include -E $PATH_WEAK | grep weak | grep -o tlib[_A-Za-z]* | sort | uniq > $WEAKS
  cat $PATH_IMPLEMENTATION | grep -o tlib[_A-Za-z]* | sort | uniq > $IMPLEMENTATIONS
  if grep -vwF -f $IMPLEMENTATIONS $WEAKS
  then
    echo $PATH_WEAK
    echo "-----------------------"
    RESULT=1
  fi
done

rm $WEAKS
rm $IMPLEMENTATIONS

popd > /dev/null

exit $RESULT
