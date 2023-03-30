using System.Text;

namespace translation;

internal class Translator : ITranslator
{
    public TypeDescription GetTypeName(string hextoken)
    {
        int token = int.Parse(hextoken, System.Globalization.NumberStyles.HexNumber);
        return (TypeDescription)token;
    }

    public string Translate(string rawdata)
    {
        // Parse the input rawdata string into a byte array
        byte[] bytes = Enumerable.Range(0, rawdata.Length)
                         .Where(x => x % 2 == 0)
                         .Select(x => Convert.ToByte(rawdata.Substring(x, 2), 16))
                         .ToArray();

        // Create a StringBuilder object to store the decoded result
        StringBuilder sb = new StringBuilder();

        // Decode the byte array and append the decoded values to the StringBuilder
        int offset = 0;
        while (offset < bytes.Length)
        {
            TypeDescription type = (TypeDescription)bytes[offset++];

            sb.AppendLine($"Type: {type} ({(int)type})");

            switch (type)
            {
                case TypeDescription.NullData:
                    sb.AppendLine("Value: Null");
                    break;

                case TypeDescription.Array:
                    sb.AppendLine("Value: Null");
                    break;
                case TypeDescription.Structure:
                    int structureLength = bytes[offset++];
                    int structureEndIndex = offset + structureLength;

                    while (offset < structureEndIndex)
                    {
                        TypeDescription field = (TypeDescription)bytes[offset++];

                        sb.AppendLine($"  Type: {field} ({(int)field})");

                        switch (field)
                        {
                            case TypeDescription.Boolean:
                                bool value = bytes[offset++] != 0;
                                sb.AppendLine($"  Value: {value}");
                                break;

                            case TypeDescription.BitString:
                                int bitStringLength = bytes[offset++];
                                int numBytes = (bitStringLength + 7) / 8;
                                byte[] bitStringData = new byte[numBytes];
                                Array.Copy(bytes, offset, bitStringData, 0, numBytes);
                                string bitString = "";
                                for (int i = 0; i < bitStringLength; i++)
                                {
                                    byte b = bitStringData[i / 8];
                                    int bitIndex = 7 - (i % 8);
                                    bitString += ((b >> bitIndex) & 1) == 1 ? "1" : "0";
                                }
                                sb.AppendLine($"  Value: {bitString}");
                                offset += numBytes;
                                break;


                            case TypeDescription.OctetString:
                                int octetStringLength = bytes[offset++];
                                byte[] octetStringData = new byte[octetStringLength];
                                Array.Copy(bytes, offset, octetStringData, 0, octetStringLength);
                                string octetString = BitConverter.ToString(octetStringData).Replace("-", "");
                                sb.AppendLine($"  Value: {octetString}");
                                offset += octetStringLength;
                                break;

                            case TypeDescription.VisibleString:
                                int visibleStringLength = bytes[offset++];
                                byte[] visibleStringData = new byte[visibleStringLength];
                                Array.Copy(bytes, offset, visibleStringData, 0, visibleStringLength);
                                string visibleString = Encoding.ASCII.GetString(visibleStringData);
                                sb.AppendLine($"  Value: {visibleString}");
                                offset += visibleStringLength;
                                break;

                            case TypeDescription.CompactArray:
                                int compactArrayLength = bytes[offset++];
                                sb.AppendLine($"  Value: {compactArrayLength} items");
                                for (int i = 0; i < compactArrayLength; i++)
                                {
                                    byte compactArrayItem = bytes[offset++];
                                    sb.AppendLine($"    Item {i}: {compactArrayItem}");
                                }
                                break;
                            case TypeDescription.Enum:
                                int length = bytes[offset++];

                                if (offset + length <= bytes.Length)
                                {
                                    if (length == 0)
                                    {
                                        sb.AppendLine("Value: Empty");
                                    }
                                    else
                                    {
                                        byte[] data = new byte[length];
                                        Array.Copy(bytes, offset, data, 0, length);

                                        sb.AppendLine($"Value: {BitConverter.ToString(data).Replace("-", "")}");
                                        offset += length;
                                    }
                                }
                                else
                                {
                                    sb.AppendLine($"Value: Invalid data length ({length})");
                                }
                                break;

                            case TypeDescription.Integer:
                                sbyte signedByte = (sbyte)bytes[offset];
                                sb.AppendLine($"Value: {signedByte}");
                                offset += sizeof(sbyte);
                                break;

                            case TypeDescription.Long:
                            case TypeDescription.LongUnsigned:
                                sb.AppendLine($"Value: {BitConverter.ToInt16(bytes, offset)}");
                                offset += sizeof(short);
                                break;

                            case TypeDescription.Unsigned:
                            case TypeDescription.DoubleLongUnsigned:
                            case TypeDescription.Float:
                                sb.AppendLine($"Value: {BitConverter.ToUInt16(bytes, offset)}");
                                offset += sizeof(ushort);
                                break;

                            case TypeDescription.DoubleLong:
                            case TypeDescription.Long64:
                                sb.AppendLine($"Value: {BitConverter.ToInt32(bytes, offset)}");
                                offset += sizeof(int);
                                break;

                            case TypeDescription.Long64Unsigned:
                                sb.AppendLine($"Value: {BitConverter.ToUInt32(bytes, offset)}");
                                offset += sizeof(uint);
                                break;

                            case TypeDescription.DateTime:
                                sb.AppendLine($"Value: {DateTime.FromOADate(BitConverter.ToDouble(bytes, offset))}");
                                offset += sizeof(double);
                                break;

                            case TypeDescription.Date:
                                sb.AppendLine($"Value: {DateTime.FromOADate(BitConverter.ToInt32(bytes, offset))}");
                                offset += sizeof(int);
                                break;

                            case TypeDescription.Time:
                                sb.AppendLine($"Value: {TimeSpan.FromTicks(BitConverter.ToInt32(bytes, offset) * TimeSpan.TicksPerMillisecond)}");
                                offset += sizeof(int);
                                break;

                            default:
                                sb.AppendLine("Value: Unknown");
                                break;
                        }
                    }
                        break;
            }
        }

        return sb.ToString();
    }

}
