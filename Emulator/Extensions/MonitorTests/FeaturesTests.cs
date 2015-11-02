//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using NUnit.Framework;
using Emul8.UserInterface;
using Emul8.UserInterface.Commands;

namespace MonitorTests
{
    [TestFixture]
    public class FeaturesTests
    {
        [Test]
        public void AutoLoadCommandTest()
        {
            var commandInteraction = new CommandInteractionEater();
            var commandInstance = new TestCommand(monitor);
            monitor.Parse("help", commandInteraction);
            var contents = commandInteraction.GetContents();
            Assert.IsTrue(contents.Contains(commandInstance.Description));
        }

        [SetUp]
        public void SetUp()
        {
            monitor = new Monitor();
        }

        private Monitor monitor;
    }

    public class TestCommand : AutoLoadCommand
    {
        public TestCommand(Monitor monitor):base(monitor, "featuresTests.TestCommand", "is just a test command.")
        {
        }
    }
}

