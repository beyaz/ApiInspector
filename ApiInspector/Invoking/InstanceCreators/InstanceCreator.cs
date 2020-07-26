using System;

namespace ApiInspector.Invoking
{
    class InstanceCreator
    {
            
            
        public object Create(Type targetType, BOAContext boaContext)
        {
            var instance = InstanceCreatorForObjectHelperDerivedClasses.TryCreate(targetType, boaContext);
            if (instance != null)
            {
                return instance;
            }

            return InstanceCreatorDefault.TryCreate(targetType, boaContext);
        }
    }
}