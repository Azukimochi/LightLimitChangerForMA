using System;

namespace io.github.azukimochi
{
    [Serializable]
    public struct TargetShaders
    {
        public static TargetShaders Everything => new TargetShaders() { IsEverything = true };
        public static TargetShaders None => new TargetShaders() { };

        public bool IsEverything;
        public string[] Targets;

        public override string ToString() => IsEverything ? "Everything" : (Targets == null || Targets.Length == 0) ? "None" : string.Join(", ", Targets);

        public bool Contains(string name)
        {
            if (IsEverything)
                return true;

            for(int i = 0; i < Targets?.Length; i++)
            {
                if (Targets[i] == name)
                    return true;
            }
            return false;
        }
    }
}
