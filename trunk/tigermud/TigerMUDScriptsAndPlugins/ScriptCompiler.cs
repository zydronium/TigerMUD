#region TigerMUD License
/*
/-------------------------------------------------------------\
|    _______  _                     __  __  _    _  _____     |
|   |__   __|(_)                   |  \/  || |  | ||  __ \    |
|      | |    _   __ _   ___  _ __ | \  / || |  | || |  | |   |
|      | |   | | / _` | / _ \| '__|| |\/| || |  | || |  | |   |
|      | |   | || (_| ||  __/| |   | |  | || |__| || |__| |   |
|      |_|   |_| \__, | \___||_|   |_|  |_| \____/ |_____/    |
|                 __/ |                                       |
|                |___/                  Copyright (c) 2004    |
\-------------------------------------------------------------/

TigerMUD. A Multi User Dungeon engine.
Copyright (C) 2004 Adam Miller et al.

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; either version 2 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

You can contact the TigerMUD developers at www.tigermud.com or at
http://sourceforge.net/projects/tigermud.

The full licence can be found in <root>/docs/TigerMUD_license.txt
*/
#endregion

using System;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;
using System.Collections;

namespace TigerMUD
{
    [Serializable]
    public class ScriptCompiler : MarshalByRefObject, TigerMUD.IScriptCompiler
    {
        public ScriptCompiler()
        {
        }

        /// <summary>
        /// Returns all the Objects from a file.
        /// </summary>
        /// <param name="Filename">Name of the input file.</param>
        /// <returns>Objects in an array.</returns>
        public Object[] GetObjectsFromFile(String Filename, out string errors)
        {
            try
            {
                return GetObjectsFromCode(LoadCode(Filename), out errors);
            }
            catch (Exception ex)
            {
                errors = ex.Message + ex.StackTrace + ex.StackTrace;
                return null;
            }

        }

        /// <summary>
        /// Returns all the Objects from an array of files.
        /// </summary>
        /// <param name="Filenames">Array of filesnames to get an IDemo from.</param>
        /// <returns></returns>
        public Object[] GetObjectsFromFiles(String[] Filenames, out string errors)
        {
            Object[] Objects = new Object[Lib.MaxGameCommands];
            errors = "";
            System.Reflection.Assembly ScriptAssembly = null;
            try
            {
                ScriptAssembly = CompileFiles(Filenames, out errors);
                if (ScriptAssembly != null)
                {
                    // Errors will come back with an Objects array with all nulls
                    Objects=GetObjectsFromAssembly(ScriptAssembly, out errors);
                    if (errors != "")
                    {
                        Lib.PrintLine(errors);
                    }
                }
                else
                {
                    Lib.PrintLine("ScriptCompiler found no assemblies to retrieve objects from");
                    return Objects;
                }
            }
            catch (Exception ex)
            {
                errors += ex.Message + ex.StackTrace + ex.StackTrace + "\r\n";
                return Objects;
            }
            return Objects;
        }

        /// <summary>
        /// Returns all the Objects from a a string of code.
        /// </summary>
        /// <param name="Code">String containing the source code.</param>
        /// <returns>Objects in an array.</returns>
        public Object[] GetObjectsFromCode(String Code, out string errors)
        {
            errors = "";
            System.Reflection.Assembly ScriptAssembly = null;

            try
            {
                ScriptAssembly = CompileCode(Code, out errors);
            }
            catch 
            {
                throw new Exception(errors);
            }

            return GetObjectsFromAssembly(ScriptAssembly, out errors);
        }

