using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Offworld.SystemCore;

namespace Offworld.GameCore.Text
{
    public static class TextEntryGlobals
    {
        private static Dictionary<string, string> substitutions = null;
        private static Dictionary<string, ParsedInfoText> parsedTextInfos = null;

        public static Dictionary<string, string> Substitutions { set { substitutions = value; } }
        public static Dictionary<string, ParsedInfoText> ParsedTextInfos { set { parsedTextInfos = value; } }

        public static bool TryGetSubstitution(string key, out string result)
        {
            if ((substitutions != null) && substitutions.ContainsKey(key))
            {
                result = substitutions [key];
                return true; //success
            }
            else
            {
                result = null;
                return false; //failure
            }
        }

        public static bool TryGetTextEntry(string key, LanguageType language, out TextEntry result)
        {
            if((parsedTextInfos != null) && parsedTextInfos.ContainsKey(key))
            {
                result = parsedTextInfos[key].GetEntry(language);
                return true; //success
            }
            else
            {
                result = null;
                return false;
            }
        }
    }

    public class TextEntry
    {
        private enum TextEntryState
        {
            INITIALIZED,
            PARSING,
            READY,
        }

        private TextEntryState state = TextEntryState.INITIALIZED;
        private string original;
        private TextNodeRoot [] entries;
        private LanguageType language;
        private GenderType gender = GenderType.MALE;

        public int NumEntries { get { Parse(); return entries.Length; } }
        public GenderType Gender { get { Parse(); return gender; } }

        public TextEntry(string original, LanguageType language = 0)
        {
            this.original = original;
            this.language = language;
            Assert.IsNotNull(this.original);
        }

        //this is a potentially recursive call graph, so detect infinite cycles and log an error
        public void Parse()
        {
            if(state == TextEntryState.INITIALIZED)
            {
                using (new UnityProfileScope("TextEntry.Parse"))
                {
                    state = TextEntryState.PARSING;
                    gender = ParseGender(ref original);
                    entries = Parse(original, language);
                    Assert.IsTrue(entries.Length >= 1);
                    state = TextEntryState.READY;
                }
            }
            else if(state == TextEntryState.PARSING) //cycle detected
            {
                Debug.LogError("[Text] Infinite cycle detected while parsing: " + original);
            }
        }

        public string Evaluate(int index)
        {
            Parse();
            if(index >= entries.Length)
                index = 0;

            return entries[index].Evaluate();
        }

        public StringBuilder Evaluate(StringBuilder output, int index, params TextVariable [] arguments)
        {
            Parse();
            if(index >= entries.Length)
                index = 0;

            entries[index].Evaluate(output, language, arguments);
            return output;
        }

        //helper function for UnitTests
        public string EvaluateHelper(int index, params TextVariable [] arguments)
        {
            return Evaluate(new StringBuilder(), index, arguments).ToString();
        }

        public bool IsSimpleNode(int index)
        {
            Parse();
            if(index >= entries.Length)
                index = 0;

            return entries[index].IsSimpleNode();
        }

        private static TextNodeRoot [] Parse(string value, LanguageType language)
        {
            if(value.Contains("~")) //multi-case
            {
                string [] parts = value.Split('~');
                TextNodeRoot [] result = new TextNodeRoot [parts.Length];
                for(int i=0; i<parts.Length; i++)
                    result[i] = new TextNodeRoot(parts[i], language);

                return result;
            }
            else //simple case
            {
                TextNodeRoot [] result = {new TextNodeRoot(value, language)};
                return result;
            }
        }

        private static GenderType ParseGender(ref string value)
        {
            GenderType result = GenderType.MALE;
            if (value.Contains("<male>"))
            {
                result = GenderType.MALE;
                value = value.Replace("<male>", "");
            }

            if (value.Contains("<female>"))
            {
                result = GenderType.FEMALE;
                value = value.Replace("<female>", "");
            }

            return result;
        }
    }

    interface TextNode
    {
        string Evaluate();
        void Evaluate(StringBuilder output, LanguageType language, TextVariable [] arguments);
    }

    interface TextNodeContainer : TextNode
    {
        void Add(TextNode node);
    }

    class TextNodeRoot : TextNode
    {
        private static Regex outerRegex;
        private static Regex markupRegex;
        private static Regex braceRegex;
        private TextNode contents;

        public TextNodeRoot(string source, LanguageType language)
        {
            contents = Parse(source, language);
        }

