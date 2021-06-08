using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WSLIPConf.ViewModels;

namespace WSLIPConf.Helpers
{
    public class QuickCSV : ObservableBase
    {

        private string text;

        private List<List<string>> rows;

        private List<string> columnHeaders;


        public QuickCSV()
        {
            rows = new List<List<string>>();
            columnHeaders = new List<string>();
        }

        public QuickCSV(string text)
        {
            Text = text;
        }

        public string Text
        {
            get => text;
            set
            {
                if (SetProperty(ref text, value))
                {
                    IngestText();
                }
            }
        }

        public List<List<string>> Rows
        {
            get => rows;
            set
            {
                SetProperty(ref rows, value);
            }
        }

        public List<string> ColumnHeaders
        {
            get => columnHeaders;
            set
            {
                SetProperty(ref columnHeaders, value);
            }
        }


        private void IngestText()
        {
            rows = new List<List<string>>();
            columnHeaders = new List<string>();

            if (string.IsNullOrEmpty(text)) return;

            bool inQuote = false, inEsc = false;

            var cleaned = text.Trim();

            var lines = text.Split("\r\n");

            StringBuilder sb = new StringBuilder();
            var lineOut = new List<string>();

            foreach (var line in lines)
            {
                var chars = line.ToCharArray();

                foreach (var inChar in chars)
                {
                    if (inQuote)
                    {
                        // quoted string logic

                        if (!inEsc && inChar == '\\')
                        {
                            // escaped character avoidance logic
                            inEsc = true;
                        }
                        else if (inEsc)
                        {
                            // escaped character avoided, continue scanning quoted string.
                            inEsc = false;
                        }
                        else if (inChar == '\"')
                        {
                            // quoted string complete, switch back to object scanning.
                            inQuote = false;
                        }
                        else
                        {
                            sb.Append(inChar);
                        }
                    }
                    else if (inChar == '\"')
                    {
                        // quoted string avoidance logic
                        inQuote = true;
                    }
                    else if (inChar == ',')
                    {
                        lineOut.Add(sb.ToString());
                    }
                }

                rows.Add(lineOut);
                lineOut = new List<string>();
            }

            columnHeaders.AddRange(rows[0]);

        }

    }
}
