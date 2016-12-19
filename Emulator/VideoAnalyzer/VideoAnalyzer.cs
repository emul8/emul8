//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Backends.Video;
using Emul8.Peripherals.Video;
using Emul8.Core;
using Emul8.Peripherals.Input;
using Xwt;
using Emul8.Utilities;
using Antmicro.Migrant;
using System.IO;
using Xwt.Drawing;
using System.Collections.Generic;
using Emul8.Peripherals;
using System;
using Emul8.CLI;

namespace Emul8.Extensions.Analyzers.Video
{
    [Transient]
    public class VideoAnalyzer : GUIPeripheralBackendAnalyzer<VideoBackend>, IExternal, IConnectable<IPointerInput>, IConnectable<IKeyboard>
    {
        public override Widget Widget { get { return analyserWidget; } }

        public void AttachTo(IKeyboard keyboardToAttach)
        {
            displayWidget.AttachTo(keyboardToAttach);
        }

        public void AttachTo(IPointerInput inputToAttach)
        {
            displayWidget.AttachTo(inputToAttach);
        }

        public void DetachFrom(IPointerInput inputToDetach)
        {
            displayWidget.DetachFrom(inputToDetach);
        }

        public void DetachFrom(IKeyboard keyboardToDetach)
        {
            displayWidget.DetachFrom(keyboardToDetach);
        }

        protected override void OnAttach(VideoBackend backend)
        {
            Init((AutoRepaintingVideo)backend.Video);
        }
        
        private void Init(AutoRepaintingVideo videoPeripheral)
        {
            element = videoPeripheral;
            lastRewrite = CustomDateTime.Now;
            EnsureAnalyserWidget();

            videoPeripheral.ConfigurationChanged += (w, h, f, e) => ApplicationExtensions.InvokeInUIThread(() => displayWidget.SetDisplayParameters(w, h, f, e));
            videoPeripheral.FrameRendered += displayWidget.DrawFrame;

            displayWidget.InputAttached += i =>
            {
                if (i is IKeyboard)
                {
                    keyboardsComboBox.SelectedItem = i;
                }
                else if (i is IPointerInput)
                {
                    pointersComboBox.SelectedItem = i;
                }
            };
        }

