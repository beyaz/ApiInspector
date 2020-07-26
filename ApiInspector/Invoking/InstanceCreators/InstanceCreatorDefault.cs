using System;
using BOA.Base;

namespace ApiInspector.Invoking
{
    /// <summary>
    ///     The instance creator default
    /// </summary>
    class InstanceCreatorDefault
    {
        #region Public Methods
        /// <summary>
        ///     Tries the create.
        /// </summary>
        public static object TryCreate(Type targetType, BOAContext boaContext)
        {
            var instance     = Activator.CreateInstance(targetType);
            var objectHelper = instance as ObjectHelper;
            if (objectHelper != null)
            {
                objectHelper.Context = boaContext.GetObjectHelper().Context;
            }

            return instance;
        }
        #endregion
    }
}