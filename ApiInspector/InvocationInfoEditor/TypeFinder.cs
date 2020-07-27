using System;
using System.Linq;
using System.Reflection;

namespace ApiInspector.InvocationInfoEditor
{
    class TypeFinder
    {
        public Type Find(string fullName)
        {
            var type = Type.GetType(fullName);
            if (type != null)
            {
                return type;
            }

            if (fullName.StartsWith("BOA.Card.Contracts.",StringComparison.OrdinalIgnoreCase))
            {
                type = Assembly.Load(@"BOA.Card.Contracts").GetTypes().FirstOrDefault(t=>t.FullName == fullName);    
            }

            return type;
        }
    }
}