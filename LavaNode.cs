using System;
using System.Threading.Tasks;

namespace Victoria
{
    public sealed class LavaNode : IDisposable
    {
        internal LavaNode()
        {
        }

        /// <summary>
        ///     Node info such as name and id.
        /// </summary>
        public (string Name, int Num) Id { get; private set; }

        public void Dispose()
        {
        }

        private event Func<ValueTask> _log;
    }
}