//css_inc EasyScript.cs
//css_nuget EasyObject
namespace EasyScript;

using static Global.EasyObject;

public class Program
{
    public static void Main(string[] args)
    {
        Log(args, "args");
        Echo("helloハロー©");
        var engine = new Global.EasyScript();
        engine.SetValue("x", 222);
        var result = engine.EvaluateAsEasyObject(
            """
            var answer = $1 + x;
            echo(answer, "answer");
            log(answer, "answer");
            return answer;
            """, 111);

        Echo(result.IsNumber);
        Echo(result);
    }
}
