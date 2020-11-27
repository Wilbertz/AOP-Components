using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using MethodBoundaryAspect.Fody.Attributes;
using NLog;

namespace AOP.Attributes
{
    public sealed class LoggingAttribute : OnMethodBoundaryAspect
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private static readonly Lazy<int> FirstIndentation = new Lazy<int>(() =>
        {
            Logger.Trace("Lazy Initialization called.");
            var st = new StackTrace(false);
            return st.FrameCount;
        });

        public override void OnEntry(MethodExecutionArgs args)
        {
            if (!(args.Method.DeclaringType is null))
            {
                Logger.Info($"{GetIndentation()}Init: {args.Method.DeclaringType.FullName}.{args.Method.Name} [{args.Arguments.Length}] params");
            }
            foreach (var item in args.Method.GetParameters())
            {
                Logger.Debug($"{GetIndentation()}{item.Name}: {args.Arguments[item.Position]}");
            }
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            if (args.ReturnValue is Task t)
            {
                t.ContinueWith(task =>
                {
                    if (task.GetType().IsGenericType)
                    {
                        dynamic taskWithResult = task;
                        Logger.Info($"{GetIndentation(1)}Exit: [{taskWithResult.Result}]");
                    }
                    else
                    {
                        Logger.Info($"{GetIndentation(1)}Exit: []");
                    }
                });
            }
            else
            {
                Logger.Info($"{GetIndentation()}Exit: [{args.ReturnValue}]");
            }
        }

        public override void OnException(MethodExecutionArgs args)
        {
            Logger.Error($"{GetIndentation()}OnException: {args.Exception.GetType()}: {args.Exception.Message}");
        }

        private string GetIndentation(int additionalIndentation = 0)
        {
            StackTrace st = new StackTrace(false);

            return String.Concat(Enumerable.Repeat("   ", Math.Max(st.FrameCount + additionalIndentation - FirstIndentation.Value, 0)));
        }
    }
}
