using System;

namespace Tiger.Loader.Lib
{
	interface IRemoteLoader_V1
	{}

	public delegate void MessageOutEventHandler(int fromID, string fromApp, int toID, string toApp, string messageText);

	public interface ISubApp_V1
	{
		Version	Version{get;}
		string	ShortName{get;}
		string	HelpText{get;}
		bool	Unloadable{get;}

		event	MessageOutEventHandler MessageOut;

		bool	Start();
		bool	Stop();
		bool	Restart();
		void	MessageIn(int fromID, string fromApp, int toID, string toApp, string messageText);
	}
}
