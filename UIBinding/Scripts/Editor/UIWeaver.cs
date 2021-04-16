using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;

using UnityDebug = UnityEngine.Debug;
using UnityAssembly = UnityEditor.Compilation.Assembly;

namespace HDV.UIBinding
{
    /// <summary>
    /// Manage Weaving UI binding IL code
    /// </summary>
    public static class UIWeaver
    {
        /// <summary>
        /// Hook complie event
        /// </summary>
        [InitializeOnLoadMethod]
        public static void OnInitializeOnLoadMethod()
        {
            CompilationPipeline.assemblyCompilationFinished += OnCompilationFinished;

            // We only need to run this once per session
            // after that, all assemblies will be weaved by the event
            if (!SessionState.GetBool("UIBINDER_WEAVED", false))
            {
                // reset session flag
                SessionState.SetBool("UIBINDER_WEAVED", true);

                //Weave first time
                foreach (UnityAssembly assembly in CompilationPipeline.GetAssemblies())
                {
                    if (File.Exists(assembly.outputPath))
                    {
                        OnCompilationFinished(assembly.outputPath, new CompilerMessage[0]);
                    }
                }
#if UNITY_2019_3_OR_NEWER
                EditorUtility.RequestScriptReload();
#else
                UnityEditorInternal.InternalEditorUtility.RequestScriptReload();
#endif
            }
        }

        private static ModuleDefinition _mainModule;

        static HashSet<string> GetDependecyPaths(string assemblyPath)
        {
            // build directory list for later asm/symbol resolving using CompilationPipeline refs
            HashSet<string> dependencyPaths = new HashSet<string>
            {
                Path.GetDirectoryName(assemblyPath)
            };
            foreach (UnityAssembly unityAsm in CompilationPipeline.GetAssemblies())
            {
                if (unityAsm.outputPath != assemblyPath)
                    continue;

                foreach (string unityAsmRef in unityAsm.compiledAssemblyReferences)
                {
                    dependencyPaths.Add(Path.GetDirectoryName(unityAsmRef));
                }
            }

            return dependencyPaths;
        }

