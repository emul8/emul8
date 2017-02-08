rm -rf $DIR
mkdir -p $DIR/{bin,licenses}

#copy the main content
cp -r $BASE/output/$TARGET/*.{dll,exe,dll.config} $DIR/bin
cp -r $BASE/{scripts,platforms,.emul8root} $DIR
cp $BASE/RobotFrontend/*robot $DIR/bin

sed -i.bak 's/cwd=${DIRECTORY}/cwd=${CURDIR}/g' $DIR/bin/*robot
rm $DIR/bin/*robot.bak

#copy the licenses
#some files already include the library name
find $BASE/Emulator $BASE/External $BASE/Tools/packaging/macos -iname "*-license" -exec cp {} $DIR/licenses \;

#others will need a parent directory name.
find $BASE/{Emulator,External} -iname "license" -print0 |\
    while IFS= read -r -d $'\0' file
do
    full_dirname=${file%/*}
    dirname=${full_dirname##*/}
    cp $file $DIR/licenses/$dirname-license
done

cp $BASE/LICENSE $DIR/licenses/LICENSE
