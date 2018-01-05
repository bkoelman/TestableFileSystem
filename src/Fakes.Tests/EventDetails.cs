using System;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Tests
{
    internal sealed class EventDetails
    {
        [NotNull]
        public string EventName { get; }

        [NotNull]
        public EventArgs Args { get; }

        public EventDetails([NotNull] string eventName, [NotNull] EventArgs args)
        {
            Guard.NotNull(eventName, nameof(eventName));
            Guard.NotNull(args, nameof(args));

            EventName = eventName;
            Args = args;
        }
    }
}
