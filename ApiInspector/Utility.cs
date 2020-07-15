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

        public static bool IsSuccess<T>(Func<T> action, ref  T target)
        {
            try
            {
                target = action();

                return true;
            }
            catch (Exception )
            {
                return false;
            }

        }
        #endregion
    }
}