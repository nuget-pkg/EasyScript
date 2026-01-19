//css_inc EasyScript.cs
//css_nuget EasyObject
namespace EasyScript.Demo;

using System;
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
        engine.Debug = true;
        engine.SetValue("x", 222);
        var result = engine.EvaluateFileAsEasyObject(
            "my-file.js",
            """
            class Person {
                name: string;
                age: number;
            
                constructor(name: string, age: number) {
                    this.name = name;
                    this.age = age;
                }
            
                greet() {
                    console.log("Hello");
                }
            }
            function add2(a:number, b:number) { return a + b; }
            var answer:number = add2($1, x);
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
        try
        {
            engine.Execute("""
                console.log("aaa", "bbb");
                throw new Error("my-error");
                """);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Unhandled Exception({Global.Sys.FullName(e)}): {e.Message}");
            Console.Error.WriteLine(e.StackTrace);
        }
    }
}
