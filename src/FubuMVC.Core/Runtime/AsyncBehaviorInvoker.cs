using System;
using System.Threading.Tasks;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Registration.Nodes;

namespace FubuMVC.Core.Runtime
{
    public class AsyncBehaviorInvoker : BehaviorInvoker
    {
        public AsyncBehaviorInvoker(IBehaviorFactory factory, BehaviorChain chain) : base(factory, chain)
        {
        }

        protected override void Invoke(IEntrypointActionBehavior behavior, Action onComplete)
        {
            Task.Factory.StartNew(behavior.Invoke, TaskCreationOptions.AttachedToParent)
            .ContinueWith(x =>
            {
                behavior.Dispose();
                onComplete();
            }, TaskContinuationOptions.AttachedToParent);
        }
    }
}