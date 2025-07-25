﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PTS.SmartXml
{
    public static class SharedUtils
    {

        public static bool ContainsNode(XmlNode parent, XmlNode node, bool ignoreAttributesWhenMatching)
        {
            XmlNode temp = null;
            return ContainsNode(parent, node, ref temp, ignoreAttributesWhenMatching);
        }

        public static bool ContainsNode(XmlNode parent, XmlNode node, ref XmlNode foundNode, bool ignoreAttributesWhenMatching)
        {
            XmlAttributeCollection attrs = node.Attributes;
            foreach (XmlNode childNode in parent.ChildNodes)
            {
                if (childNode.Name == node.Name)
                {
                    if (ignoreAttributesWhenMatching)
                    {
                        foundNode = childNode;
                        return true;
                    }
                    XmlAttributeCollection attrsChild = childNode.Attributes;
                    if (attrs == null && attrsChild == null)
                    {
                        foundNode = childNode;
                        return true;
                    }
                    if (attrs != null && attrsChild != null && attrs.Count == attrsChild.Count)
                    {
                        bool b = true;
                        foreach (XmlAttribute attr in attrs)
                        {
                            XmlNode attrChild = attrsChild.GetNamedItem(attr.Name);
                            if (attrChild == null)
                            {
                                b = false;
                                break;
                            }
                            if (attrChild.Value != attr.Value)
                            {
                                b = false;
                                break;
                            }
                        }
                        if (b)
                        {
                            foundNode = childNode;
                            return true;
                        }
                    }
                }
            }
            foundNode = null;
            return false;
        }
    }
}
