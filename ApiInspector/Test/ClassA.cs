using System;
using BOA.Common.Helpers;
using BOA.Common.Types;

namespace ApiInspector.Test
{
    [Serializable]
    public class ClassA
    {
        public ClassA InnerA { get; set; }

        public string StringProp0 { get; set; }
        public string StringProp1 { get; set; }
        public string StringProp2 { get; set; }
        public string StringProp21 { get; set; }

        public int IntProp0 { get; set; }
        public int IntProp1 { get; set; }
        public int IntProp2 { get; set; }

        public int? NullableIntProp0 { get; set; }
        public int? NullableIntProp1 { get; set; }
        public int? NullableIntProp2 { get; set; }

        public short  ShortProp0 { get; set; }
        public short  ShortProp1 { get; set; }
        public short  ShortProp2 { get; set; }

        public short? NullableShortProp0 { get; set; }
        public short? NullableShortProp1 { get; set; }
        public short? NullableShortProp2 { get; set; }


        public bool  BooleanProp0 { get; set; }
        public bool  BooleanProp1 { get; set; }
        public bool  BooleanProp2 { get; set; }

        public bool? NullableBooleanProp0 { get; set; }
        public bool? NullableBooleanProp1 { get; set; }
        public bool? NullableBooleanProp2 { get; set; }

        public DateTime  DateTimeProp0 { get; set; }
        public DateTime  DateTimeProp1 { get; set; }
        public DateTime  DateTimeProp2 { get; set; }

        public DateTime? NullableDateTimeProp0 { get; set; }
        public DateTime? NullableDateTimeProp1 { get; set; }
        public DateTime? NullableDateTimeProp2 { get; set; }
    }


    [Serializable]
    public class ClassB
    {
        public ClassB InnerB { get; set; }
        public ClassA InnerA { get; set; }

        public string StringProp0 { get; set; }
        public string StringProp1 { get; set; }
        public string StringProp2 { get; set; }
        public string StringProp21 { get; set; }

        public int IntProp0 { get; set; }
        public int IntProp1 { get; set; }
        public int IntProp2 { get; set; }

        public int? NullableIntProp0 { get; set; }
        public int? NullableIntProp1 { get; set; }
        public int? NullableIntProp2 { get; set; }

        public short  ShortProp0 { get; set; }
        public short  ShortProp1 { get; set; }
        public short  ShortProp2 { get; set; }

        public short? NullableShortProp0 { get; set; }
        public short? NullableShortProp1 { get; set; }
        public short? NullableShortProp2 { get; set; }


        public bool  BooleanProp0 { get; set; }
        public bool  BooleanProp1 { get; set; }
        public bool  BooleanProp2 { get; set; }

        public bool? NullableBooleanProp0 { get; set; }
        public bool? NullableBooleanProp1 { get; set; }
        public bool? NullableBooleanProp2 { get; set; }

        public DateTime  DateTimeProp0 { get; set; }
        public DateTime  DateTimeProp1 { get; set; }
        public DateTime  DateTimeProp2 { get; set; }

        public DateTime? NullableDateTimeProp0 { get; set; }
        public DateTime? NullableDateTimeProp1 { get; set; }
        public DateTime? NullableDateTimeProp2 { get; set; }

        public string Method0()
        {
            return "Called: string Method0()";
        }

        public string Method0(string p0)
        {
            return "Called: string Method0(string p0)";
        }
        public string Method0(string p0,string p1)
        {
            return "Called: string Method0(string p0,string p1)";
        }
        public string Method0(string p0,int p1)
        {
            return "Called: string Method0(string p0,int p1)";
        }

        public string Method1(string p0,int p1,ClassB bInstance)
        {
            return "Called: public string Method1(string p0,int p1,ClassB bInstance)";
        }

        public ClassA Method2(string p0,int p1,ClassB bInstance)
        {
            return new ClassA();
        }

        public string[] Method3(string p0,int p1,ClassB bInstance)
        {
            return new[] {"A"};
        }

        public static int AddOne(int value)
        {
            return value + 1;
        }

        public static GenericResponse<ClassA> GetGenericResponseA(string stringProp0, string stringProp1)
        {
            var response = ResponseFactoryHelper.GetGenericResponse<ClassA>();

            response.Value = new ClassA
            {
                StringProp0 = stringProp0,
                StringProp1 = stringProp1
            };

            return response;
        }

    }
    
}
