#region TigerMUD License
/*
/-------------------------------------------------------------\
|    _______  _                     __  __  _    _  _____     |
|   |__   __|(_)                   |  \/  || |  | ||  __ \    |
|      | |    _   __ _   ___  _ __ | \  / || |  | || |  | |   |
|      | |   | | / _` | / _ \| '__|| |\/| || |  | || |  | |   |
|      | |   | || (_| ||  __/| |   | |  | || |__| || |__| |   |
|      |_|   |_| \__, | \___||_|   |_|  |_| \____/ |_____/    |
|                 __/ |                                       |
|                |___/                  Copyright (c) 2004    |
\-------------------------------------------------------------/

TigerMUD. A Multi User Dungeon engine.
Copyright (C) 2004 Adam Miller et al.

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; either version 2 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

You can contact the TigerMUD developers at www.tigermud.com or at
http://sourceforge.net/projects/tigermud.

The full licence can be found in <root>/docs/TigerMUD_license.txt
*/
#endregion

using System;
using System.Collections;
using System.ComponentModel;
using System.ServiceProcess;
using System.Configuration.Install;

namespace TigerMUD
{
	/// <summary>
	/// Used by installutil.exe to install TigerMUD as a service.
	/// Try installutil.exe 'tigermud.exe' to install TigerMUD as a service.
	/// </summary>
  [RunInstaller(true)]
  public class TigerMUDServiceInstaller : System.Configuration.Install.Installer
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;
    private ServiceInstaller serviceInstaller;
    private ServiceProcessInstaller processInstaller;

    public TigerMUDServiceInstaller()
    {
      // This call is required by the Designer.
      InitializeComponent();

      InstallTigerMUDService();
    }

    public void InstallTigerMUDService()
    {
      serviceInstaller = new ServiceInstaller();
      processInstaller = new ServiceProcessInstaller();

      // Use the local system account
      processInstaller.Account = ServiceAccount.LocalSystem;

      // Automatically started
      serviceInstaller.StartType = ServiceStartMode.Automatic;

      // The service name
      serviceInstaller.ServiceName = "TigerMUD Service";

      // Add the installer to the collection
      Installers.Add(serviceInstaller);
      Installers.Add(processInstaller);
    }

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}


		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion
	}
}
