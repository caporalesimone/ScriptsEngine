using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using System.Diagnostics;
using ScriptsEngine.Logger;

namespace ScriptsEngine
{
    public static class CSharpCompiler
    {
        private const string START_METHOD_NAME = "Run";
        private const string STOP_METHOD_NAME = "StopScript";

        #region PrivateMethods
        private static CompilerParameters CompilerSettings(bool IncludeDebugInformation, List<string> assemblies)
        {
            CompilerParameters parameters = new();
            List<string> assemblies_cfg = GetReferenceAssemblies(); // Gets all assemblies inside the Assemblies.cfg file
            List<string> allAssemblies = assemblies.Concat(assemblies_cfg).ToList(); // Merges asseblies found in assemblies.cfg with assemblies from directives

            foreach (string assembly in allAssemblies)
            {
                parameters.ReferencedAssemblies.Add(assembly);
            }

            parameters.GenerateInMemory = true; // True - memory generation, false - external file generation
            parameters.GenerateExecutable = false; // True - exe file generation, false - dll file generation
            parameters.TreatWarningsAsErrors = false; // Set whether to treat all warnings as errors.
            parameters.WarningLevel = 4; // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/errors-warnings
            //parameters.CompilerOptions = "/optimize"; // Set compiler argument to optimize output.
            //parameters.CompilerOptions = "-langversion:8.0";
            parameters.CompilerOptions = "-parallel";
            parameters.IncludeDebugInformation = IncludeDebugInformation; // Build in debug or release
            return parameters;
        }
        private class CompilerOptions : IProviderOptions
        {
            string _compilerVersion = "8.0";
            IDictionary<string, string> _compilerOptions = new Dictionary<string, string>() { };
            public string CompilerVersion { get => _compilerVersion; set { _compilerVersion = value; } }
            public bool WarnAsError => false;
            public bool UseAspNetSettings => true;
            public string CompilerFullPath => Path.Combine("roslyn", "csc.exe");
            public int CompilerServerTimeToLive => 60 * 60; // 1h
            IDictionary<string, string> IProviderOptions.AllOptions { get => _compilerOptions; }
            IDictionary<string, string> Options { set { _compilerOptions = value; } } // For Debug
        }
        private static List<string> GetReferenceAssemblies()
        {
            List<string> assemblies = new();

            string assemblies_cfg_path = Path.Combine(Assistant.Engine.RootPath, "Scripts", "Assemblies.cfg");

            if (File.Exists(assemblies_cfg_path))
            {
                using StreamReader ip = new(assemblies_cfg_path);
                string line;

                while ((line = ip.ReadLine()) != null)
                {
                    if (line.Length > 0 && !line.StartsWith("#"))  // # means comment
                        assemblies.Add(line);
                }
            }

            // Replace with full path of all assemblies that are in razor path
            for (int i = 0; i < assemblies.Count; i++)
            {
                string assembly_path = Path.Combine(Assistant.Engine.RootPath, assemblies[i]);
                if (File.Exists(assembly_path))
                {
                    assemblies[i] = assembly_path;
                }
            }
            /*
            // Adding Razor and Ultima.dll as default
            assemblies.Add(Assistant.Engine.RootPath + Path.DirectorySeparatorChar + "RazorEnhanced.exe");
            assemblies.Add(Assistant.Engine.RootPath + Path.DirectorySeparatorChar + "Ultima.dll");
            */
            return assemblies;
        }
        /// <summary>
        /// Parses the CompilerResults created by Roslyn compiler and convert everything into a log
        /// </summary>
        /// <param name="results"></param>
        /// <param name="log"></param>
        /// <returns>True if compiler errors exists</returns>
        private static bool ManageCompileResult(CompilerResults results, ref SELogger log)
        {
            foreach (CompilerError error in results.Errors)
            {
                LogLevel logType = error.IsWarning ? LogLevel.Warning : LogLevel.Error;

                log.AddLog(logType, $"Compiler: {error.ErrorNumber} at {error.Line}: {error.ErrorText} in file {error.FileName}");
            }

            return results.Errors.HasErrors;
        }
        /// <summary>
        /// This function finds for a specific directive inside a script. 
        /// </summary>
        /// <param name="filename">File where look for the directive</param>
        /// <param name="directive">Which directive must be found</param>
        /// <param name="directiveList">List of all directives found</param>
        /// <returns>Returns false if file does not exist</returns>
        private static bool FindDirectivesInFile(string filename, string directive, ref List<string> directiveList)
        {
            if (!File.Exists(filename))
            {
                return false;
            }

            // Searching all the lines with the directive
            foreach (string line in File.ReadAllLines(filename))
            {
                if (line.ToLower().Contains(directive))
                {
                    string file = Regex.Replace(line, directive, "", RegexOptions.IgnoreCase).Trim(); // Removing the directive token from the string
                    directiveList.Add(file);
                }

                // If namespace string is found function can stop searching because all directives must exists before namespace string
                if (line.ToLower().Contains("namespace")) { break; }
            }
            return true;
        }
        /// <summary>
        /// Estract the filename from a file directive
        /// </summary>
        /// <param name="directive">String containing the whole directive</param>
        /// <param name="basepath">Basepath for the relative path directives</param>
        /// <param name="ignoreBasePath">Force to ignore the BasePath on the &lt; &gt; directive </param>
        /// <param name="filename">Extracted filename</param>
        /// <returns>Returns false if an error occurs</returns>
        private static bool ExtractFileNameFromDirective(string directive, string basepath, bool ignoreBasePath, out string filename)
        {
            if (directive.StartsWith("<") && directive.EndsWith(">"))
            {
                // Relative path. Adding base folder
                filename = directive.Substring(1, directive.Length - 2); // Removes < >
                if (!ignoreBasePath) filename = Path.GetFullPath(Path.Combine(basepath, filename)); // Basepath is Scripts folder
            }
            else if (directive.StartsWith("\"") && directive.EndsWith("\""))
            {
                // Absolute path. Adding as is
                filename = directive.Substring(1, directive.Length - 2); // Removes " "
                filename = Path.GetFullPath(filename); // This should resolve the relative ../ path
            }
            else
            {
                filename = "";
                return false;
            }
            return true;
        }
        /// <summary>
        /// This function searches for our custom directive //#assembly that allow include a specific assembly (DLL) into the script
        /// </summary>
        /// <param name="filesList">Full path of all files where search for the directive</param>
        /// <param name="assemblies">List of all assemblies that must be inclide during the compile process</param>
        /// <param name="errorwarnings">List of error and warnings</param>
        private static bool FindAllAssembliesIncludedInCSharpScripts(List<string> filesList, ref List<string> assemblies, ref SELogger log)
        {
            const string directive = "//#assembly";

            foreach (string filename in filesList)
            {
                List<string> foundDirectives = new();
                if (!FindDirectivesInFile(filename, directive, ref foundDirectives))
                {
                    log.AddLog(LogLevel.Error, $"Error on directive {directive}. Unable to find {filename}.");
                    return false;
                }

                // No directive found, it's ok. Let's exit
                if (foundDirectives.Count <= 0)
                {
                    return true;
                }

                string basepath = Path.GetDirectoryName(filename); // BasePath of the imported file

                foreach (string line in foundDirectives)
                {
                    if (!ExtractFileNameFromDirective(line, basepath, true, out string assembly))
                    {
                        log.AddLog(LogLevel.Error, $"Error on RE Directive: {directive}.");
                        return false;
                    }
                    assemblies.Add(assembly);
                }
            }

            return true;
        }
        /// <summary>
        /// This function searches for our custom directive //#import that allows import classes from other C# files
        /// The directive must be added anywhere before the namespace and can be used in C stile with &gt; &lt; or ""
        /// Using relative path with &gt; &lt; the base directory will the Scripts folder
        /// </summary>
        /// <param name="sourceFile">Full path of the source file</param>
        /// <param name="filesList">List of all files that must be compiled (it's a recursive list)</param>
        /// <param name="errorwarnings">List of error and warnings</param>
        private static bool FindAllIncludedCSharpScript(string sourceFile, ref List<string> filesList, ref SELogger log)
        {
            const string directive = "//#import";

            // Searching all the lines with the directive
            List<string> imports = new();
            if (!FindDirectivesInFile(sourceFile, directive, ref imports))
            {
                log.AddLog(LogLevel.Error, $"Error on directive {directive}. Unable to find {sourceFile}.");
                return false;
            }

            string basepath = Path.GetDirectoryName(sourceFile); // BasePath of the imported file
            filesList.Add(sourceFile);

            // If nothing is found return only the main file
            if (imports.Count == 0) { return true; }

            foreach (string line in imports)
            {
                if (!ExtractFileNameFromDirective(line, basepath, false, out string file))
                {
                    log.AddLog(LogLevel.Error, $"Error on RE Directive: {directive}.");
                    return false;
                }

                // I search if already exists in the filesList
                var match = filesList.FirstOrDefault(stringToCheck => stringToCheck.Contains(file));
                if (match == null)
                {
                    FindAllIncludedCSharpScript(file, ref filesList, ref log);
                }
            }

            return true;
        }
        /// <summary>
        /// This function checks for directive //#forcedebug
        /// By default all scripts are builded in release. Only the button "Debug Mode", in the Script Editor, allow you to compile in debug.
        /// If this directive is present, the script will be builded in debug instead of release bypassing all the default rules
        /// </summary>
        /// <param name="sourceFile">Filename of the main source file</param>
        /// <param name="debug_requested">If false, Razor is requesting to run the script in release</param>
        /// <returns></returns>
        private static bool CheckForceDebugDirective(string sourceFile, bool debug_requested)
        {
            if (debug_requested) return true; // If already Razor is requesting debug no check needed

            const string directive = "//#forcedebug";

            // Searching the directive in all lines untill "namespace"
            foreach (string line in File.ReadAllLines(sourceFile))
            {
                if (line.ToLower().Contains(directive))
                {
                    return true;
                }

                // If namespace directive is found stop searching
                if (line.ToLower().Contains("namespace")) { break; }
            }
            return false;
        }
        /// <summary>
        /// Looks into the assembly and count how many methods with name {methodName} exists in all classes
        /// </summary>
        /// <param name="assembly">Assembly where look into</param>
        /// <param name="methodName">Method to be found</param>
        /// <param name="foundMethod">Method object found</param>
        /// <returns></returns>
        private static int FindMethod(Assembly assembly, string methodName, out MethodInfo foundMethod)
        {
            // This is important for methods visibility. Check if all of these flags are really needed.
            BindingFlags bf = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod;

            foundMethod = null;
            int methodsCount = 0;

            // Search trough all methods and finds Run then calls it
            foreach (Type mt in assembly.GetTypes())
            {
                if (mt != null)
                {
                    MethodInfo method = mt.GetMethod(methodName, bf);
                    if (method != null)
                    {
                        foundMethod = method;
                        methodsCount++;
                    }
                }
            }

            // If found more than 1 method function fails and will return a null method
            if (methodsCount > 1) foundMethod = null;

            return methodsCount;
        }
        /// <summary>
        /// This function calls a method of the script class by Name
        /// </summary>
        /// <param name="assembly">Assambly that contains the script</param>
        /// <param name="scriptInstance">Instance of the script class</param>
        /// <param name="methodName">Method name</param>
        /// <param name="error">error founds</param>
        /// <returns>false on errors</returns>
        private static bool CallScriptMethodByName(Assembly assembly, object scriptInstance, string methodName, out string error)
        {
            error = "";

            if (FindMethod(assembly, methodName, out MethodInfo run) > 1)
            {
                error += $"Found more than one 'public void {methodName}' method in script.\nMust be only one {methodName} method.";
                //throw new Microsoft.Scripting.SyntaxErrorException(error, null, new SourceSpan(), 0, Severity.FatalError);
                //throw new Exception(error);
                return false;
            }

            // If Run method does not exists would be rised an exception later but better to throw a
            // SyntaxErrorException now and log it too
            if (run == null)
            {
                error += $"Required method 'public void {methodName}' missing from script.";
                //throw new Microsoft.Scripting.SyntaxErrorException(error, null, new SourceSpan(), 0, Severity.FatalError);
                //throw new Exception(error);
                return false;
            }

            // Calls the method
            run.Invoke(scriptInstance, null);
            return true;
        }
        /*
        public bool CompileFromText(string source, out List<string> errorwarnings, out Assembly assembly)
        {
            CompilerOptions opt = new();
            CSharpCodeProvider provider = new(opt);


            string myTempFile = Path.Combine(Path.GetTempPath(), "re_script.cs");

            Misc.SendMessage("Compiling C# Script");
            CompilerParameters compileParameters = CompilerSettings(true); // When compiler is invoked from the editor it's always in debug mode
            CompilerResults results = provider.CompileAssemblyFromSource(compileParameters, source); // Compiling
            Misc.SendMessage("Compile Done");

            assembly = null;
            errorwarnings = new();
            bool has_error = ManageCompileResult(results, ref errorwarnings);
            if (has_error)
            {
                var error = results.Errors[0];
                var a = new SourceLocation(0, error.Line, error.Column);
                throw new SyntaxErrorException(error.ErrorText, results.PathToAssembly, error.ErrorNumber, "", new SourceSpan(a, a), 0, Severity.Error);
            }
            else
            {
                assembly = results.CompiledAssembly;
            }
            return has_error;
        }
        */
        #endregion

