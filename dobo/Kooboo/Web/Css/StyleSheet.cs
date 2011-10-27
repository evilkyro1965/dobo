using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Kooboo.Web.Css
{
    public class StyleSheet
    {
        private List<Statement> _statements;

        public IList<Statement> Statements
        {
            get
            {
                if (_statements == null)
                {
                    _statements = new List<Statement>();
                }
                return _statements;
            }
        }

        public static StyleSheet Load(string str)
        {
            string s = Regex.Replace(str, @"(/\*[\w\W]*?\*/)|(<--)|(-->)", String.Empty);
            return Load(new StyleSheetReader(s));
        }

        public static StyleSheet Load(StyleSheetReader reader)
        {
            StyleSheet result = new StyleSheet();

            while (!reader.EndOfStream)
            {
                switch (reader.Status)
                {
                    case StyleSheetReader.ReadStatus.AtRule:
                        result.Statements.Add(AtRule.Parse(reader.ReadAtRule()));
                        break;
                    case StyleSheetReader.ReadStatus.RuleSet:
                        result.Statements.Add(RuleSet.Parse(reader.ReadRuleSet()));
                        break;
                    default:
                        break;
                }                
            }

            return result;
        }

        public static StyleSheet Load(TextReader reader)
        {
            return Load(new StyleSheetReader(reader));
        }

        public static StyleSheet Load(Stream stream)
        {
            return Load(new StyleSheetReader(stream));
        }
    }
}
