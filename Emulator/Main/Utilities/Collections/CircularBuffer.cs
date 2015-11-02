//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using System.Collections;

namespace Emul8.Utilities.Collections
{
    public class CircularBuffer<T> : IEnumerable<T>
    {
        public CircularBuffer(int size)
        {
            buffer = new T[size];
        }

        public void Clear() 
        {
            wasOverflow = false;
            currentPosition = 0;
        }

        public void Add(T element)
        {
            buffer[currentPosition] = element;
            UpdateIndex();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if(!wasOverflow)
            {
                Array.Copy(buffer, array, currentPosition);
                return;
            }
            var start = currentPosition + 1;
            var rightSideLength = buffer.Length - start;
            Array.Copy(buffer, start, array, arrayIndex, rightSideLength);
            Array.Copy(buffer, 0, array, arrayIndex + rightSideLength, start - 1);
        }

        public IEnumerator<T> GetEnumerator()
        {
            if(wasOverflow)
            {
                var end = currentPosition;
                var currentYield = currentPosition + 1;
                while(currentYield != end)
                {
                    yield return buffer[currentYield];
                    currentYield++;
                    if(currentYield == buffer.Length)
                    {
                        currentYield = 0;
                    }
                }
            }
            else
            {
                for(var i = 0; i < currentPosition; i++)
                {
                    yield return buffer[i];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void UpdateIndex()
        {
            currentPosition++;
            if(currentPosition == buffer.Length)
            {
                wasOverflow = true;
                currentPosition = 0;
            }
        }

        private readonly T[] buffer;
        private int currentPosition;
        private bool wasOverflow;
    }
}

