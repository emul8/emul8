#!/usr/bin/python
from __future__ import print_function
import os
import sys
import getopt
import argparse
import subprocess
import signal

this_path = os.path.abspath(os.path.dirname(__file__))
nunit_path = os.path.join(this_path, './../../External/Tools/nunit-console.exe')
bin_directory = os.path.join(this_path, 'tests')

def build_project(path):
    """
Tries to build a project at given path. Because of a bug in xbuild which sometimes crashes after build we retry it, up to 5 times, if received code is 134 (abort).
"""
    print("\t Building {0}".format(path))
    for i in range(5):
        ret_code = subprocess.call(['xbuild', '/p:OutputPath={0}'.format(bin_directory), '/nologo', '/verbosity:quiet', '/p:OutputDir=tests_output', '/p:Configuration={0}'.format(configuration), path])
        if ret_code == 0:
            return
        elif ret_code != 134:
            break
    print("Building project `{}` failed with error code: {}".format(path, ret_code))
    sys.exit(ret_code)


def ignore_sig():
    signal.signal(signal.SIGTSTP, signal.SIG_IGN)

def report_signal_handler(singnum, frame):
    print("Test iteration {0}, currently on project `{1}`.".format(counter, project))


def run(args):
    process = subprocess.Popen(args, cwd=bin_directory, bufsize = 1, preexec_fn = ignore_sig, stdout =
                subprocess.PIPE, stderr = subprocess.STDOUT)
    while True:
        line = process.stdout.readline()
        ret = process.poll()
        if ret is not None:
            return ret == 0
        else:
            if line and not line.isspace() and 'GLib-GObject-CRITICAL' not in line and 'GLib-CRITICAL' not in line:
                output.write(line)

# parsing cmd-line arguments
parser = argparse.ArgumentParser()
parser.add_argument("tests", help="List of test files", nargs='*')
parser.add_argument("-f", "--fixture", dest="fixture", help="Fixture to test", metavar="FIXTURE")
parser.add_argument("-n", "--repeat", dest="repeat_count", nargs="?", type=int, const=0, default=1, help="Repeat tests a number of times (no-flag: 1, no-value: infinite)")
parser.add_argument("-d", "--debug", dest="debug_mode", action="store_true", default=False, help="Debug mode")
parser.add_argument("-o", "--output", dest="output", action="store", default=None, help="Output file, default STDOUT.")
parser.add_argument("-b", "--buildbot", dest="buildbot", action="store_true", default=False, help="Buildbot mode. Before running tests prepare environment, i.e., create tap0 interface.")
parser.add_argument("-t", "--tests", dest="tests_file", action="store", default=None, help="Path to a file with a list of assemblies with tests to run.")
options  = parser.parse_args()

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
if 'FIXTURE' in os.environ:
    options.fixture = os.environ['FIXTURE']
if options.fixture:
    print("Testing fixture: " + options.fixture)
if options.tests_file != None:
    options.tests.extend([line.rstrip() for line in open(options.tests_file)])

configuration = 'Debug' if options.debug_mode else 'Release'

#set stdout as default
output = sys.stdout
if not options.output is None:
    try:
        output = open(options.output)
    except Exception as e:
        print("Failed opening output file. Falling back to STDOUT.")

#register signal handlers
signal.signal(signal.SIGUSR1, report_signal_handler)
signal.signal(signal.SIGTSTP, report_signal_handler)

print("Building projects.")
for project in options.tests:
    if project.endswith('csproj'):
        build_project(project)

print("Starting tests. Use STOP signal (default: Ctrl+Z) to check progress.")

#main loop
fail_count = 0
counter = 0
while options.repeat_count == 0 or counter < options.repeat_count:
    counter += 1

    robot_tests = []
    for project in options.tests:
        if project.endswith('csproj'):
            filename = os.path.split(project)[1]
            subprocess.call(['bash', '-c', 'cp -r ' + os.path.dirname(nunit_path) + '/* ' + bin_directory + ''])
            copied_nunit_path = bin_directory + "/nunit-console.exe"
            args = ['mono', copied_nunit_path, '-noshadow', '-nologo', '-labels', '-domain:None', filename.replace("csproj", "dll")]
            if options.fixture:
                args.append('-run:' + options.fixture)
        elif project.endswith('robot'):
            robot_tests.append(project)
            continue

        if not run(args):
            fail_count += 1

    if any(robot_tests):

        emul8_robot_frontend_binary_folder = os.path.join(this_path, '../../output/{0}'.format(configuration))
        emul8_robot_frontend_binary = os.path.join(emul8_robot_frontend_binary_folder, 'RobotFrontend.exe')

        emul8_robot_frontend_process = subprocess.Popen(['mono', emul8_robot_frontend_binary, '9999'], cwd=emul8_robot_frontend_binary_folder, bufsize=1)

        args = ['robot', '-N', 'Emul8_Suite', '-C', 'on', '-v', 'SKIP_RUNNING_SERVER:True']
        if(options.debug_mode):
            args.append('-v')
            args.append('CONFIGURATION:Debug')
        args.extend(robot_tests)

        if not run(args):
            fail_count += 1

        os.kill(emul8_robot_frontend_process.pid, 15)
        emul8_robot_frontend_process.wait()

output.flush()
if not output is sys.stdout:
    output.close()

if fail_count > 0:
    print("Failed tests count: {}!".format(fail_count))
    sys.exit(1)
