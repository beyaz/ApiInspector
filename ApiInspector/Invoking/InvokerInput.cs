using System;
using System.Collections.Generic;
using System.Reflection;
using ApiInspector.Models;

namespace ApiInspector.Invoking
{
    class InvokerInput
    {
        public InvocationInfo InvocationInfo       { get; }
        public Action<string> Trace                { get; }
        public BOAContext     BoaContext           { get; }
        public MethodInfo     MethodInfo           { get; set; }
        public List<object>   InvocationParameters { get; set; }
        public Type           TargetType           { get; set; }

        #region Constructors
       
        public InvokerInput(InvocationInfo invocationInfo, Action<string> trace, BOAContext boaContext)
        {
            InvocationInfo = invocationInfo;
            Trace          = trace;
            BoaContext     = boaContext;
        }
        #endregion
        
    }
}