using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Content;
using System;
using System.Linq;
using Mono.Addins;
using MonoDevelop.PackageManagement;
using System.Collections.Generic;
using MonoDevelop.Projects;
using System.Threading.Tasks;
using System.IO;
using Addin2000;

namespace Featuralizer
{
	class FeaturalizerHandler : CommandHandler
	{
		protected async override void Run()
		{
			// TODO Open up a GUI to select feature

			MonoDevelop.Ide.Gui.Document doc = IdeApp.Workbench.ActiveDocument;

			var projects = IdeApp.Workspace.GetAllProjects();

			// Hack - we assume that all projects needs autofac
			//await InstallPackage(projects, "autofac", "3.5.2");

			// Hack - we assume that the first project is the PCL project
			var formsPclProject = projects.First();
			var iOSProject = projects.First(e => e.Name.Contains("iOS"));

			// Get some constants for file transforms later on
			var variables = new Dictionary<string, string>();
			variables.Add("%NAMESPACE%", (formsPclProject as DotNetProject).DefaultNamespace);
			variables.Add("%IOSNAMESPACE%", (iOSProject as DotNetProject).DefaultNamespace);

			CreateDirectory(formsPclProject, "Views");
			CreateDirectory(formsPclProject, "ViewModels");

			DeployFile(@"Templates/Mvvm/Files/Forms.PCL/Bootstrapper.cs.template", formsPclProject, "", variables);
			DeployFile(@"Templates/Mvvm/Files/Forms.PCL/Resolver.cs.template", formsPclProject, "", variables);
			DeployFile(@"Templates/Mvvm/Files/Forms.iOS/Bootstrapper.cs.template", iOSProject, "", variables);

			// Hack - Inject some code if not present
			InsertAsFirstLineInFunction(iOSProject, "AppDelegate.cs", "FinishedLaunching", "Bootstrapper.Initialize();");

		}

		protected override void Update(CommandInfo info)
		{
			MonoDevelop.Ide.Gui.Document doc = IdeApp.Workbench.ActiveDocument;
			info.Enabled = true; // doc != null; // todo fix stuff later
		}

		private async Task InstallPackage(IEnumerable<Project> projects, string package, string version)
		{
			var p = new PackageManagementPackageReference(package, version);

			foreach (var project in projects)
			{
				var isInstalled = PackageManagementServices.ProjectOperations.GetInstalledPackages(project).Any(e => e.Id.ToLower() == package.ToLower());

				if (!isInstalled)
				{
					await PackageManagementServices.ProjectOperations.InstallPackagesAsync(
						"https://www.nuget.org/api/v2/", project, new[] { p });
				}
			}
		}

		private void CreateDirectory(string path)
		{
			if (!System.IO.Directory.Exists(path))
			{
				System.IO.Directory.CreateDirectory(path);
			}
		}

		private void CreateDirectory(Project project, string folder)
		{
			CreateDirectory(Path.Combine(project.BaseDirectory, folder));
			project.AddDirectory(folder);
		}

		void DeployFile(string template, Project project, string targetFolder, 
		                Dictionary<string, string> variables)
		{
			// HACK - we assume it's a DotNetProject
			var c = project as DotNetProject;

			var filename = Path.GetFileName(template).TrimEnd(".template");
			var target = Path.Combine(project.BaseDirectory, targetFolder, filename);

			// Replace stuff
			var data = File.ReadAllText(template);
			foreach (var variable in variables)
			{
				data = data.Replace(variable.Key, variable.Value);
			}

			// Write file to disk
			File.WriteAllText(target, data);
			project.AddFile(target);
		}

		void InsertAsFirstLineInFunction(Project project, string filename, string methodName, 
		                                 string code)
		{
			var c = project as DotNetProject;
			var target = Path.Combine(project.BaseDirectory, filename);

			var data = File.ReadAllText(target);

			if (data.Contains(code))
				return;

			// Hack - find the metod and assume alot of stuff - should be replaced with
			// a proper regex or something smarter.
			var index = data.IndexOf(methodName, StringComparison.InvariantCultureIgnoreCase);
			if (index == -1)
				return;

			index = data.IndexOf('{', index);
			data = data.Insert(index + 1, $"\n\t\t\t{code}\n");

			File.WriteAllText(target, data);
		}
	}
}
