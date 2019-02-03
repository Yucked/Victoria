namespace Victoria.Entities.Payloads
{
    internal abstract class BasePayload
    {
        protected BasePayload(string op)
        {
            Op = op;
        }

        public string Op { get; }
    }
}