using System;
using System.Linq;

namespace Victoria.Common
{
    /// <summary>
    /// 
    /// </summary>
    public readonly struct Ensure
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objects"></param>
        public static void NotNull(params object[] objects)
        {
            if (objects.Length == 0)
                return;

            foreach (var obj in objects)
                if (obj is null)                
                    Throw.ArgNull(obj.GetType().Name, "Argument cannot be null.");                
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mainValue"></param>
        /// <param name="values"></param>
        public static void Constraints<T>(T mainValue, params T[] values) where T : struct
        {
            if (values.Any(x => x.Equals(mainValue)))
                return;

            var names = values.Select(x => Enum.GetName(typeof(T), x));
            var constraints = string.Join(", ", names);
            Throw.Exception($"Unable to ensure any of the following constraints: {constraints}");
        }
    }
}
