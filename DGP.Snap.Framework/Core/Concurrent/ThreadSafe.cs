using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace DGP.Snap.Framework.Core.Concurrent
{
    /// <summary>
    /// Making any call to a function thread safe
    /// <code>Calculation c = new Calculation();
    /// dynamic ts = new ThreadSafe(c);
    /// ts.Add();
    /// </code>
    /// </summary>
    public class ThreadSafe : DynamicObject
    {
        private readonly object o;
        private readonly TypeInfo t;
        private readonly object myLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadSafe"/> class.
        /// </summary>
        /// <param name="o">
        /// The wrapped object whose operations will be made thread-safe.
        /// </param>
        public ThreadSafe(object o)
        {
            this.o = o;
            this.t = o.GetType().GetTypeInfo();
            this.myLock = new object();
        }

        /// <inheritdoc />
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            PropertyInfo prop = this.t.GetDeclaredProperty(binder.Name);
            if (prop != null)
            {
                lock (this.myLock)
                {
                    prop.SetValue(this.o, value);
                }

                return true;
            }

            FieldInfo field = this.t.GetDeclaredField(binder.Name);
            if (field != null)
            {
                lock (this.myLock)
                {
                    field.SetValue(this.o, value);
                }

                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            PropertyInfo prop = this.t.GetDeclaredProperty(binder.Name);
            if (prop != null)
            {
                lock (this.myLock)
                {
                    result = prop.GetValue(this.o);
                }

                return true;
            }

            FieldInfo field = this.t.GetDeclaredField(binder.Name);
            if (field != null)
            {
                lock (this.myLock)
                {
                    result = field.GetValue(this.o);
                }

                return true;
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Attempts to find the indexer property.
        /// </summary>
        /// <returns>
        /// The <see cref="PropertyInfo"/> for the indexer property, if found;
        /// otherwise, <see langword="null"/>.
        /// </returns>
        private PropertyInfo GetIndexedProperty()
        {
            // TODO: Is there a better way to do this?

            PropertyInfo prop = this.t.GetDeclaredProperty("Item");
            if (prop == null || prop.GetIndexParameters().Length == 0)
            {
                prop = this.t.DeclaredProperties.FirstOrDefault(p => p.GetIndexParameters().Length != 0);
            }

            return prop;
        }

        /// <inheritdoc />
        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            PropertyInfo prop = this.GetIndexedProperty();
            if (prop != null)
            {
                lock (this.myLock)
                {
                    prop.SetValue(this.o, value, indexes);
                }

                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            PropertyInfo prop = this.GetIndexedProperty();
            if (prop != null)
            {
                lock (this.myLock)
                {
                    result = prop.GetValue(this.o, indexes);
                }

                return true;
            }

            result = null;
            return false;
        }

        /// <inheritdoc />
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            MethodInfo method = this.t.GetDeclaredMethod(binder.Name);
            if (method != null)
            {
                lock (this.myLock)
                {
                    result = method.Invoke(this.o, args);
                }

                return true;
            }

            result = null;
            return false;
        }
    }
}
