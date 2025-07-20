using PTS.SmartXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace PTS
{
    public class PatchOperationAddIfModPresent : PatchOperationPathed
    {
        public string doesRequire;
        public bool ignoreAttributesWhenMatching = false;

        public XmlContainer value;

        public override void Complete(string modIdentifier)
        {
            base.Complete(modIdentifier);
        }

        protected override bool ApplyWorker(XmlDocument xml)
        {
            if (!string.IsNullOrWhiteSpace(doesRequire))
            {
                var split = doesRequire.Split(',');
                foreach (var mod in split)
                {
                    if (!ModsConfig.IsActive(mod.Trim()))
                    {
                        return true; //a required mod isnt loaded, so we dont apply the patch
                    }
                }
            }

            XmlNode node = value.node;
            bool matched = false;
            foreach (XmlNode xmlNode in xml.SelectNodes(xpath).Cast<XmlNode>().ToArray())
            {
                matched = true;
                foreach (XmlNode addNode in node.ChildNodes)
                {
                    xmlNode.AppendChild(xmlNode.OwnerDocument.ImportNode(addNode, true));
                }
            }
            return matched;
        }
    }
}