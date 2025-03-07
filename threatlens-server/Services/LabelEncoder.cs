namespace threatlens_server.Services
{
    public class LabelEncoder
    {
        private readonly Dictionary<string, int> _encodingDictionary;
        private int _nextIndex;

        public LabelEncoder()
        {
            _encodingDictionary = new Dictionary<string, int>();
            _nextIndex = 0;
        }

        public int Encode(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            if (!_encodingDictionary.ContainsKey(value))
            {
                _encodingDictionary[value] = _nextIndex++;
            }
            return _encodingDictionary[value];
        }
    }
}
