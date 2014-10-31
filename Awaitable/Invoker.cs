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

using System.Reflection;
using System.Threading.Tasks;

namespace Awaitable
{
    class Invoker
    {
        private readonly object[] _arguments;
        private readonly MethodInfo _method;

        public Invoker(MethodInfo method, object[] arguments)
        {
            _method = method;
            _arguments = arguments;
        }

        public object Invoke(object value)
        {
            return typeof(Task).IsAssignableFrom(_method.ReturnType)
                ? _method.Invoke(value, _arguments) //Already Async, return as is.
                : Task.Run(() => _method.Invoke(value, _arguments));
        }
    }
}