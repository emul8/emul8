#!/usr/bin/python
from __future__ import print_function
import os
import sys
import getopt
import argparse
import subprocess
import signal

nunit_path = os.path.abspath(os.path.join(os.path.dirname(__file__), './../../External/Tools/nunit-console.exe'))
bin_directory = os.path.abspath(os.path.join(os.path.dirname(__file__), 'tests'))

test_projects = map(os.path.abspath, [
        "./../../Emulator/Main/Tests/SystemTests/SystemTests.csproj",
        "./../../Emulator/Main/Tests/UnitTests/UnitTests.csproj",
        "./../../Emulator/Peripherals/Test/PeripheralsTests/PeripheralsTests.csproj",
        "./../../Emulator/Extensions/MonitorTests/MonitorTests.csproj",
])

def build_project(path):
    """
Tries to build a project at given path. Because of a bug in xbuild which sometimes crashes after build we retry it, up to 5 times, if received code is 134 (abort).
"""
    print("\t Building {0}".format(path))
    for i in range(5):
        ret_code = subprocess.call(['xbuild', '/p:OutputPath={0}'.format(bin_directory), '/nologo', '/verbosity:quiet', '/p:OutputDir=tests_output', '/p:Configuration={0}'.format('Debug' if options.debug_mode else 'Release'), path])
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


# parsing cmd-line arguments
parser = argparse.ArgumentParser()
parser.add_argument("-f", "--fixture", dest="fixture", help="Fixture to test", metavar="FIXTURE")
parser.add_argument("-n", "--repeat", dest="repeat_count", nargs="?", type=int, const=0, default=1, help="Repeat tests a number of times (no-flag: 1, no-value: infinite)")
parser.add_argument("-d", "--debug", dest="debug_mode", action="store_true", default=False, help="Debug mode")
parser.add_argument("-o", "--output", dest="output", action="store", default=None, help="Output file, default STDOUT.")
parser.add_argument("-b", "--buildbot", dest="buildbot", action="store_true", default=False, help="Buildbot mode. Before running tests prepare environment, i.e., create tap0 interface.")
parser.add_argument("-t", "--tests", dest="tests", action="store", default=None, help="Path to a file with a list of assemblies with tests to run.")
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
if options.tests != None:
   used_tests = [line.rstrip() for line in open(options.tests)]

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
for project in used_projects:
    build_project(project)

print("Starting tests. Use STOP signal (default: Ctrl+Z) to check progress.")

#main loop
fail_count = 0
counter = 0
while options.repeat_count == 0 or counter < options.repeat_count:
    counter += 1

    for project in used_projects:
        filename = os.path.split(project)[1]
        subprocess.call(['bash', '-c', 'cp -r ' + os.path.dirname(nunit_path) + '/* ' + bin_directory + ''])
        copied_nunit_path = bin_directory + "/nunit-console.exe"
        args = ['mono', copied_nunit_path, '-noshadow', '-nologo', '-labels', '-domain:None', filename.replace("csproj", "dll")]
        if options.fixture:
            args.append('-run:' + options.fixture)
        process = subprocess.Popen(args, cwd=bin_directory, bufsize = 1, preexec_fn = ignore_sig, stdout =
                subprocess.PIPE, stderr = subprocess.STDOUT)
        while True:
            line = process.stdout.readline()
            ret = process.poll()
            if ret is not None:
                if ret != 0:
                    fail_count += 1
                break
            else:
                if line and not line.isspace() and 'GLib-GObject-CRITICAL' not in line and 'GLib-CRITICAL' not in line:
                    output.write(line)

output.flush()
if not output is sys.stdout:
    output.close()

if fail_count > 0:
    print("Failed tests count: {}!".format(fail_count))
    sys.exit(1)
