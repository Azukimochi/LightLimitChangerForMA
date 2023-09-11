namespace io.github.azukimochi
{
    internal readonly struct HashCode
    {
        private const int Prime1 = 1117;
        private const int Prime2 = 1777;

        private readonly int? _result;

        private HashCode(int? result) => _result = result;

        public HashCode Append<T>(T value)
        {
            int hash = _result ?? Prime1;
            hash = hash * Prime2 + value?.GetHashCode() ?? 0;
            return new HashCode(hash);
        }

        public override int GetHashCode() => _result ?? 0;
    }
}