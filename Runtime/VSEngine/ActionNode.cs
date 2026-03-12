using XNode;
using System.Collections.Generic;
using UnityEngine;


#nullable enable

namespace Core.VSEngine
{
    // [Node.NodeTint("#d214e3")]
    // public abstract class ActionNode : VSNodeBase
    // {
    //     
    //     public virtual object? ResolveValue(string portName)
    //     {
    //         NodePort? port = GetPort(portName);
    //
    //         if (port == null)
    //         {
    //             Debug.LogError($"Port {portName} is null");
    //
    //             return null;
    //         }
    //
    //         return ActionGraphUtil.ResolvePortValue(port);
    //     }
    //
    //     /// <summary>
    //     /// Only used for combat, consider using <see cref="Node.GetNodeFromPort(string)"/>
    //     /// </summary>
    //     protected Node? GetExitToNode()
    //     {
    //         NodePort? exitPort = GetOutputPort("exit")?.Connection;
    //         
    //         if (exitPort != null)
    //         {
    //             return exitPort.node;
    //         }
    //
    //         return null;
    //     }
    //
    //     protected Node? GetConnectedNode(string portName)
    //     {
    //         NodePort? port = GetOutputPort(portName).Connection;
    //         if (port != null)
    //         {
    //             if (port.IsConnected)
    //             {
    //                 return port.Connection.node;
    //             }
    //         }
    //         return null;
    //     }
    //     
    //     protected virtual object? ResolveMultiValue(string portName, int index)
    //     {
    //         NodePort? port = GetPort($"{portName} {index}");
    //
    //         if (port == null) {
    //             // Log.LogError($"Port {portName} {index} is null");
    //             return null;
    //         }
    //
    //         return ActionGraphUtil.ResolvePortValue(port);
    //     }
    //
    //     protected Node? GetConnectedMultiNode(string portName, int index)
    //     {
    //         NodePort? exitPort = GetPort($"{portName} {index}");
    //
    //         if (exitPort == null)
    //         {
    //             return null;
    //         }
    //
    //         return GetNodeFromPort(exitPort);
    //     }
    //
    //
    //
    //     /// <summary>
    //     /// This isn't used anywhere, it's probably just here to suppress xNode warnings.
    //     /// </summary>
    //     public override object? GetValue(NodePort port)
    //     {
    //         return null;
    //     }
    // }
}
