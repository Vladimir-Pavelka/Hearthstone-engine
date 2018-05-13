namespace Domain
{
    public static class IdProvider
    {
        private static int _lastId;

        public static int GetNext()
        {
            _lastId += 1;
            return _lastId;
        }

        public static void Reset()
        {
            _lastId = 0;
        }
    }
}