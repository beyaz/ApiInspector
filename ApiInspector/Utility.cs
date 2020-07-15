using System;

namespace ApiInspector
{
    static class Utility
    {
        #region Public Methods
        public static Exception TryRun(Action action)
        {
            try
            {
                action();
                return null;
            }
            catch (Exception e)
            {
                return e;
            }
        }
        #endregion
    }
}