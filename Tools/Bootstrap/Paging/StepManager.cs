//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Collections.Generic;
using System.Linq;

namespace Emul8.Bootstrap
{
    public class StepManager
    {
        public StepManager()
        {
            steps = new List<IStep>();
        }
        
        public StepManager AddStep(IStep step)
        {
            steps.Add(step);
            return this;
        }
        
        public T GetStep<T>()
        {
            return steps.OfType<T>().SingleOrDefault();
        }
        
        public IEnumerable<T> GetSteps<T>()
        {
            return steps.OfType<T>();
        }
        
        public IEnumerable<T> GetPreviousSteps<T>(IStep step)
        {
            return steps.TakeWhile(x => x != step).OfType<T>();
        }
        
        public void Run()
        {
            var currentDirection = 0;
            var currentIndex = 0;
            while(currentIndex < steps.Count)
            {
                PreviousStep = currentIndex > 0 ? steps[currentIndex - 1] : null;
                CurrentStep = steps[currentIndex];
                
                var result = CurrentStep.Run(this);
                switch(result)
                {
                case IStepResult.Next:
                    currentDirection = 1;
                    break;
                case IStepResult.Previous:
                    currentDirection = -1;
                    break;
                case IStepResult.Skip:
                    // do not change direction
                    break;
                default:
                    IsCancelled = true;
                    return;
                }
                
                currentIndex += currentDirection;
            }
        }
        
        public IStep CurrentStep { get; private set; }
        public IStep PreviousStep { get; private set; }
        
        public bool IsCancelled { get; private set; }
        
        private readonly List<IStep> steps;
    }
}

