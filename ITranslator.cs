namespace translation;

public interface ITranslator
{
    TypeDescription GetTypeName(string hextoken);
    String Translate(string rawdata);
}
