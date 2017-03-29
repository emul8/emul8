#!/usr/bin/python
# pylint: disable=C0301,C0103,C0111
from __future__ import print_function
import os
import sys
import argparse
import subprocess
import fnmatch
import robot
import nunit_results_merger

this_path = os.path.abspath(os.path.dirname(__file__))
results_directory = os.path.join(this_path, 'tests')
ROBOT_FRONTEND_PORT = 9999

class TestSuite(object):
    @staticmethod
    def get_type(path):
        if path.endswith('csproj'):
            return 'nunit'
        if path.endswith('robot'):
            return 'robot'
        return None

    @staticmethod
    def create(path):
        if TestSuite.get_type(path) == 'nunit':
            return NUnitTestSuite(path)
        elif TestSuite.get_type(path) == 'robot':
            return RobotTestSuite(path)
        else:
            raise Exception('Unknown test suite type: {}'.format(path))

    def __init__(self, path):
        self.path = path

class NUnitTestSuite(TestSuite):
    nunit_path = os.path.join(this_path, './../../External/Tools/nunit-console.exe')
    output_files = []
    instances_count = 0

    def __init__(self, path):
        super(NUnitTestSuite, self).__init__(path)

    def prepare(self):
        NUnitTestSuite.instances_count += 1
        print("Building {0}".format(self.path))
        result = subprocess.call(['xbuild', '/p:OutputPath={0}'.format(results_directory), '/nologo', '/verbosity:quiet', '/p:OutputDir=tests_output', '/p:Configuration={0}'.format(configuration), self.path])
        if result != 0:
            print("Building project `{}` failed with error code: {}".format(self.path, result))
        return result

    def run(self, fixture=None):
        print('Running ' + self.path)
        # copying nunit console binaries seems to be necessary in order to use -domain:None switch; otherwise it is not needed
        copied_nunit_path = os.path.join(results_directory, 'nunit-console.exe')
        if not os.path.isfile(copied_nunit_path):
            subprocess.call(['bash', '-c', 'cp -r {0}/* {1}'.format(os.path.dirname(NUnitTestSuite.nunit_path), results_directory)])

        project_file = os.path.split(self.path)[1]
        output_file = project_file.replace('csproj', 'xml')
        args = ['mono', copied_nunit_path, '-domain:None', '-noshadow', '-nologo', '-labels', '-xml:{}'.format(output_file), project_file.replace("csproj", "dll")]
        if options.port is not None:
            if options.suspend:
                print('Waiting for a debugger at port: {}'.format(options.port))
            args.insert(1, '--debug')
            args.insert(2, '--debugger-agent=transport=dt_socket,server=y,suspend={0},address=127.0.0.1:{1}'.format('y' if options.suspend else 'n', options.port))
        elif options.debug_mode:
            args.insert(1, '--debug')
        if fixture:
            args.append('-run:' + fixture)

        NUnitTestSuite.output_files.append(os.path.join(results_directory, output_file))
        process = subprocess.Popen(args, cwd=results_directory, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
        while True:
            line = process.stdout.readline()
            ret = process.poll()
            if ret is not None:
                return ret == 0
            if line and not line.isspace() and 'GLib-' not in line:
                output.write(line)

    def cleanup(self):
        NUnitTestSuite.instances_count -= 1
        if NUnitTestSuite.instances_count == 0:
            # merge nunit results
            print("Aggregating all nunit results")
            output = os.path.join(results_directory, 'nunit_output.xml')
            nunit_results_merger.merge(NUnitTestSuite.output_files, output)
            print('Output:  {}'.format(output))

class RobotTestSuite(TestSuite):
    instances_count = 0
    emul8_robot_frontend_process = None
    hotspot_action = ['None', 'Pause']
    log_files = []

    def __init__(self, path):
        super(RobotTestSuite, self).__init__(path)
        self._dependencies_met = set()

    def prepare(self):
        RobotTestSuite.instances_count += 1
        if RobotTestSuite.instances_count > 1:
            return

        emul8_robot_frontend_binary_folder = os.path.join(this_path, '../../output/{0}'.format(configuration))
        emul8_robot_frontend_binary = os.path.join(emul8_robot_frontend_binary_folder, 'RobotFrontend.exe')

        if not os.path.isfile(emul8_robot_frontend_binary):
            print("Robot frontend binary not found! Did you forget to bootstrap and build the Emul8?")
            sys.exit(1)

        args = ['mono', emul8_robot_frontend_binary, str(ROBOT_FRONTEND_PORT)]
        RobotTestSuite.emul8_robot_frontend_process = subprocess.Popen(args, cwd=emul8_robot_frontend_binary_folder, bufsize=1)

    def run(self, fixture=None):
        result = True

        tests_with_hotspots = []
        tests_without_hotspots = []
        _suite = robot.parsing.model.TestData(source=self.path)
        for test in _suite.testcase_table.tests:
            if any(step.name == 'Hot Spot' for step in test.steps):
                tests_with_hotspots.append(test.name)
            else:
                tests_without_hotspots.append(test.name)

        if any(tests_without_hotspots):
            result = result and self._run_inner(fixture, None, tests_without_hotspots)
        if any(tests_with_hotspots):
            for hotspot in RobotTestSuite.hotspot_action:
                result = result and self._run_inner(fixture, hotspot, tests_with_hotspots)

        return result

    def _get_dependencies(self, test_case):
        _suite = robot.parsing.model.TestData(source=self.path)
        test = next(t for t in _suite.testcase_table.tests if t.name == test_case)
        requirements = [s.args[0] for s in test.steps if s.name == 'Requires']
        if len(requirements) == 0:
            return set()
        if len(requirements) > 1:
            raise Exception('Too many requirements for a single test. At most one is allowed.')
        providers = [t for t in _suite.testcase_table.tests if any(s.name == 'Provides' and s.args[0] == requirements[0] for s in t.steps)]
        if len(providers) > 1:
            raise Exception('Too many providers for state {0} found: {1}'.format(requirements[0], ', '.join(providers.name)))
        if len(providers) == 0:
            raise Exception('No provider for state {0} found'.format(requirements[0]))
        res = self._get_dependencies(providers[0].name)
        res.add(providers[0].name)
        return res

    def cleanup(self):
        RobotTestSuite.instances_count -= 1
        if RobotTestSuite.instances_count == 0:
            if RobotTestSuite.emul8_robot_frontend_process:
                os.kill(RobotTestSuite.emul8_robot_frontend_process.pid, 15)
                RobotTestSuite.emul8_robot_frontend_process.wait()
            if len(RobotTestSuite.log_files) > 0:
                print("Aggregating all robot results")
                robot.rebot(*RobotTestSuite.log_files, processemptysuite=True, name='Emul8 Suite', outputdir=results_directory, output='robot_output.xml')

    @staticmethod
    def _create_suite_name(test_name, hotspot):
        return test_name + (' [HotSpot action: {0}]'.format(hotspot) if hotspot else '')

    def _run_dependencies(self, test_cases_names):
        test_cases_names.difference_update(self._dependencies_met)
        if not any(test_cases_names):
            return True
        self._dependencies_met.update(test_cases_names)
        return self._run_inner(None, None, test_cases_names)

    def _run_inner(self, fixture, hotspot, test_cases_names):
        file_name = os.path.splitext(os.path.basename(self.path))[0]
        suite_name = RobotTestSuite._create_suite_name(file_name, hotspot)

        variables = ['SKIP_RUNNING_SERVER:True']
        if hotspot:
            variables.append('HOTSPOT_ACTION:' + hotspot)
        if options.debug_mode:
            variables.append('CONFIGURATION:Debug')

        test_cases = [(test_name, '{0}.{1}'.format(suite_name, test_name)) for test_name in test_cases_names]
        if fixture:
            test_cases = [x for x in test_cases if fnmatch.fnmatch(x[1], fixture)]
            if len(test_cases) == 0:
                return True
            deps = set()
            for test_name in (t[0] for t in test_cases):
                deps.update(self._get_dependencies(test_name))
            if not self._run_dependencies(deps):
                return False

        metadata = 'HotSpot_Action:{0}'.format(hotspot if hotspot else '-')
        log_file = os.path.join(results_directory, '{0}{1}.xml'.format(file_name, '_' + hotspot if hotspot else ''))
        RobotTestSuite.log_files.append(log_file)
        return robot.run(self.path, runemptysuite=True, output=log_file, log=None, report=None, metadata=metadata, name=suite_name, variable=variables, test=[t[1] for t in test_cases]) == 0

# parsing cmd-line arguments
parser = argparse.ArgumentParser()
parser.add_argument("tests", help="List of test files", nargs='*')
parser.add_argument("-f", "--fixture",  dest="fixture", help="Fixture to test", metavar="FIXTURE")
parser.add_argument("-n", "--repeat",   dest="repeat_count", nargs="?", type=int, const=0, default=1, help="Repeat tests a number of times (no-flag: 1, no-value: infinite)")
parser.add_argument("-d", "--debug",    dest="debug_mode",  action="store_true",  default=False, help="Debug mode")
parser.add_argument("-o", "--output",   dest="output",      action="store",       default=None,  help="Output file, default STDOUT.")
parser.add_argument("-b", "--buildbot", dest="buildbot",    action="store_true",  default=False, help="Buildbot mode. Before running tests prepare environment, i.e., create tap0 interface.")
parser.add_argument("-t", "--tests",    dest="tests_file",  action="store",       default=None,  help="Path to a file with a list of assemblies with tests to run. This is ignored if any test file is passed as positional argument.")
parser.add_argument("-p", "--port",     dest="port",        action="store",       default=None,  help="Debug port.")
parser.add_argument("-s", "--suspend",  dest="suspend",     action="store_true",  default=False, help="Suspend test waiting for a debugger.")
parser.add_argument("-T", "--type",     dest="test_type",   action="store",       default="all", help="Type of test to execute: nunit, robot or all (default)")
options = parser.parse_args()

if options.buildbot:
    print("Preparing Environment")
    ret_code = subprocess.call(['/usr/sbin/tunctl', '-d', 'tap0'])
    if ret_code != 0:
        print('Error while removing old tap0 interface')
        sys.exit(ret_code)
    ret_code = subprocess.call(['/usr/sbin/tunctl', '-t', 'tap0', '-u', str(os.getuid())])
    if ret_code != 0:
        print('Error while creating tap0 interface')
        sys.exit(ret_code)
if options.debug_mode:
    print("Running in debug mode.")
elif options.port is not None or options.suspend:
    print('Port/suspend options can be used in debug mode only.')
    sys.exit(1)
if 'FIXTURE' in os.environ:
    options.fixture = os.environ['FIXTURE']
if options.fixture:
    print("Testing fixture: " + options.fixture)
if options.tests_file is not None and not options.tests:
    options.tests = [line.rstrip() for line in open(options.tests_file)]
if options.port == str(ROBOT_FRONTEND_PORT):
    print('Port {} is reserved for Robot Frontend and cannot be used for remote debugging.'.format(ROBOT_FRONTEND_PORT))
    sys.exit(1)

configuration = 'Debug' if options.debug_mode else 'Release'

#set stdout as default
output = sys.stdout
if not options.output is None:
    try:
        output = open(options.output)
    except Exception as e:
        print("Failed to open output file. Falling back to STDOUT.")

tests_suites = [TestSuite.create(path) for path in options.tests if options.test_type == "all" or TestSuite.get_type(path) == options.test_type]

print("Preparing suites")
for suite in tests_suites:
    suite.prepare()

print("Starting suites")
tests_failed = False
counter = 0
while options.repeat_count == 0 or counter < options.repeat_count:
    counter += 1
    for suite in tests_suites:
        if not suite.run(options.fixture):
            tests_failed = True

print("Cleaning up suites")
for suite in tests_suites:
    suite.cleanup()

output.flush()
if output is not sys.stdout:
    output.close()

if tests_failed:
    print("Some tests failed :( See logs for details!")
    sys.exit(1)
print("Tests finished sucessfully :)")
