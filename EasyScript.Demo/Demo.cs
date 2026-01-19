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
        var engine = new Global.EasyScript(
            asmArray: [typeof(Program).Assembly],
            transpile: true,
            debug: true
            );
        engine.SetValue("x", 222);
        var result = engine.EvaluateAsEasyObject(
            """
            var answer:number = $1 + x;
            $echo(answer, "answer");
            $log(answer, "answer");
            answer;

            """, 111);

        Echo(result.IsNumber);
        Echo(result);
        engine.Execute("""
            var EasyScript = $namespace("EasyScript");
            var result = EasyScript.Demo.Program.Add2(1111, 2222); 
            $echo(result, "result");

            """);
        Echo(engine.GetValue("result"));
    }
}
