﻿namespace Core.Tests {
	using NUnit.Framework;
	using ScrewTurn.Wiki;
	using ScrewTurn.Wiki.PluginFramework;
	using ScrewTurn.Wiki.Tests;

	public class FilesStorageProviderTests : FilesStorageProviderTestScaffolding {

		public override IFilesStorageProviderV30 GetProvider() {
			FilesStorageProvider prov = new FilesStorageProvider();
			prov.Init(MockHost(), "");
			return prov;
		}

		[Test]
		public void Init() {
			IFilesStorageProviderV30 prov = GetProvider();
			prov.Init(MockHost(), "");

			Assert.IsNotNull(prov.Information, "Information should not be null");
		}

	}

}
