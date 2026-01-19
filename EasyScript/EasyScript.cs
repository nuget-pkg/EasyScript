//css_inc JintScript.cs
//css_inc IEasyObject.cs
//css_nuget EasyObject
//css_nuget Jint
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using static Global.EasyObject;

namespace Global;

public class EasyScript //: IEasyScript
{
    // ReSharper disable once MemberCanBePrivate.Global
    protected Jint.Engine? Engine = null;
    // ReSharper disable once MemberCanBePrivate.Global
    protected List<Assembly> Assemblies = new List<Assembly>();
    // ReSharper disable once MemberCanBePrivate.Global
    protected EasyScript Transformer = null;
    // ReSharper disable once MemberCanBePrivate.Global
    public bool Transform = false;
    // ReSharper disable once MemberCanBePrivate.Global
    public bool Debug = false;
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
                Transformer = new EasyScript();
                Transformer.Transform = false;
                Transformer.Debug = false;
                var assembly = typeof(EasyScript).Assembly;
                //Log(assembly.GetManifestResourceNames());
                var text = Sys.ResourceAsText(assembly, "EasyScript:https-cdn.jsdelivr.net-npm-@babel-standalone@7.28.6-babel.js");
                //Echo(text, "text");
                Transformer.Execute(text);
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
            //code = Transformer.Evaluate("""
            //                            $$transform($1, $2)
            //                            """, fileName, code);
            code = Transformer.Call("$$transform", fileName, code);
        }
        if (Debug)
        {
            Log($"EasyScript.{methodName}(\"{fileName}\") started:");
            //Log(vars, "params");
            for (int i = 0; i < vars.Length; i++)
            {
                //Log($"{EasyObject.FromObject(vars[i]).ToJson(indent: false)}", $"  #parameter ${i + 1}");
                Log(vars[i], $"  #parameter ${i + 1}");
            }
            var lines = Sys.TextToLines(code);
            for (int i = 0; i < lines.Count; i++)
            {
                Log($"{i+1,4}: {lines[i]}");
            }
        }
        //if (Debug) Echo(code, "[Tranformed]");
        return code;
    }
    public void SetValue(string name, dynamic? value)
    {
        if (Debug)
        {
            if (!name.StartsWith("$"))
            {
                //Log($"EasyScript.SetValue(\"{name}\", {EasyObject.FromObject(value).ToJson(indent: false)})");
                Log(value, $"EasyScript.SetValue(\"{name}\", value) where value is");
            }
        }
        Engine!.Execute($"globalThis.{name}=({EasyObject.FromObject(value).ToJson()})");
    }
    public dynamic? GetValue(string name)
    {
        var result = FromObject(Engine!.GetValue(name).ToObject());
        if (Debug)
        {
            //Log($"EasyScript.GetValue(\"{name}\") => {EasyObject.FromObject(result).ToJson(indent: false)}");
            Log(result, $"EasyScript.GetValue(\"{name}\") returned");
        }
        return result;
    }
    public EasyObject GetValueAsEasyObject(string name)
    {
        var result =  EasyObject.FromObject(GetValue(name));
        if (Debug)
        {
            //Log($"EasyScript.GetValueAsEasyObject(\"{name}\") => {EasyObject.FromObject(result).ToJson(indent: false)}");
            Log(result, $"EasyScript.GetValueAsEasyObject(\"{name}\") returned");
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
        if (vars is null) vars = new object[] { };
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
        string fileName = "<unknown>";
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
        if (vars is null) vars = new object[] { };
        for (int i = 0; i < vars.Length; i++)
        {
            SetValue($"${i + 1}", vars[i]);
        }
        //var result = engine!.Evaluate(script).ToObject();
        var result = FromObject(Engine!.Evaluate(script, fileName).ToObject()).ToObject();
        for (int i = 0; i < vars.Length; i++)
        {
            Engine!.Execute($"delete globalThis.${i + 1};");
        }
        if (Debug)
        {
            //Log($"EasyScript.{methodName}(\"{fileName}\") => {EasyObject.FromObject(result).ToJson(indent: false)}");
            Log(result, $"EasyScript.{methodName}(\"{fileName}\") returned");
        }
        return result;
    }
    public dynamic? EvaluateFile(string fileName, string script, params object[] vars)
    {
        return EvaluateeWithMethodName("EvaluateFile", fileName, script, vars);
    }
    public dynamic? Evaluate(string script, params object[] vars)
    {
        //return EvaluateFile("<unknown>", script, vars);
        string fileName = "<unknown>";
        return EvaluateeWithMethodName("Evaluate", fileName, script, vars);
    }
    public EasyObject EvaluateFileAsEasyObject(string fileName, string script, params object[] vars)
    {
        return EasyObject.FromObject(EvaluateeWithMethodName("EvaluateFileAsEasyObject", fileName, script, vars));
    }
    public EasyObject EvaluateAsEasyObject(string script, params object[] vars)
    {
        string fileName = "<unknown>";
        return EasyObject.FromObject(EvaluateeWithMethodName("EvaluateAsEasyObject", fileName, script, vars));
    }
    public dynamic? Call(string name, params object[] vars)
    {
        if (vars is null) vars = new object[] { };
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
    public EasyObject CallAsEasyObject(string name, params object[] vars)
    {
        return EasyObject.FromObject(Call(name, vars));
    }
}
