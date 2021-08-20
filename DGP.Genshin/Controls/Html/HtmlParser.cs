using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace DGP.Genshin.Controls.Html
{
    /// <summary>
    ///     HtmlParser class accepts a string of possibly badly formed Html, parses it and returns a string
    ///     of well-formed Html that is as close to the original string in content as possible
    /// </summary>
    internal class HtmlParser
    {
        // ---------------------------------------------------------------------
        //
        // Constructors
        //
        // ---------------------------------------------------------------------

        #region Constructors

        /// <summary>
        ///     Constructor. Initializes the _htmlLexicalAnalayzer element with the given input string
        /// </summary>
        /// <param name="inputString">
        ///     string to parsed into well-formed Html
        /// </param>
        private HtmlParser(string inputString)
        {
            // Create an output xml document
            this._document = new XmlDocument();

            // initialize open tag stack
            this._openedElements = new Stack<XmlElement>();

            this._pendingInlineElements = new Stack<XmlElement>();

            // initialize lexical analyzer
            this._htmlLexicalAnalyzer = new HtmlLexicalAnalyzer(inputString);

            // get first token from input, expecting text
            this._htmlLexicalAnalyzer.GetNextContentToken();
        }

        #endregion Constructors

        // ---------------------------------------------------------------------
        //
        // Internal Methods
        //
        // ---------------------------------------------------------------------

        #region Internal Methods

        /// <summary>
        ///     Instantiates an HtmlParser element and calls the parsing function on the given input string
        /// </summary>
        /// <param name="htmlString">
        ///     Input string of pssibly badly-formed Html to be parsed into well-formed Html
        /// </param>
        /// <returns>
        ///     XmlElement rep
        /// </returns>
        internal static XmlElement ParseHtml(string htmlString)
        {
            HtmlParser htmlParser = new HtmlParser(htmlString);

            XmlElement htmlRootElement = htmlParser.ParseHtmlContent();

            return htmlRootElement;
        }

        // .....................................................................
        //
        // Html Header on Clipboard
        //
        // .....................................................................

        // Html header structure.
        //      Version:1.0
        //      StartHTML:000000000
        //      EndHTML:000000000
        //      StartFragment:000000000
        //      EndFragment:000000000
        //      StartSelection:000000000
        //      EndSelection:000000000
        internal const string HtmlHeader =
            "Version:1.0\r\nStartHTML:{0:D10}\r\nEndHTML:{1:D10}\r\nStartFragment:{2:D10}\r\nEndFragment:{3:D10}\r\nStartSelection:{4:D10}\r\nEndSelection:{5:D10}\r\n";

        internal const string HtmlStartFragmentComment = "<!--StartFragment-->";
        internal const string HtmlEndFragmentComment = "<!--EndFragment-->";

        /// <summary>
        ///     Extracts Html string from clipboard data by parsing header information in htmlDataString
        /// </summary>
        /// <param name="htmlDataString">
        ///     String representing Html clipboard data. This includes Html header
        /// </param>
        /// <returns>
        ///     String containing only the Html data part of htmlDataString, without header
        /// </returns>
        internal static string ExtractHtmlFromClipboardData(string htmlDataString)
        {
            int startHtmlIndex = htmlDataString.IndexOf("StartHTML:", StringComparison.Ordinal);
            if (startHtmlIndex < 0)
            {
                return "ERROR: Urecognized html header";
            }
            // TODO: We assume that indices represented by strictly 10 zeros ("0123456789".Length),
            // which could be wrong assumption. We need to implement more flrxible parsing here
            startHtmlIndex =
                Int32.Parse(htmlDataString.Substring(startHtmlIndex + "StartHTML:".Length, "0123456789".Length));
            if (startHtmlIndex < 0 || startHtmlIndex > htmlDataString.Length)
            {
                return "ERROR: Urecognized html header";
            }

            int endHtmlIndex = htmlDataString.IndexOf("EndHTML:", StringComparison.Ordinal);
            if (endHtmlIndex < 0)
            {
                return "ERROR: Urecognized html header";
            }
            // TODO: We assume that indices represented by strictly 10 zeros ("0123456789".Length),
            // which could be wrong assumption. We need to implement more flrxible parsing here
            endHtmlIndex = Int32.Parse(htmlDataString.Substring(endHtmlIndex + "EndHTML:".Length, "0123456789".Length));
            if (endHtmlIndex > htmlDataString.Length)
            {
                endHtmlIndex = htmlDataString.Length;
            }

            return htmlDataString.Substring(startHtmlIndex, endHtmlIndex - startHtmlIndex);
        }

        /// <summary>
        ///     Adds Xhtml header information to Html data string so that it can be placed on clipboard
        /// </summary>
        /// <param name="htmlString">
        ///     Html string to be placed on clipboard with appropriate header
        /// </param>
        /// <returns>
        ///     String wrapping htmlString with appropriate Html header
        /// </returns>
        internal static string AddHtmlClipboardHeader(string htmlString)
        {
            StringBuilder stringBuilder = new StringBuilder();

            // each of 6 numbers is represented by "{0:D10}" in the format string
            // must actually occupy 10 digit positions ("0123456789")
            int startHtml = HtmlHeader.Length + 6 * ("0123456789".Length - "{0:D10}".Length);
            int endHtml = startHtml + htmlString.Length;
            int startFragment = htmlString.IndexOf(HtmlStartFragmentComment, 0, StringComparison.Ordinal);
            if (startFragment >= 0)
            {
                startFragment = startHtml + startFragment + HtmlStartFragmentComment.Length;
            }
            else
            {
                startFragment = startHtml;
            }
            int endFragment = htmlString.IndexOf(HtmlEndFragmentComment, 0, StringComparison.Ordinal);
            if (endFragment >= 0)
            {
                endFragment = startHtml + endFragment;
            }
            else
            {
                endFragment = endHtml;
            }

            // Create HTML clipboard header string
            stringBuilder.AppendFormat(HtmlHeader, startHtml, endHtml, startFragment, endFragment, startFragment,
                endFragment);

            // Append HTML body.
            stringBuilder.Append(htmlString);

            return stringBuilder.ToString();
        }

        #endregion Internal Methods

        // ---------------------------------------------------------------------
        //
        // Private methods
        //
        // ---------------------------------------------------------------------

        #region Private Methods

        private void InvariantAssert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception("Assertion error: " + message);
            }
        }

        /// <summary>
        ///     Parses the stream of html tokens starting
        ///     from the name of top-level element.
        ///     Returns XmlElement representing the top-level
        ///     html element
        /// </summary>
        private XmlElement ParseHtmlContent()
        {
            // Create artificial root elelemt to be able to group multiple top-level elements
            // We create "html" element which may be a duplicate of real HTML element, which is ok, as HtmlConverter will swallow it painlessly..
            XmlElement htmlRootElement = this._document.CreateElement("html", XhtmlNamespace);
            OpenStructuringElement(htmlRootElement);

            while (this._htmlLexicalAnalyzer.NextTokenType != HtmlTokenType.Eof)
            {
                if (this._htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.OpeningTagStart)
                {
                    this._htmlLexicalAnalyzer.GetNextTagToken();
                    if (this._htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.Name)
                    {
                        string htmlElementName = this._htmlLexicalAnalyzer.NextToken.ToLower();
                        this._htmlLexicalAnalyzer.GetNextTagToken();

                        // Create an element
                        XmlElement htmlElement = this._document.CreateElement(htmlElementName, XhtmlNamespace);

                        // Parse element attributes
                        ParseAttributes(htmlElement);

                        if (this._htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.EmptyTagEnd ||
                            HtmlSchema.IsEmptyElement(htmlElementName))
                        {
                            // It is an element without content (because of explicit slash or based on implicit knowledge aboout html)
                            AddEmptyElement(htmlElement);
                        }
                        else if (HtmlSchema.IsInlineElement(htmlElementName))
                        {
                            // Elements known as formatting are pushed to some special
                            // pending stack, which allows them to be transferred
                            // over block tags - by doing this we convert
                            // overlapping tags into normal heirarchical element structure.
                            OpenInlineElement(htmlElement);
                        }
                        else if (HtmlSchema.IsBlockElement(htmlElementName) ||
                                 HtmlSchema.IsKnownOpenableElement(htmlElementName))
                        {
                            // This includes no-scope elements
                            OpenStructuringElement(htmlElement);
                        }
                    }
                }
                else if (this._htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.ClosingTagStart)
                {
                    this._htmlLexicalAnalyzer.GetNextTagToken();
                    if (this._htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.Name)
                    {
                        string htmlElementName = this._htmlLexicalAnalyzer.NextToken.ToLower();

                        // Skip the name token. Assume that the following token is end of tag,
                        // but do not check this. If it is not true, we simply ignore one token
                        // - this is our recovery from bad xml in this case.
                        this._htmlLexicalAnalyzer.GetNextTagToken();

                        CloseElement(htmlElementName);
                    }
                }
                else if (this._htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.Text)
                {
                    AddTextContent(this._htmlLexicalAnalyzer.NextToken);
                }
                else if (this._htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.Comment)
                {
                    AddComment(this._htmlLexicalAnalyzer.NextToken);
                }

                this._htmlLexicalAnalyzer.GetNextContentToken();
            }

            // Get rid of the artificial root element
            if (htmlRootElement.FirstChild is XmlElement &&
                htmlRootElement.FirstChild == htmlRootElement.LastChild &&
                htmlRootElement.FirstChild.LocalName.ToLower() == "html")
            {
                htmlRootElement = (XmlElement)htmlRootElement.FirstChild;
            }

            return htmlRootElement;
        }

        private XmlElement CreateElementCopy(XmlElement htmlElement)
        {
            XmlElement htmlElementCopy = this._document.CreateElement(htmlElement.LocalName, XhtmlNamespace);
            for (int i = 0; i < htmlElement.Attributes.Count; i++)
            {
                XmlAttribute attribute = htmlElement.Attributes[i];
                htmlElementCopy.SetAttribute(attribute.Name, attribute.Value);
            }
            return htmlElementCopy;
        }

        private void AddEmptyElement(XmlElement htmlEmptyElement)
        {
            InvariantAssert(this._openedElements.Count > 0,
                "AddEmptyElement: Stack of opened elements cannot be empty, as we have at least one artificial root element");
            XmlElement htmlParent = this._openedElements.Peek();
            htmlParent.AppendChild(htmlEmptyElement);
        }

        private void OpenInlineElement(XmlElement htmlInlineElement) => this._pendingInlineElements.Push(htmlInlineElement);

        // Opens structurig element such as Div or Table etc.
        private void OpenStructuringElement(XmlElement htmlElement)
        {
            // Close all pending inline elements
            // All block elements are considered as delimiters for inline elements
            // which forces all inline elements to be closed and re-opened in the following
            // structural element (if any).
            // By doing that we guarantee that all inline elements appear only within most nested blocks
            if (HtmlSchema.IsBlockElement(htmlElement.LocalName))
            {
                while (this._openedElements.Count > 0 && HtmlSchema.IsInlineElement(this._openedElements.Peek().LocalName))
                {
                    XmlElement htmlInlineElement = this._openedElements.Pop();
                    InvariantAssert(this._openedElements.Count > 0,
                        "OpenStructuringElement: stack of opened elements cannot become empty here");

                    this._pendingInlineElements.Push(CreateElementCopy(htmlInlineElement));
                }
            }

            // Add this block element to its parent
            if (this._openedElements.Count > 0)
            {
                XmlElement htmlParent = this._openedElements.Peek();

                // Check some known block elements for auto-closing (LI and P)
                if (HtmlSchema.ClosesOnNextElementStart(htmlParent.LocalName, htmlElement.LocalName))
                {
                    this._openedElements.Pop();
                    htmlParent = this._openedElements.Count > 0 ? this._openedElements.Peek() : null;
                }

                // NOTE:
                // Actually we never expect null - it would mean two top-level P or LI (without a parent).
                // In such weird case we will loose all paragraphs except the first one...
                htmlParent?.AppendChild(htmlElement);
            }

            // Push it onto a stack
            this._openedElements.Push(htmlElement);
        }

        private bool IsElementOpened(string htmlElementName) => this._openedElements.Any(openedElement => openedElement.LocalName == htmlElementName);

        private void CloseElement(string htmlElementName)
        {
            // Check if the element is opened and already added to the parent
            InvariantAssert(this._openedElements.Count > 0,
                "CloseElement: Stack of opened elements cannot be empty, as we have at least one artificial root element");

            // Check if the element is opened and still waiting to be added to the parent
            if (this._pendingInlineElements.Count > 0 && this._pendingInlineElements.Peek().LocalName == htmlElementName)
            {
                // Closing an empty inline element.
                // Note that HtmlConverter will skip empty inlines, but for completeness we keep them here on parser level.
                XmlElement htmlInlineElement = this._pendingInlineElements.Pop();
                InvariantAssert(this._openedElements.Count > 0,
                    "CloseElement: Stack of opened elements cannot be empty, as we have at least one artificial root element");
                XmlElement htmlParent = this._openedElements.Peek();
                htmlParent.AppendChild(htmlInlineElement);
            }
            else if (IsElementOpened(htmlElementName))
            {
                while (this._openedElements.Count > 1) // we never pop the last element - the artificial root
                {
                    // Close all unbalanced elements.
                    XmlElement htmlOpenedElement = this._openedElements.Pop();

                    if (htmlOpenedElement.LocalName == htmlElementName)
                    {
                        return;
                    }

                    if (HtmlSchema.IsInlineElement(htmlOpenedElement.LocalName))
                    {
                        // Unbalances Inlines will be transfered to the next element content
                        this._pendingInlineElements.Push(CreateElementCopy(htmlOpenedElement));
                    }
                }
            }

            // If element was not opened, we simply ignore the unbalanced closing tag
        }

        private void AddTextContent(string textContent)
        {
            OpenPendingInlineElements();

            InvariantAssert(this._openedElements.Count > 0,
                "AddTextContent: Stack of opened elements cannot be empty, as we have at least one artificial root element");

            XmlElement htmlParent = this._openedElements.Peek();
            XmlText textNode = this._document.CreateTextNode(textContent);
            htmlParent.AppendChild(textNode);
        }

        private void AddComment(string comment)
        {
            OpenPendingInlineElements();

            InvariantAssert(this._openedElements.Count > 0,
                "AddComment: Stack of opened elements cannot be empty, as we have at least one artificial root element");

            XmlElement htmlParent = this._openedElements.Peek();
            XmlComment xmlComment = this._document.CreateComment(comment);
            htmlParent.AppendChild(xmlComment);
        }

        // Moves all inline elements pending for opening to actual document
        // and adds them to current open stack.
        private void OpenPendingInlineElements()
        {
            if (this._pendingInlineElements.Count > 0)
            {
                XmlElement htmlInlineElement = this._pendingInlineElements.Pop();

                OpenPendingInlineElements();

                InvariantAssert(this._openedElements.Count > 0,
                    "OpenPendingInlineElements: Stack of opened elements cannot be empty, as we have at least one artificial root element");

                XmlElement htmlParent = this._openedElements.Peek();
                htmlParent.AppendChild(htmlInlineElement);
                this._openedElements.Push(htmlInlineElement);
            }
        }

        private void ParseAttributes(XmlElement xmlElement)
        {
            while (this._htmlLexicalAnalyzer.NextTokenType != HtmlTokenType.Eof && //
                   this._htmlLexicalAnalyzer.NextTokenType != HtmlTokenType.TagEnd && //
                   this._htmlLexicalAnalyzer.NextTokenType != HtmlTokenType.EmptyTagEnd)
            {
                // read next attribute (name=value)
                if (this._htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.Name)
                {
                    string attributeName = this._htmlLexicalAnalyzer.NextToken;
                    this._htmlLexicalAnalyzer.GetNextEqualSignToken();

                    this._htmlLexicalAnalyzer.GetNextAtomToken();

                    string attributeValue = this._htmlLexicalAnalyzer.NextToken;
                    xmlElement.SetAttribute(attributeName, attributeValue);
                }
                this._htmlLexicalAnalyzer.GetNextTagToken();
            }
        }

        #endregion Private Methods

        // ---------------------------------------------------------------------
        //
        // Private Fields
        //
        // ---------------------------------------------------------------------

        #region Private Fields

        internal const string XhtmlNamespace = "http://www.w3.org/1999/xhtml";

        private readonly HtmlLexicalAnalyzer _htmlLexicalAnalyzer;

        // document from which all elements are created
        private readonly XmlDocument _document;

        // stack for open elements
        private readonly Stack<XmlElement> _openedElements;
        private readonly Stack<XmlElement> _pendingInlineElements;

        #endregion Private Fields
    }
}
