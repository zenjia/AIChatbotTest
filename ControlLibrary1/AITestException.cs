using System;

namespace AiTest
{
    public class AITestException : Exception
    {
        public AITestException(string msg)
            : base(msg)
        {

        }
    }

    public class AITestConnectFailedException : Exception
    {
        public AITestConnectFailedException(string msg)
            : base(msg)
        {

        }
    }

    public class AITestAuthFailedException : AITestException
    {
        public AITestAuthFailedException(string msg) : base(msg)
        {
        }
    }

    public class AITestTimeoutException : AITestException
    {
        public AITestTimeoutException(string msg) : base(msg)
        {
        }
    }

    public class AITestReadTimeoutException : AITestException
    {
        public AITestReadTimeoutException(string msg) : base(msg)
        {
        }
    }
}