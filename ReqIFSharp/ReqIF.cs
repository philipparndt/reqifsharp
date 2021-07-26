﻿// -------------------------------------------------------------------------------------------------
// <copyright file="ReqIF.cs" company="RHEA System S.A.">
//
//   Copyright 2017 RHEA System S.A.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace ReqIFSharp
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// The <see cref="ReqIF"/> class constitutes the root element of a ReqIF Exchange Document.
    /// </summary>
    [Serializable]
    [XmlRoot("REQ-IF", Namespace = "http://www.omg.org/spec/ReqIF/20110401/reqif.xsd", IsNullable = false)]
    public class ReqIF : IXmlSerializable
    {
        /// <summary>
        /// Gets the XML Attributes in the document. Use this to add additional namespaces.
        /// </summary>
        public List<XmlAttribute> Attributes = new List<XmlAttribute>();

        /// <summary>
        /// Gets the mandatory Exchange Document header, which contains metadata relevant for this exchange.
        /// </summary>
        public ReqIFHeader TheHeader { get; set; }

        /// <summary>
        /// Gets the mandatory Exchange Document content.
        /// </summary>
        public ReqIFContent CoreContent { get; set; }

        /// <summary>
        /// Gets the optional Exchange Document content based on tool extensions, if such extensions and content are present.
        /// </summary>
        public List<ReqIFToolExtension> ToolExtension { get; set; } = new List<ReqIFToolExtension>();

        /// <summary>
        /// Gets or sets the default language encoding of the Exchange XML Document content
        /// </summary>
        /// <remarks>
        /// The format is defined by the standard for specifying languages in XML documents proposed by the W3C <see cref="http://www.w3.org/TR/xml11/#sec-lang-tag"/>
        /// </remarks>
        public string Lang { get; set; }

        /// <summary>
        /// This method is reserved and should not be used.
        /// </summary>
        /// <returns>returns null</returns>
        /// <remarks>
        /// When implementing the IXmlSerializable interface, you should return null
        /// </remarks>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Generates a <see cref="ReqIF"/> object from its XML representation.
        /// </summary>
        /// <param name="reader">
        /// an instance of <see cref="XmlReader"/>
        /// </param>
        public void ReadXml(XmlReader reader)
        {
            this.Lang = reader.GetAttribute("xml:lang");

            while (reader.MoveToNextAttribute())
            {
                if (reader.Name != "xml:lang")
                {
                    var xmlAttribute = new ReqIFSharp.XmlAttribute
                    {
                        Prefix = reader.Prefix,
                        LocalName = reader.LocalName,
                        Value = reader.Value
                    };

                    Attributes.Add(xmlAttribute);
                }
            }

            while (reader.Read())
            {
                if (reader.MoveToContent() == XmlNodeType.Element)
                {
                    switch (reader.LocalName)
                    {
                        case "THE-HEADER":
                            var headerSubTreeXmlReader = reader.ReadSubtree();
                            this.TheHeader = new ReqIFHeader { DocumentRoot = this };
                            this.TheHeader.ReadXml(headerSubTreeXmlReader);
                            break;
                        case "CORE-CONTENT":
                            var coreContentTreeXmlReader = reader.ReadSubtree();
                            this.CoreContent = new ReqIFContent { DocumentRoot = this };
                            this.CoreContent.ReadXml(coreContentTreeXmlReader);
                            break;
                        case "TOOL-EXTENSIONS":
                            var toolExtensionsXmlReader = reader.ReadSubtree();

                            while (toolExtensionsXmlReader.Read())
                            {
                                if (toolExtensionsXmlReader.MoveToContent() == XmlNodeType.Element && toolExtensionsXmlReader.LocalName == "REQ-IF-TOOL-EXTENSION")
                                {
                                    var reqIfToolExtensionSubTreeXmlReader = toolExtensionsXmlReader.ReadSubtree();

                                    var reqIfToolExtension = new ReqIFToolExtension();
                                    reqIfToolExtension.ReadXml(reqIfToolExtensionSubTreeXmlReader);
                                    this.ToolExtension.Add(reqIfToolExtension);
                                }
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Converts a <see cref="ReqIF"/> object into its XML representation.
        /// </summary>
        /// <param name="writer">
        /// an instance of <see cref="XmlWriter"/>
        /// </param>
        public void WriteXml(XmlWriter writer)
        {
            if (!string.IsNullOrEmpty(this.Lang))
            {
                writer.WriteAttributeString("lang", "xml", this.Lang);
            }

            foreach (var xmlAttribute in this.Attributes)
            {
                if (xmlAttribute.Prefix != string.Empty)
                {
                    if (xmlAttribute.Prefix == "xmlns")
                    {
                        writer.WriteAttributeString(xmlAttribute.Prefix, xmlAttribute.LocalName,  null, xmlAttribute.Value);
                    }
                    else
                    {
                        writer.WriteAttributeString(xmlAttribute.LocalName, xmlAttribute.Prefix, xmlAttribute.Value);
                    }
                }
                else
                {
                    writer.WriteAttributeString(xmlAttribute.LocalName, xmlAttribute.Value);
                }
            }

            this.WriteTheHeader(writer);
            this.WriteCoreContent(writer);
            this.WriteToolExtension(writer);
        }

        /// <summary>
        /// Writes the <see cref="ReqIFHeader"/> 
        /// </summary>
        /// <param name="writer">
        /// an instance of <see cref="XmlWriter"/>
        /// </param>
        private void WriteTheHeader(XmlWriter writer)
        {
            writer.WriteStartElement("THE-HEADER");
            writer.WriteStartElement("REQ-IF-HEADER");
            this.TheHeader.WriteXml(writer);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the <see cref="ReqIFContent"/> 
        /// </summary>
        /// <param name="writer">
        /// an instance of <see cref="XmlWriter"/>
        /// </param>
        private void WriteCoreContent(XmlWriter writer)
        {
            writer.WriteStartElement("CORE-CONTENT");
            writer.WriteStartElement("REQ-IF-CONTENT");
            this.CoreContent.WriteXml(writer);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the <see cref="ReqIFToolExtension"/> 
        /// </summary>
        /// <param name="writer">
        /// an instance of <see cref="XmlWriter"/>
        /// </param>
        private void WriteToolExtension(XmlWriter writer)
        {
            if (this.ToolExtension.Count == 0)
            {
                return;
            }

            writer.WriteStartElement("TOOL-EXTENSIONS");
            foreach (var reqIfToolExtension in this.ToolExtension)
            {
                writer.WriteStartElement("REQ-IF-TOOL-EXTENSION");
                reqIfToolExtension.WriteXml(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
    }
}