        /// <summary> 
        /// Callback when compliation finished
        /// <paramref name="assemblyPath"/> Path compiled assembly
        /// <paramref name="messages"/> Compliation messages
        /// </summary>
        public static void OnCompilationFinished(string assemblyPath, CompilerMessage[] messages)
        {
            if (assemblyPath.Contains("-Editor"))
                return;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            List<Type> derivedTypes = new List<Type>();
            List<PropertyDefinition> properties = new List<PropertyDefinition>();

            bool isModified = false;
            #region Cecil
            /*HashSet<string> dependencyPaths = GetDependecyPaths(assemblyPath);
            string unityEngineCoreModuleDLL = UnityEditorInternal.InternalEditorUtility.GetEngineCoreModuleAssemblyPath();
            dependencyPaths.Add(Path.GetDirectoryName(unityEngineCoreModuleDLL));
            using (DefaultAssemblyResolver asmResolver = new DefaultAssemblyResolver())*/
            using (AssemblyDefinition currentAssembly = AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters { ReadWrite = true, ReadSymbols = true/*, AssemblyResolver = asmResolver*/ }))
            {
                /*asmResolver.AddSearchDirectory(Path.GetDirectoryName(assemblyPath));
                string directoryName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
                directoryName = directoryName?.Replace(@"file:\", "");
                asmResolver.AddSearchDirectory(directoryName);

                var dependencies = dependencyPaths.ToArray();
                if (dependencies != null)
                {
                    foreach (string path in dependencies)
                    {
                        asmResolver.AddSearchDirectory(path);
                    }
                }*/

                _mainModule = currentAssembly.MainModule;

                var p = currentAssembly.MainModule.Types.Where(o => o.IsClass).SelectMany(t => t.Properties);
                if (p != null)
                    properties.AddRange(p);

                foreach (PropertyDefinition pd in properties)
                {
                    CustomAttribute attr = pd.CustomAttributes.SingleOrDefault(i => i.AttributeType.Name == nameof(UIBindable));
                    if (attr != null)
                    {
                        Mono.Cecil.Cil.MethodBody body = pd.SetMethod.Body;

                        ILProcessor processor = body.GetILProcessor();

                        var instructions = body.Instructions;
                        var targetInst = instructions.Last();

                        #region Trying Generic
                        /*// importing the generic type
                        var eq = _mainModule.ImportReference(typeof(UIBindProxy<>));

                        // importing the type of T we want to instantiate the generic type
                        var t = _mainModule.ImportReference(typeof(float));
                        var genericEq = new GenericInstanceType(eq);
                        genericEq.GenericArguments.Add(t);
                        var importedGenericEq = _mainModule.ImportReference(genericEq);
                        // getting the method we want to call on the generic instance
                        var td = SafeResolve(importedGenericEq);
                        td.DeclaringType = SafeResolve(t);
                        var defaultMethodDef = td.Methods.Single(m => m.Name == "UpdateValue");

                        var methodRef = _mainModule.ImportReference(defaultMethodDef);

                        // Important - setting the method declaring type to the correct instantiated type
                        methodRef.DeclaringType = importedGenericEq;


                        processor.InsertBefore(targetInst, processor.Create(OpCodes.Nop));
                        processor.InsertBefore(targetInst, processor.Create(OpCodes.Ldarg_0));
                        processor.InsertBefore(targetInst, processor.Create(OpCodes.Ldstr, pd.Name));
                        processor.InsertBefore(targetInst, processor.Create(OpCodes.Ldarg_1));
                        processor.InsertBefore(targetInst, processor.Create(OpCodes.Call, methodRef));
                        isModified = true;
                        continue;

                        var td2 = _mainModule.GetType("HDV.UIBindProxy`1");
                        td2.DeclaringType = SafeResolve(_mainModule.ImportReference(typeof(float)));
                        
                        //td.DeclaringType
                        // td = _mainModule.GetType("HDV.UIBindProxyTest");
                        foreach (var md in td2.Methods)
                        {
                            if (md.Name == "UpdateValue")
                            {
                                processor.InsertBefore(targetInst, processor.Create(OpCodes.Nop));
                                processor.InsertBefore(targetInst, processor.Create(OpCodes.Ldarg_0));
                                processor.InsertBefore(targetInst, processor.Create(OpCodes.Ldstr, pd.Name));
                                processor.InsertBefore(targetInst, processor.Create(OpCodes.Ldarg_1));
                                processor.InsertBefore(targetInst, processor.Create(OpCodes.Call, md));
                                isModified = true;
                            }
                        }
                        continue;*/
                        #endregion

                        TypeDefinition td = _mainModule.GetType("HDV.UIBinding.UIBindProxy");
                        string methodName;
                        switch (pd.PropertyType.MetadataType)
                        {
                            case MetadataType.Single:
                                {
                                    methodName = "UpdateFloatValue";
                                    break;
                                }
                            /*case MetadataType.String:
                                {
                                    methodName = "UpdateStringValue";
                                    break;
                                }*/
                            case MetadataType.Int32:
                                {
                                    methodName = "UpdateIntValue";
                                    break;
                                }
                            //TODO: 제너릭이 필요하다...
                            case MetadataType.String:
                            case MetadataType.Class:
                            case MetadataType.Object:
                                {
                                    methodName = "UpdateObjectValue";
                                    break;
                                }
                            default:
                                {
                                    UnityDebug.LogError("Missing type " + pd.PropertyType.MetadataType);
                                    continue;
                                }
                        }

                        MethodDefinition md = td.Methods.Single(m => m.Name == methodName);

                        processor.InsertBefore(targetInst, processor.Create(OpCodes.Nop));
                        processor.InsertBefore(targetInst, processor.Create(OpCodes.Ldarg_0));
                        processor.InsertBefore(targetInst, processor.Create(OpCodes.Ldstr, pd.Name));
                        processor.InsertBefore(targetInst, processor.Create(OpCodes.Ldarg_1));

                        /*MethodInfo gm = gt.GetMethod("UpdateValue");
                        MethodReference gr = _mainModule.ImportReference(gm);*/
                        processor.InsertBefore(targetInst, processor.Create(OpCodes.Call, md));

                        isModified = true;
                    }
                }

                if (isModified)
                {
                    currentAssembly.Write(new WriterParameters { WriteSymbols = true });
                }
            }

            #endregion
            UnityDebug.Log("UI Weaving time: " + sw.ElapsedMilliseconds);
            sw.Stop();
        }

        private static TypeDefinition SafeResolve(TypeReference typeRef)
        {
            //could also make a static ExtensionMethod with: this ModuleDefinition module
            foreach (TypeDefinition typeDefinition in _mainModule.GetTypes())
            {
                if (typeDefinition.Namespace == typeRef.Namespace &&
                    typeDefinition.Name == typeRef.Name)
                {
                    return typeDefinition;
                }
            }
            return typeRef.Resolve();
        }
    }
}