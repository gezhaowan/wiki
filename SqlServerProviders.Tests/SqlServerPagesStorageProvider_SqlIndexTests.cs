﻿namespace SqlServerProviders.Tests
{
	using System;
	using System.Data.SqlClient;
	using System.IO;
	using NUnit.Framework;
	using Rhino.Mocks;
	using ScrewTurn.Wiki.PluginFramework;
	using ScrewTurn.Wiki.Plugins.SqlServer;
	using ScrewTurn.Wiki.SearchEngine;
	using ScrewTurn.Wiki.SearchEngine.Tests;

	[TestFixture]
	public class SqlServerPagesStorageProvider_SqlIndexTests : IndexBaseTests
	{

		//private const string ConnString = "Data Source=(local)\\SQLExpress;User ID=sa;Password=password;";
		private const string ConnString = "Data Source=(local)\\SQLExpress;Integrated Security=SSPI;";
		private const string InitialCatalog = "Initial Catalog=ScrewTurnWikiTest;";

		private readonly MockRepository _mocks = new MockRepository( );
		private readonly string _testDir = Path.Combine( Environment.GetEnvironmentVariable( "TEMP" ), Guid.NewGuid( ).ToString( ) );

		private delegate string ToStringDelegate( PageInfo p, string input );

		protected IHostV30 MockHost( )
		{
			if ( !Directory.Exists( _testDir ) ) Directory.CreateDirectory( _testDir );

			IHostV30 host = _mocks.DynamicMock<IHostV30>( );
			Expect.Call( host.GetSettingValue( SettingName.PublicDirectory ) ).Return( _testDir ).Repeat.AtLeastOnce( );
			Expect.Call( host.PrepareContentForIndexing( null, null ) ).IgnoreArguments( ).Do( (ToStringDelegate)( ( p, input ) => input ) ).Repeat.Any( );
			Expect.Call( host.PrepareTitleForIndexing( null, null ) ).IgnoreArguments( ).Do( (ToStringDelegate)( ( p, input ) => input ) ).Repeat.Any( );

			_mocks.Replay( host );

			return host;
		}

		public IPagesStorageProviderV30 GetProvider( )
		{
			SqlServerPagesStorageProvider prov = new SqlServerPagesStorageProvider( );
			prov.Init( MockHost( ), ConnString + InitialCatalog );
			return prov;
		}

		[TestFixtureSetUp]
		public void FixtureSetUp( )
		{
			// Create database with no tables
			SqlConnection cn = new SqlConnection( ConnString );
			cn.Open( );

			SqlCommand cmd = cn.CreateCommand( );
			cmd.CommandText = "if (select count(*) from sys.databases where [Name] = 'ScrewTurnWikiTest') = 0 begin create database [ScrewTurnWikiTest] end";
			cmd.ExecuteNonQuery( );

			cn.Close( );
		}

		[TearDown]
		public void TearDown( )
		{
			try
			{
				Directory.Delete( _testDir, true );
			}
			catch { }

			// Clear all tables
			SqlConnection cn = new SqlConnection( ConnString );
			cn.Open( );

			SqlCommand cmd = cn.CreateCommand( );
			cmd.CommandText = "use [ScrewTurnWikiTest]; delete from [IndexWordMapping]; delete from [IndexWord]; delete from [IndexDocument]; delete from [ContentTemplate]; delete from [Snippet]; delete from [NavigationPath]; delete from [Message]; delete from [PageKeyword]; delete from [PageContent]; delete from [CategoryBinding]; delete from [Page]; delete from [Category]; delete from [Namespace] where [Name] <> '';";
			try
			{
				cmd.ExecuteNonQuery( );
			}
			catch ( SqlException sqlex )
			{
				Console.WriteLine( sqlex.ToString( ) );
			}

			cn.Close( );
		}

		[TestFixtureTearDown]
		public void FixtureTearDown( )
		{
			// Delete database
			SqlConnection cn = new SqlConnection( ConnString );
			cn.Open( );

			SqlCommand cmd = cn.CreateCommand( );
			cmd.CommandText = "alter database [ScrewTurnWikiTest] set single_user with rollback immediate";
			try
			{
				cmd.ExecuteNonQuery( );
			}
			catch ( SqlException sqlex )
			{
				Console.WriteLine( sqlex.ToString( ) );
			}

			cmd = cn.CreateCommand( );
			cmd.CommandText = "drop database [ScrewTurnWikiTest]";
			try
			{
				cmd.ExecuteNonQuery( );
			}
			catch ( SqlException sqlex )
			{
				Console.WriteLine( sqlex.ToString( ) );
			}

			cn.Close( );

			// This is neede because the pooled connection are using a session
			// that is now invalid due to the commands executed above
			SqlConnection.ClearAllPools( );
		}

		/// <summary>
		/// Gets the instance of the index to test.
		/// </summary>
		/// <returns>The instance of the index.</returns>
		protected override IIndex GetIndex( )
		{
			SqlServerPagesStorageProvider prov = GetProvider( ) as SqlServerPagesStorageProvider;
			prov.SetFlags( true );
			return prov.Index;
		}

	}

}
