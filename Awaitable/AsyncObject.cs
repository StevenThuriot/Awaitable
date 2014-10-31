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
using System.Linq;
using System.Reflection;

namespace Awaitable
{
    class AsyncObject<T> : DynamicObject
    {
        private readonly T _value;

        public AsyncObject(T value)
        {
            _value = value;
        }

        // ReSharper disable once StaticFieldInGenericType : Generic Cache
        private static PropertyDescriptorCollection _properties;

        private static PropertyDescriptorCollection Properties
        {
            get
            {
                return _properties ?? (_properties = TypeDescriptor.GetProperties(typeof(T)));
            }
        }


        // ReSharper disable once StaticFieldInGenericType : Generic Cache
        private static ILookup<string, MethodInfo> _methodInfo;
        private static ILookup<string, MethodInfo> Info
        {
            get
            {
                return _methodInfo ??
                       (_methodInfo = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance).ToLookup(x => x.Name));
            }
        }

        private static bool CompareParameters(IReadOnlyList<Argument> parameters,
            IEnumerable<ParameterInfo> methodParameters, out IEnumerable<Argument> arguments)
        {
            var selectableArguments = methodParameters.Select(x => new SelectableArgument(x))
                                                      .ToList();

            arguments = selectableArguments;

            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];

                var methodParam = parameter.HasName
                    ? selectableArguments.FirstOrDefault(x => x.Name == parameter.Name)
                    : selectableArguments.ElementAtOrDefault(i); //Unnamed params come first and in order, select by index.

                //Check if a param has been found.
                if (methodParam == null) return false;

                if (parameter.Type == null)
                {
                    //If null, null has been passed so the type has to be a value type.
                    if (!methodParam.Type.IsValueType) return false;
                }
                else
                {
                    //If not null, parameter has to be assignable to 
                    if (!methodParam.Type.IsAssignableFrom(parameter.Type)) return false;
                }

                //Override value with passed value
                methodParam.Value = parameter.Value;
                methodParam.Selected = true;
            }

            return selectableArguments.All(x => x.Selected || x.HasDefaultValue);
        }

        private static Invoker GetInvoker(InvokeMemberBinder binder, IList<object> args)
        {
            var list = new List<Argument>();
            var names = new Stack<string>(binder.CallInfo.ArgumentNames);

            //Named parameters can always be mapped directly on the last parameters.
            for (var i = args.Count - 1; i >= 0; i--)
            {
                var argument = args[i];
                string name = null;

                if (names.Count > 0)
                    name = names.Pop();

                var arg = new Argument(name, argument);
                list.Insert(0, arg);
            }

            var methods = Info[binder.Name];
            IEnumerable<Argument> actualArguments = null;

            var method = (from methodInfo in methods
                          where CompareParameters(list, methodInfo.GetParameters().ToList(), out actualArguments)
                          select methodInfo).FirstOrDefault();

            if (method == null)
                return null;

            return new Invoker(method, actualArguments.Select(x => x.Value).ToArray());
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
            var proprety = Properties[binder.Name];
            if (proprety == null)
            {
                result = null;
                return false;
            }

            result = proprety.GetValue(_value);
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var proprety = Properties[binder.Name];
            if (proprety == null)
            {
                return false;
            }

            proprety.SetValue(_value, value);
            return true;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            var valueType = typeof(T);

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