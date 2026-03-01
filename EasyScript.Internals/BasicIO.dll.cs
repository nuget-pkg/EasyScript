//+#nuget *EasyObject*;
namespace Local
{
    using Global;
    public class BasicIO
    {
        public  void Echo(object? x, string? title = null)
        {
            EasyObject.Echo(x, title);
        }
        public  void Log(object? x, string? title = null)
        {
            EasyObject.Log(x, title);
        }
        public string ObjectToJson(object? x)
        {
            return EasyObject.ObjectToJson(x, indent: true);
        }
        public string ObjectToCompactJson(object? x)
        {
            return EasyObject.ObjectToJson(x, indent: false);
        }
        public object? ObjectToObject(object? x)
        {
            return EasyObject.ObjectToObject(x, asDynamicObject: true);
        }
    }
}
