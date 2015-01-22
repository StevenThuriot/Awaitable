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
using System.ComponentModel;
using System.Dynamic;
using Invocation;

namespace Awaitable
{
    class AsyncObject<T> : DynamicObject
    {
        private readonly T _value;

        public AsyncObject(T value)
        {
            _value = value;
        }

        private Invoker GetInvoker(InvokeMemberBinder binder, IList<object> args)
        {
            var callers = TypeInfo<T>.Methods[binder.Name];
            var tuple = CallerSelector.SelectMethod(binder, args, callers);
            var method = tuple.Item1;
            var actualArguments = tuple.Item2;

            return new Invoker(method, actualArguments);
        }







        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var invoker = GetInvoker(binder, args);

            if (invoker == null)
            {
                result = null;
                return false;
            }

            result = invoker.Invoke(_value);

            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = TypeInfo<T>.GetProperty(_value, binder.Name); ;
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            //TODO: Setters
            //proprety.SetValue(_value, value);
            return false;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            var valueType = Constants.Typed<T>.OwnerType;

            if (binder.ReturnType == valueType)
            {
                result = _value;
                return true;
            }

            var destinationType = binder.ReturnType;

            var descriptor = TypeDescriptor.GetConverter(valueType);
            if (descriptor.CanConvertTo(destinationType))
            {
                result = descriptor.ConvertTo(_value, destinationType);
                return true;
            }

            descriptor = TypeDescriptor.GetConverter(destinationType);
            if (descriptor.CanConvertFrom(valueType))
            {
                result = descriptor.ConvertFrom(_value);
                return true;
            }

            result = null;
            return false;
        }
    }
}