using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace yanfly_updater
{
	public class FileData : ObservableObject
	{
		private string m_Name;
		public string Name
		{
			get
			{
				return m_Name;
			}
			set
			{
				Set( ref m_Name, value );
			}
		}

		private string m_LocalVersion;
		public string LocalVersion
		{
			get
			{
				return m_LocalVersion;
			}
			set
			{
				Set( ref m_LocalVersion, value );

				CheckUpdatableState();
			}
		}

		private string m_RemoteVersion;
		public string RemoteVersion
		{
			get
			{
				return m_RemoteVersion;
			}
			set
			{
				Set( ref m_RemoteVersion, value );

				CheckUpdatableState();
			}
		}

		private bool m_IsUpdatable = false;
		public bool IsUpdatable
		{
			get
			{
				return m_IsUpdatable;
			}
			set
			{
				Set( ref m_IsUpdatable, value );
			}
		}

		private void CheckUpdatableState()
		{
			if( string.IsNullOrEmpty( RemoteVersion ) )
			{
				IsUpdatable = false;
				return;
			}

			if( string.IsNullOrEmpty( LocalVersion ) && !string.IsNullOrEmpty( RemoteVersion ) )
			{
				IsUpdatable = true;
				return;
			}

			double local;
			bool parse_local_result = double.TryParse( LocalVersion, NumberStyles.Currency, CultureInfo.InvariantCulture, out local );

			double remote;
			bool parse_remote_result = double.TryParse( RemoteVersion, NumberStyles.Currency, CultureInfo.InvariantCulture, out remote );

			if( parse_local_result && parse_remote_result )
			{
				IsUpdatable = remote > local;
				return;
			}
			else if( parse_local_result && !parse_remote_result )
			{
				IsUpdatable = false;
				return;
			}
			else if( !parse_local_result && parse_remote_result )
			{
				IsUpdatable = true;
				return;
			}
			else if( LocalVersion == RemoteVersion )
			{
				IsUpdatable = false;
				return;
			}
			else
			{
				IsUpdatable = true;
				return;
			}
		}
	}
}
