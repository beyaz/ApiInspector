using System;
using BOA.Base.Data;

namespace ApiInspector.Invoking.InstanceCreators
{
    /// <summary>
    ///     The instance creator for object helper derived classes
    /// </summary>
    class InstanceCreatorForObjectHelperDerivedClasses
    {
        #region Public Methods
        /// <summary>
        ///     Tries the create.
        /// </summary>
        public static object TryCreate(Type targetType, BOAContext boaContext)
        {
            var constructorInfo = targetType.GetConstructor(new[]
            {
                typeof(ExecutionDataContext)
            });
            if (constructorInfo != null)
            {
                var instance = constructorInfo.Invoke(new object[]
                {
                    boaContext.GetObjectHelper().Context
                });
                return instance;
            }

            return null;
        }
        #endregion
    }
}