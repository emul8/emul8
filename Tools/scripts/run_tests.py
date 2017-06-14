#!/usr/bin/python
# pylint: disable=C0301,C0103,C0111
import nunit_tests_provider
import tests_engine

tests_engine.register_handler('nunit', 'csproj', nunit_tests_provider.NUnitTestSuite, nunit_tests_provider.install_cli_arguments)
tests_engine.run()
