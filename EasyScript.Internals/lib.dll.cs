//css_nuget EasyObject;
namespace Local
{
    using Global;
    public class EasyScriptLibrary
    {
        public  void Echo(object? x, string? title = null)
        {
            EasyObject.Echo(x, title);
        }
        public  void Log(object? x, string? title = null)
        {
            EasyObject.Log(x, title);
        }
    }
}
