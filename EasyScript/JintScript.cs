//css_nuget Jint
using Jint;
using System.IO;
using System.Reflection;
namespace Global;
internal class JintScript
{
    public static Jint.Engine CreateEngine(params Assembly[] list)
    {
        var engine = new Jint.Engine(cfg =>
        {
            cfg.AllowClr();
            for (int i = 0; i < list.Length; i++)
            {
                cfg.AllowClr(list[i]);
            }
        });
        engine.SetValue("_globals", new JintScriptGlobal());
        engine.SetValue("console", new JintScriptConsole());
        engine.Execute("""
            var exports = {};
            /*
            var console = {
                debug() {},
                log() {},
                warn() {},
            };*/
            var $echo = _globals.echo;
            var $log = _globals.log;
            var $getenv = _globals.getenv;
            var $namespace = importNamespace;
            var $ns = importNamespace;

            """);
        return engine;
    }
}
internal class JintScriptGlobal
{
    public void echo(dynamic x, string? title = null)
    {
        EasyScriptHelper.Echo(x, title);
    }
    public void log(dynamic x, string? title = null)
    {
        EasyScriptHelper.Log(x, title);
    }
    public string? getenv(string name)
    {
        return System.Environment.GetEnvironmentVariable(name);
    }
}
internal class JintScriptConsole
{
    private void output(string methodName, params object[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            EasyScriptHelper.Echo(args[i], $"console.{methodName}(#{i + 1})");
        }
    }
    public void debug(params object[] args)
    {
        output("debug", args);
    }
    public void error(params object[] args)
    {
        output("error", args);
    }
    public void info(params object[] args)
    {
        output("info", args);
    }
    public void log(params object[] args)
    {
        output("log", args);
    }
    public void warn(params object[] args)
    {
        output("warn", args);
    }
}
