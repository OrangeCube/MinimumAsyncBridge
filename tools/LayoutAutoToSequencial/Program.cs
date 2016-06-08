using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LayoutAutoToSequencial
{
    /// <summary>
    /// This program rewrite DLLs in a folder (passed by the 1st command line argument) to change LayoutKind from Auto to Sequencial on structs.
    /// </summary>
    /// <remarks>
    /// Background:
    ///
    /// When you compile async/await codes with Release settings ("Optimize code" option),
    /// The roslyn compiler generates structs with the StructLayout(LayoutKind.Auto) attribute.
    ///
    /// These auto-layouted structs cause a fatal error on Unity game engine.
    /// To solve this problem, this program rewrites the StructLayout by using Mono.Cecil library.
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// public static async Task X()
    /// {
    ///     ...
    /// }
    /// ]]></code>
    /// 
    /// ↑ This async method is compiled to ↓ this struct.
    /// 
    /// <code><![CDATA[
    /// [CompilerGenerated]
    /// [StructLayout(LayoutKind.Auto)]
    /// private struct <X>d__0 : IAsyncStateMachine
    /// {
    ///     public int <>1__state;
    ///     public AsyncTaskMethodBuilder<> t__builder;
    ///     private TaskAwaiter<> u__1;
    ///     ...
    /// ]]></code>
    /// </example>
    class Program
    {
        static void Main(string[] args)
        {
            var path = args[0];
            Console.WriteLine("begin rewriting " + path);

            foreach (var file in Directory.GetFiles(path, "*.dll"))
            {
                Console.WriteLine("rewrite " + file);
                RewriteDll(file);
            }
        }

        /// <summary>
        /// Rewrite a dll.
        /// </summary>
        /// <remarks>
        /// A struct generated from an async/await method has following attributes:
        /// 
        /// - IsValueType (struct)
        /// - IsNestedPrivate
        /// - IsAutoLayout
        /// 
        /// (FYI, a struct is usually generaged with SequencialLayout attribute.)
        /// </remarks>
        /// <param name="file"></param>
        private static void RewriteDll(string file)
        {
            var module = ModuleDefinition.ReadModule(file);

            var types = GetAllTypes(module);
            if (!types.Any()) return;

            foreach (var t in types)
            {
                if (t.IsValueType && t.IsNestedPrivate && t.IsAutoLayout)
                    t.Attributes |= TypeAttributes.SequentialLayout;
            }

            module.Write(file);
        }

        /// <summary>
        /// List all types in <paramref name="module"/> recursively.
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        private static IEnumerable<TypeDefinition> GetAllTypes(ModuleDefinition module) => GetAllTypes(module.Types);

        private static IEnumerable<TypeDefinition> GetAllTypes(IEnumerable<TypeDefinition> types) => types.SelectMany(t => GetAllTypes(t));

        private static IEnumerable<TypeDefinition> GetAllTypes(TypeDefinition t)
        {
            yield return t;

            if (t.NestedTypes != null && t.NestedTypes.Any())
            {
                foreach (var nt in GetAllTypes(t.NestedTypes))
                {
                    yield return nt;
                }
            }
        }
    }
}