        public string Evaluate()
        {
            return contents.Evaluate();
        }

        public void Evaluate(StringBuilder output, LanguageType language, TextVariable [] arguments)
        {
            contents.Evaluate(output, language, arguments);
        }

        public bool IsSimpleNode()
        {
            return (contents is TextNodeString);
        }

        private static TextNode Parse(string value, LanguageType language)
        {
            //https://msdn.microsoft.com/en-us/library/az24scfc%28v=vs.110%29.aspx
            //extracts "{...}" and "<...>" tokens
            if(outerRegex == null)
            {
                const string bracePair = @"\{(?<BraceContents>.*?)\}";
                const string markupPair = @"\<(?<MarkupContents>.*?)\>";
                outerRegex = new Regex(bracePair + "|" + markupPair, RegexOptions.ExplicitCapture | RegexOptions.Compiled);
            }

            MatchCollection matches = null;
            if(value.Contains("{") || value.Contains("<"))
            {
                using(new UnityProfileScope("OuterMatches"))
                    matches = outerRegex.Matches(value);
            }
            
            if((matches == null) || (matches.Count == 0)) //no special matches
            {
                return new TextNodeString(value);
            }
            else //parse variables
            {
                using (new UnityProfileScope("ParseMatches"))
                {
                    Stack<TextNodeContainer> nodeStack = new Stack<TextNodeContainer>();
                    nodeStack.Push(new TextNodeList()); //root

                    int currentIndex = 0;
                    foreach(Match match in matches)
                    {
                        if(match.Index > currentIndex) //fill in regular text tokens
                        {
                            string plainText = value.Substring(currentIndex, match.Index - currentIndex);
                            nodeStack.Peek().Add(new TextNodeString(plainText));
                        }

                        //advance currentIndex to the end of the match
                        currentIndex = match.Index + match.Length;

                        //add new token
                        if(match.Groups["BraceContents"].Success)
                        {
                            if(!ParseBrace(nodeStack, match.Groups["BraceContents"].Value))
                                return ReportError(value);
                        }
                        else if(match.Groups["MarkupContents"].Success) 
                        {
                            if(!ParseMarkup(nodeStack, match.Groups["MarkupContents"].Value, language))
                                return ReportError(value);
                        }
                        else //error
                        {
                            return ReportError(value);
                        }
                    }

                    //fill in last regular text token
                    if(currentIndex < value.Length)
                    {
                        string plainText = value.Substring(currentIndex);
                        nodeStack.Peek().Add(new TextNodeString(plainText));
                    }

                    //check if balanced
                    if(nodeStack.Count != 1)
                    {
                        return ReportError(value);
                    }

                    //check if it reduced to single plain text
                    TextNodeList result = nodeStack.Pop() as TextNodeList;
                    if(result == null)
                    {
                        return ReportError(value);
                    }

                    //success
                    return result.GetSimplestForm();
                }
            }
        }

        private static bool ParseBrace(Stack<TextNodeContainer> nodeStack, string contents)
        {
            using (new UnityProfileScope("ParseBrace"))
            {
                if (braceRegex == null)
                {
                    const string argumentNumber = @"^(?<Argument>\d+)";
                    const string optionalIndex = @"(,(?<Index>\d+))?";
                    braceRegex = new Regex(argumentNumber + optionalIndex, RegexOptions.ExplicitCapture | RegexOptions.Compiled);
                }

                string trimmedContents = contents.Trim();
                Match match = braceRegex.Match(trimmedContents);

                if(!match.Success)
                    return false; //failed

                int argument = 0;
                if(!int.TryParse(match.Groups["Argument"].Value, out argument))
                    return false; //failed

                int index = 0;
                if(match.Groups["Index"].Success && !int.TryParse(match.Groups["Index"].Value, out index))
                    return false; //failed

                //success
                nodeStack.Peek().Add(new TextNodeArgument(argument, index, "{" + contents + "}"));
                return true;
            }
        }

