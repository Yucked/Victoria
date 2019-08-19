using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using NJS = Newtonsoft.Json.JsonSerializer;

namespace Victoria.Common
{
    /// <summary>
    /// 
    /// </summary>
    public readonly struct Json
    {
        private static readonly NJS NewtonSerializer = new NJS();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Deserialize<T>(ReadOnlyMemory<byte> bytes)
        {
            using var stream = new MemoryStream(bytes.ToArray());
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader);
            return NewtonSerializer.Deserialize<T>(reader);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ReadOnlyMemory<byte> Serialize<T>(T value)
        {
            var serializeObject = JsonConvert.SerializeObject(value);
            return Encoding.UTF8.GetBytes(serializeObject);
        }
    }
}
