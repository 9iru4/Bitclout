using System.IO;
using System.Xml.Serialization;

namespace Bitclout
{
    /// <summary>
    /// Для сохранения в файл
    /// </summary>
    public static class SerializeHelper
    {
        /// <summary>
        /// Сериализация
        /// </summary>
        /// <typeparam name="T">Тип обаекта</typeparam>
        /// <param name="toSerialize">Обьект</param>
        /// <returns>Строку для сохранения</returns>
        public static string Serialize<T>(this T toSerialize)
        {
            XmlSerializer xml = new XmlSerializer(typeof(T));
            StringWriter writer = new StringWriter();
            xml.Serialize(writer, toSerialize);
            return writer.ToString();
        }

        /// <summary>
        /// Десириализация
        /// </summary>
        /// <typeparam name="T">Тип обькта</typeparam>
        /// <param name="toDesirealize">Обьект</param>
        /// <returns>обьект</returns>
        public static T Desirialize<T>(this string toDesirealize)
        {
            XmlSerializer xml = new XmlSerializer(typeof(T));
            StringReader reader = new StringReader(toDesirealize);
            return (T)xml.Deserialize(reader);
        }
    }
}
