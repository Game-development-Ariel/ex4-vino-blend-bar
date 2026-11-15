using System.Reflection;
using NUnit.Framework;

namespace VinoBlendBar.Tests.PlayMode
{
    /// <summary>Reads and writes private fields and calls private methods of the game's components.</summary>
    public static class PlayReflect
    {
        private const BindingFlags Any = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public static T Get<T>(object target, string field)
        {
            return (T)Field(target.GetType(), field).GetValue(target);
        }

        public static T GetStatic<T>(System.Type type, string field)
        {
            return (T)Field(type, field).GetValue(null);
        }

        public static void Set(object target, string field, object value)
        {
            Field(target.GetType(), field).SetValue(target, value);
        }

        public static object Call(object target, string method, params object[] arguments)
        {
            MethodInfo info = target.GetType().GetMethod(method, Any);
            Assert.IsNotNull(info, $"{target.GetType().Name} has no method {method}");
            return info.Invoke(target, arguments);
        }

        private static FieldInfo Field(System.Type type, string field)
        {
            FieldInfo info = type.GetField(field, Any);
            Assert.IsNotNull(info, $"{type.Name} has no field {field}");
            return info;
        }
    }
}
