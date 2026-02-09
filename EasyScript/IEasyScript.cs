// ReSharper disable once CheckNamespace
namespace Global;

public interface IEasyScript
{
    public void SetValue(string name, dynamic? value);
    public dynamic? GetValue(string name);
    //public EasyObject GetValueAsEasyObject(string name);
    public void ExecuteFile(string fileName, string script, params object[] vars);
    public void Execute(string script, params object[] vars);
    public dynamic? EvaluateFile(string fileName, string script, params object[] vars);
    public dynamic? Evaluate(string script, params object[] vars);
    //public EasyObject EvaluateFileAsEasyObject(string fileName, string script, params object[] vars);
    //public EasyObject EvaluateAsEasyObject(string script, params object[] vars);
    public dynamic? Call(string name, params object[] vars);
    //public EasyObject CallAsEasyObject(string name, params object[] vars);
}