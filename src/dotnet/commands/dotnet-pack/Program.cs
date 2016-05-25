// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.ProjectModel;
using Microsoft.DotNet.Tools.Common;
using Microsoft.DotNet.Tools.Pack;

namespace Microsoft.DotNet.Tools.Compiler
{
    public class PackCommand
    {
        public static int Run(string[] args)
        {
            DebugHelper.HandleDebugSwitch(ref args);

            var app = new CommandLineApplication();
            app.Name = "dotnet pack";
            app.FullName = ".NET Packager";
            app.Description = "Packager for the .NET Platform";
            app.HelpOption("-h|--help");

            var output = app.Option("-o|--output <OUTPUT_DIR>", "Directory in which to place outputs", CommandOptionType.SingleValue);
            var noBuild = app.Option("--no-build", "Do not build project before packing", CommandOptionType.NoValue);
            var buildBasePath = app.Option("-b|--build-base-path <OUTPUT_DIR>", "Directory in which to place temporary build outputs", CommandOptionType.SingleValue);
            var configuration = app.Option("-c|--configuration <CONFIGURATION>", "Configuration under which to build", CommandOptionType.SingleValue);
            var versionSuffix = app.Option("--version-suffix <VERSION_SUFFIX>", "Defines what `*` should be replaced with in version field in project.json", CommandOptionType.SingleValue);
            var serviceable = app.Option("-s|--serviceable", "Set the serviceable flag in the package", CommandOptionType.NoValue);
            var path = app.Argument("<PROJECT>", "The project to compile, defaults to the current directory. Can be a path to a project.json or a project directory");

            app.OnExecute(() =>
            {
                // Locate the project and get the name and full path
                var pathValue = path.Value;
                if (string.IsNullOrEmpty(pathValue))
                {
                    pathValue = Directory.GetCurrentDirectory();
                }

                if (!pathValue.EndsWith(Project.FileName))
                {
                    pathValue = Path.Combine(pathValue, Project.FileName);
                }

                if (!File.Exists(pathValue))
                {
                    Reporter.Error.WriteLine($"Unable to find a project.json in {pathValue}");
                    return 1;
                }

                var configValue = configuration.Value() ?? Constants.DefaultConfiguration;
                var outputValue = output.Value();
                var buildBasePathValue = PathUtility.GetFullPath(buildBasePath.Value());


                // Map dotnet pack arguments into NuGet pack arguments
                string properties = $"Configuration={configValue}";

                List<string> arguments = new List<string>() { "pack", "--properties", properties };

                if (!noBuild.HasValue())
                {
                    arguments.Add("--build");
                }
                if (!string.IsNullOrWhiteSpace(outputValue))
                {
                    arguments.Add("--output-directory");
                    arguments.Add(outputValue);
                }
                if (!string.IsNullOrWhiteSpace(buildBasePathValue))
                {
                    arguments.Add("--base-path");
                    arguments.Add(buildBasePathValue);
                }
				if (serviceable.HasValue()
				{
					arguments.Add("--serviceable");
				}

                arguments.Add("--verbosity");
                arguments.Add("Verbose");

                arguments.Add(pathValue);

                return NuGet3.Pack(arguments);
            });

            try
            {
                return app.Execute(args);
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.Error.WriteLine(ex);
#else
                Console.Error.WriteLine(ex.Message);
#endif
                return 1;
            }
        }
    }
}