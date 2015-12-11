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

def get_project(project_id):
    """Tries to convert project argument into the supported path.
Argument can be either a part of path or a number from the list.
If the argument is a part of path and matches against more than one candidate
an exception is thrown.
"""
    try:
        #id argument is numerical, try to get it from the project list
        project_id = int(project_id)
        return test_projects[project_id]
    except ValueError:
        #arugment wasn't numerical, so we check if the name is in supported projects
        pass
    candidates = [ (num,candidate) for (num,candidate) in enumerate(test_projects) if project_id in candidate]
    if len(candidates) == 0:
        raise LookupError("`{0}` is an invalid project name.".format(project_id))
    if len(candidates) > 1:
        candidate_numbers = list(zip(*candidates))[0]
        raise LookupError("`{0}` is an ambiguous candidate for project numbers: {1}.".format(project_id, candidate_numbers))
    return candidates[0][1]

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
parser.add_argument("-p", "--project", dest="projects", nargs="*", help="Use only specified projects (no-flag: all projects, no-value: project listing).")
parser.add_argument("-d", "--debug", dest="debug_mode", action="store_true", default=False, help="Debug mode")
parser.add_argument("-o", "--output", dest="output", action="store", default=None, help="Output file, default STDOUT.")
parser.add_argument("-b", "--buildbot", dest="buildbot", action="store_true", default=False, help="Buildbot mode. Before running tests prepare environment, i.e., create tap69 interface.")
parser.add_argument("-t", "--tests", dest="tests", action="store", default=None, help="Path to a file with a list of assemblies with tests to run.")
options  = parser.parse_args()

if options.buildbot:
    print("Preparing Environment")
    ret_code = subprocess.call(['/usr/sbin/tunctl', '-d', 'tap69'])
    if ret_code != 0:
        print('Error while removing old tap69 interface')
        sys.exit(ret_code)
    ret_code = subprocess.call(['/usr/sbin/tunctl', '-t', 'tap69', '-u', str(os.getuid())])
    if ret_code != 0:
        print('Error while creating tap69 interface')
        sys.exit(ret_code)
if options.debug_mode:
    print("Running in debug mode.")
if 'FIXTURE' in os.environ:
    options.fixture = os.environ['FIXTURE']
if options.fixture:
    print("Testing fixture: " + options.fixture)
if options.tests != None:
   test_projects = [line.rstrip() for line in open(options.tests)]
if options.projects != None:
    #tries to convert passed arguments to project paths
    failed = False
    try:
        used_projects = []
        for project in options.projects:
            used_projects.append(get_project(project))
    except Exception as e:
        print("Error setting up project list: {0}".format(e))
        failed = True
    if failed or len(options.projects) == 0:
        print("Valid projects and their numerical mapping are: ")
        for (num, name) in enumerate(test_projects):
            print("{} - {}".format(num,name))
        exit(1)
else:
    used_projects = test_projects

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
