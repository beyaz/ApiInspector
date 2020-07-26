using System;

namespace ApiInspector.Invoking.InstanceCreators
{
    /// <summary>
    ///     The instance creator
    /// </summary>
    class InstanceCreator
    {
        #region Public Methods
        /// <summary>
        ///     Creates the specified target type.
        /// </summary>
        public object Create(Type targetType, BOAContext boaContext)
        {
            var instance = InstanceCreatorForObjectHelperDerivedClasses.TryCreate(targetType, boaContext);
            if (instance != null)
            {
                return instance;
            }

            return InstanceCreatorDefault.TryCreate(targetType, boaContext);
        }
        #endregion
    }
}