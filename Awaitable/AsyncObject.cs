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

        private static Invoker GetInvoker(InvokeMemberBinder binder, IEnumerable<object> args)
        {
            var callers = TypeInfo<T>.GetMethod(binder.Name);
            var tuple = CallerSelector.SelectMethod(binder, args, callers);
            
            if (tuple == null) return null;

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
            return TypeInfo<T>.TryGetProperty(_value, binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return TypeInfo<T>.TrySetProperty(_value, binder.Name, value);
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