using System;
using Mono.Cecil;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ILRepacking;
using Microsoft.Extensions.DependencyModel;


#if NETCOREAPP2_0
namespace Mono.Cecil
{
	public sealed class AssemblyResolutionException : Exception
	{
		readonly AssemblyNameReference reference;

		public AssemblyNameReference AssemblyReference
		{
			get { return reference; }
		}

		public AssemblyResolutionException(AssemblyNameReference reference)
			: base(string.Format("Failed to resolve assembly: '{0}'", reference))
		{
			this.reference = reference;
		}
	}

	public class DotNetCoreAssemblyResolver : IAssemblyResolver
	{
		internal ILogger Logger;
		private readonly Dictionary<string, Lazy<AssemblyDefinition>> _libraries = new Dictionary<string, Lazy<AssemblyDefinition>>();
		private readonly List<string> _searchPaths = new List<string>();

		public DotNetCoreAssemblyResolver(ILogger logger)
		{
			Logger = logger;

			//var compileLibraries = DependencyContext.Default.CompileLibraries;
			//foreach (var library in compileLibraries)
			//{
			//	var path = library.ResolveReferencePaths().FirstOrDefault();
			//	if (string.IsNullOrEmpty(path))
			//		continue;

			//	_libraries.Add(library.Name, new Lazy<AssemblyDefinition>(() => AssemblyDefinition.ReadAssembly(path, new ReaderParameters() { AssemblyResolver = this })));
			//}
			//var runtimeLibraries = DependencyContext.Default.RuntimeLibraries;
			//foreach (var library in runtimeLibraries)
			//{
			//	var path = library.Path;
			//	_libraries.Add(library.Name, new Lazy<AssemblyDefinition>(() => AssemblyDefinition.ReadAssembly(path, new ReaderParameters() { AssemblyResolver = this })));
			//}
		}

		public void AddSearchDirectory(string path)
		{
			_searchPaths.Add(path);
		}

		public void RegisterAssembly(AssemblyDefinition mergedAssembly)
		{
			Logger.Verbose($"Register Assembly called for {mergedAssembly.FullName}.");
			//var context = DependencyContext.Load(Assembly.Load(mergedAssembly.FullName));
			//var compileLibraries = context.CompileLibraries;
			//foreach (var library in compileLibraries)
			//{
			//	var path = library.ResolveReferencePaths().FirstOrDefault();
			//	if (string.IsNullOrEmpty(path))
			//		continue;

			//	_libraries.Add(library.Name, new Lazy<AssemblyDefinition>(() => AssemblyDefinition.ReadAssembly(path, new ReaderParameters() { AssemblyResolver = this })));
			//}
			_libraries.Add(mergedAssembly.Name.Name, new Lazy<AssemblyDefinition>(() => mergedAssembly));
		}

		public virtual AssemblyDefinition Resolve(string fullName)
		{
			return Resolve(fullName, new ReaderParameters());
		}

		public virtual AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
		{
			if (fullName == null)
				throw new ArgumentNullException(nameof(fullName));

			return Resolve(AssemblyNameReference.Parse(fullName), parameters);
		}

		public AssemblyDefinition Resolve(AssemblyNameReference name)
		{
			return Resolve(name, new ReaderParameters());
		}

		IEnumerable<string> IterateFolderForFile(string rootFolder, string name, string filter)
		{
			try
			{
				IEnumerable<string> files = Directory.GetFiles(rootFolder, filter, SearchOption.TopDirectoryOnly);
				var directories = Directory.GetDirectories(rootFolder);
				return directories.Aggregate(files, (current, directory) => current.Union(IterateFolderForFile(directory, name, filter)));
			}
			catch (Exception e)
			{
				return new string[0];
			}
		}

		public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			Lazy<AssemblyDefinition> asm;
			if (_libraries.TryGetValue(name.Name, out asm))
				return asm.Value;

			foreach (var path in _searchPaths)
			{
				var files = IterateFolderForFile(path, name.Name, $"{name.Name}.dll");
				foreach (var file in files)
				{
					_libraries.Add(name.Name, new Lazy<AssemblyDefinition>(() => AssemblyDefinition.ReadAssembly(file, new ReaderParameters() { AssemblyResolver = this })));

					if (_libraries.TryGetValue(name.Name, out asm))
						return asm.Value;
				}
			}

			if (_libraries.TryGetValue(name.Name, out asm))
				return asm.Value;

			throw new AssemblyResolutionException(name);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
				return;

			foreach (var lazy in _libraries.Values)
			{
				if (!lazy.IsValueCreated)
					continue;

				lazy.Value.Dispose();
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}

namespace ILRepacking
{
	public class RepackAssemblyResolver : DotNetCoreAssemblyResolver
	{
		public RepackAssemblyResolver(ILogger logger) : base(logger)
		{
		}

		public void RegisterAssemblies(IList<AssemblyDefinition> mergedAssemblies)
		{
			foreach (var assemblyDefinition in mergedAssemblies)
			{
				RegisterAssembly(assemblyDefinition);
			}
		}
	}
}
#else
namespace ILRepacking
{
	public class RepackAssemblyResolver : DefaultAssemblyResolver
	{
		public void RegisterAssemblies(IList<AssemblyDefinition> mergedAssemblies)
		{
			foreach (var assemblyDefinition in mergedAssemblies)
			{
				RegisterAssembly(assemblyDefinition);
			}
		}
	}
}
#endif
