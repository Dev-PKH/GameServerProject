using System.Xml;

namespace PacketGenerator
{
    class Program
    {
        const string PDLPath = "C:\\Users\\kanghyeon\\Desktop\\GitFoloder\\CSharp\\GameServerProject\\Server\\PacketGenerator";

        static string genPackets;

        static void Main(string[] args)
        {
            XmlReaderSettings settings = new()
            {
                IgnoreComments= true, // 주석 무시
                IgnoreWhitespace = true // 스페이스바 무시
            };

            // 작업이 끝날 때 Dispose를 실행
            using (XmlReader xmlReader = XmlReader.Create(Path.Combine(PDLPath, "PDL.xml"), settings))
            {
                xmlReader.MoveToContent();

                while(xmlReader.Read())
                {
                    if (xmlReader.Depth == 1 && xmlReader.NodeType == XmlNodeType.Element)
                        ParsePacket(xmlReader);

                    //Console.WriteLine(xmlReader.Name + " / " + xmlReader["name"]);
                }

                File.WriteAllText("GenePacket.cs", genPackets);
            }
        }

        public static void ParsePacket(XmlReader reader)
        {
            // 해당 deptp의 이름과 다를 때
            if (reader.Name.ToLower() != "packet")
            {
                Console.WriteLine("Invalid Packet Node");
                return;
            }

            string packetName = reader["name"];
            if(string.IsNullOrEmpty(packetName))
            {
                Console.WriteLine("Packet name null");
                return;
            }

            Tuple<string,string,string> t = ParseMembers(reader);
            genPackets += string.Format(PacketFormat.packetFormat,
                packetName, t.Item1, t.Item2, t.Item3);
        }

        // {1} 멤버 변수들
        // {2} 멤버 변수 Read
        // {3} 멤버 변수 rite
        public static Tuple<string, string, string> ParseMembers(XmlReader reader)
        {
            string packetName = reader["name"];

            string memberCode = "";
            string readCode = "";
            string writeCode = "";

            int depth = reader.Depth + 1;
            while(reader.Read())
            {
                // 자식 depth가 아닐 때
                if (reader.Depth != depth)
                    break;

                string memberName = reader["name"];
                if(string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("Member name null");
                    return null;
                }

                // 이미 채운 값에는 enter처리
                if (string.IsNullOrEmpty(memberCode) == false)
                    memberCode += Environment.NewLine;

                if (string.IsNullOrEmpty(readCode) == false)
                    readCode += Environment.NewLine;

                if (string.IsNullOrEmpty(writeCode) == false)
                    writeCode += Environment.NewLine;

                string memberType = reader.Name.ToLower();
                switch(memberType)
                {
                    case "bool":
                    case "byte":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readFormat, memberName, ToMemberType(memberType), memberType);
                        writeCode += string.Format(PacketFormat.writeFormat, memberName, memberType);
                        break;
                    case "string":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readStringFormat, memberName);
                        writeCode += string.Format(PacketFormat.writeStringFormat, memberName);
                        break;
                    case "list":
                        Tuple<string, string, string> t = ParseList(reader);
                        memberCode += t.Item1;
                        readCode += t.Item2;
                        writeCode += t.Item3;
                        break;
                    default:
                        break;
                }
            }

            memberCode = memberCode.Replace("\n", "\n\t");
            readCode = readCode.Replace("\n", "\n\t\t");
            writeCode = writeCode.Replace("\n", "\n\t\t");

            return new(memberCode, readCode, writeCode);
        }

        public static Tuple<string, string, string> ParseList(XmlReader reader)
        {
            string listName = reader["name"];
            if(string.IsNullOrEmpty(listName))
            {
                Console.WriteLine("List without name");
                return null;
            }

            Tuple<string, string, string> t = ParseMembers(reader);
            string merberCode = string.Format(PacketFormat.memberListFormat,
                FirstCharToUpper(listName),
                FirstCharToLower(listName),
                t.Item1, t.Item2, t.Item3);

            string readCode = string.Format(PacketFormat.readListFormat,
                FirstCharToUpper(listName),
                FirstCharToLower(listName));

            string writeCode = string.Format(PacketFormat.writeListFormat,
                FirstCharToUpper(listName),
                FirstCharToLower(listName));

            return new(merberCode, readCode, writeCode);
        }

        public static string ToMemberType(string memberType)
        {
            switch(memberType)
            {
                case "bool":
                    return "ToBoolean";
                case "short":
                    return "ToInt16";
                case "ushort":
                    return "ToUInt16";
                case "int":
                    return "ToInt32";
                case "long":
                    return "ToInt64";
                case "float":
                    return "ToSingle";
                case "double":
                    return "ToDouble";
                default:
                    return "";
            }
        }

        public static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToUpper() + input.Substring(1);
        }

        public static string FirstCharToLower(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToLower() + input.Substring(1);
        }
    }
}