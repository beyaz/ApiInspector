using System;
using System.Linq;
using System.Reflection;
using BOA.Process.Kernel.Card;

namespace ApiInspector.InvocationInfoEditor
{
    class TypeFinder
    {
        #region Public Methods
        public Type Find(string fullName)
        {
            var type = Type.GetType(fullName);
            if (type != null)
            {
                return type;
            }

            if (fullName.StartsWith("BOA.Card.Contracts.", StringComparison.OrdinalIgnoreCase))
            {
                type = Assembly.Load(@"BOA.Card.Contracts").GetTypes().FirstOrDefault(t => t.FullName == fullName);
            }

            if (fullName.StartsWith("BOA.Process.Kernel.Card.", StringComparison.OrdinalIgnoreCase))
            {
                type = Assembly.Load(@"BOA.Process.Kernel.Card").GetTypes().FirstOrDefault(t => t.FullName == fullName);
            }

            if (fullName.StartsWith("BOA.Integration.Model.MobileBranch.", StringComparison.OrdinalIgnoreCase))
            {
                CardService.UseLocalProxy = true;

                type                      = Assembly.Load(@"BOA.Integration.Model.MobileBranch").GetTypes().FirstOrDefault(t => t.FullName == fullName);
            }
            

            return type;
        }
        #endregion
    }
}