using System;
using Mono.Addins;
using Mono.Addins.Description;

[assembly: Addin(
	"Addin2000",
	Namespace = "Addin2000",
	Version = "1.0"
)]

[assembly: AddinName("Addin2000")]
[assembly: AddinCategory("IDE extensions")]
[assembly: AddinDescription("Addin2000")]
[assembly: AddinAuthor("johankarlsson")]

[assembly: AddinDependency("::MonoDevelop.Core", MonoDevelop.BuildInfo.Version)]
[assembly: AddinDependency("::MonoDevelop.Ide", MonoDevelop.BuildInfo.Version)]