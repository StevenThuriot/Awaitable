namespace Awaitable
{
    public static class AwaitableExtensions
    {
        public static dynamic Awaitable<T>(this T value)
        {
            return new AsyncObject<T>(value);
        }
    }
}