#nullable enable

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using XNode;

namespace Core.VSEngine.SharedNodes {
    /// <summary>
    /// If you would like your node to be compatible with all scripting systems, use this interface
    /// </summary>
    public interface ISharedNode {
        public Node Node { get; }

        public void ResolveControl(ICollection<Node> nextNodes) {
        }

        /// <summary>
        /// <b>NOTE:</b> this won't be called on synchronous scripts.
        /// </summary>
        public async UniTask ResolveControlAsync(ICollection<Node> nextNodes) {
            await UniTask.CompletedTask;

            ResolveControl(nextNodes);
        }

        public virtual object? ResolveValue(string fieldName) {
            NodePort? port = Node.GetPort(fieldName);

            if (port == null) {
                return null;
            }

            return ActionGraphUtil.ResolvePortValue(port);
        }

        /// <summary>
        /// Helper to easily type return values, don't override this.
        /// </summary>
        public sealed T? ResolveValue<T>(string fieldName) {
            return (T?)ResolveValue(fieldName);
        }
    }
}
