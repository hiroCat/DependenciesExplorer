using System.Collections.Generic;
using System.Xml;

namespace GEXFConverter
{
    public static class ConverterToGEXF
    {
        public static void Convert(List<string> node, List<(string,string)> edges)
        {
            var nodeDict = getDictForNodes(node);

            var doc = new XmlDocument();
            var d = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            var r = doc.DocumentElement;
            doc.InsertBefore(d, r);

            var gexf = doc.CreateElement("gexf");
            doc.AppendChild(gexf);

            var graph = doc.CreateElement("graph");
            var graphAtt = doc.CreateAttribute("mode");
            var graphAtt2 = doc.CreateAttribute("defaultedgetype");
            graphAtt.Value = "static";
            graphAtt2.Value = "directed";
            graph.Attributes.Append(graphAtt);
            graph.Attributes.Append(graphAtt2);
            gexf.AppendChild(graph);

            var nodes = doc.CreateElement("nodes");
            graph.AppendChild(nodes);
            foreach(var n in nodeDict)
            {
                var nd = doc.CreateElement("node");
                var ndAtt = doc.CreateAttribute("id");
                ndAtt.Value = n.Value.ToString();
                nd.Attributes.Append(ndAtt);
                var ndAtt2 = doc.CreateAttribute("label");
                ndAtt2.Value = n.Key;
                nd.Attributes.Append(ndAtt2);
                nodes.AppendChild(nd);
            }

            var edg = doc.CreateElement("edges");
            graph.AppendChild(edg);
            var i = 0;
            foreach (var e in edges)
            {
                var ed = doc.CreateElement("edge");
                var edAtt = doc.CreateAttribute("id");
                edAtt.Value = i.ToString();
                ed.Attributes.Append(edAtt);
                var edAtt2 = doc.CreateAttribute("source");
                edAtt2.Value = nodeDict[e.Item1].ToString();
                ed.Attributes.Append(edAtt2);
                var edAtt3 = doc.CreateAttribute("target");
                edAtt3.Value = nodeDict[e.Item2].ToString(); 
                ed.Attributes.Append(edAtt3);
                edg.AppendChild(ed);
                ++i;
            }

            doc.Save("thatfile.xml");
        }

        private static Dictionary<string, int> getDictForNodes(List<string> nodes)
        {
            var nodeDict = new Dictionary<string, int>();
            var i = 0;
            foreach (var n in nodes)
            {
                nodeDict.Add(n, i);
                ++i;
            }
            return nodeDict;
        }
    }
}

//<gexf xmlns = "http://www.gexf.net/1.2draft" version="1.2">
//    <meta lastmodifieddate = "2009-03-20" >
//        < creator > Gexf.net </ creator >
//        < description > A hello world! file</description>
//    </meta>
//    <graph mode = "static" defaultedgetype="directed">
//        <nodes>
//            <node id = "0" label="Hello" />
//            <node id = "1" label="Word" />
//        </nodes>
//        <edges>
//            <edge id = "0" source="0" target="1" />
//        </edges>
//    </graph>
//</gexf>