using System;
using System.Collections.Generic;
using System.Linq;

namespace FunEngine.Encoding
{
    public class CSV
    {

        //读取csv
        private const string csvFormat = @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)";

        public static List<string[]> Read(string csvText)
        {
            List<string[]> list = new List<string[]>();
            string[] lines = csvText.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string rawStr = lines[i];
                if (!String.IsNullOrEmpty(rawStr) && rawStr != "\r")
                {
                    string[] content = (from System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(rawStr,
                                        csvFormat,
                                        System.Text.RegularExpressions.RegexOptions.ExplicitCapture)
                                        select m.Groups[1].Value).ToArray();
                    list.Add(content);
                }
            }
            //
            return list;
        }
    }
}
