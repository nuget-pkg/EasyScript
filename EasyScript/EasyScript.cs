using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
//using static Global.EasyObject;
using static Global.EasyScriptHelper;
using static System.Net.WebRequestMethods;

// ReSharper disable once CheckNamespace
namespace Global;

public class EasyScript: IEasyScript
{
    // ReSharper disable once MemberCanBePrivate.Global
    //protected readonly Jint.Engine? Engine /*= null*/;
    protected readonly dynamic? Engine /*= null*/;
    // ReSharper disable once MemberCanBePrivate.Global
    protected readonly List<Assembly> Assemblies = [];
    // ReSharper disable once MemberCanBePrivate.Global
    public bool Debug /*= false*/;
    public string TypeName = "EasyScript";
    // ReSharper disable once MemberCanBePrivate.Global
    protected EasyScript? Transformer /*= null*/;
    // ReSharper disable once MemberCanBePrivate.Global
    protected bool Transform /*= false*/;
    public EasyScript(
        Assembly[]? assemblies = null,
        bool debug = false,
        bool transform = false
        )
    {
        Debug = debug;
        Transform = transform;
        if (assemblies != null)
        {
            foreach (var asm in assemblies)
            {
                Assemblies.Add(asm);
            }
        }
        Engine = JintScript.CreateEngine(Assemblies.ToArray());
    }

