//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
namespace Emul8.Core
{
    public class MachineStateChangedEventArgs
    {
        public MachineStateChangedEventArgs(State state)
        {
            CurrentState = state;
        }

        public State CurrentState { get; private set; }

        public enum State
        {
            Started,
            Paused,
            Disposed
        }
    }
}

