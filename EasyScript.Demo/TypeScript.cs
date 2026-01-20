using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Global;

public class TypeScript: IEasyScript
{
    private readonly EasyScript _engine = null;
    public TypeScript(
        Assembly[]? assemblies = null,
        bool debug = false)
    {
        _engine = new EasyScript(assemblies: assemblies, debug: debug, transform: true)
        {
            TypeName = "TypeScript"
        };
    }
    public void SetValue(string name, dynamic value)
    {
        _engine.SetValue(name, value);
    }
    public dynamic GetValue(string name)
    {
        return _engine.GetValue(name);
    }
    public EasyObject GetValueAsEasyObject(string name)
    {
        return _engine.GetValueAsEasyObject(name);
    }
    public void ExecuteFile(string fileName, string script, params object[] vars)
    {
        _engine.ExecuteFile(fileName, script, vars);
    }
    public void Execute(string script, params object[] vars)
    {
        _engine.Execute(script, vars);
    }
    public dynamic EvaluateFile(string fileName, string script, params object[] vars)
    {
        return _engine.EvaluateFile(fileName, script, vars);
    }
    public dynamic Evaluate(string script, params object[] vars)
    {
        return _engine.Evaluate(script, vars);
    }
    public EasyObject EvaluateFileAsEasyObject(string fileName, string script, params object[] vars)
    {
        return _engine.EvaluateFileAsEasyObject(fileName, script, vars);
    }
    public EasyObject EvaluateAsEasyObject(string script, params object[] vars)
    {
        return _engine.EvaluateAsEasyObject(script, vars);
    }
    public dynamic Call(string name, params object[] vars)
    {
        return _engine.Call(name, vars);
    }
    public EasyObject CallAsEasyObject(string name, params object[] vars)
    {
        return _engine.CallAsEasyObject(name, vars);
    }
}