    // ReSharper disable once MemberCanBePrivate.Global
    protected string TransformCode(string methodName, string fileName, string code, object[] vars)
    {
        if (Transform)
        {
            if (Transformer == null)
            {
                Transformer = new EasyScript
                {
                    Transform = false,
                    Debug = false
                };
                var assembly = typeof(EasyScript).Assembly;
                //Log(assembly.GetManifestResourceNames());
                var text = Sys.ResourceAsText(assembly, "EasyScript:https_cdn.jsdelivr.net_npm_@babel-standalone@7.28.6_babel.js");
                //Echo(text, "text");
                Transformer.Execute(text!);
                Transformer.Execute("""
                                    function $$transform(fileName, code) {
                                        //$log(fileName, "transform(): fileName");
                                        //$log(code, "transform(): code(01)");
                                        code = Babel.transform(code,
                                                               { presets: ["typescript"],
                                    	                         filename: fileName,
                                                                 sourceType: "script"
                                                               }).code;
                                        //$log(code, "transform(): code(2)");
                                        //throw new Error("xxx");
                                        return code;
                                    }

                                    """);
            }
            code = Transformer.Call("$$transform", fileName, code)!;
        }
        // ReSharper disable once InvertIf
        if (Debug)
        {
            Log(DateTime.Now, $"{this.TypeName}.{methodName}(\"{fileName}\") started at");
            for (int i = 0; i < vars.Length; i++)
            {
                Log(vars[i], $"  #parameter ${i + 1}");
            }
            var lines = Sys.TextToLines(code);
            for (int i = 0; i < lines.Count; i++)
            {
                Log($"{i+1,4}: {lines[i]}");
            }
        }
        return code;
    }
    public void SetValue(string name, dynamic? value)
    {
        if (Debug)
        {
            if (!name.StartsWith("$"))
            {
                Log(value, $"{this.TypeName}.SetValue(\"{name}\", value) where value is");
            }
        }
        Engine!.Execute($"globalThis.{name}=({ObjectToJson(value)})");
    }
    public dynamic? GetValue(string name)
    {
        var result = ObjectToObject(Engine!.GetValue(name));
        if (Debug)
        {
            Log(result, $"{this.TypeName}.GetValue(\"{name}\") returned");
        }
        return result;
    }
#if false
    public EasyObject GetValueAsEasyObject(string name)
    {
        var result =  EasyObject.FromObject(GetValue(name));
        if (Debug)
        {
            Log(result, $"{this.TypeName}.GetValueAsEasyObject(\"{name}\") returned");
        }
        return result;
    }
#endif
    private void ExecuteWithMethodName(string methodName, string fileName, string script, params object[] vars)
    {
        if (Transform)
        {
            if (fileName.EndsWith(".js") || fileName.EndsWith(".ts"))
            {
                fileName = Path.GetFileNameWithoutExtension(fileName);
            }
            fileName += "(transformed).ts";
        }
        script = TransformCode(methodName, fileName, script, vars);
        //if (vars is null) vars = new object[] { };
        for (int i = 0; i < vars.Length; i++)
        {
            SetValue($"${i + 1}", vars[i]);
        }
        Engine!.Execute(script, fileName);
        for (int i = 0; i < vars.Length; i++)
        {
            Engine!.Execute($"delete globalThis.${i + 1};");
        }
    }
    public void ExecuteFile(string fileName, string script, params object[] vars)
    {
        ExecuteWithMethodName("ExecuteFile", fileName, script, vars);
    }
    public void Execute(string script, params object[] vars)
    {
        string fileName = "<anonymous>";
        ExecuteWithMethodName("Execute", fileName, script, vars);
    }
    private dynamic? EvaluateeWithMethodName(string methodName, string fileName, string script, params object[] vars)
    {
        if (Transform)
        {
            if (fileName.EndsWith(".js") || fileName.EndsWith(".ts"))
            {
                fileName = Path.GetFileNameWithoutExtension(fileName);
            }
            fileName += "(transformed).ts";
        }
        script = TransformCode(methodName, fileName, script, vars);
        //if (vars is null) vars = new object[] { };
        for (int i = 0; i < vars.Length; i++)
        {
            SetValue($"${i + 1}", vars[i]);
        }
        var result = ObjectToObject(Engine!.Evaluate(script, fileName).ToObject());
        for (int i = 0; i < vars.Length; i++)
        {
            Engine!.Execute($"delete globalThis.${i + 1};");
        }
        if (Debug)
        {
            Log(result, $"{this.TypeName}.{methodName}(\"{fileName}\") returned");
        }
        return result;
    }
    public dynamic? EvaluateFile(string fileName, string script, params object[] vars)
    {
        return EvaluateeWithMethodName("EvaluateFile", fileName, script, vars);
    }
    public dynamic? Evaluate(string script, params object[] vars)
    {
        string fileName = "<anonymous>";
        return EvaluateeWithMethodName("Evaluate", fileName, script, vars);
    }
    //public EasyObject EvaluateFileAsEasyObject(string fileName, string script, params object[] vars)
    //{
    //    return EasyObject.FromObject(EvaluateeWithMethodName("EvaluateFileAsEasyObject", fileName, script, vars));
    //}
    //public EasyObject EvaluateAsEasyObject(string script, params object[] vars)
    //{
    //    string fileName = "<anonymous>";
    //    return EasyObject.FromObject(EvaluateeWithMethodName("EvaluateAsEasyObject", fileName, script, vars));
    //}
    public dynamic? Call(string name, params object[] vars)
    {
        //if (vars is null) vars = new object[] { };
        string script = name + "(";
        for (int i = 0; i < vars.Length; i++)
        {
            if (i > 0) script += ", ";
            script += $"${i + 1}";
        }
        script += ")";
        var result = Evaluate(script, vars);
        for (int i = 0; i < vars.Length; i++)
        {
            Engine!.Execute($"delete globalThis.${i + 1};");
        }
        return result;
    }
    //public EasyObject CallAsEasyObject(string name, params object[] vars)
    //{
    //    return EasyObject.FromObject(Call(name, vars));
    //}
}

internal static class EasyScriptHelper
{
    static readonly Assembly? assembly = null;
    static readonly dynamic? instance = null;
    static EasyScriptHelper()
    {
        assembly = Sys.LoadFromResource(typeof(EasyScriptHelper).Assembly, "EasyScript:lib.dll");
        Type myType = assembly!.GetType("Local.EasyScriptLibrary")!;
        instance = Activator.CreateInstance(myType!)!;
    }
    public static void Echo(object? x, string? title = null)
    {
        instance!.Echo(x, title);
    }
    public static void Log(object? x, string? title = null)
    {
        instance!.Log(x, title);
    }
    public static string ObjectToJson(object? x)
    {
        return instance!.ObjectToJson(x);
    }
    public static object? ObjectToObject(object? x)
    {
        return instance!.ObjectToObject(x);
    }
}