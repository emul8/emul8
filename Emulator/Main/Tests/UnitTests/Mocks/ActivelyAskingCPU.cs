//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Threading;
using Emul8.Core;

namespace UnitTests.Mocks
{
    public class ActivelyAskingCPU : EmptyCPU
    {
        public ActivelyAskingCPU(Machine machine, long addressToAsk) : base(machine)
        {
            this.addressToAsk = addressToAsk;
            tokenSource = new CancellationTokenSource();
            finished = new ManualResetEventSlim();
        }

        public override void Start()
        {
            Resume();
        }

        public override void Resume()
        {
            finished.Reset();
            new Thread(() => AskingThread(tokenSource.Token))
            {
                IsBackground = true,
                Name = "AskingThread"
            }.Start();
        }

        public override void Pause()
        {
            tokenSource.Cancel();
            finished.Wait();
        }

        private void AskingThread(CancellationToken token)
        {
            while(!token.IsCancellationRequested)
            {
                machine.SystemBus.ReadDoubleWord(addressToAsk);
            }
            finished.Set();
        }

        private CancellationTokenSource tokenSource;
        private readonly ManualResetEventSlim finished;
        private readonly long addressToAsk;
    }
}

