using Discord;
using System;
using System.Threading.Tasks;

namespace Victoria
{
    public sealed class LavaNode : IAsyncDisposable
    {
        public event Func<LogMessage, ValueTask> Log;



        internal LavaNode()
        {

        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}