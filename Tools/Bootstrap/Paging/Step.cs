//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿namespace Emul8.Bootstrap
{
    public abstract class Step<T> : IStep where T: Dialog
    {
        public IStepResult Run(StepManager manager)
        {
            if(!ShouldBeShown(manager))
            {
                return IStepResult.Skip;
            }
            
            Dialog = CreateDialog();
            
            IStepResult result;
            switch(Dialog.Show())
            {
            case DialogResult.Ok:
                result = IStepResult.Next;
                break;
            case DialogResult.Back:
                result = IStepResult.Previous;
                break;
            default:
                result = IStepResult.Cancel;
                break;
            }
            
            if(result == IStepResult.Next)
            {
                OnSuccess();
            }
            
            return result;
        }
        
        protected virtual bool ShouldBeShown(StepManager m)
        {
            return true;
        }
        
        protected abstract T CreateDialog();
        
        protected virtual void OnSuccess()
        {
        }
        
        public T Dialog { get; private set; }
    }
}

