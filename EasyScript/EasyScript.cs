//css_inc JintScript.cs
//css_inc IEasyObject.cs
//css_nuget EasyObject
//css_nuget Jint
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using static Global.EasyObject;

namespace Global;

public class EasyScript: IEasyScript
{
    // ReSharper disable once MemberCanBePrivate.Global
    protected Jint.Engine? Engine = null;
    // ReSharper disable once MemberCanBePrivate.Global
    protected List<Assembly> AsmList = new List<Assembly>();
    // ReSharper disable once MemberCanBePrivate.Global
    protected EasyScript Transformer = null;
    // ReSharper disable once MemberCanBePrivate.Global
    protected bool Debug = false;
    public EasyScript(
        Assembly[]? asmArray = null,
        bool allocEmptyExports = false,
        bool transpile = false,
        bool debug = false)
    {
        if (asmArray != null)
        {
            foreach (var asm in asmArray)
            {
                AsmList.Add(asm);
            }
        }
        Engine = JintScript.CreateEngine(AsmList.ToArray());
        if (allocEmptyExports)
        {
            Engine!.Execute("globalThis.exports = {}");
        }
        Debug = debug;
        if (transpile)
        {
            Transformer = new EasyScript(allocEmptyExports: true);
            Transformer.Execute("""
                                var console = {
                                    debug() {},
                                    log() {},
                                    warn() {},
                                };

                                """);
            var assembly = typeof(EasyScript).Assembly;
            //Log(assembly.GetManifestResourceNames());
            var text = Sys.ResourceAsText(assembly, "EasyScript:https-cdn.jsdelivr.net-npm-@babel-standalone@7.28.6-babel.js");
            //Echo(text, "text");
            //var engine = new EasyScript(allocEmptyExports: true);
            Transformer.Execute(text);
            Transformer.Execute("""
                                function transform(code) {
                                    return Babel.transform(code,
                                                           { presets: ["typescript"],
                                		                     filename: 'file.ts',
                                                             sourceType: "script"
                                                           }).code;
                                }

                                """);
        }
    }

    protected string Tranform(string code, object[] vars)
    {
        if (Transformer != null)
        {
            code = Transformer.Evaluate("""
                                        return transform($1)
                                        """, code);
        }
        if (Debug)
        {
            var lines = Sys.TextToLines(code);
            for (int i = 0; i < lines.Count; i++)
            {
                Log($"{i+1,4}: {lines[i]}");
            }
            //Log(vars, "params");
            for (int i = 0; i < vars.Length; i++)
            {
                Log($"   ${i + 1} = {EasyObject.FromObject(vars[i]).ToJson(indent: false)}");
            }
        }
        return code;
    }
    public void SetValue(string name, dynamic? value)
    {
        if (!name.StartsWith("$"))
        {
            Log($"   EasyScript.SetValue(\"{name}\", {EasyObject.FromObject(value).ToJson(indent: false)})");
        }
        Engine!.Execute($"globalThis.{name}=({EasyObject.FromObject(value).ToJson()})");
    }
    public dynamic? GetValue(string name)
    {
        var result = FromObject(Engine!.GetValue(name).ToObject());
        Log($"   EasyScript.GetValue(\"{name}\") => {EasyObject.FromObject(result).ToJson(indent: false)}");
        return result;
    }
    public EasyObject GetValueAsEasyObject(string name)
    {
        return EasyObject.FromObject(GetValue(name));
    }
    public void Execute(string script, params object[] vars)
    {
        script = Tranform(script, vars);
        if (vars is null) vars = new object[] { };
        for (int i = 0; i < vars.Length; i++)
        {
            SetValue($"${i + 1}", vars[i]);
        }
        Engine!.Execute(script);
        for (int i = 0; i < vars.Length; i++)
        {
            Engine!.Execute($"delete globalThis.${i + 1};");
        }
    }
    public dynamic? Evaluate(string script, params object[] vars)
    {
        script = Tranform(script, vars);
        if (vars is null) vars = new object[] { };
        for (int i = 0; i < vars.Length; i++)
        {
            SetValue($"${i + 1}", vars[i]);
        }
        //var result = engine!.Evaluate(script).ToObject();
        var result = FromObject(Engine!.Evaluate(script).ToObject()).ToObject();
        for (int i = 0; i < vars.Length; i++)
        {
            Engine!.Execute($"delete globalThis.${i + 1};");
        }
        return result;
    }
    public EasyObject EvaluateAsEasyObject(string script, params object[] vars)
    {
        return EasyObject.FromObject(Evaluate(script, vars));
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
