﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Management.Automation;
using ICSharpCode.PackageManagement.Scripting;
using ICSharpCode.SharpDevelop.Project;
using NuGet;

namespace ICSharpCode.PackageManagement.Cmdlets
{
	[Cmdlet(VerbsLifecycle.Install, "Package", DefaultParameterSetName = ParameterAttribute.AllParameterSets)]
	public class InstallPackageCmdlet : PackageManagementCmdlet
	{
		public InstallPackageCmdlet()
			: this(
				ServiceLocator.PackageManagementService,
				ServiceLocator.PackageManagementConsoleHost,
				null)
		{
		}
		
		public InstallPackageCmdlet(
			IPackageManagementService packageManagementService,
			IPackageManagementConsoleHost consoleHost,
			ICmdletTerminatingError terminatingError)
			: base(packageManagementService, consoleHost, terminatingError)
		{
		}
		
		[Parameter(Position = 0, Mandatory = true)]
		public string Id { get; set; }
		
		[Parameter(Position = 1)]
		public string ProjectName { get; set; }
		
		[Parameter(Position = 2)]
		public Version Version { get; set; }
		
		[Parameter(Position = 3)]
		public string Source { get; set; }
		
		[Parameter]
		public SwitchParameter IgnoreDependencies { get; set; }
		
		protected override void ProcessRecord()
		{
			ThrowErrorIfProjectNotOpen();
			InstallPackage();
		}
		
		void InstallPackage()
		{
			PackageSource packageSource = GetActivePackageSource(Source);
			MSBuildBasedProject project = GetActiveProject(ProjectName);
			
			InstallPackageAction action = CreateInstallPackageTask(packageSource, project);
			action.Execute();
		}
		
		InstallPackageAction CreateInstallPackageTask(PackageSource packageSource, MSBuildBasedProject project)
		{
			InstallPackageAction action = PackageManagementService.CreateInstallPackageAction();
			action.PackageId = Id;
			action.PackageVersion = Version;
			action.Project = project;
			action.PackageSource = packageSource;
			action.IgnoreDependencies = IgnoreDependencies.IsPresent;
			return action;
		}
	}
}
