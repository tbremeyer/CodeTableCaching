using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace CodeTableCaching
{
    class Program
    {
        private static readonly Dictionary<Type, Dictionary<XmlAttributeOverrides, XmlSerializer>> XmlSerializerOfXmlAttributeOfType = new Dictionary<Type, Dictionary<XmlAttributeOverrides, XmlSerializer>>();
        private static readonly Dictionary<Type, XmlSerializer> XmlSerializerOfType = new Dictionary<Type, XmlSerializer>();

        // ReSharper disable once UnusedParameter.Local
        static void Main(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();
            const string fileNameDecompressed = @"ProductTable.xml";
            using (var stream = new FileStream(fileNameDecompressed, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                _ = GetDeserializedXmlObject(stream, typeof(ProductTable));
            }
            stopwatch.Stop();
            Console.WriteLine($"It took {stopwatch.ElapsedMilliseconds} milliseconds to deserialize ProductTable");
        }

        public static object GetDeserializedXmlObject(Stream xmlStream, Type type)
        {
            return GetDeserializedXmlObject(xmlStream, type, null);
        }

        public static object GetDeserializedXmlObject(Stream xmlStream, Type type, XmlAttributeOverrides xmlAttributeOverrides,
            bool checkCharacters = false)
        {
            try
            {
                XmlSerializer serializer = GetSuitableXmlSerializer(type, xmlAttributeOverrides);
                if (xmlStream.CanSeek)
                    xmlStream.Position = 0;

                XmlReader xmlReader =
                    XmlReader.Create(xmlStream, new XmlReaderSettings() { CheckCharacters = checkCharacters });
                object result = serializer.Deserialize(xmlReader);
                xmlReader.Close();
                xmlStream.Close();
                return result;
            }
            catch (Exception e)
            {
                throw new Exception("GetDeserializedXMLObject failed.", e);
            }
        }

        public static XmlSerializer GetSuitableXmlSerializer(Type type, XmlAttributeOverrides attributeOverrides)
        {
            XmlSerializer result;
            if (attributeOverrides != null)
            {
                Dictionary<XmlAttributeOverrides, XmlSerializer> dict;
                if (!XmlSerializerOfXmlAttributeOfType.TryGetValue(type, out dict))
                    XmlSerializerOfXmlAttributeOfType[type] = dict = new Dictionary<XmlAttributeOverrides, XmlSerializer>();

                if (!dict.TryGetValue(attributeOverrides, out result))
                    dict[attributeOverrides] = result = new XmlSerializer(type, attributeOverrides);
            }
            else
            {
                if (!XmlSerializerOfType.TryGetValue(type, out result))
                    XmlSerializerOfType[type] = result = new XmlSerializer(type);
            }
            return result;
        }

    }
}
