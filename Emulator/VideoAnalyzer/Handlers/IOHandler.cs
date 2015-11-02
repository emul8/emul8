//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Peripherals.Input;
using Xwt;
using Emul8.Extensions.Analyzers.Video.Events;

namespace Emul8.Extensions.Analyzers.Video.Handlers
{
    internal class IOHandler
    {
        public IOHandler(FrameBufferDisplayWidget widget)
        {
            this.widget = widget;
            this.source = new XWTEventSource(widget);
            this.source.AttachHandler(this);
        }

        public event Func<bool> GrabConfirm;

        public event Action<PointerHandler, PointerHandler> PointerInputAttached;

        public void MouseMoved(int x, int y, int dx, int dy)
        {
            var ph = pointerHandler;
            if(ph != null && status != Status.NotGrabbed)
            {
                ph.PointerMoved(x, y, dx, dy);
            }
        }

        public void ButtonPressed(PointerButton button)
        {
            var ph = pointerHandler;
            if(ph != null)
            {
                if(status == Status.NotGrabbed && button == PointerButton.Left)
                {
                    var gc = GrabConfirm;
                    if(gc != null && !gc())
                    {
                        return;
                    }

                    source.DetachHandler();
                    source = new X11EventSource(widget);
                    source.AttachHandler(this);
                    status = Status.Grabbed;
                    return;
                }

                ph.ButtonPressed((int)button);
            }
        }

        public void ButtonReleased(PointerButton button)
        {
            var ph = pointerHandler;
            if(ph != null)
            {
                ph.ButtonReleased((int)button);
            }
        }

        public void KeyPressed(KeyScanCode key)
        {
            var kh = keyboardHandler;

            lalt |= (key == KeyScanCode.AltL);
            lctrl |= (key == KeyScanCode.CtrlL);
            lshift |= (key == KeyScanCode.ShiftL);

            if(lctrl && lalt && lshift)
            {
                lalt = false;
                lctrl = false;
                lshift = false;

                if(status == Status.Grabbed)
                {
                    // ask if we should grab
                    source.DetachHandler();
                    source = new XWTEventSource(widget);
                    source.AttachHandler(this);
                    status = Status.NotGrabbed;
                }

                if(kh != null)
                {
                    kh.Release(KeyScanCode.AltL);
                    kh.Release(KeyScanCode.CtrlL);
                    kh.Release(KeyScanCode.ShiftL);
                }
                return;
            }

            if(kh != null)
            {
                kh.Press(key);
            }
        }

        public void KeyReleased(KeyScanCode key)
        {
            lalt &= (key != KeyScanCode.AltL);
            lctrl &= (key != KeyScanCode.CtrlL);
            lshift &= (key != KeyScanCode.ShiftL);

            var kh = keyboardHandler;
            if(kh != null)
            {
                kh.Release(key);
            }
        }

        public void Attach(IPointerInput pointer = null, IKeyboard keyboard = null)
        {
            if(pointer != null)
            {
                var previousPointerHandler = pointerHandler;

                if(pointer is IRelativePositionPointerInput)
                {
                    crossDrawable = false;
                    status = Status.NotGrabbed;
                    pointerHandler = new RelativePointerHandler((IRelativePositionPointerInput)pointer);
                }
                else if(pointer is IAbsolutePositionPointerInput)
                {
                    crossDrawable = true;
                    status = Status.NotGrabbable;
                    pointerHandler = new AbsolutePointerHandler((IAbsolutePositionPointerInput)pointer, widget);
                }

                var pia = PointerInputAttached;
                if(pia != null)
                {
                    pia(pointerHandler, previousPointerHandler);
                }
            }

            if(keyboard != null)
            {
                keyboardHandler = keyboard;
            }
        }

        public void Detach(bool pointer = false, bool keyboard = false)
        {
            if(pointer)
            {
                pointerHandler = null;
            }

            if(keyboard)
            {
                keyboardHandler = null;
            }
        }

        public bool DrawCross { get { return crossDrawable; } }

        public int X { get { return pointerHandler is AbsolutePointerHandler ? ((AbsolutePointerHandler)pointerHandler).X : -1; } }
        public int Y { get { return pointerHandler is AbsolutePointerHandler ? ((AbsolutePointerHandler)pointerHandler).Y : -1; } }

        private bool crossDrawable;
        private PointerHandler pointerHandler;
        private IKeyboard keyboardHandler;

        private IEventSource source;
        private FrameBufferDisplayWidget widget;

        private bool lctrl;
        private bool lalt;
        private bool lshift;

        private Status status;

        private enum Status
        {
            Grabbed,
            NotGrabbable,
            NotGrabbed
        }
    }
}
