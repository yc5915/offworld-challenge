using System;
using System.IO;
using System.Collections.Generic;

namespace Offworld.SystemCore
{
    public class MJSONwriter : IDisposable
    {
        private StreamWriter writer;
        private int numTabs = 1;
        private Stack<bool> firstLineStack = new Stack<bool>();
        

        public MJSONwriter(string name)
        {
            writer = new StreamWriter(name);
            if (writer != null)
                writer.WriteLine("{");

            firstLineStack.Push(true);
        }

        private void WriteTabs()
        {
            for (int i = 0; i < numTabs; i++)
                writer.Write("\t");
        }

        private void WriteCommaIfNeeded()
        {
            if (firstLineStack.Peek() == false)
                writer.WriteLine(",");
        }

        private void WriteCommaSpaceIfNeeded()
        {
            if (firstLineStack.Peek() == false)
                writer.Write(", ");
        }

        private void WriteCommaAndNewLine()
        {
            if (firstLineStack.Peek() == false)
                writer.Write(",");

            writer.WriteLine();
        }

        private void WriteNewLineIfNeeded()
        {
            if (firstLineStack.Peek() == false)
                writer.WriteLine();
        }

        private void NotFirstLine()
        {
            firstLineStack.Pop();
            firstLineStack.Push(false);
        }

        public void StartNamedGroup(string name)
        {
            WriteCommaIfNeeded();
            WriteTabs();
            writer.WriteLine("\"" + name + "\" : {");

            numTabs++;
            firstLineStack.Push(true);
        }

        public void StartObject()
        {
            WriteCommaAndNewLine();
            WriteTabs();
            writer.WriteLine("{");

            numTabs++;
            firstLineStack.Push(true);
        }

        public void EndObject()
        {
            WriteNewLineIfNeeded();
            firstLineStack.Pop();

            if (numTabs > 1)
                numTabs--;

            WriteTabs();
            writer.Write("}");
            NotFirstLine();
        }

        public void StartArray(string name)
        {
            WriteCommaAndNewLine();
            WriteTabs();
            writer.Write("\"" + name + "\" : [ ");

            numTabs++;

            firstLineStack.Push(true);
        }

        public void StartArray()
        {
            WriteCommaAndNewLine();
            WriteTabs();
            writer.Write("[ ");

            numTabs++;

            NotFirstLine();
            firstLineStack.Push(true);
        }

        // Elements are written on the same line
        public void WriteElement( int value )
        {
            WriteCommaSpaceIfNeeded();
            writer.Write(value);
            NotFirstLine();
        }

        // Elements are written on the same line
        public void WriteElement(string name)
        {
            WriteCommaSpaceIfNeeded();
            writer.Write("\"" + name + "\"");
            NotFirstLine();
        }

        public void WriteNamedElement(string name, object value)
        {
            WriteCommaIfNeeded();
            WriteTabs();
            writer.Write("\"" + name + "\" : " + value.ToString() );
            NotFirstLine();
        }

        public void EndArray( bool innerArray, bool notLast)
        {
            if (numTabs > 1)
                numTabs--;

            if (!innerArray)
            {
                WriteNewLineIfNeeded();
                WriteTabs();
                writer.Write("]");
            }
            else
                writer.Write(" ]");

            firstLineStack.Pop();
            NotFirstLine();
        }

        public void WriteValue(string name, string value)
        {
            WriteCommaIfNeeded();
            WriteTabs();

            writer.Write("\"" + name + "\" : \"" + value + "\"");
            NotFirstLine();
        }

        public void WriteValue(string name, object value)
        {
            if (firstLineStack.Peek() == false)
                writer.WriteLine(",");

            WriteTabs();
            writer.Write("\"" + name + "\" : " + value.ToString());
            NotFirstLine();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (writer != null)
                {
                    WriteNewLineIfNeeded();
                    firstLineStack.Pop();

                    writer.WriteLine("}");
                    writer.Close();
                }
            }
        }
    }
}
