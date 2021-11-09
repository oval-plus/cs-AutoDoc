using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using System.Reflection;

namespace api
{
    public class GenerateMd
    {
        public GenerateMd()
        {
        }

        public void ConvertToHtml(List<MethodData> methodDataList)
        {
            string mdText = BuildFuncName(FieldMethod.configJson["FuncName"]);
            mdText += BuildTOC();
            foreach (MethodData methodData in methodDataList)
            {
                mdText += BuildApi(methodData);
            }
            mdText += BuildMethod();
            string res = Markdig.Markdown.ToHtml(mdText);
            string outputPath = FieldMethod.configJson["OutputPath"] + FieldMethod.configJson["FuncName"];
            System.IO.FileInfo fi = new System.IO.FileInfo(outputPath + @".pdf");
            PdfWriter writer = new PdfWriter(fi);
            HtmlConverter.ConvertToPdf(res, writer);
            using (StreamWriter sw = new StreamWriter(outputPath + @".md"))
            {
                sw.WriteLine(mdText);
            }
            Console.WriteLine("Generate Complete!");
        }

        private string BuildFuncName(string funcName)
        {
            return "# " + funcName + " \n";
        }

        private string BuildApiName(string titleName, bool isChinese = true)
        {
            string name;
            if (isChinese)
            {
                name = "## " + titleName;
            }
            else
            {
                name = "**api name:** " + titleName + "\n";
            }
            return name + "\n";
        }

        private string BuildMember(Type member)
        {
            string res = member.Name + "\n";
            foreach (FieldInfo field in member.GetFields())
            {
                Type fieldType = field.FieldType;
                string realType = fieldType.Name;
                if (fieldType.IsGenericType)
                {
                    realType = realType.Substring(4, realType.Length - 6) + "<" + fieldType.GetGenericArguments()[0].Name + ">";
                }
                if (realType == "Int64")
                {
                    realType = "long";
                }
                if (realType == "Int32") { realType = "int"; }
                res += realType + " " + field.Name + "; \n";
            }
            return res;
        }

        private string BuildAnchor(string anchor)
        {
            return "[" + anchor + "]" + "(#" + anchor + ")";
        }

        /// <summary>
        /// build template of content
        /// </summary>
        /// <returns></returns>
        private string BuildTOC()
        {
            return "[TOC] \n\n";
        }

        private string BuildParam(ClassTree outputParam, string apiName, bool isOutput)
        {
            string res;
            if (isOutput) { res = "**back->front** \n"; }
            else { res = "**front->back** \n"; }
            if (outputParam is null)
            {
                return res + "No params" + "\n";
            }
            ClassData cur = outputParam.Root;
            res += BuildAnchor(cur.RealType.Name) + "\n";
            res += "\n";
            return res;
        }

        private string BuildApi(MethodData methodData)
        {
            string res = "";

            res += BuildApiName(methodData.ChineseName, true);
            res += BuildApiName(methodData.Name, false);
            res += BuildParam(methodData.Input, methodData.ChineseName, false);
            res += BuildParam(methodData.Output, methodData.ChineseName, true);
            return res;
        }

        private string BuildMethod()
        {
            string res = BuildFuncName(funcName: "方法内容") ;
            // 1. 找到每个需要添加在方法内容中的方法
            foreach (ClassTree tree in FieldMethod.VisitedData)
            {
                // 2. 给方法添加标题和用途锚点
                res += BuildApiName(titleName: tree.Root.Name, true);
                res += "used for: ";
                foreach (string apiName in tree.Root.ApiNames)
                {
                    res += BuildAnchor(apiName) + ", ";
                }
                res += "\n";
                // 3. 把方法展开
                res += "```cs\n";
                Queue<ClassData> queue = new Queue<ClassData>();
                queue.Enqueue(tree.Root);

                while (queue.Count() != 0)
                {
                    ClassData cur = queue.Dequeue();
                    res += BuildMember(cur.RealType) + "\n";
                    foreach (ClassData child in cur.ClassChildren)
                    {
                        queue.Enqueue(child);
                    }
                }
                res += "```\n";
            }
            return res;
        }
    }
}