        /// <summary>
        /// Returns all the Command and Action objects from an Assembly.
        /// </summary>
        /// <param name="Code">String containing the source code.</param>
        /// <returns>Objects in an array.</returns>
        private Object[] GetObjectsFromAssembly(System.Reflection.Assembly ScriptAssembly, out string errors)
        {
            errors = "";
            System.Collections.ArrayList ScriptArray = new System.Collections.ArrayList();
            if (ScriptAssembly == null) return null;
            
            try
            {
                foreach (Type type in ScriptAssembly.GetTypes())
                {
                    try
                    {
                        if (type.GetInterface("ICommand") != null)
                        {
                            Command Command = (Command)Activator.CreateInstance(type);
                            ScriptArray.Add(Command);
                        }
                        if (type.GetInterface("IAction") != null)
                        {
                            Action Action = (Action)Activator.CreateInstance(type);
                            ScriptArray.Add(Action);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to get Objects from Assembly.  Exception: " + ex.Message + ex.StackTrace + ex.StackTrace);
                    }
                }
            }
            catch (ReflectionTypeLoadException e)
            {
                foreach (Exception ex in e.LoaderExceptions)
                {
                    Lib.PrintLine("ReflectionTypeLoadException in ScriptLoader: GetObjectsFromAssembly crashed with the error: " + ex.Message + ex.StackTrace + ex.StackTrace);
                }
                return (Object[])ScriptArray.ToArray(typeof(Object));
            }
            catch (Exception ex)
            {
                Lib.PrintLine("Exception in ScriptCompiler: GetObjectsFromAssembly crashed with the error: " + ex.Message + ex.StackTrace + ex.StackTrace);
                return (Object[])ScriptArray.ToArray(typeof(Object));
            }

            return (Object[])ScriptArray.ToArray(typeof(Object));
        }

        ///// <summary>
        ///// Compiles a file into an Assembly.
        ///// </summary>
        ///// <param name="Filename">Name of the input file.</param>
        ///// <returns>Assembly containing the compiled code.</returns>
        //public Assembly CompileFile(String Filename, out string errors)
        //{
        //    errors = "";
        //    try
        //    {
        //        //return CompileCode(LoadCode(Filename), out errors);
        //        return CompileFiles(new String[] { Filename }, out errors);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("CompileFile:" + ex.Message + ex.StackTrace + ex.StackTrace);
        //    }

        //}

        /// <summary>
        /// Compiles a series of files into an assembly.
        /// </summary>
        /// <param name="Filenames">Array of filenames to compile.</param>
        /// <returns></returns>
        public Assembly CompileFiles(String[] Filenames, out string errors)
        {
            errors = "";
            CSharpCodeProvider Provider = new CSharpCodeProvider();
            ICodeCompiler Compiler = Provider.CreateCompiler();
            CompilerParameters Parameters = new CompilerParameters();
            Parameters.GenerateExecutable = false;
            Parameters.GenerateInMemory = true;
            Parameters.TreatWarningsAsErrors = false;
            // DEBUG TODO XXX Problem
            //Parameters.CompilerOptions = String.Format("/lib:" + @"c:\tigermud\bin\debug");
            

            // Get location of assemblies that we reference
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Parameters.ReferencedAssemblies.Add(assembly.Location);
            }
            CompilerResults Results = Compiler.CompileAssemblyFromFileBatch(Parameters, Filenames);
            if (Results.Errors.Count > 0)
            {
                foreach (CompilerError error in Results.Errors)
                {
                    errors += error.ToString() + "\r\n";
                }
            }
            Assembly ScriptAssembly = Results.CompiledAssembly;
            return ScriptAssembly;
        }

        /// <summary>
        /// Compiles a string source code from memory into an Assembly.
        /// </summary>
        /// <param name="Code">String containing the source code to compile.</param>
        /// <returns>Assembly containing the compiled code.</returns>
        public Assembly CompileCode(String Code, out string errors)
        {
            errors = "";
            CSharpCodeProvider Provider = new CSharpCodeProvider();
            ICodeCompiler Compiler = Provider.CreateCompiler();
            CompilerParameters Parameters = new CompilerParameters();
            Parameters.GenerateExecutable = false;
            Parameters.GenerateInMemory = true;
            Parameters.TreatWarningsAsErrors = false;
            // Get location of assemblies that we reference
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Parameters.ReferencedAssemblies.Add(assembly.Location);
            }
            CompilerResults Results = Compiler.CompileAssemblyFromSource(Parameters, Code);
            if (Results.Errors.Count > 0)
            {
                foreach (CompilerError error in Results.Errors)
                {
                    errors += error.ToString() + "\r\n";
                }
            }
            Assembly ScriptAssembly = Results.CompiledAssembly;
            return ScriptAssembly;
        }

        /// <summary>
        /// Loads the code in the file into a string.
        /// </summary>
        /// <param name="Filename">Name of the input file.</param>
        /// <returns>String containing the source code in the file.</returns>
        private string LoadCode(String Filename)
        {
            StreamReader sr = new StreamReader(Filename);
            return sr.ReadToEnd().Trim();
        }
    }
}
