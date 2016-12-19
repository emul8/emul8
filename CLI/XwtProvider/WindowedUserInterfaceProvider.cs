//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System;
using Xwt;
using System.Collections.Generic;
using Emul8.Peripherals;
using Emul8.UserInterface;

namespace Emul8.CLI
{
    public class WindowedUserInterfaceProvider : IUserInterfaceProvider
    {
        public void ShowAnalyser(IAnalyzableBackendAnalyzer analyzer, string name)
        {
            var guiWidget = analyzer as IHasWidget;
            if(guiWidget == null)
            {
                throw new ArgumentException("Wrong analyzer provided, expected object of type 'IHasGUIWidget'");
            }
            
            var window = new Window();
            window.Title = name;
            window.Height = 600;
            window.Width = 800;
            
            window.Content = guiWidget.Widget;
            
            openedWindows.Add(analyzer, window);
            window.Closed += (sender, e) => openedWindows.Remove(analyzer);
            
            window.Show();
        }

        public void HideAnalyser(IAnalyzableBackendAnalyzer analyzer)
        {
            var guiAnalyzer = analyzer as IHasWidget;
            if(guiAnalyzer == null)
            {
                throw new ArgumentException("Wrong analyzer provided, expected object of type 'IHasGUIWidget'");
            }
            
            Window win;
            if(openedWindows.TryGetValue(analyzer, out win))
            {
                win.Close();
                openedWindows.Remove(analyzer);
            }
        }
        
        private readonly Dictionary<IAnalyzableBackendAnalyzer, Window> openedWindows = new Dictionary<IAnalyzableBackendAnalyzer, Window>();
    }
}