        private static bool ParseMarkup(Stack<TextNodeContainer> nodeStack, string contents, LanguageType language)
        {
            using (new UnityProfileScope("ParseMarkup"))
            {
                if (markupRegex == null)
                {
                    const string singular = @"(?<Singular>singular_(?<Argument>\d+))";
                    const string plural = "(?<Plural>plural)";
                    const string plural2 = "(?<Plural2>plural2)";
                    const string trueExpression = @"(?<True>true_(?<Argument>\d+))";
                    const string falseExpression = "(?<False>false)";
                    const string masculineExpression = @"(?<Masculine>masculine_(?<Argument>\d+))";
                    const string feminineExpression = "(?<Feminine>feminine)";
                    const string end = "(?<End>end)";
                    const string capitalization = @"(?<Capitalization>/?((?<Uppercase>uppercase)|(?<Lowercase>lowercase)|(?<Sentencecase>sentencecase)))";
                    const string comment = "(?<Comment>comment.*)";
                    const string color = "(?<Color>color=(?<Argument>.*))";
                    const string or = "|";
                    const string lineBeginning = "^(";
                    const string lineEnding = ")$";

                    markupRegex = new Regex(lineBeginning + singular + or + plural + or + plural2
                                            + or + trueExpression + or + falseExpression
                                            + or + masculineExpression + or + feminineExpression
                                            + or + end + or + capitalization
                                            + or + comment + or + color + lineEnding,
                                            RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.Compiled);
                }

                string trimmedContents = contents.Trim();
                Match match = markupRegex.Match(trimmedContents);
                if (!match.Success)
                {
                    //check if substitution available
                    string substitute;
                    if(TextEntryGlobals.TryGetSubstitution(trimmedContents, out substitute))
                    {
                        nodeStack.Peek().Add(new TextNodeString(substitute));
                    }
                    else
                    {
                        //check if referencing other text entry
                        string [] arguments = trimmedContents.Split(',');
                        TextEntry otherEntry;
                        if(TextEntryGlobals.TryGetTextEntry(arguments[0], language, out otherEntry))
                        {
                            if(arguments.Length > 2)
                                return false; //error

                            int index = 0;
                            if((arguments.Length == 2) && !int.TryParse(arguments[1], out index))
                                return false; //error

                            nodeStack.Peek().Add(new TextNodeString(otherEntry.Evaluate(index)));
                        }
                        else //otherwise, leave markup unmodified
                        {
                            nodeStack.Peek().Add(new TextNodeString("<" + trimmedContents + ">"));
                        }
                    }
                }
                else if (match.Groups ["Singular"].Success)
                {
                    int argument = 0;
                    if (!int.TryParse(match.Groups ["Argument"].Value, out argument))
                        return false; //failed

                    //push new node onto the stack
                    TextNodeSingularPlural node = new TextNodeSingularPlural(argument);
                    nodeStack.Peek().Add(node);
                    nodeStack.Push(node);
                }
                else if (match.Groups ["Plural"].Success)
                {
                    TextNodeSingularPlural node = nodeStack.Peek() as TextNodeSingularPlural;
                    if ((node == null) || (node.ParsingPluralType != PluralType.SINGULAR))
                        return false; //failed

                    node.ParsingPluralType = PluralType.PLURAL;
                }
                else if (match.Groups ["Plural2"].Success)
                {
                    TextNodeSingularPlural node = nodeStack.Peek() as TextNodeSingularPlural;
                    if ((node == null) || (node.ParsingPluralType != PluralType.PLURAL))
                        return false; //failed
                
                    node.ParsingPluralType = PluralType.PLURAL2;
                }
                else if (match.Groups ["True"].Success)
                {
                    int argument = 0;
                    if (!int.TryParse(match.Groups ["Argument"].Value, out argument))
                        return false; //failed
                
                    //push new node onto the stack
                    TextNodeTrueFalse node = new TextNodeTrueFalse(argument);
                    nodeStack.Peek().Add(node);
                    nodeStack.Push(node);
                }
                else if (match.Groups ["False"].Success)
                {
                    TextNodeTrueFalse node = nodeStack.Peek() as TextNodeTrueFalse;
                    if ((node == null) || !node.IsParsingTrueNode)
                        return false; //failed
                
                    node.IsParsingTrueNode = false;
                }
                else if (match.Groups ["Masculine"].Success)
                {
                    int argument = 0;
                    if (!int.TryParse(match.Groups ["Argument"].Value, out argument))
                        return false; //failed

                    //push new node onto the stack
                    TextNodeMasculineFeminine node = new TextNodeMasculineFeminine(argument);
                    nodeStack.Peek().Add(node);
                    nodeStack.Push(node);
                }
                else if (match.Groups ["Feminine"].Success)
                {
                    TextNodeMasculineFeminine node = nodeStack.Peek() as TextNodeMasculineFeminine;
                    if ((node == null) || !node.IsParsingMasculineNode)
                        return false; //failed
                
                    node.IsParsingMasculineNode = false;
                }
                else if(match.Groups["End"].Success)
                {
                    //check if matching <singular><plural><end> or <true><false><end>
                    TextNodeSingularPlural nodeSingular = nodeStack.Peek() as TextNodeSingularPlural;
                    TextNodeTrueFalse nodeTrue = nodeStack.Peek() as TextNodeTrueFalse;
                    TextNodeMasculineFeminine nodeMasculine = nodeStack.Peek() as TextNodeMasculineFeminine;
                    if(nodeSingular != null)
                    {
                        if(nodeSingular.ParsingPluralType == PluralType.SINGULAR)
                            return false;
                    }
                    else if(nodeTrue != null)
                    {
                        if(nodeTrue.IsParsingTrueNode)
                            return false;
                    }
                    else if(nodeMasculine != null)
                    {
                        if(nodeMasculine.IsParsingMasculineNode)
                            return false;
                    }
                    else //both null
                    {
                        return false;
                    }

                    //pop node off of stack
                    nodeStack.Pop();
                }
                else if(match.Groups["Capitalization"].Success)
                {
                    CapitalizationType type = CapitalizationType.UPPERCASE;
                    if(match.Groups["Uppercase"].Success)
                        type = CapitalizationType.UPPERCASE;
                    else if(match.Groups["Lowercase"].Success)
                        type = CapitalizationType.LOWERCASE;
                    else if(match.Groups["Sentencecase"].Success)
                        type = CapitalizationType.SENTENCECASE;
                    else
                        return false; //failed

                    if(match.Value.StartsWith("/")) //closing tag
                    {
                        TextNodeCapitalization node = nodeStack.Peek() as TextNodeCapitalization;
                        if((node == null) || (node.CapitalizationType != type))
                            return false; //failed

                        nodeStack.Pop();
                    }
                    else //open tag
                    {
                        TextNodeCapitalization node = new TextNodeCapitalization(type);
                        nodeStack.Peek().Add(node);
                        nodeStack.Push(node);
                    }
                }
                else if(match.Groups["Comment"].Success)
                {
                    //ignore parsed comments
                }
                else if(match.Groups["Color"].Success)
                {
                    string argument = match.Groups ["Argument"].Value.Trim();
                    string substitute;
                    if(!TextEntryGlobals.TryGetSubstitution(argument, out substitute))
                        return false; //failed

                    nodeStack.Peek().Add(new TextNodeString("<color=" + substitute + ">"));
                }
                else
                {
                    return false; //failed
                }

                return true; //success
            }
        }

