using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.CodeQL.Analytics.Cli.Commands.Reports
{
    public class MarkdownElements
    {
        public virtual void createHeading(string heading, int depth, StreamWriter writer)
        {
            int i = 0;
            string headingDepth = string.Empty;
            while(i < depth)
            {
                headingDepth += "#";
                i++;
            }
            writer.WriteLine(headingDepth + " " + heading);
        }
        public virtual string createChartsFolder(string filepath)
        {
           var dir = (Path.Combine(Path.GetDirectoryName(filepath), "charts"));
           if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
         
        }
    }
}
