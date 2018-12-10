using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace yanfly_updater
{
	public class DLManager
	{
		private Dictionary<string, string> m_Cache = new Dictionary<string, string>();

		public string Get( string url, string key = null )
		{
			string url_key = url;
			if( !string.IsNullOrEmpty( key ) )
			{
				url_key = key;
			}

			string content = m_Cache.FirstOrDefault( dict => dict.Key == url_key ).Value;
			if( content != null )
			{
				return content;
			}

			using( var client = new WebClient() )
			{
				var str = client.DownloadString( url );
				m_Cache.Add( url_key, str );
				return str;
			}
		}
	}
}
