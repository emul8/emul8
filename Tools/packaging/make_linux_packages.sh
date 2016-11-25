#!/bin/bash

set -e
set -u

#change dir to script location
cd "${0%/*}"

export PATH=`gem environment gemdir`/bin:$PATH
TARGET="Release"
BASE=../..

REMOVE_WORKDIR=true

DATE=""
COMMIT=""

RPM_MIN_DIST="f23"

function usage {
    echo "$0 {version-number} [-d] [-n] [-h] [-l]"
}

function help {
    usage
    echo
    echo -e "-d\tuse Debug configuration"
    echo -e "-n\tcreate a nightly build with date and commit SHA"
    echo -e "-l\tdo not remove workdir after building"
    echo -e "-h\tprint this help message"
}

function is_dep_available {
    if ! command -v $1 >/dev/null 2>&1
    then
        echo "$1 is missing. Install it to continue."
        return 1
    fi
    return 0

}

#expand this list if needed. bsdtar is required for arch packages.
if ! is_dep_available fpm ||\
    ! is_dep_available rpm ||\
    ! is_dep_available bsdtar
then
    exit
fi

if [ $# -lt 1 ]
then
    usage
    exit
fi

VERSION=$1

shift
while getopts "dhnl" opt
do
    case $opt in
        d)
            TARGET="Debug"
            ;;
        n)
            DATE="+`date +%Y%m%d`"
            pushd $BASE >/dev/null
            COMMIT="git`git rev-parse --short HEAD`"
            popd >/dev/null
            ;;
        h)
            help
            exit
            ;;
        l)
            REMOVE_WORKDIR=false
            ;;
        \?)
            echo "Invalid option: -$OPTARG"
            usage
            exit
            ;;
    esac
done

VERSION="$VERSION$DATE$COMMIT"

DIR=emul8_$VERSION

rm -rf $DIR
mkdir -p $DIR/bin
mkdir -p $DIR/licenses

#copy the main content
cp -r $BASE/output/$TARGET/*.{dll,exe} $DIR/bin
cp -r $BASE/{scripts,platforms,.emul8root} $DIR

#copy the licenses
#some files already include the library name
find $BASE/Emulator $BASE/External -iname "*-license" -exec cp {} $DIR/licenses \;

#others will need a parent directory name.
find $BASE/Emulator $BASE/External -iname "license" -print0 | while IFS= read -r -d $'\0' file
do
    full_dirname=${file%/*}
    dirname=${full_dirname##*/}
    cp $file $DIR/licenses/$dirname-license
done

cp $BASE/LICENSE $DIR/licenses/LICENSE

PACKAGES=packages/$TARGET
OUTPUT=$BASE/$PACKAGES

GENERAL_FLAGS=(\
    -f -n emul8 -v $VERSION --license MIT\
    --category devel --provides emul8 -a native\
    -m 'Piotr Zierhoffer <pzierhoffer@antmicro.com>'\
    --vendor 'Piotr Zierhoffer <pzierhoffer@antmicro.com>'\
    --description 'The Emul8 Framework'\
    --url 'www.emul8.org'\
    --after-install update_icon_cache.sh\
    --after-remove update_icon_cache.sh\
    $DIR/=/opt/emul8\
    emul8.sh=/usr/bin/emul8\
    Emul8.desktop=/usr/share/applications/Emul8.desktop\
    icons/=/usr/share/icons/hicolor
    )

### create debian package
fpm -s dir -t deb\
    -d 'mono-complete >= 4.6' -d gtk-sharp2 -d screen -d gksu\
    --deb-no-default-config-files\
    "${GENERAL_FLAGS[@]}" >/dev/null

mkdir -p $OUTPUT/deb
deb=emul8*deb
echo -n "Created a Debian package in $PACKAGES/deb/"
echo $deb
mv $deb $OUTPUT/deb

### create rpm package
fpm -s dir -t rpm\
    -d 'mono-complete >= 4.6' -d gtk-sharp2 -d screen -d beesu\
    --rpm-dist $RPM_MIN_DIST\
    --rpm-auto-add-directories\
    "${GENERAL_FLAGS[@]}" >/dev/null

mkdir -p $OUTPUT/rpm
rpm=emul8*rpm
echo -n "Created a Fedora package in $PACKAGES/rpm/"
echo $rpm
mv $rpm $OUTPUT/rpm

### create arch package
fpm -s dir -t pacman\
    -d mono -d gtk-sharp-2 -d screen -d gksu\
    "${GENERAL_FLAGS[@]}" >/dev/null

mkdir -p $OUTPUT/arch
arch=emul*.pkg.tar.xz
echo -n "Created an Arch package in $PACKAGES/arch/"
echo $arch
mv $arch $OUTPUT/arch

#cleanup unless user requests otherwise
if $REMOVE_WORKDIR
then
    rm -rf $DIR
fi

