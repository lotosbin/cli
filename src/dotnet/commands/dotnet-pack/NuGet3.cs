using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.DotNet.Cli.Utils;
using NugetProgram = NuGet.CommandLine.XPlat.Program;

namespace Microsoft.DotNet.Tools.Pack
{
    internal static class NuGet3
    {
        public static int Pack(IEnumerable<string> args)
        {
            var prefixArgs = new List<string>();

            prefixArgs.Add("pack");

            var result = Run(Enumerable.Concat(
                    prefixArgs,
                    args).ToArray());

            return result;
        }

        private static int Run(string[] nugetArgs)
        {
            var nugetAsm = typeof(NugetProgram).GetTypeInfo().Assembly;
            var mainMethod = nugetAsm.EntryPoint;
            return (int)mainMethod.Invoke(null, new object[] { nugetArgs });
        }
    }
}
