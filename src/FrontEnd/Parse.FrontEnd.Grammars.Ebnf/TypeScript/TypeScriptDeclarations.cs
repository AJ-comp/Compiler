using System;
using System.IO;

namespace Janglim.FrontEnd.Grammars.TypeScript
{
    /// <summary>
    /// Supplies TypeScript 7.0.2 interface, type alias, enum, namespace and ambient
    /// declaration productions for composition with the JavaScript language layer.
    /// Existing JavaScript expressions, statements, bindings and module items are
    /// reused; TypeScript declaration and type syntax retains its concrete-tree nodes.
    /// Ambient restrictions are checked after parsing in TypeScript validation.
    /// </summary>
    internal static class TypeScriptDeclarations
    {
        internal const string ResourceName = "Janglim.TypeScript.Declarations.ebnf";

        internal static string Load()
        {
            using (var stream = typeof(TypeScriptDeclarations).Assembly.GetManifestResourceStream(ResourceName))
            {
                if (stream == null)
                    throw new InvalidOperationException("The embedded TypeScript declaration grammar is missing.");
                using (var reader = new StreamReader(stream)) return reader.ReadToEnd();
            }
        }
    }
}
