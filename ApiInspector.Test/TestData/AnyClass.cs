using System;

namespace ApiInspector.TestData
{
    [Serializable]
    public class AnyInput
    {
        public string StringProperty { get; set; }
        public int IntegerProperty { get; set; }

    }
    class AnyClass
    {
        #region Public Methods
        public string AnyMethod_0()
        {
            return "0";
        }

        public string AnyMethod_1(string a, int b, string c)
        {
            return a + "-" + b + "-" + c;
        }

        public static string AnyMethod_2( string a, int b, string c, int d)
        {
            return a + "-" + b + "-" + c+"-"+d;
        }

        public string AnyMethod_3(string a, int b, AnyInput anyInput, string c)
        {
            return a + "-" + b + "-" +anyInput.StringProperty + "-"+anyInput.IntegerProperty +"-" +  c;
        }
        #endregion
    }
}