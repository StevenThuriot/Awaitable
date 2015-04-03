using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Invocation;

namespace Awaitable
{
    class Invoker
    {
        private readonly IEnumerable<object> _arguments;
        private readonly MethodCaller _caller;

        public Invoker(MethodCaller caller, IEnumerable<object> arguments)
        {
            _caller = caller;
            _arguments = arguments;
        }

        public object Invoke(object value)
        {
            var arguments = _arguments.ToList();

            if (!_caller.IsStatic)
                arguments.Insert(0, value);

            if (_caller.IsAsync)
                return _caller.Call(arguments);

            return Task.Run(() => _caller.Call(arguments));
        }
    }
}