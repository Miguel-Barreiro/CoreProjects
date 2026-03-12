using XNode;

#nullable enable

namespace Core.VSEngine
{
    public abstract class NodeExecutionResult
    {
        private static readonly TerminateExecutionResult terminateExecutionResult = new TerminateExecutionResult();
        private static readonly PauseExecutionResult pauseExecutionResult = new PauseExecutionResult();

        /// <summary>
        /// Returns a static instance of TerminateExecutionResult
        /// </summary>
        public static NodeExecutionResult Terminate
        {
            get
            {
                return terminateExecutionResult;
            }
        }

        /// <summary>
        /// Returns a static instance of PauseExecutionResult
        /// </summary>
        public static NodeExecutionResult Pause
        {
            get
            {
                return pauseExecutionResult;
            }
        }

        /// <summary>
        /// If a next node is defined, it creates an instance of ContinueExecutionResult that references the next node, otherwise 
        /// it returns a static instance of TerminateExecutionResult to represent that the execution should terminate not having new nodes to execute
        /// </summary>
        /// <param name="nextNode">The node to execute to continue the execution. If null, this method will instead retrun TerminateExecutionResult</param>
        public static NodeExecutionResult ContinueWith(Node? nextNode)
        {
            if(nextNode == null)
            {
                return Terminate;
            }

            return new ContinueExecutionResult(nextNode);
        }

        public bool ShouldTerminate()
        {
            return this is TerminateExecutionResult;
        }

        public bool ShouldPause()
        {
            return this is PauseExecutionResult;
        }

        public bool ShouldContinue()
        {
            return this is ContinueExecutionResult;
        }

        public class TerminateExecutionResult : NodeExecutionResult
        {
            protected internal TerminateExecutionResult()
            {
            }
        }

        public class PauseExecutionResult : NodeExecutionResult
        {
            protected internal PauseExecutionResult()
            {
            }
        }

        public class ContinueExecutionResult : NodeExecutionResult
        {
            public readonly Node NextNode;

            protected internal ContinueExecutionResult(Node nextNode)
            {
                NextNode = nextNode;
            }
        }
    }

}