        private void EnsureAnalyserWidget()
        {
            var emulation = EmulationManager.Instance.CurrentEmulation;

            if(analyserWidget == null)
            {
                // create display widget and attach it to the emulation
                displayWidget = new FrameBufferDisplayWidget();

                var keyboards = FindKeyboards();
                var pointers = FindPointers();

                // create other widgets
                var displayModeComboBox = new ComboBox();
                displayModeComboBox.Items.Add(DisplayMode.Stretch);
                displayModeComboBox.Items.Add(DisplayMode.Fit);
                displayModeComboBox.Items.Add(DisplayMode.Center);

                displayModeComboBox.SelectionChanged += (sender, e) => displayWidget.Mode = (DisplayMode)displayModeComboBox.SelectedItem;
                ApplicationExtensions.InvokeInUIThread(() => 
                {
                    displayModeComboBox.SelectedIndex = 1;
                });

                keyboardsComboBox = new ComboBox();
                if(keyboards != null)
                {
                    foreach(var kbd in keyboards)
                    {
                        string name;
                        emulation.TryGetEmulationElementName(kbd, out name);
                        keyboardsComboBox.Items.Add(kbd, name);
                    }
                    keyboardsComboBox.SelectionChanged += (sender, e) =>
                        emulation.Connector.Connect((IKeyboard)keyboardsComboBox.SelectedItem, displayWidget);
                }
                keyboardsComboBox.SelectedIndex = 0;

                pointersComboBox = new ComboBox();
                if(pointers != null)
                {
                    foreach(var ptr in pointers)
                    {
                        string name;
                        emulation.TryGetEmulationElementName(ptr, out name);
                        pointersComboBox.Items.Add(ptr, name);
                    }
                    pointersComboBox.SelectionChanged += (sender, e) =>
                        emulation.Connector.Connect((IPointerInput)pointersComboBox.SelectedItem, displayWidget);
                }
                pointersComboBox.SelectedIndex = 0;

                var snapshotButton = new Button("Take screenshot!");
                snapshotButton.Clicked += (sender, e) =>
                {
                    var emul8HomeDir = Misc.GetUserDirectory();
                    var screenshotDir = Path.Combine(emul8HomeDir, "screenshots");
                    Directory.CreateDirectory(screenshotDir);
                    var filename = Path.Combine(screenshotDir, string.Format("screenshot-{0:yyyy_M_d_HHmmss}.png", CustomDateTime.Now));
                    displayWidget.SaveCurrentFrameToFile(filename);
                    MessageDialog.ShowMessage("Screenshot saved in {0}".FormatWith(filename));
                };

                var configurationPanel = new HBox();
                configurationPanel.PackStart(new Label("Display mode:"));
                configurationPanel.PackStart(displayModeComboBox);
                configurationPanel.PackStart(new Label(), true);
                configurationPanel.PackStart(new Label("Keyboard:"));
                configurationPanel.PackStart(keyboardsComboBox);
                configurationPanel.PackStart(new Label("Pointer:"));
                configurationPanel.PackStart(pointersComboBox);
                configurationPanel.PackStart(new Label(), true);
                configurationPanel.PackStart(snapshotButton);

                var svc = new VBox();
                svc.PackStart(configurationPanel);
                svc.PackStart(new Label());
                var sv = new ScrollView();
                sv.Content = svc;
                sv.HeightRequest = 50;
                sv.BorderVisible = false;
                sv.VerticalScrollPolicy = ScrollPolicy.Never;

                var summaryVB = new HBox();
                var resolutionL = new Label("unknown");
                displayWidget.DisplayParametersChanged += (w, h, f) => ApplicationExtensions.InvokeInUIThread(() => resolutionL.Text = string.Format("{0} x {1} ({2})", w, h, f));
                summaryVB.PackStart(new Label("Resolution: "));
                summaryVB.PackStart(resolutionL);
                summaryVB.PackStart(new Label(), true);
                var cursorPositionL = new Label("unknown");
                displayWidget.PointerMoved += (x, y) => ApplicationExtensions.InvokeInUIThread(() => cursorPositionL.Text = (x == -1 && y == -1) ? "unknown" : string.Format("{0} x {1}", x, y));
                summaryVB.PackStart(new Label("Cursor position: "));
                summaryVB.PackStart(cursorPositionL);
                summaryVB.PackStart(new Label(), true);
                summaryVB.PackStart(new Label("Framerate: "));
                framerateL = new Label("unknown");
                displayWidget.FrameDrawn += RefreshFramerate;
                summaryVB.PackStart(framerateL);

                var vbox = new VBox();
                vbox.PackStart(sv);
                vbox.PackStart(displayWidget, true, true);
                vbox.PackStart(summaryVB);
                analyserWidget = vbox;
            }
        }

        private IEnumerable<IKeyboard> FindKeyboards()
        {
            Machine machine;
            return EmulationManager.Instance.CurrentEmulation.TryGetMachineForPeripheral(element, out machine) ? machine.GetPeripheralsOfType<IKeyboard>() : null;
        }

        private IEnumerable<IPointerInput> FindPointers()
        {
            Machine machine;
            return EmulationManager.Instance.CurrentEmulation.TryGetMachineForPeripheral(element, out machine) ? machine.GetPeripheralsOfType<IPointerInput>() : null;
        }

        private void RefreshFramerate()
        {
            var now = CustomDateTime.Now;
            if(prev == null)
            {
                prev = now;
                return;
            }

            if((now - lastRewrite).TotalSeconds > 1)
            {
                framerateL.Text = string.Format("{0} fps", (int)(1 / (now - prev).Value.TotalSeconds));
                lastRewrite = now;
            }
            prev = now;
        }

        private FrameBufferDisplayWidget displayWidget;
        private Widget analyserWidget;
        private ComboBox keyboardsComboBox;
        private ComboBox pointersComboBox;
        private IPeripheral element;
        private Label framerateL;
        private DateTime? prev;
        private DateTime lastRewrite;
    }
}

