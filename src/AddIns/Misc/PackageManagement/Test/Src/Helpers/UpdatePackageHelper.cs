﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using ICSharpCode.PackageManagement;
using ICSharpCode.PackageManagement.Design;
using NuGet;

namespace PackageManagement.Tests.Helpers
{
	public class UpdatePackageHelper
	{
		PackageManagementService packageManagementService;
		
		public FakePackage TestPackage = new FakePackage() {
			Id = "Test"
		};
		
		public FakePackageRepository PackageRepository = new FakePackageRepository();
		public List<PackageOperation> PackageOperations = new List<PackageOperation>();
		
		public UpdatePackageHelper(PackageManagementService packageManagementService)
		{
			this.packageManagementService = packageManagementService;
		}
		
		public void UpdateTestPackage()
		{
			var action = packageManagementService.CreateUpdatePackageAction();
			action.PackageRepository = PackageRepository;
			action.Package = TestPackage;
			action.Operations = PackageOperations;
			action.Execute();
		}
		
		public FakePackage AddPackageInstallOperation()
		{
			var package = new FakePackage("Package to install");
			var operation = new PackageOperation(package, PackageAction.Install);
			PackageOperations.Add(operation);
			return package;
		}
		
		public PackageSource PackageSource = new PackageSource("http://sharpdevelop/packages");
		public TestableProject TestableProject = ProjectHelper.CreateTestProject();
		public bool UpdateDependencies;
		public Version Version;
		
		public void UpdatePackageById(string packageId)
		{
			var action = packageManagementService.CreateUpdatePackageAction();
			action.PackageId = packageId;
			action.PackageVersion = Version;
			action.PackageSource = PackageSource;
			action.Project = TestableProject;
			action.UpdateDependencies = UpdateDependencies;
			action.Execute();
		}
	}
}
