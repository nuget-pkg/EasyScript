//css_inc JintScript.cs
//css_inc IEasyObject.cs
//css_nuget EasyObject
//css_nuget Jint
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static Global.EasyObject;

namespace Global;

public class EasyScript: IEasyScript
{
    protected Jint.Engine? engine = null;
    protected List<Assembly> asmList = new List<Assembly>();
    public EasyScript(Assembly[]? asmArray = null, bool allocEmptyExports = false)
    {
        if (asmArray != null)
        {
            foreach (var asm in asmArray)
            {
                asmList.Add(asm);
            }
        }
        engine = JintScript.CreateEngine(asmList.ToArray());
        if (allocEmptyExports)
        {
            engine.Execute("globalThis.exports = {}");
        }
    }
    public void SetValue(string name, dynamic? value)
    {
        engine!.Execute($"globalThis.{name}=({EasyObject.FromObject(value).ToJson()})");
    }
    public dynamic? GetValue(string name)
    {
        return FromObject(engine!.GetValue(name).ToObject());
    }
    public EasyObject GetValueAsEasyObject(string name)
    {
        return EasyObject.FromObject(GetValue(name));
    }
    public void Execute(string script, params object[] vars)
    {
        if (vars is null) vars = new object[] { };
        for (int i = 0; i < vars.Length; i++)
        {
            SetValue($"${i + 1}", vars[i]);
        }
        engine!.Execute(script);
        for (int i = 0; i < vars.Length; i++)
        {
            engine!.Execute($"delete globalThis.${i + 1};");
        }
    }
    public dynamic? Evaluate(string script, params object[] vars)
    {
        if (vars is null) vars = new object[] { };
        for (int i = 0; i < vars.Length; i++)
        {
            SetValue($"${i + 1}", vars[i]);
        }
        //var result = engine!.Evaluate(script).ToObject();
        var result = FromObject(engine!.Evaluate(script).ToObject()).ToObject();
        for (int i = 0; i < vars.Length; i++)
        {
            engine!.Execute($"delete globalThis.${i + 1};");
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
            engine!.Execute($"delete globalThis.${i + 1};");
        }
        return result;
    }
    public EasyObject CallAsEasyObject(string name, params object[] vars)
    {
        return EasyObject.FromObject(Call(name, vars));
    }
}
