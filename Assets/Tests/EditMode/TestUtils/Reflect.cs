using System.Reflection;
using NUnit.Framework;

namespace VinoBlendBar.Tests.EditMode
{
    /// <summary>
    /// Reads and writes private [SerializeField] fields, and calls private methods,
    /// so a test can set up a component the same way the Inspector does.
    /// </summary>
    public static class Reflect
    {
        private const BindingFlags Any = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public static T Get<T>(object target, string field)
        {
            return (T)Field(target.GetType(), field).GetValue(target);
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

        public static object CallStatic(System.Type type, string method, params object[] arguments)
        {
            MethodInfo info = type.GetMethod(method, Any);
            Assert.IsNotNull(info, $"{type.Name} has no method {method}");
            return info.Invoke(null, arguments);
        }

        private static FieldInfo Field(System.Type type, string field)
        {
            FieldInfo info = type.GetField(field, Any);
            Assert.IsNotNull(info, $"{type.Name} has no field {field}");
            return info;
        }
    }
}
