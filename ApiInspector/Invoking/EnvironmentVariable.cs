using System;

namespace ApiInspector.Invoking
{
    class EnvironmentVariable
    {
        public string GetUserName()
        {
            return Environment.UserName;
        }
    }
}