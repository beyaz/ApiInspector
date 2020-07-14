namespace ApiInspector.Test
{
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
        #endregion
    }
}