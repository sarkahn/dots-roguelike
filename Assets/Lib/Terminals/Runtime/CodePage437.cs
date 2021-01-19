using Unity.Collections;

namespace Sark.Terminals
{
    public static class CodePage437
    {
        public static NativeArray<byte> StringToCP437(string str, Allocator allocator)
        {
            NativeArray<byte> bytes = new NativeArray<byte>(str.Length, allocator);

            for (int i = 0; i < str.Length; ++i)
                bytes[i] = ToCP437(str[i]);

            return bytes;
        }

        //public static string CP437ToString(NativeArray<byte> bytes)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    for (int i = 0; i < bytes.Length; ++i)
        //        sb.Append(ToChar(bytes[i]));
        //    return sb.ToString();
        //}

        public static byte[] StringToCP437Alloc(string str)
        {
            var bytes = new byte[str.Length];
            for (int i = 0; i < str.Length; ++i)
                bytes[i] = ToCP437(str[i]);

            return bytes;
        }

        public static byte ToCP437(char c)
        {
            switch (c)
            {
                case '☺': return 1;
                case '☻': return 2;
                case '♥': return 3;
                case '♦': return 4;
                case '♣': return 5;
                case '♠': return 6;
                case '•': return 7;
                case '◘': return 8;
                case '○': return 9;
                case '◙': return 10;
                case '♂': return 11;
                case '♀': return 12;
                case '♪': return 13;
                case '♫': return 14;
                case '☼': return 15;
                case '►': return 16;
                case '◄': return 17;

                case '↕': return 18;
                case '‼': return 19;
                case '¶': return 20;
                case '§': return 21;
                case '▬': return 22;
                case '↨': return 23;
                case '↑': return 24;
                case '↓': return 25;
                case '→': return 26;
                case '←': return 27;
                case '∟': return 28;
                case '↔': return 29;
                case '▲': return 30;
                case '▼': return 31;
                case ' ': return 32;
                case '!': return 33;

                case '"': return 34;
                case '#': return 35;
                case '$': return 36;
                case '%': return 37;
                case '&': return 38;
                case '\'': return 39;
                case '(': return 40;
                case ')': return 41;
                case '*': return 42;
                case '+': return 43;
                case ',': return 44;
                case '-': return 45;
                case '.': return 46;
                case '/': return 47;
                case '0': return 48;
                case '1': return 49;

                case '2': return 50;
                case '3': return 51;
                case '4': return 52;
                case '5': return 53;
                case '6': return 54;
                case '7': return 55;
                case '8': return 56;
                case '9': return 57;
                case ':': return 58;
                case ';': return 59;
                case '<': return 60;
                case '=': return 61;
                case '>': return 62;
                case '?': return 63;
                case '@': return 64;
                case 'A': return 65;

                case 'B': return 66;
                case 'C': return 67;
                case 'D': return 68;
                case 'E': return 69;
                case 'F': return 70;
                case 'G': return 71;
                case 'H': return 72;
                case 'I': return 73;
                case 'J': return 74;
                case 'K': return 75;
                case 'L': return 76;
                case 'M': return 77;
                case 'N': return 78;
                case 'O': return 79;
                case 'P': return 80;
                case 'Q': return 81;

                case 'R': return 82;
                case 'S': return 83;
                case 'T': return 84;
                case 'U': return 85;
                case 'V': return 86;
                case 'W': return 87;
                case 'X': return 88;
                case 'Y': return 89;
                case 'Z': return 90;
                case '[': return 91;
                case '\\': return 92;
                case ']': return 93;
                case '^': return 94;
                case '_': return 95;
                case '`': return 96;
                case 'a': return 97;

                case 'b': return 98;
                case 'c': return 99;
                case 'd': return 100;
                case 'e': return 101;
                case 'f': return 102;
                case 'g': return 103;
                case 'h': return 104;
                case 'i': return 105;
                case 'j': return 106;
                case 'k': return 107;
                case 'l': return 108;
                case 'm': return 109;
                case 'n': return 110;
                case 'o': return 111;
                case 'p': return 112;
                case 'q': return 113;

                case 'r': return 114;
                case 's': return 115;
                case 't': return 116;
                case 'u': return 117;
                case 'v': return 118;
                case 'w': return 119;
                case 'x': return 120;
                case 'y': return 121;
                case 'z': return 122;
                case '{': return 123;
                case '|': return 124;
                case '}': return 125;
                case '~': return 126;
                case '⌂': return 127;
                case 'Ç': return 128;
                case 'ü': return 129;

                case 'é': return 130;
                case 'â': return 131;
                case 'ä': return 132;
                case 'à': return 133;
                case 'å': return 134;
                case 'ç': return 135;
                case 'ê': return 136;
                case 'ë': return 137;
                case 'è': return 138;
                case 'ï': return 139;
                case 'î': return 140;
                case 'ì': return 141;
                case 'Ä': return 142;
                case 'Å': return 143;
                case 'É': return 144;
                case 'æ': return 145;

                case 'Æ': return 146;
                case 'ô': return 147;
                case 'ö': return 148;
                case 'ò': return 149;
                case 'û': return 150;
                case 'ù': return 151;
                case 'ÿ': return 152;
                case 'Ö': return 153;
                case 'Ü': return 154;
                case '¢': return 155;
                case '£': return 156;
                case '¥': return 157;
                case '₧': return 158;
                case 'ƒ': return 159;
                case 'á': return 160;
                case 'í': return 161;

                case 'ó': return 162;
                case 'ú': return 163;
                case 'ñ': return 164;
                case 'Ñ': return 165;
                case 'ª': return 166;
                case 'º': return 167;
                case '¿': return 168;
                case '⌐': return 169;
                case '¬': return 170;
                case '½': return 171;
                case '¼': return 172;
                case '¡': return 173;
                case '«': return 174;
                case '»': return 175;
                case '░': return 176;
                case '▒': return 177;

                case '▓': return 178;
                case '│': return 179;
                case '┤': return 180;
                case '╡': return 181;
                case '╢': return 182;
                case '╖': return 183;
                case '╕': return 184;
                case '╣': return 185;
                case '║': return 186;
                case '╗': return 187;
                case '╝': return 188;
                case '╜': return 189;
                case '╛': return 190;
                case '┐': return 191;
                case '└': return 192;
                case '┴': return 193;

                case '┬': return 194;
                case '├': return 195;
                case '─': return 196;
                case '┼': return 197;
                case '╞': return 198;
                case '╟': return 199;
                case '╚': return 200;
                case '╔': return 201;
                case '╩': return 202;
                case '╦': return 203;
                case '╠': return 204;
                case '═': return 205;
                case '╬': return 206;
                case '╧': return 207;
                case '╨': return 208;
                case '╤': return 209;

                case '╥': return 210;
                case '╙': return 211;
                case '╘': return 212;
                case '╒': return 213;
                case '╓': return 214;
                case '╫': return 215;
                case '╪': return 216;
                case '┘': return 217;
                case '┌': return 218;
                case '█': return 219;
                case '▄': return 220;
                case '▌': return 221;
                case '▐': return 222;
                case '▀': return 223;
                case 'α': return 224;
                case 'ß': return 225;

                case 'Γ': return 226;
                case 'π': return 227;
                case 'Σ': return 228;
                case 'σ': return 229;
                case 'µ': return 230;
                case 'τ': return 231;
                case 'Φ': return 232;
                case 'Θ': return 233;
                case 'Ω': return 234;
                case 'δ': return 235;
                case '∞': return 236;
                case 'φ': return 237;
                case 'ε': return 238;
                case '∩': return 239;
                case '≡': return 240;
                case '±': return 241;

                case '≥': return 242;
                case '≤': return 243;
                case '⌠': return 244;
                case '⌡': return 245;
                case '÷': return 246;
                case '≈': return 247;
                case '°': return 248;
                case '∙': return 249;
                case '·': return 250;
                case '√': return 251;
                case 'ⁿ': return 252;
                case '²': return 253;
                case '■': return 254;

                default: return 0;
            }
        }