        private static TextNodeString ReportError(string value)
        {
            Debug.LogError("Error parsing text: " + value);
            return new TextNodeString(value);
        }
    }

    class TextNodeString : TextNode
    {
        private string text;

        public TextNodeString(string text)
        { 
            this.text = text; 
        }

        public void Merge(TextNodeString next)
        {
            text += next.text;
        }

        public string Evaluate()
        {
            return text;
        }

        public void Evaluate(StringBuilder output, LanguageType language, TextVariable [] arguments) 
        { 
            output.Append(text); 
        }
    }

    class TextNodeArgument : TextNode
    {
        private int argument, index;
        private string source;
        
        public TextNodeArgument(int argument, int index, string source)
        {
            this.argument = argument;
            this.index = index;
            this.source = source;
        }
        
        public string Evaluate()
        {
            return source; //error, so leave source as-is
        }

        public void Evaluate(StringBuilder output, LanguageType language, TextVariable [] arguments)
        {
            if((arguments == null) || !MathUtilities.InRange(argument, 0, arguments.Length - 1) || (arguments[argument] == null))
                output.Append(source); //error, so leave source as-is
            else
                arguments[argument].Evaluate(output, language, index);
        }
    }

    class TextNodeSingularPlural : TextNodeContainer
    {
        private int argument;
        private TextNodeList singular = new TextNodeList();
        private TextNodeList plural = new TextNodeList();
        private TextNodeList plural2 = null;
        private PluralType parsingPluralType = PluralType.SINGULAR;

        public PluralType ParsingPluralType
        {
            get { return parsingPluralType; }
            set { parsingPluralType = value; if(parsingPluralType == PluralType.PLURAL2) plural2 = new TextNodeList(); }
        }

