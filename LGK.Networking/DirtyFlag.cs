// See LICENSE file in the root directory
//

namespace LGK.Networking
{
    public static class DirtyFlag
    {
        public const byte INCREMENTAL_SYNC = 0;
        public const byte INITIAL_SYNC = 1;

        public const byte ONE = 1 << 1;
        public const byte TWO = 1 << 2;
        public const byte THREE = 1 << 3;
        public const byte FOUR = 1 << 4;
        public const byte FIVE = 1 << 5;
        public const byte SIX = 1 << 6;
        public const byte SEVEN = 1 << 7;
    }
}
