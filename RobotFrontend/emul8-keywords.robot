# emul8: ignore test

*** Settings ***
Library         Process
Library         OperatingSystem

*** Variables ***
${SKIP_RUNNING_SERVER}  False
${CONFIGURATION}        Release
${PORT_NUMBER}          9999
${DIRECTORY}            ${CURDIR}/../output/${CONFIGURATION}
${BINARY_NAME}          ./RobotFrontend.exe

*** Keywords ***
Setup
    Run Keyword Unless  ${SKIP_RUNNING_SERVER}
    ...   Start Process  mono  ${BINARY_NAME}  ${PORT_NUMBER}  cwd=${DIRECTORY}

    Wait Until Keyword Succeeds  10s  1s
    ...   Import Library  Remote  http://localhost:${PORT_NUMBER}/

    Reset Emulation

Teardown
    Run Keyword Unless  ${SKIP_RUNNING_SERVER}
    ...   Stop Remote Server

    Run Keyword Unless  ${SKIP_RUNNING_SERVER}
    ...   Wait For Process