        public TextNodeSingularPlural(int argument)
        {
            this.argument = argument;
            parsingPluralType = PluralType.SINGULAR;
        }

        public void Add(TextNode node)
        {
            if(parsingPluralType == PluralType.SINGULAR)
                singular.Add(node);
            else if(parsingPluralType == PluralType.PLURAL)
                plural.Add(node);
            else
                plural2.Add(node);
        }

        public string Evaluate()
        {
            //error, so leave source as-is
            if(plural2 == null)
                return string.Format("<singular_{0}>{1}<plural>{2}<end>", argument, singular.Evaluate(), plural.Evaluate());
            else
                return string.Format("<singular_{0}>{1}<plural>{2}<plural2>{3}<end>", argument, singular.Evaluate(), plural.Evaluate(), plural2.Evaluate());
                
        }

        public void Evaluate(StringBuilder output, LanguageType language, TextVariable [] arguments)
        {
            if((arguments == null) || !MathUtilities.InRange(argument, 0, arguments.Length - 1) || (arguments[argument] == null))
            {
                //error, so leave source as-is
                output.Append("<singular_").Append(argument).Append(">");
                singular.Evaluate(output, language, arguments);
                output.Append("<plural>");
                plural.Evaluate(output, language, arguments);
                if(plural2 != null)
                {
                    output.Append("<plural2>");
                    plural2.Evaluate(output, language, arguments);
                }
                output.Append("<end>");
            }
            else
            {
                PluralType pluralType = arguments[argument].GetPluralType();
                if(pluralType == PluralType.SINGULAR)
                    singular.Evaluate(output, language, arguments);
                else if((pluralType == PluralType.PLURAL) || (plural2 == null))
                    plural.Evaluate(output, language, arguments);
                else
                    plural2.Evaluate(output, language, arguments);
            }
        }
    }

    class TextNodeTrueFalse : TextNodeContainer
    {
        private int argument;
        private TextNodeList trueNode = new TextNodeList();
        private TextNodeList falseNode = new TextNodeList();
        private bool isParsingTrueNode = true;
        
        public bool IsParsingTrueNode
        {
            get { return isParsingTrueNode; }
            set { isParsingTrueNode = value; }
        }
        
        public TextNodeTrueFalse(int argument)
        {
            this.argument = argument;
            isParsingTrueNode = true;
        }
        
        public void Add(TextNode node)
        {
            if(isParsingTrueNode)
                trueNode.Add(node);
            else
                falseNode.Add(node);
        }
        
        public string Evaluate()
        {
            //error, so leave source as-is
            return string.Format("<true_{0}>{1}<false>{2}<end>", argument, trueNode.Evaluate(), falseNode.Evaluate());
        }
        
        public void Evaluate(StringBuilder output, LanguageType language, TextVariable [] arguments)
        {
            if((arguments == null) || !MathUtilities.InRange(argument, 0, arguments.Length - 1) || (arguments[argument] == null))
            {
                //error, so leave source as-is
                output.Append("<true_").Append(argument).Append(">");
                trueNode.Evaluate(output, language, arguments);
                output.Append("<false>");
                falseNode.Evaluate(output, language, arguments);
                output.Append("<end>");
            }
            else
            {
                if(arguments[argument].IsTrue())
                    trueNode.Evaluate(output, language, arguments);
                else
                    falseNode.Evaluate(output, language, arguments);
            }
        }
    }

    class TextNodeMasculineFeminine : TextNodeContainer
    {
        private int argument;
        private TextNodeList masculineNode = new TextNodeList();
        private TextNodeList feminineNode = new TextNodeList();
        private bool isParsingMasculineNode = true;
        
        public bool IsParsingMasculineNode
        {
            get { return isParsingMasculineNode; }
            set { isParsingMasculineNode = value; }
        }
        
        public TextNodeMasculineFeminine(int argument)
        {
            this.argument = argument;
            isParsingMasculineNode = true;
        }
        
        public void Add(TextNode node)
        {
            if(isParsingMasculineNode)
                masculineNode.Add(node);
            else
                feminineNode.Add(node);
        }
        
        public string Evaluate()
        {
            //error, so leave source as-is
            return string.Format("<masculine_{0}>{1}<feminine>{2}<end>", argument, masculineNode.Evaluate(), feminineNode.Evaluate());
        }
        
