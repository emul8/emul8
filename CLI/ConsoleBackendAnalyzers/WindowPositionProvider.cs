//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Xwt;

namespace Emul8.CLI
{
    public class WindowPositionProvider
    {
        static WindowPositionProvider()
        {
            Instance = new WindowPositionProvider();
        }

        public static WindowPositionProvider Instance { get; private set; }

        public Point GetNextPosition()
        {
            lock(innerLock)
            {
                var result = nextPosition;
                nextPosition.X += offset.X;
                nextPosition.Y += offset.Y;
                return result;
            }
        }

        private WindowPositionProvider()
        {
            nextPosition = new Point(0, 0);
            offset = new Point(30, 50);
            innerLock = new object();
        }

        private readonly object innerLock;

        private Point nextPosition;
        private readonly Point offset;
    }
}