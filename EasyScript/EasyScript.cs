using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Global;

public class EasyScript: IEasyScript
{
    internal static dynamic _BasicIO = Sys.CreateInstanceFromResource(typeof(EasyScript).Assembly, "EasyScript:BasicIO.dll", "Local.BasicIO");
    protected readonly Jint.Engine? Engine /*= null*/;
    protected readonly List<Assembly> Assemblies = [];
    public bool Debug /*= false*/;
    public string TypeName = "EasyScript";
    protected EasyScript? Transformer /*= null*/;
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
        if (Debug)
        {
            _BasicIO.Log(DateTime.Now, $"{this.TypeName}.{methodName}(\"{fileName}\") started at");
            for (int i = 0; i < vars.Length; i++)
            {
                _BasicIO.Log(vars[i], $"  #parameter ${i + 1}");
            }
            var lines = Sys.TextToLines(code);
            for (int i = 0; i < lines.Count; i++)
            {
                _BasicIO.Log($"{i+1,4}: {lines[i]}");
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
                _BasicIO.Log(value, $"{this.TypeName}.SetValue(\"{name}\", value) where value is");
            }
        }
        Engine!.Execute($"globalThis.{name}=({_BasicIO.ObjectToJson(value)})");
    }
    public dynamic? GetValue(string name)
    {
        var result = _BasicIO.ObjectToObject(Engine!.GetValue(name).ToObject());
        if (Debug)
        {
            _BasicIO.Log(result, $"{this.TypeName}.GetValue(\"{name}\") returned");
        }
        return result;
    }
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
    private dynamic? EvaluateWithMethodName(string methodName, string fileName, string script, params object[] vars)
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
        for (int i = 0; i < vars.Length; i++)
        {
            SetValue($"${i + 1}", vars[i]);
        }
        var result = _BasicIO.ObjectToObject(Engine!.Evaluate(script, fileName).ToObject());
        for (int i = 0; i < vars.Length; i++)
        {
            Engine!.Execute($"delete globalThis.${i + 1};");
        }
        if (Debug)
        {
            _BasicIO.Log(result, $"{this.TypeName}.{methodName}(\"{fileName}\") returned");
        }
        return result;
    }
    public dynamic? EvaluateFile(string fileName, string script, params object[] vars)
    {
        return EvaluateWithMethodName("EvaluateFile", fileName, script, vars);
    }
    public dynamic? Evaluate(string script, params object[] vars)
    {
        string fileName = "<anonymous>";
        return EvaluateWithMethodName("Evaluate", fileName, script, vars);
    }
    public dynamic? Call(string name, params object[] vars)
    {
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
}
