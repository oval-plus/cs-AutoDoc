using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api
{
    public class ClassData
    {
        public string Name { get; set; }
        public Type ClassType { get; set; }
        public Type RealType { get; set; }
        public bool IsGeneric { get; set; }
        public HashSet<string> ApiNames { get; set; }
        public List<ClassData> ClassChildren { get; set; }
        public ClassData ClassParent
        {
            get; set;
        }
        public ClassData()
        {
            Name = "";
            IsGeneric = false;
            ClassType = null;
            RealType = null;
            ClassParent = null;
            ApiNames = new HashSet<string>();
            ClassChildren = new List<ClassData>();
        }
    }

    public class ClassTree
    {
        public ClassData Root { get; set; }
        private List<ClassData> class_list;
        public List<ClassData> ClassList
        {
            get { return class_list; }
            set { class_list = value; }
        }

        public ClassTree()
        {
            Root = null;
            ClassList = new List<ClassData>();
        }
    }

    public class ClassTreeComparer : IComparer<ClassTree>
    {
        public int Compare(ClassTree ct1, ClassTree ct2)
        {
            return ct1.Root.Name.CompareTo(ct2.Root.Name);
        }
    }

    public class MethodData : IComparer<MethodData>
    {
        public string ChineseName { get; set; }
        public string Name { get; set; }
        public ClassTree Input { get; set; }
        public ClassTree Output { get; set; }

        public MethodData() { }

        public int Compare(MethodData md1, MethodData md2)
        {
            return md1.Name.CompareTo(md2.Name);
        }
    }

    public class MethodDataComparer : IComparer<MethodData>
    {
        public int Compare(MethodData md1, MethodData md2)
        {
            return md1.Name.CompareTo(md2.Name);
        }
    }

    public class ApiMethod
    {
        public string ApiName { get; set; }
        public string MethodName { get; set; }
        public string ChineseName { get; set; }
        public string LogicType { get; set; }
        public string InputDll { get; set; }
        public string OutputDll { get; set; }
        public string InputParam { get; set; }
        public ApiMethod()
        {
            ApiName = "";
            MethodName = "";
            ChineseName = "";
            LogicType = "";
            InputDll = "";
            OutputDll = "";
            InputParam = "";
        }
    }

}
