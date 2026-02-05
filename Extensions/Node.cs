using System.Collections.Generic;
using Godot;

namespace CrossedDimensions.Extensions;

public static class NodeExtensions
{
    /// <summary>
    /// Calls a method on a GodotObject and attempts to cast the return value to
    /// the specified type.
    /// </summary>
    /// <typeparam name="TReturn">The expected return type.</typeparam>
    /// <param name="obj">The GodotObject to call the method on.</param>
    /// <param name="name">The name of the method to call.</param>
    /// <param name="args">The arguments to pass to the method.</param>
    /// <returns>
    /// The return value cast to the specified type, or null if the method
    /// does not exist or the return value cannot be cast.
    /// </returns>
    public static TReturn CallScriptMethod<TReturn>(
        this GodotObject obj,
        string name,
        params Variant[] args
    )
        where TReturn : GodotObject
    {
        if (obj.HasMethod(name))
        {
            var ret = obj.Call(name, args);
            if (ret.AsGodotObject() is TReturn typedRet)
            {
                return typedRet;
            }
        }

        return null;
    }

    /// <summary>
    /// Enumerates the children of a Node without returning an allocated array.
    /// </summary>
    /// <remarks>
    /// Godot's GetChildren() method returns a Godot.Collections.Array, which is
    /// enumerable, but allocated each time it's called. This method provides a
    /// way to iterate over the children without allocating an array.
    /// </remarks>
    /// <param name="node">The node whose children to enumerate.</param>
    /// <returns>An enumerable of the node's children.</returns>
    public static IEnumerable<Node> EnumerateChildren(this Node node)
    {
        int childCount = node.GetChildCount();

        for (int i = 0; i < childCount; i++)
        {
            yield return node.GetChild(i);

            // handle the case where the node gets freed during iteration
            if (!Godot.Node.IsInstanceValid(node) || node.IsQueuedForDeletion())
            {
                // decrease i and childCount to offset the upcoming increment
                i--;
                childCount--;
            }
        }
    }

    /// <summary>
    /// Checks if a node has a child node at the specified path and returns it.
    /// </summary>
    /// <typeparam name="T">The expected type of the child node.</typeparam>
    /// <param name="node">The node to check.</param>
    /// <param name="path">The path to the child node.</param>
    /// <param name="result">The child node if found; otherwise, <c>null</c>.</param>
    /// <returns>
    /// <c>true</c> if the node has a child at the specified path; otherwise,
    /// <c>false</c>.
    /// </returns>
    public static bool HasNode<T>(this Node node, string path, out T result)
        where T : Node
    {
        result = node.GetNodeOrNull<T>(path);
        return result != null;
    }
}
