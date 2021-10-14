#define JSON
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Configuration;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace api
{
    public class FieldMethod
    {
        private static List<string> DefaultNameList;
        private static List<string> NameList;
        private static Configuration configXML;
        private static IConfiguration configJson;
        private static List<ApiMethod> ApiMethods;
        internal static List<ClassTree> VisitedData;

        public FieldMethod() 
        {
            DefaultNameList = new List<string>();
            NameList = new List<string>();
            VisitedData = new List<ClassTree>();
            ApiMethods = new List<ApiMethod>();
        }

        [Conditional("XML")]
        private static void InitConfigXML()
        {
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = AppDomain.CurrentDomain.BaseDirectory + @"config/App.config";
            configXML = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            DefaultNameList = new List<string>(configXML.AppSettings.Settings["default_data"].Value.Split(new char[] { ';' }));
            NameList = new List<string>(configXML.AppSettings.Settings["exception_data"].Value.Split(new char[] { ';' }));
            NameList.AddRange(DefaultNameList);
        }

        [Conditional("JSON")]
        public static void InitConfigJson()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"config/config.json";
            configJson = new ConfigurationBuilder().AddJsonFile(path).Build();
            configJson.GetSection("ApiMethod").Bind(ApiMethods);
            DefaultNameList = new List<string>(configJson["DefaultData"].Split(new char[] { ';' }));
            NameList = new List<string>(configJson["ExceptionData"].Split(new char[] { ';' }));
            NameList.AddRange(DefaultNameList);
        }

        public void Main()
        {
            // 1. 读config文件找到方法名和方法返回参数，找方法传参
            InitConfigJson();
            // 2. 反射从dll中找类
            Assembly assemblyDll = Assembly.LoadFrom(configJson["dll_path"]); // load dll

            // 3. 建立返回参数和传参的树
            List<MethodData> methodDataList = ConstructFieldTree(assemblyDll);
            // 5. 写md
            GenerateMd md = new GenerateMd();
            md.ConvertToHtml("aggregate", methodDataList);

        }

        private List<MethodData> ConstructFieldTree(Assembly assemblyDll)
        {
            List<MethodData> methodDataList = new List<MethodData>();
            for (int i = 0; i < ApiMethods.Count(); ++i)
            {
                ApiMethod apiMethod = ApiMethods[i];
                Type logicType = assemblyDll.GetType(apiMethod.LogicType);
                MethodInfo cur_method = logicType.GetMethod(apiMethod.ApiName);
                Type returnType = cur_method.ReturnType;
                ClassTree tree = null;
                if (returnType != typeof(void))
                {
                    tree = GetParam(returnType);
                    AddTree(tree.Root, apiMethod.ChineseName);
                }

                ClassTree inputTree = null;
                if (apiMethod.InputDll != "" && apiMethod.InputParam != "")
                {
                    Assembly inputDll = Assembly.LoadFrom(apiMethod.InputDll); // load dll
                    Type inputType = inputDll.GetType(apiMethod.InputParam);
                    inputTree = GetParam(inputType);
                    AddTree(inputTree.Root, apiMethod.ChineseName);
                }

                // 4. 把方法名和参数进行对应
                MethodData newMethodData = new MethodData();
                newMethodData.Name = apiMethod.ApiName;
                methodDataList.Add(newMethodData);
                newMethodData.ChineseName = apiMethod.ChineseName;
                newMethodData.Output = tree;
                newMethodData.Input = inputTree;
                
            }
            return methodDataList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static ClassTree GetParam(Type dataType)
        {
            ClassData data = new ClassData();
            data.RealType = dataType;
            if (dataType.IsGenericType)
            {
                data.IsGeneric = true;
                data.RealType = dataType.GetGenericArguments()[0];
            }
            data.ClassType = dataType;
            data.ClassParent = null;
            data.Name = data.RealType.Name;
            ClassTree tree = new ClassTree();
            tree.Root = data;
            // 如果有新类，需要把新类添加进树中
            ConstructTree(tree);
            return tree;
        }

        private void AddTree(ClassData cur, string apiName)
        {
            ClassTree finder = new ClassTree();
            finder.Root = cur;
            ClassTreeComparer comparer = new ClassTreeComparer();
            int loc = VisitedData.BinarySearch(finder, comparer);
            ClassTree treeLoc = null;
            if (loc < 0)
            {
                treeLoc = new ClassTree();
                treeLoc.Root = cur;
                VisitedData.Add(treeLoc);
            }
            else
            {
                treeLoc = VisitedData[loc];
            }
            treeLoc.Root.ApiNames.Add(apiName);
        }

        private static void ConstructTree( ClassTree tree)
        {
            // bfs
            Queue<ClassData> queue = new Queue<ClassData>();
            queue.Enqueue(tree.Root);
            HashSet<string> visited = new HashSet<string>();

            while (queue.Count() != 0)
            {
                ClassData cur = queue.Dequeue();
                visited.Add(cur.Name);
                FieldInfo[] fieldInfos = cur.RealType.GetTypeInfo().GetFields();
                foreach (FieldInfo childCur in fieldInfos)
                {
                    Type childType = childCur.FieldType;
                    // 如果该类为泛型，需要去除泛型得到真实的类
                    if (childCur.FieldType.IsGenericType)
                    {
                        childType = childCur.FieldType.GetGenericArguments()[0];
                    }
                    if (NameList.Contains(childType.Name)) { continue; }
                    if (visited.Contains(childCur.Name)) { continue; }
                    ClassData childData = new ClassData();
                    childData.ClassParent = cur;
                    childData.ClassType = childCur.FieldType;
                    childData.RealType = childType;
                    childData.Name = childCur.Name;
                    cur.ClassChildren.Add(childData);
                    tree.ClassList.Add(childData);
                    queue.Enqueue(childData);
                }
            }

        }
    }
}
