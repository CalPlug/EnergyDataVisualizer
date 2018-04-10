// Taken from https://developer.xamarin.com/samples/monotouch/ios11/ARKitPlacingObjects/
// A Xamarin port of the Placing Objects Sample from Apple
// MIT License

using Foundation;
using Percept.SCNNodes;


namespace Percept
{
	public interface IVirtualObjectManagerDelegate
	{
        void CouldNotPlace(VirtualObject virtualObject);
        void ObjectTapped(VirtualObject virtualObject);
        void NothingTapped();
        void TransformDidChangeFor(VirtualObject virtualObject);
        void TranslationFinishedFor(VirtualObject virtualObject);
    }
}
