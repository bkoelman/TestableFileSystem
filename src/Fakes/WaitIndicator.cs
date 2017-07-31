using JetBrains.Annotations;

namespace TestableFileSystem.Fakes
{
    public class WaitIndicator
    {
        [NotNull]
        private readonly object lockObject = new object();

        private State state;

        [NotNull]
        public static readonly WaitIndicator None = new NoWaitIndicator();

        public virtual void Reset()
        {
            lock (lockObject)
            {
                state = State.Initial;
            }
        }

        internal virtual void SetStarted()
        {
            lock (lockObject)
            {
                state = State.Started;
            }
        }

        public virtual void SetCompleted()
        {
            lock (lockObject)
            {
                state = State.Completed;
            }
        }

        public virtual void WaitForStart()
        {
            while (true)
            {
                lock (lockObject)
                {
                    if (state != State.Initial)
                    {
                        return;
                    }
                }
            }
        }

        internal virtual void WaitForComplete()
        {
            while (true)
            {
                lock (lockObject)
                {
                    if (state == State.Completed)
                    {
                        return;
                    }
                }
            }
        }

        private enum State
        {
            Initial,
            Started,
            Completed
        }

        private sealed class NoWaitIndicator : WaitIndicator
        {
            public override void Reset()
            {
            }

            internal override void SetStarted()
            {
            }

            public override void SetCompleted()
            {
            }

            public override void WaitForStart()
            {
            }

            internal override void WaitForComplete()
            {
            }
        }
    }
}
