#define JSON
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace api
{
    class Program
    {
        static void Main(string[] args)
        {
            //clone_test.Main();
            Type cls = typeof(IdInfo);
            //FieldMethod.GetMethod(cls, "TestReflection");
            //FieldMethod.InitNameList();
            //FieldMethod.InitConfigJson();
            FieldMethod res = new FieldMethod();
            res.Main();

            //FieldMethod.GetParam(cls, null);
        }
    }

    public class test
    {
        [Conditional("DEBUG")]
        public void CheckState()
        {
            Console.WriteLine("debug success");
        }

        public void PrintOut()
        {
            CheckState();
            //int[] foo = (from n in Enumerable.Range(0, 100)
            //             select n * n).ToArray();

            //foo.ForAll((n) => Console.WriteLine(n.ToString()));
            Console.WriteLine("end");
        }

    }

    public static class Exceptions
    {
        public static void ForAll<T>(
            this IEnumerable<T> sequence, Action<T> action)
        {
            foreach (T item in sequence)
            {
                action(item);
            }
        }
    }
}
