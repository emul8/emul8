*** Settings ***
Suite Setup     Setup
Suite Teardown  Teardown
Resource        ${CURDIR}/emul8-keywords.robot

*** Test Cases ***
Should Print Help
    ${x}=  Execute Command     help
           Should Contain      ${x}    Available commands:
