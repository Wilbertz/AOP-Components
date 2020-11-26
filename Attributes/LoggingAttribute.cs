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
        private static int _firstIndentation = -1;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public override void OnEntry(MethodExecutionArgs args)
        {
            if (!(args.Method.DeclaringType is null))
            {
                var blubber =
                    $"{GetIndentation()}Init: {args.Method.DeclaringType.FullName}.{args.Method.Name} [{args.Arguments.Length}] params";
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
            if (_firstIndentation == -1)
            {
                _firstIndentation = st.FrameCount;
            }

            return String.Concat(Enumerable.Repeat("   ", Math.Max(st.FrameCount + additionalIndentation - _firstIndentation, 0)));
        }
    }
}
