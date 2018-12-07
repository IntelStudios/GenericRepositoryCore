using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Helpers
{
    public static class GRInstantiator
    {
        public delegate object ConstructorDelegate();

        public static ConstructorDelegate GetConstructor(string typeName)
        {
            Type t = Type.GetType(typeName);
            return GetConstructor(t);
        }

        public static ConstructorDelegate GetConstructor(Type t)
        {
            
            ConstructorInfo ctor = t.GetConstructor(new Type[0]);

            string methodName = t.Name + "Ctor";
            DynamicMethod dm = new DynamicMethod(methodName, t, new Type[0], typeof(Activator));

            ILGenerator ilgen = dm.GetILGenerator();
            ilgen.Emit(OpCodes.Newobj, ctor);
            ilgen.Emit(OpCodes.Ret);

            ConstructorDelegate creator = (ConstructorDelegate)dm.CreateDelegate(typeof(ConstructorDelegate));

            return creator;
        }

        public static T CreateInstance<T>()
        {
            ConstructorDelegate ctr = GetConstructor(typeof(T));
            T entity = (T)ctr();
            return entity;
        }

        public static object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}
