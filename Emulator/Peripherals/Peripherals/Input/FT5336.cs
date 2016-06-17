//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using Emul8.Logging;
using Emul8.Peripherals.I2C;
using System.Linq;
using Emul8.Utilities;

namespace Emul8.Peripherals.Input
{
    /// <summary>
    /// This class differs from FT5x06. Although it was based on FT5x06 datasheet, it is inconsistent
    /// with the Linux driver we used to create FT5x06.cs.
    /// This name is used because of STM32F7 Cube, providing such a driver.
    /// </summary>
    public class FT5336 : II2CPeripheral, IAbsolutePositionPointerInput
    {
        public FT5336(Machine machine, bool isRotated = false)
        {
            this.machine = machine;
            this.isRotated = isRotated;
            IRQ = new GPIO();
            Reset();
        }

        public void Reset()
        {
            IRQ.Unset();
            currentReturnValue = null;
            lastWriteRegister = 0;
            for(ushort i = 0; i < touchedPoints.Length; ++i)
            {
                touchedPoints[i] = new TouchedPoint {
                    Type = PointType.Reserved,
                    X = 0,
                    Y = 0,
                    Id = i
                };
            }
        }

        public void Write(byte[] data)
        {
            lastWriteRegister = (Registers)data[0];
            if((int)lastWriteRegister < TouchEndOffset && (int)lastWriteRegister >= TouchBeginOffset)
            {
                PrepareTouchData((byte)(lastWriteRegister - TouchBeginOffset), ((int)lastWriteRegister - TouchBeginOffset) / (TouchEndOffset - TouchBeginOffset));
                return;
            }
            switch(lastWriteRegister)
            {
            case Registers.TouchDataStatus:
                SetReturnValue((byte)touchedPoints.Count(x => x.Type == PointType.Contact || x.Type == PointType.Down));
                break;
            case Registers.InterruptStatus:
                break;
            case Registers.ChipVendorId:
                SetReturnValue(ChipVendorId);
                break;
            default:
                this.Log(LogLevel.Warning, "Unhandled write to offset 0x{0:X}{1:X}.", lastWriteRegister,
                    data.Length == 1 ? String.Empty : ", values {0}".FormatWith(data.Skip(1).Select(x => "0x" + x.ToString("X")).Stringify(", ")));
                break;
            }
        }

        public byte[] Read()
        {
            return currentReturnValue;
        }

        public void MoveTo(int x, int y)
        {
            machine.ReportForeignEvent(x, y, MoveToInner);
            Update();
        }

        public void Press(MouseButton button = MouseButton.Left)
        {
            machine.ReportForeignEvent(button, PressInner);
            Update();
        }

        public void Release(MouseButton button = MouseButton.Left)
        {
            machine.ReportForeignEvent(button, ReleaseInner);
            Update();
        }

        public int MaxX { get; set; }

        public int MaxY { get; set; }

        public int MinX
        {
            get
            {
                return 0;
            }
        }

        public int MinY
        {
            get
            {
                return 0;
            }
        }

        public GPIO IRQ { get; private set; }

        private void Update()
        {
            IRQ.Set(touchedPoints.Any(x => x.Type == PointType.Down || x.Type == PointType.Contact));
        }

        private void PrepareTouchData(byte offset, int pointNumber)
        {
            switch((TouchDataRegisters)offset)
            {
            case TouchDataRegisters.TouchXLow:
                SetReturnValue(touchedPoints[pointNumber].X.LoByte());
                break;
            case TouchDataRegisters.TouchXHigh:
                SetReturnValue((byte)(((int)touchedPoints[pointNumber].Type << 6) | (touchedPoints[pointNumber].X.HiByte() & 0xF)));
                break;
            case TouchDataRegisters.TouchYLow:
                SetReturnValue(touchedPoints[pointNumber].Y.LoByte());
                break;
            case TouchDataRegisters.TouchYHigh:
                SetReturnValue((byte)((touchedPoints[pointNumber].Id << 4) | (touchedPoints[pointNumber].Y.HiByte() & 0xF)));
                break;
            case TouchDataRegisters.TouchWeight:
                break;
            case TouchDataRegisters.TouchMisc:
                break;
            default:
                throw new Exception("Should not reach here.");
            }
        }

        private void SetReturnValue(params byte[] bytes)
        {            
            currentReturnValue = bytes;
        }

        private void MoveToInner(int x, int y)
        {
            if(!isRotated)
            {
                touchedPoints[0].X = (ushort)x;
                touchedPoints[0].Y = (ushort)y;
            }
            else
            {
                touchedPoints[0].X = (ushort)y;
                touchedPoints[0].Y = (ushort)x;
            }
            if(touchedPoints[0].Type == PointType.Down || touchedPoints[0].Type == PointType.Contact)
            {
                this.NoisyLog("Moving the pointer at {0}x{1}", touchedPoints[0].X, touchedPoints[0].Y);
                touchedPoints[0].Type = PointType.Contact;
            }
        }

        private void PressInner(MouseButton button)
        {
            this.NoisyLog("Pressing the pointer at {0}x{1}", touchedPoints[0].X, touchedPoints[0].Y);
            touchedPoints[0].Type = PointType.Contact;
        }

        private void ReleaseInner(MouseButton button)
        {
            this.NoisyLog("Releasing the pointer at {0}x{1}", touchedPoints[0].X, touchedPoints[0].Y);
            touchedPoints[0].Type = PointType.Up;
        }

        private byte[] currentReturnValue;
        private Registers lastWriteRegister;
        private readonly bool isRotated;
        private readonly Machine machine;

        private readonly TouchedPoint[] touchedPoints = new TouchedPoint[5];

        private const byte TouchBeginOffset = 0x3;
        private const byte TouchEndOffset = 0x21;
        private const byte ChipVendorId = 0x51;

        private struct TouchedPoint
        {
            public UInt16 X;
            public UInt16 Y;
            public UInt16 Id;
            public PointType Type;
        }

        private enum PointType
        {
            Down = 0,
            Up = 1,
            Contact = 2,
            Reserved = 3
        }

        private enum TouchDataRegisters
        {
            TouchXHigh = 0x0,
            TouchXLow = 0x1,
            TouchYHigh = 0x2,
            TouchYLow = 0x3,
            TouchWeight = 0x4,
            TouchMisc = 0x5,
        }

        private enum Registers
        {
            GestureId = 0x1,
            TouchDataStatus = 0x2,
            InterruptStatus = 0xA4,
            ChipVendorId = 0xA8
        }
    }
}

