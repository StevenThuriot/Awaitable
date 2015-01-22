#region License
//  
// Copyright 2014 Steven Thuriot
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
#endregion

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