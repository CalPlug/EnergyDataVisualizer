// Taken from https://developer.xamarin.com/samples/monotouch/ios11/ARKitPlacingObjects/
// A Xamarin port of the Placing Objects Sample from Apple
// MIT License
using Foundation;
using SceneKit;
using System.Collections.Generic;
using System.Linq;

namespace Percept.SCNNodes
{
    //Base class for non static/scene file (not SCNReferenceNode)  3d objects that we display to the user.
    //It's purpose is to provide a set of data / utility functions that makes interactions between the object and the world/user more standardized.
    //e.g. how the object interacts with gestures.
    public class VirtualObject : SCNNode
	{
		// Use average of recent virtual object distances to avoid rapid changes in object scale.
		List<float> recentVirtualObjectDistances = new List<float>();
		public List<float> RecentVirtualObjectDistances { get => recentVirtualObjectDistances; }

        //Check if the node belongs to a parent of a certain type.
        //If the parent of that type exists, return the parent as a SCNNode,
        //otherwise return null.
		public static SCNNode GetParentOfType<T>(SCNNode node)
		{
			// End recursion on success
			if (node is T)
			{
				return node;
			}
			// End recursion because we've gotten to the root object with no parent
			if (node.ParentNode == null)
			{
				return null;
			}
			// Recurse up the scene graph
			return GetParentOfType<T>(node.ParentNode);
		}
	}
}
