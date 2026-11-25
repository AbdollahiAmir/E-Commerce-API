namespace Shop_API.Exceptions
{
    public class LockedException : Exception
    {
        public LockedException(string message) : base(message)
        {
        }
    }
}
