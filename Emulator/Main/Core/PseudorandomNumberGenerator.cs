//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Logging;

namespace Emul8.Core
{
    public class PseudorandomNumberGenerator
    {
        public PseudorandomNumberGenerator()
        {
            instanceSeed = new Random().Next();
            generator = new Random(instanceSeed);

            Logger.Log(LogLevel.Info, "Pseudorandom Number Generator was created with seed: {0}", instanceSeed);
        }

        public void ResetSeed(int newSeed)
        {
            if(wasSeedUsed)
            {
                Logger.Log(LogLevel.Warning, "Pseudorandom Number Generator seed has changed since last usage from: {0} to: {1}", instanceSeed, newSeed);
            }
            instanceSeed = newSeed;
            generator = new Random(instanceSeed);
            wasSeedUsed = false;
        }

        public int GetCurrentSeed()
        {
            return instanceSeed;
        }

        public double NextDouble()
        {
            wasSeedUsed = true;
            return generator.NextDouble();
        }

        public int Next()
        {
            wasSeedUsed = true;
            return generator.Next();
        }

        public int Next(int maxValue)
        {
            wasSeedUsed = true;
            return generator.Next(maxValue);
        }

        public int Next(int minValue, int maxValue)
        {
            wasSeedUsed = true;
            return generator.Next(minValue, maxValue);
        }

        public void NextBytes(byte[] buffer)
        {
            wasSeedUsed = true;
            generator.NextBytes(buffer);
        }

        private bool wasSeedUsed;
        private int instanceSeed;
        private Random generator;
    }
}

