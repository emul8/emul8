#!/bin/bash

TESTS_FILE=$PWD/target/tests.txt
PROPERTIES_FILE=$PWD/target/properties.csproj
if [ ! -f "$TESTS_FILE" ]; then
    echo "Tests file not found. Please run ./bootstrap.sh script"
    exit 1
fi

./Tools/scripts/run_tests.py --properties-file "$PROPERTIES_FILE" -t "$TESTS_FILE" $@
