//css_nuget EasyObject;
namespace Local
{
    using Global;
    public static class EasyScriptLibrary
    {
        public static void Echo(object? x, string? title = null)
        {
            EasyObject.Echo(x, title);
        }
        public static void Log(object? x, string? title = null)
        {
            EasyObject.Log(x, title);
        }
        public static string ObjectToJson(object? x)
        {
            return EasyObject.ObjectToJson(x, indent: true);
        }
        public static object? ObjectToObject(object? x)
        {
            return EasyObject.ObjectToObject(x, asDynamicObject: true);
        }
    }
}
