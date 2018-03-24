// Taken from https://developer.xamarin.com/samples/monotouch/ios11/ARKitPlacingObjects/
// A Xamarin port of the Placing Objects Sample from Apple
// MIT License


namespace Percept.ObjectExtensions
{
	public static class Matrix4Extensions
	{
		public static SceneKit.SCNVector3 Translation(this OpenTK.NMatrix4 self)
		{
			return new SceneKit.SCNVector3(self.M14, self.M24, self.M34);
		}

        public static SceneKit.SCNVector3 Translation(this SceneKit.SCNMatrix4 self)
        {
            return new SceneKit.SCNVector3(self.M41, self.M42, self.M43);
        }

        //TODO this does not work, why?
        //public static void Translate(this SceneKit.SCNMatrix4 self, SceneKit.SCNVector3 vec)
        //{
        //    self.M41 = self.M41 + vec.X;
        //    self.M42 = self.M42 + vec.Y;
        //    self.M43 = self.M43 + vec.Z;
        //}

        public static OpenTK.NMatrix4 Copy(this OpenTK.NMatrix4 self)
        {
            return new OpenTK.NMatrix4(self.M11, self.M12, self.M13, self.M14,
                                self.M21, self.M22, self.M23, self.M24,
                                self.M31, self.M32, self.M33, self.M34,
                                self.M41, self.M42, self.M43 , self.M44);
        }

        //SCNMatrix4 are column major, and opentk.nmatrix4 are row major
        public static SceneKit.SCNMatrix4 ToSceneKit(this OpenTK.NMatrix4 self)
        {
            //we transpose to get column major
            return new SceneKit.SCNMatrix4( self.M11, self.M21, self.M31, self.M41,
                                            self.M12, self.M22, self.M32, self.M42,
                                            self.M13, self.M23, self.M33, self.M43,
                                            self.M14, self.M24, self.M34, self.M44);
        }


        public static OpenTK.NMatrix4 ToOpenTK(this SceneKit.SCNMatrix4 self)
        {
            //we transpose to get column major
            return new OpenTK.NMatrix4( self.M11, self.M21, self.M31, self.M41,
                                        self.M12, self.M22, self.M32, self.M42,
                                        self.M13, self.M23, self.M33, self.M43,
                                        self.M14, self.M24, self.M34, self.M44);
        }

    }
}
