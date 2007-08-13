using System;
using System.Reflection;

namespace Tiger.Loader.Lib
{
	[Serializable]
	public class RemoteLoader : MarshalByRefObject
	{
		public RemoteLoader()
		{}

		public object LoadObject(string directory, string filename, string className)
		{
			try
			{
				//AJ: random bit of code that maybe used in the future
				

				//AJ: Used to use LoadFile but that doesn't support dependancies 
				//Assembly	subAss	= Assembly.LoadFrom(directory + @"\" + filename);
                //Assembly subAss = Assembly.Load(directory + @"\" + filename);
                AssemblyName an = new AssemblyName();
				an.CodeBase = directory + @"\" + filename;
                Assembly subAss = Assembly.Load(an);


				return subAss.CreateInstance(className);
			}
			catch(Exception ex)
			{
				throw ex;
			}
		}
	}
}