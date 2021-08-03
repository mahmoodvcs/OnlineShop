using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

namespace MahtaKala.SharedServices
{
    public abstract class NestedScope : IDisposable
    {
        private static readonly ThreadLocal<Dictionary<Type, NestedScope>> allScopes = new ThreadLocal<Dictionary<Type, NestedScope>>(() => new Dictionary<Type, NestedScope>());

        protected NestedScope()
        {
            allScopes.Value[GetType()] = this;
        }

        ~NestedScope()
        {
            Dispose(false);
        }

        internal static object GetState()
        {
            return allScopes.Value;
        }

        internal static void RestoreState(object state)
        {
            if (state == null)
            {
                ClearState();
                return;
            }

            if (!(state is Dictionary<Type, NestedScope> dic))
            {
                throw new Exception("Invalid NestedScope type");
            }

            allScopes.Value = dic;

            foreach (var item in dic)
            {
                var setInstance = typeof(NestedScope<>).MakeGenericType(item.Key).GetMethod("SetInstance", BindingFlags.NonPublic | BindingFlags.Static);
                setInstance.Invoke(null, new object[] { item.Value });
            }
        }

        internal static void ClearState()
        {
            foreach (var item in allScopes.Value)
            {
                var setInstance = typeof(NestedScope<>).MakeGenericType(item.Key).GetMethod("SetInstance", BindingFlags.NonPublic | BindingFlags.Static);
                setInstance.Invoke(null, new object[] { null });
            }

            allScopes.Value = new Dictionary<Type, NestedScope>();
        }

        protected abstract NestedScope GetPrevious();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            var previous = GetPrevious();
            if (previous == null)
            {
                allScopes.Value.Remove(GetType());
            }
            else
            { allScopes.Value[GetType()] = previous; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class NestedScope<T> : NestedScope
            where T : NestedScope<T>
    {
        private static readonly ThreadLocal<T> instance = new ThreadLocal<T>();

        private readonly T previous;

        public NestedScope()
        {
            previous = instance.Value;
            SetInstance((T)this);
        }

        public static T Instance => instance.Value;

        public static bool Exists => instance.Value != null;

        protected sealed override NestedScope GetPrevious() => previous;

        protected T GetTypedPrevious() => previous;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;

            SetInstance(previous);
        }

        private static void SetInstance(T inst)
        {
            instance.Value = inst;
        }
    }
}
