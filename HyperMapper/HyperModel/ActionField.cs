namespace HyperMapper.HyperModel
{
    public class ActionField
    {
        public ActionField(Key key, FieldType type)
        {
            Key = key;
            Type = type;
        }

        public Key Key { get; private set; }
        public FieldType Type { get; private set; }
        public object Value { get; }

        public enum FieldType
        {
            Text,
            Password
        }
    }

    
}