        public static char ToChar(byte c)
        {
            switch (c)
            {

                case 1: return '☺';
                case 2: return '☻';
                case 3: return '♥';
                case 4: return '♦';
                case 5: return '♣';
                case 6: return '♠';
                case 7: return '•';
                case 8: return '◘';
                case 9: return '○';
                case 10: return '◙';
                case 11: return '♂';
                case 12: return '♀';
                case 13: return '♪';
                case 14: return '♫';
                case 15: return '☼';

                case 16: return '►';
                case 17: return '◄';
                case 18: return '↕';
                case 19: return '‼';
                case 20: return '¶';
                case 21: return '§';
                case 22: return '▬';
                case 23: return '↨';
                case 24: return '↑';
                case 25: return '↓';
                case 26: return '→';
                case 27: return '←';
                case 28: return '∟';
                case 29: return '↔';
                case 30: return '▲';
                case 31: return '▼';

                case 32: return ' ';
                case 33: return '!';
                case 34: return '"';
                case 35: return '#';
                case 36: return '$';
                case 37: return '%';
                case 38: return '&';
                case 39: return '\'';
                case 40: return '(';
                case 41: return ')';
                case 42: return '*';
                case 43: return '+';
                case 44: return ',';
                case 45: return '-';
                case 46: return '.';
                case 47: return '/';

                case 48: return '0';
                case 49: return '1';
                case 50: return '2';
                case 51: return '3';
                case 52: return '4';
                case 53: return '5';
                case 54: return '6';
                case 55: return '7';
                case 56: return '8';
                case 57: return '9';
                case 58: return ':';
                case 59: return ';';
                case 60: return '<';
                case 61: return '=';
                case 62: return '>';
                case 63: return '?';

                case 64: return '@';
                case 65: return 'A';
                case 66: return 'B';
                case 67: return 'C';
                case 68: return 'D';
                case 69: return 'E';
                case 70: return 'F';
                case 71: return 'G';
                case 72: return 'H';
                case 73: return 'I';
                case 74: return 'J';
                case 75: return 'K';
                case 76: return 'L';
                case 77: return 'M';
                case 78: return 'N';
                case 79: return 'O';

                case 80: return 'P';
                case 81: return 'Q';
                case 82: return 'R';
                case 83: return 'S';
                case 84: return 'T';
                case 85: return 'U';
                case 86: return 'V';
                case 87: return 'W';
                case 88: return 'X';
                case 89: return 'Y';
                case 90: return 'Z';
                case 91: return '[';
                case 92: return '\\';
                case 93: return ']';
                case 94: return '^';
                case 95: return '_';

                case 96: return '`';
                case 97: return 'a';
                case 98: return 'b';
                case 99: return 'c';
                case 100: return 'd';
                case 101: return 'e';
                case 102: return 'f';
                case 103: return 'g';
                case 104: return 'h';
                case 105: return 'i';
                case 106: return 'j';
                case 107: return 'k';
                case 108: return 'l';
                case 109: return 'm';
                case 110: return 'n';
                case 111: return 'o';

                case 112: return 'p';
                case 113: return 'q';
                case 114: return 'r';
                case 115: return 's';
                case 116: return 't';
                case 117: return 'u';
                case 118: return 'v';
                case 119: return 'w';
                case 120: return 'x';
                case 121: return 'y';
                case 122: return 'z';
                case 123: return '{';
                case 124: return '|';
                case 125: return '}';
                case 126: return '~';
                case 127: return '⌂';

                case 128: return 'Ç';
                case 129: return 'ü';
                case 130: return 'é';
                case 131: return 'â';
                case 132: return 'ä';
                case 133: return 'à';
                case 134: return 'å';
                case 135: return 'ç';
                case 136: return 'ê';
                case 137: return 'ë';
                case 138: return 'è';
                case 139: return 'ï';
                case 140: return 'î';
                case 141: return 'ì';
                case 142: return 'Ä';
                case 143: return 'Å';

                case 144: return 'É';
                case 145: return 'æ';
                case 146: return 'Æ';
                case 147: return 'ô';
                case 148: return 'ö';
                case 149: return 'ò';
                case 150: return 'û';
                case 151: return 'ù';
                case 152: return 'ÿ';
                case 153: return 'Ö';
                case 154: return 'Ü';
                case 155: return '¢';
                case 156: return '£';
                case 157: return '¥';
                case 158: return '₧';
                case 159: return 'ƒ';

                case 160: return 'á';
                case 161: return 'í';
                case 162: return 'ó';
                case 163: return 'ú';
                case 164: return 'ñ';
                case 165: return 'Ñ';
                case 166: return 'ª';
                case 167: return 'º';
                case 168: return '¿';
                case 169: return '⌐';
                case 170: return '¬';
                case 171: return '½';
                case 172: return '¼';
                case 173: return '¡';
                case 174: return '«';
                case 175: return '»';

                case 176: return '░';
                case 177: return '▒';
                case 178: return '▓';
                case 179: return '│';
                case 180: return '┤';
                case 181: return '╡';
                case 182: return '╢';
                case 183: return '╖';
                case 184: return '╕';
                case 185: return '╣';
                case 186: return '║';
                case 187: return '╗';
                case 188: return '╝';
                case 189: return '╜';
                case 190: return '╛';
                case 191: return '┐';

                case 192: return '└';
                case 193: return '┴';
                case 194: return '┬';
                case 195: return '├';
                case 196: return '─';
                case 197: return '┼';
                case 198: return '╞';
                case 199: return '╟';
                case 200: return '╚';
                case 201: return '╔';
                case 202: return '╩';
                case 203: return '╦';
                case 204: return '╠';
                case 205: return '═';
                case 206: return '╬';
                case 207: return '╧';

                case 208: return '╨';
                case 209: return '╤';
                case 210: return '╥';
                case 211: return '╙';
                case 212: return '╘';
                case 213: return '╒';
                case 214: return '╓';
                case 215: return '╫';
                case 216: return '╪';
                case 217: return '┘';
                case 218: return '┌';
                case 219: return '█';
                case 220: return '▄';
                case 221: return '▌';
                case 222: return '▐';
                case 223: return '▀';

                case 224: return 'α';
                case 225: return 'ß';
                case 226: return 'Γ';
                case 227: return 'π';
                case 228: return 'Σ';
                case 229: return 'σ';
                case 230: return 'µ';
                case 231: return 'τ';
                case 232: return 'Φ';
                case 233: return 'Θ';
                case 234: return 'Ω';
                case 235: return 'δ';
                case 236: return '∞';
                case 237: return 'φ';
                case 238: return 'ε';
                case 239: return '∩';

                case 240: return '≡';
                case 241: return '±';
                case 242: return '≥';
                case 243: return '≤';
                case 244: return '⌠';
                case 245: return '⌡';
                case 246: return '÷';
                case 247: return '≈';
                case 248: return '°';
                case 249: return '∙';
                case 250: return '·';
                case 251: return '√';
                case 252: return 'ⁿ';
                case 253: return '²';
                case 254: return '■';

                default: return ' ';
            }

        }
    }

}