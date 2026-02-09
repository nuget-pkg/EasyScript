//css_nuget EasyObject;
//css_nuget Jint;

namespace Global
{
    using Global;
    using System.Reflection;
    using static EasyObject;
    using Jint;
    public class EasyScriptLibrary
    {
        public void Echo(object? x, string? title = null)
        {
            Echo(x, title);
        }
        public void Log(object? x, string? title = null)
        {
            Log(x, title);
        }
        public string ObjectToJson(object? x)
        {
            return FromObject(x).ToJson(indent: true); ;
        }
        public object? ObjectToObject(object? x)
        {
            return FromObject(x).ToObject(asDynamicObject: true);
        }
    }
    public class JintScript
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
            var $echo = _globals.echo;
            var $log = _globals.log;
            var $getenv = _globals.getenv;
            var $namespace = importNamespace;
            var $ns = importNamespace;

            """);
            return engine;
        }
    }
    public class JintScriptGlobal
    {
        public void echo(dynamic x, string? title = null)
        {
            Echo(x, title);
        }
        public void log(dynamic x, string? title = null)
        {
            Log(x, title);
        }
        public string? getenv(string name)
        {
            return System.Environment.GetEnvironmentVariable(name);
        }
    }
    public class JintScriptConsole
    {
        private void output(string methodName, params object[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                Echo(args[i], $"console.{methodName}(#{i + 1})");
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
}
