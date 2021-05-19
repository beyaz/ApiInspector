namespace ApiInspector.TestData
{
    /// <summary>
    ///     Any class
    /// </summary>
    class AnyClass
    {
        #region Public Methods
        /// <summary>
        ///     Anies the method 2.
        /// </summary>
        public static string AnyMethod_2(string a, int b, string c, int d)
        {
            return a + "-" + b + "-" + c + "-" + d;
        }

        /// <summary>
        ///     Anies the method 0.
        /// </summary>
        public string AnyMethod_0()
        {
            return "0";
        }

        /// <summary>
        ///     Anies the method 1.
        /// </summary>
        public string AnyMethod_1(string a, int b, string c)
        {
            return a + "-" + b + "-" + c;
        }

       

        /// <summary>
        ///     Anies the method 3.
        /// </summary>
        public string AnyMethod_3(string a, int b, AnyInput anyInput, string c)
        {
            return a + "-" + b + "-" + anyInput.StringProperty + "-" + anyInput.IntegerProperty + "-" + c;
        }
        #endregion
    }
}