        #region PublicMethods
        /// <summary>
        /// Validates the main script if contains the Run method and warn if the StopScript is missing.
        /// </summary>
        /// <param name="scriptPath"></param>
        /// <param name="errorwarnings"></param>
        /// <returns></returns>
        public static bool ValidateMainScript(string scriptPath, ref SELogger log)
        {
            bool run_found = false;
            bool stop_found = false;

            string start_pattern = @$"\s*void\s+{START_METHOD_NAME}\s*\(\s*\)";
            string stop_pattern = @$"\s*void\s+{STOP_METHOD_NAME}\s*\(\s*\)";

            if (!File.Exists(scriptPath))
            {
                log.AddLog(LogLevel.Error, $"Unable to find the script {scriptPath}");
                return false;
            }

            foreach (string line in File.ReadAllLines(scriptPath))
            {
                if (Regex.Match(line, start_pattern).Success == true)
                {
                    run_found = true;
                }

                if (Regex.Match(line, stop_pattern).Success == true)
                {
                    stop_found = true;
                }
            }

            if (run_found == false)
            {
                log.AddLog(LogLevel.Error, $"Missing required method 'public void {START_METHOD_NAME}()'");
            }

            if (stop_found == false)
            {
                log.AddLog(LogLevel.Warning, $"Missing optional method 'public void {STOP_METHOD_NAME}()'");
            }

            return run_found;
        }
        //
        // https://medium.com/swlh/replace-codedom-with-roslyn-but-bin-roslyn-csc-exe-not-found-6a5dd9290bf2
        // https://stackoverflow.com/questions/20018979/how-can-i-target-a-specific-language-version-using-codedom
        // https://docs.microsoft.com/it-it/dotnet/api/microsoft.csharp.csharpcodeprovider.-ctor?view=net-5.0
        // https://github.com/aspnet/RoslynCodeDomProvider/blob/main/src/Microsoft.CodeDom.Providers.DotNetCompilerPlatform/Util/IProviderOptions.cs
        // https://josephwoodward.co.uk/2016/12/in-memory-c-sharp-compilation-using-roslyn
        /// <summary>
        /// This method Compiles the script and checks that Run method exists
        /// </summary>
        /// <param name="scriptPath">Path of the script in the filesystem</param>
        /// <param name="debug">Compile debug or release</param>
        /// <param name="errorwarnings">List or error and warnings</param>
        /// <param name="assembly">the output assembly</param>
        /// <param name="runMethod">the class containing the Run method of the script</param>
        /// <returns>false is build failed</returns>
        public static bool CompileFromFile(string scriptPath, bool debug, ref SELogger log, out Assembly assembly, out MethodInfo runMethod)
        {
            log.AddLog(LogLevel.Info, "Script build started.");
            assembly = null;
            runMethod = null;

            debug = CheckForceDebugDirective(scriptPath, debug); // The forcedebug directive, if present, will override the value of debug variable

            List<string> filesList = new() { }; // List of files.

            bool bRet = FindAllIncludedCSharpScript(scriptPath, ref filesList, ref log);
            if (bRet == false)
            { 
                return false;
            }

            List<string> assembliesList = new() { }; // List of assemblies.
            bRet = FindAllAssembliesIncludedInCSharpScripts(filesList, ref assembliesList, ref log);
            if (bRet == false)
            {
                return false;
            }

            if (debug)
            {
                log.AddLog(LogLevel.Info, "Compiling C# Script in DEBUG " + Path.GetFileName(scriptPath));
            } 
            else
            {
                log.AddLog(LogLevel.Info, "Compiling C# Script in RELEASE " + Path.GetFileName(scriptPath));
            }

            DateTime start = DateTime.Now;

            CompilerOptions m_opt = new();
            CSharpCodeProvider m_provider = new(m_opt);
            CompilerParameters m_compileParameters = CompilerSettings(true, assembliesList);

            m_compileParameters.IncludeDebugInformation = debug;
            CompilerResults results = m_provider.CompileAssemblyFromFile(m_compileParameters, filesList.ToArray()); // Compiling

            DateTime stop = DateTime.Now;
            log.AddLog(LogLevel.Info, "Script compiled in " + (stop - start).TotalMilliseconds.ToString("F0") + " ms");


            bool has_error = ManageCompileResult(results, ref log);

            if (!has_error)
            {
                assembly = results.CompiledAssembly;

                // Looking for multiple Run Methods
                int countRunMethod = FindMethod(assembly, START_METHOD_NAME, out runMethod);
                if (countRunMethod != 1)
                {
                    assembly = null; // Assembly is not valid anymore
                    runMethod = null; // Run method is not valid anymore
                    has_error = true;
                    log.AddLog(LogLevel.Error, $"Found {countRunMethod} {START_METHOD_NAME} method in the Assemby");
                }
            }

            return !has_error;
        }
        /// <summary>
        /// Creates an Instance of the script class
        /// </summary>
        /// <param name="assembly">Asembly of the class</param>
        /// <param name="runMethod">Method contained in the class that needs to be instantiated</param>
        /// <returns>The instance</returns>
        public static object CreateScriptInstance(Assembly assembly, MethodInfo runMethod)
        {
            return Activator.CreateInstance(runMethod.DeclaringType);
        }
        /// <summary>
        /// This function calls a method of the script class
        /// </summary>
        /// <param name="assembly">Assambly that contains the script</param>
        /// <param name="scriptInstance">Instance of the script class</param>
        /// <param name="method">Method that will be called</param>
        /// <param name="error">error founds</param>
        public static void CallScriptMethod(Assembly assembly, object scriptInstance, MethodInfo method, out string error)
        {
            error = "";

            if ((scriptInstance != null) && (method != null))
            {
                method.Invoke(scriptInstance, null);
            }
        }
        /// <summary>
        /// Tries to calls the StopScript Method of the script
        /// </summary>
        /// <param name="assembly">Assambly that contains the script</param>
        /// <param name="scriptInstance">Instance of the script class</param>
        /// <param name="error">error founds</param>
        public static void CallStopScriptMethod(Assembly assembly, object scriptInstance, out string error)
        {
            CallScriptMethodByName(assembly, scriptInstance, STOP_METHOD_NAME, out error);
        }
    }
    #endregion
}
