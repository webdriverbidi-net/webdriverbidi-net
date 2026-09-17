// Prints the version at which an assembly references another assembly, read from its metadata
// without loading it. Run as a .NET file-based program:
//
//   dotnet run scripts/print-assembly-reference-version.cs -- <assembly-path> <referenced-assembly-name>
//
// Exit codes:
//   0 — the reference was found and its version printed
//   1 — the assembly does not reference the named assembly
//   2 — usage error

using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: print-assembly-reference-version.cs <assembly-path> <referenced-assembly-name>");
    return 2;
}

using FileStream stream = File.OpenRead(args[0]);
using PEReader peReader = new(stream);
MetadataReader metadataReader = peReader.GetMetadataReader();
foreach (AssemblyReferenceHandle handle in metadataReader.AssemblyReferences)
{
    AssemblyReference reference = metadataReader.GetAssemblyReference(handle);
    if (metadataReader.StringComparer.Equals(reference.Name, args[1]))
    {
        Console.WriteLine(reference.Version);
        return 0;
    }
}

Console.Error.WriteLine($"{args[0]} does not reference {args[1]}");
return 1;
