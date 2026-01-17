//css_inc EasyScript.cs
//css_nuget EasyObject
namespace EasyScript.Demo;

using static Global.EasyObject;

public class Program
{
    public static int Add2(int a, int b)
        { return a + b; }
    public static void Main(string[] args)
    {
        Log(args, "args");
        Echo("helloハロー©");
        var engine = new Global.EasyScript([typeof(Program).Assembly]);
        engine.SetValue("x", 222);
        var result = engine.EvaluateAsEasyObject(
            """
            var answer = $1 + x;
            $echo(answer, "answer");
            $log(answer, "answer");
            return answer;

            """, 111);

        Echo(result.IsNumber);
        Echo(result);
        engine.Execute("""
            var EasyScript = $namespace("EasyScript");
            $echo(EasyScript.Demo.Program.Add2(1111, 2222));

            """);
    }
}
