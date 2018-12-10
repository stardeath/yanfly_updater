using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace yanfly_updater
{
	public class Model : ViewModelBase
	{
		public Model()
		{
			CheckForUpdate = new RelayCommand( CheckForUpdateCommand );
			UpdateSelected = new RelayCommand( UpdateSelectedCommand );

			if( string.IsNullOrEmpty( Remote ) )
			{
				Remote = "http://yanfly.moe/plugins/en/";
			}
		}

		private string m_Local = "";
		public string Local
		{
			get
			{
				return App.Config["Directory"]["Local"].ToString();
			}
			set
			{
				if( Set( ref m_Local, value ) )
				{
					App.Config["Directory"]["Local"] = value;
				}
			}
		}

		private string m_Backup = "";
		public string Backup
		{
			get
			{
				return App.Config["Directory"]["Backup"].ToString();
			}
			set
			{
				if( Set( ref m_Backup, value ) )
				{
					App.Config["Directory"]["Backup"] = value;
				}
			}
		}

		private string m_Remote = "";
		public string Remote
		{
			get
			{
				return App.Config["Directory"]["Remote"].ToString();
			}
			set
			{
				if( Set( ref m_Remote, value ) )
				{
					App.Config["Directory"]["Remote"] = value;
				}
			}
		}

		private ObservableConcurrentDictionary<string, FileData> m_Dict = new ObservableConcurrentDictionary<string, FileData>();
		public ObservableConcurrentDictionary<string, FileData> Dict
		{
			get
			{
				return m_Dict;
			}
			private set
			{
				Set( ref m_Dict, value );
			}
		}


		public ICommand CheckForUpdate { get; private set; }
		public ICommand UpdateSelected { get; private set; }

		private DLManager m_DLManager = new DLManager();

		private string UrlConcat( string str0, string str1 )
		{
			if( str0.EndsWith( "/" ) )
			{
				return str0 + str1;
			}
			else
			{
				return str0 + '/' + str1;
			}
		}

		private string GetVersion( string file_content )
		{
			Regex[] regexes = new Regex[]
			{
				new Regex( "^.*Version: ([0-9.]*)$", RegexOptions.Multiline ),
				new Regex( "^.*@plugindesc v([0-9]*\\.[0-9]*).*$", RegexOptions.Multiline )
			};

			foreach( var regex in regexes )
			{
				var match = regex.Match( file_content );
				if( match.Success )
				{
					return match.Groups[1].Value;
				}
			}

			return "0x" + file_content.GetHashCode().ToString( "x" );
		}

		private void CheckLocalFiles()
		{
			if( !Directory.Exists( Local ) )
			{
				return;
			}

			var files = Directory.EnumerateFiles( Local, "*.js" );

			foreach( string current_file in files )
			{
				FileInfo fi = new FileInfo( current_file );
				try
				{
					Dict.Add( fi.Name, new FileData { Name = fi.Name } );
				}
				catch( Exception ex )
				{
					throw new Exception( "logic error : " + ex.Message );
				}
			}

			Task.Factory.StartNew
			(
				() =>
				{
					foreach( string current_file in files )
					{
						FileInfo fi = new FileInfo( current_file );

						FileData fd = Dict.FirstOrDefault( el => el.Key.Contains( fi.Name ) ).Value;
						if( fd != null )
						{
							string text = System.IO.File.ReadAllText( fi.FullName );
							fd.LocalVersion = GetVersion( text );
						}
						else
						{
							throw new Exception( "logic error" );
						}
					}
				}
			);
		}

		private void CheckRemoteFiles()
		{
			Dictionary<string, string> remote_file_list = new Dictionary<string, string>();

			var str = m_DLManager.Get( Remote );
			var str_list = str.Split( new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries );

			Regex regex = new Regex( ".*href=\"(.*.js)\"" );

			foreach( string l in str_list )
			{
				var matches = regex.Matches( l );

				if( matches.Count == 1 )
				{
					string name = matches[0].Groups[1].Value;

					remote_file_list.Add( name, UrlConcat( Remote, name ) );

					try
					{
						Dict.Add( name, new FileData { Name = name } );
					}
					catch( Exception ex )
					{
						throw new Exception( "logic error : " + ex.Message );
					}
				}
			}

			Task.Factory.StartNew
			(
				() =>
				{
					foreach( var element in remote_file_list )
					{
						string text = m_DLManager.Get( element.Value, element.Key );

						string version = GetVersion( text );

						FileData fd = Dict.FirstOrDefault( el => el.Key.Contains( element.Key ) ).Value;
						if( fd != null )
						{
							fd.RemoteVersion = version;
						}
						else
						{
							throw new Exception( "logic error" );
						}
					}
				}
			);
		}

		private void CheckForUpdateCommand()
		{
			CheckLocalFiles();
			CheckRemoteFiles();
		}

		private void UpdateSelectedCommand()
		{
			if( string.IsNullOrEmpty( Backup ) )
			{
				Backup = Path.Combine( Local, "backup" );
			}

			if( !Directory.Exists( Backup ) )
			{
				Directory.CreateDirectory( Backup );
			}

			foreach( var element in Dict )
			{
				if( element.Value.IsUpdatable )
				{
					string name_wo_ext = Path.GetFileNameWithoutExtension( element.Value.Name );
					string ext = Path.GetExtension( element.Value.Name );
					string new_name = name_wo_ext + "_" + element.Value.LocalVersion + ext;

					string src = Path.Combine( Local, element.Value.Name );
					string dst = Path.Combine( Backup, new_name );

					if( File.Exists( src ) )
					{
						Directory.Move( src, dst );
					}

					string content = m_DLManager.Get( "", element.Value.Name );

					var stream = File.CreateText( src );
					stream.Write( content );
					stream.Close();
				}
			}

			CheckLocalFiles();
		}
	}
}