        public void Evaluate(StringBuilder output, LanguageType language, TextVariable [] arguments)
        {
            if((arguments == null) || !MathUtilities.InRange(argument, 0, arguments.Length - 1) || (arguments[argument] == null))
            {
                //error, so leave source as-is
                output.Append("<masculine_").Append(argument).Append(">");
                masculineNode.Evaluate(output, language, arguments);
                output.Append("<feminine>");
                feminineNode.Evaluate(output, language, arguments);
                output.Append("<end>");
            }
            else
            {
                if(arguments[argument].GetGender(language) == GenderType.MALE)
                    masculineNode.Evaluate(output, language, arguments);
                else
                    feminineNode.Evaluate(output, language, arguments);
            }
        }
    }

    public enum CapitalizationType
    {
        UPPERCASE,
        LOWERCASE,
        SENTENCECASE,
    }

    class TextNodeCapitalization : TextNodeContainer
    {
        private CapitalizationType capitalizationType = CapitalizationType.UPPERCASE;
        private TextNodeList nodeList = new TextNodeList();
        private string evaluateCached = null;

        public CapitalizationType CapitalizationType { get { return capitalizationType; } }

        public TextNodeCapitalization(CapitalizationType capitalizationType)
        {
            this.capitalizationType = capitalizationType;
        }

        public void Add(TextNode node)
        {
            nodeList.Add(node);
        }

        public string Evaluate()
        {
            //consider calculating this more efficiently at load-time
            if(evaluateCached == null)
            {
                StringBuilder output = new StringBuilder();

                int startIndex = output.Length;
                output.Append(nodeList.Evaluate());
                if(capitalizationType == CapitalizationType.UPPERCASE)
                {
                    for(int i=startIndex; i<output.Length; i++)
                        output[i] = char.ToUpperInvariant(output[i]);
                }
                else if(capitalizationType == CapitalizationType.LOWERCASE)
                {
                    for(int i=startIndex; i<output.Length; i++)
                        output[i] = char.ToLowerInvariant(output[i]);
                }
                else if(capitalizationType == CapitalizationType.SENTENCECASE)
                {
                    if(output.Length > startIndex)
                        output[startIndex] = char.ToUpperInvariant(output[startIndex]);
                }

                evaluateCached = output.ToString();
            }

            return evaluateCached;
        }

        public void Evaluate(StringBuilder output, LanguageType language, TextVariable [] arguments)
        {
            int startIndex = output.Length;
            nodeList.Evaluate(output, language, arguments);
            if(capitalizationType == CapitalizationType.UPPERCASE)
            {
                for(int i=startIndex; i<output.Length; i++)
                    output[i] = char.ToUpperInvariant(output[i]);
            }
            else if(capitalizationType == CapitalizationType.LOWERCASE)
            {
                for(int i=startIndex; i<output.Length; i++)
                    output[i] = char.ToLowerInvariant(output[i]);
            }
            else if(capitalizationType == CapitalizationType.SENTENCECASE)
            {
                if(output.Length > startIndex)
                    output[startIndex] = char.ToUpperInvariant(output[startIndex]);
            }
        }
    }

    class TextNodeList : TextNodeContainer
    {
        private List<TextNode> nodes = new List<TextNode>();

        public int Count { get { return nodes.Count; } }

        public TextNodeList()
        { 
        }

        public TextNode GetSimplestForm()
        {
            if(nodes.Count == 1)
                return nodes[0];
            else
                return this;
        }

        public void Add(TextNode node)
        {
            //try to merge consecutive plain text
            if((node is TextNodeString) && (nodes.Count > 0) && (nodes[nodes.Count - 1] is TextNodeString))
            {
                TextNodeString source = node as TextNodeString;
                TextNodeString destination = nodes[nodes.Count - 1] as TextNodeString;
                destination.Merge(source);
            }
            else
            {
                nodes.Add(node);
            }
        }

        public string Evaluate()
        {
            //error, so leave source as-is
            StringBuilder output = new StringBuilder();
            for(int i=0; i<nodes.Count; i++)
                output.Append(nodes[i].Evaluate());
            return output.ToString();
        }

        public void Evaluate(StringBuilder output, LanguageType language, TextVariable [] arguments)
        {
            for(int i=0; i<nodes.Count; i++)
                nodes[i].Evaluate(output, language, arguments);
        }
    }
}