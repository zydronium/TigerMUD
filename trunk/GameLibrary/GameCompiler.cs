using System;
using System.Collections;
using System.Text;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;
using System.IO;

namespace GameLibrary
{
    public class GameCompiler
    {

        /// <summary>
        /// Fills the given arraylist with Command class instances from source code
        /// </summary>
        /// <param name="commands"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public string InitCommands(GameContext gamecontext, string file)
        {
            if (gamecontext == null) throw new ArgumentNullException("gamecontext");

            string errors = string.Empty;
            string errorstrings = string.Empty;
            ArrayList Objects = new ArrayList();
            ArrayList Commands = new ArrayList();
            ArrayList sourcefiles = new ArrayList();
            ArrayList assemblyfiles = new ArrayList();
            Console.WriteLine("Compiling and loading commands...");

            // Load commands from source files
            if (file == null)
            {
                sourcefiles = gamecontext.GetFilesRecursive(gamecontext.CommandsFolder, "*.cs");
                assemblyfiles = gamecontext.GetFilesRecursive(gamecontext.AssemblyFolder, "*.dll");

            }
            else
                sourcefiles.Add(file);


            if (sourcefiles.Count > 0)
            {
                try
                {
                    // Get objects from assemblies
                    foreach (string assembly in assemblyfiles)
                    {
                        if (Objects.Count < 1) Objects = GetObjectsFromAssembly(Assembly.LoadFrom(assembly), out errors);
                        else Objects.AddRange(GetObjectsFromAssembly(Assembly.LoadFrom(assembly), out errors));
                    }

                    // Get objects from sourcefiles
                    if (Objects.Count<1) Objects=(GetObjectsFromFiles(ConvertToStringArray(sourcefiles), out errors));
                        else Objects.AddRange(GetObjectsFromFiles(ConvertToStringArray(sourcefiles), out errors));
                   
                    errorstrings += errors;
                }
                catch
                {
                    return errorstrings;
                }
                if (Objects != null)
                {
                    // If this comes back equal to it's init length, then nothing was entered into the array
                    if (Objects.Count > 0)
                    {
                        foreach (Object c in Objects)
                        {
                            gamecontext.AddCommand((Command)c);
                        }
                    }
                }
            }
            return errorstrings;
        }


        /// <summary>
        /// Compiles a list of source files into an assembly.
        /// </summary>
        /// <param name="Filenames">Array of filenames to compile.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        public Assembly CompileFiles(String[] filenames, out string errors)
        {
            errors = string.Empty;
            CodeDomProvider cdp = CodeDomProvider.CreateProvider("C#");
            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateExecutable = false;
            parameters.GenerateInMemory = true;
            parameters.TreatWarningsAsErrors = false;
            // Get location of assemblies that we reference
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                parameters.ReferencedAssemblies.Add(assembly.Location);
            }
            CompilerResults results = cdp.CompileAssemblyFromFile(parameters, filenames);
            if (results.Errors.Count > 0)
            {
                foreach (CompilerError error in results.Errors)
                {
                    errors += error.ToString() + "\r\n";
                }
            }
            Assembly scriptassembly = results.CompiledAssembly;
            return scriptassembly;
        }

        /// <summary>
        /// Returns all the Objects from an array of compiled source files.
        /// </summary>
        /// <param name="Filenames">Array of filenames to get a compiled code objects from.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected ArrayList GetObjectsFromFiles(String[] filenames, out string errors)
        {
            ArrayList Objects = new ArrayList();
            errors = string.Empty;
            System.Reflection.Assembly ScriptAssembly = null;
            try
            {
                ScriptAssembly = CompileFiles(filenames, out errors);
                if (ScriptAssembly != null)
                {
                    // Errors will come back with an Objects array with all nulls
                    Objects = GetObjectsFromAssembly(ScriptAssembly, out errors);
                    if (!String.IsNullOrEmpty(errors))
                    {
                        Console.WriteLine(errors);
                    }
                }
                else
                {
                    Console.WriteLine("ScriptCompiler found no assemblies to retrieve objects from");
                    return Objects;
                }
            }
            catch (Exception ex)
            {
                errors += ex.Message + ex.StackTrace + "\r\n";
                return Objects;
            }
            return Objects;
        }

        /// <summary>
        /// Returns all the Command and Action objects from an Assembly.
        /// </summary>
        /// <param name="Code">String containing the source code.</param>
        /// <returns>Objects in an array.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected ArrayList GetObjectsFromAssembly(System.Reflection.Assembly scriptassembly, out string errors)
        {
            errors = string.Empty;
            System.Collections.ArrayList ScriptArray = new System.Collections.ArrayList();
            if (scriptassembly == null) return null;

            try
            {
                foreach (Type type in scriptassembly.GetTypes())
                {
                    try
                    {
                        if (type.BaseType.ToString() == "GameLibrary.Command")
                        {

                            Command Command = (Command)Activator.CreateInstance(type);
                            ScriptArray.Add(Command);
                        }
                    }
                    catch (Exception exc)
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to get Objects from Assembly.  Exception: " + exc.Message);
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (Exception e in ex.LoaderExceptions)
                {
                    Console.WriteLine("ReflectionTypeLoadException in ScriptLoader: GetObjectsFromAssembly crashed with the error: " + e.Message);
                }
                return ScriptArray;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in ScriptCompiler: GetObjectsFromAssembly crashed with the error: " + ex.Message);
                return ScriptArray;
            }

            return ScriptArray;
        }

        /// <summary>
        /// Returns filenames of every file in and below the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public ArrayList GetFilesRecursive(string path, string mask)
        {
            ArrayList list = new ArrayList();
            DirectoryInfo di = new DirectoryInfo(path);
            foreach (string f in Directory.GetFiles(di.FullName, mask))
            {
                FileInfo fi = new FileInfo(f);
                list.Add(fi.FullName);
            }
            foreach (string d in Directory.GetDirectories(path))
            {
                Console.WriteLine(d);
                list.AddRange(GetFilesRecursive(d, mask));
            }
            return list;
        }

        protected String[] ConvertToStringArray(ArrayList input)
        {
            if (input == null) throw new ArgumentNullException("input");

            // Allocate the memory for the array.
            String[] output = new String[input.Count];
            //Then copy to the array, like this.
            // Copy the elements to the array.
            input.CopyTo(output);
            return output;
        }

    }
}
