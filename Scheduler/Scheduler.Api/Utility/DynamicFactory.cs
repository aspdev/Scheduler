using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets.Internal;
using Scheduler.Shared.Abstract;
using Scheduler.Shared.Interfaces;


namespace Scheduler.Api.Utility
{
    public static class DynamicFactory
    {
        
        public static ISpeciesOriginator GetSpeciesOriginator(string assemblyPath, DateTime date, List<Object> arguments)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender,args) => LoadAssemblyFromFile(sender, args, assemblyPath);
            
            if (!Directory.Exists(assemblyPath))
            {
                throw new InvalidOperationException($"Could not find the assembly path {assemblyPath}");
            }

            IEnumerable<string> assemblyFiles =
                Directory.EnumerateFiles(assemblyPath, "*.dll", SearchOption.TopDirectoryOnly);

            
            string assemblyFile = assemblyFiles.First();
            
            Assembly assembly = Assembly.LoadFrom(assemblyFile);

            ISpeciesOriginator speciesOriginator = null;

            foreach (var type in assembly.ExportedTypes)
            {
                if (type.IsClass && typeof(ISpeciesOriginator).IsAssignableFrom(type))
                {
                    arguments.Add(date);
                    Object[] args = arguments.ToArray();

                    // parametry w konstruktorach muszą mieć taką samą kolejność jak w talicy Object[] args;

                    speciesOriginator = Activator.CreateInstance(type, args) as ISpeciesOriginator;
                    arguments.Remove(date);
                    break;
                }
            }

            return speciesOriginator;
            
        }
          
        public static List<Requirement> GetRequirements(string assemblyPath, DateTime date, List<Object> arguments)
        {
            if (!Directory.Exists(assemblyPath))
            {
                throw new InvalidOperationException($"Could not find the assembly path {assemblyPath}");
            }

            IEnumerable<string> assemblyFiles =
                Directory.EnumerateFiles(assemblyPath, "*.dll", SearchOption.TopDirectoryOnly);

            string assemblyFile = assemblyFiles.First();

            Assembly assembly = Assembly.LoadFrom(assemblyFile);

            var requirements = new List<Requirement>();

            foreach (var type in assembly.ExportedTypes)
            {
                if (type.IsClass && type.IsSubclassOf(typeof(Requirement)))
                {
                    arguments.Add(date);
                    Object[] args = arguments.ToArray();

                    // parametry w konstruktorach muszą mieć taką samą kolejność jak w talicy Object[] args;

                    Requirement requirement = Activator.CreateInstance(type, args) as Requirement;
                    requirements.Add(requirement);
                    arguments.Remove(date);
                }
            }

            return requirements;
        }

        private static Assembly LoadAssemblyFromFile(object sender, ResolveEventArgs args, string assemblyPath)
        {
            string assemblyName = string.Concat(args.Name.Remove(args.Name.IndexOf(',')), ".dll");
            string path = Path.Combine(assemblyPath, assemblyName);
            return Assembly.LoadFile(path);
        }
    }
}
