using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;

namespace yanfly_updater
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			Exit += App_Exit;

			Config = new IniFile();
			try
			{
				Config.Load( "config.ini" );
			}
			catch( FileNotFoundException ex )
			{
				Config["Directory"]["Local"] = "";
				Config["Directory"]["Backup"] = "";
				Config["Directory"]["Remote"] = "";
			}
		}

		private void App_Exit( object sender, ExitEventArgs e )
		{
			Config.Save( "config.ini" );
		}

		private static IniFile m_Config;
		public static IniFile Config
		{
			get
			{
				return m_Config;
			}
			private set
			{
				m_Config = value;
			}
		}
	}
}
