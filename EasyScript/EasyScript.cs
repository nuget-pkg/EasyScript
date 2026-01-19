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
    public bool Transform = true;
    // ReSharper disable once MemberCanBePrivate.Global
    public bool Debug = false;
    public EasyScript(
        Assembly[]? asmArray = null
        //,
        //bool allocEmptyExports = false,
        //bool transform = false,
        //bool debug = false
        )
    {
        if (asmArray != null)
        {
            foreach (var asm in asmArray)
            {
                AsmList.Add(asm);
            }
        }
        Engine = JintScript.CreateEngine(AsmList.ToArray());
        //if (allocEmptyExports)
        //{
        //    Engine!.Execute("globalThis.exports = {}");
        //}
        //Debug = debug;
        //if (transform)
        //{
        //    Transformer = new EasyScript(
        //        //allocEmptyExports: true
        //        //,
        //        //transform: false,
        //        //debug: false
        //        );
        //    Transformer.Execute("""
        //                        var console = {
        //                            debug() {},
        //                            log() {},
        //                            warn() {},
        //                        };

        //                        """);
        //    var assembly = typeof(EasyScript).Assembly;
        //    //Log(assembly.GetManifestResourceNames());
        //    var text = Sys.ResourceAsText(assembly, "EasyScript:https-cdn.jsdelivr.net-npm-@babel-standalone@7.28.6-babel.js");
        //    //Echo(text, "text");
        //    //var engine = new EasyScript(allocEmptyExports: true);
        //    Transformer.Execute(text);
        //    Transformer.Execute("""
        //                        function transform(fileName, code) {
        //                            //$log(fileName, "transform(): fileName");
        //                            //$log(code, "transform(): code(01)");
        //                            code = Babel.transform(code,
        //                                                   { presets: ["typescript"],
        //                        	                         filename: fileName,
        //                                                     sourceType: "script"
        //                                                   }).code;
        //                            //$log(code, "transform(): code(2)");
        //                            //throw new Error("xxx");
        //                            return code;
        //                        }

        //                        """);
        //}
    }

    protected string TransformCode(string fileName, string code, object[] vars)
    {
        if (Transform)
        {
            if (Transformer == null)
            {
                Transformer = new EasyScript(
                    //allocEmptyExports: true
                    //,
                    //transform: false,
                    //debug: false
                    );
                Transformer.Transform = false;
                Transformer.Debug = false;
                var assembly = typeof(EasyScript).Assembly;
                //Log(assembly.GetManifestResourceNames());
                var text = Sys.ResourceAsText(assembly, "EasyScript:https-cdn.jsdelivr.net-npm-@babel-standalone@7.28.6-babel.js");
                //Echo(text, "text");
                Transformer.Execute(text);
                Transformer.Execute("""
                                    function transform(fileName, code) {
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
            if (fileName.EndsWith(".js") || fileName.EndsWith(".ts"))
            {
                fileName = Path.GetFileNameWithoutExtension(fileName);
            }
            fileName += "(transformed).ts";
            code = Transformer.Evaluate("""
                                        return transform($1, $2)
                                        """, fileName, code);
        }
        if (Debug)
        {
            Log(fileName, "fileName");
            var lines = Sys.TextToLines(code);
            for (int i = 0; i < lines.Count; i++)
            {
                Log($"{i+1,4}: {lines[i]}");
            }
            //Log(vars, "params");
            for (int i = 0; i < vars.Length; i++)
            {
                //Log($"${i + 1} = {EasyObject.FromObject(vars[i]).ToJson(indent: false)}");
                Log($"{EasyObject.FromObject(vars[i]).ToJson(indent: false)}", $"parameter ${i + 1}");
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
                Log($"EasyScript.SetValue(\"{name}\", {EasyObject.FromObject(value).ToJson(indent: false)})");
            }
        }
        Engine!.Execute($"globalThis.{name}=({EasyObject.FromObject(value).ToJson()})");
    }
    public dynamic? GetValue(string name)
    {
        var result = FromObject(Engine!.GetValue(name).ToObject());
        if (Debug)
        {
            Log($"EasyScript.GetValue(\"{name}\") => {EasyObject.FromObject(result).ToJson(indent: false)}");
        }
        return result;
    }
    public EasyObject GetValueAsEasyObject(string name)
    {
        return EasyObject.FromObject(GetValue(name));
    }
    public void ExecuteFile(string fileName, string script, params object[] vars)
    {
        script = TransformCode(fileName, script, vars);
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
    public void Execute(string script, params object[] vars)
    {
        ExecuteFile("<unknown>", script, vars);
    }
    public dynamic? EvaluateFile(string fileName, string script, params object[] vars)
    {
        script = TransformCode(fileName, script, vars);
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
    public dynamic? Evaluate(string script, params object[] vars)
    {
        return EvaluateFile("<unknown>", script, vars);
    }
    public EasyObject EvaluateFileAsEasyObject(string fileName, string script, params object[] vars)
    {
        return EasyObject.FromObject(EvaluateFile(fileName, script, vars));
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
