using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Performance
{
    /// <summary>
    /// <para>Pushes the Framerate value to a Text component.</para>
    /// </summary>
    public class FpsCounter : MonoBehaviour
    {
        [Header("// Sample Groups of Data ")]
        public bool GroupSampling = true;
        public int SampleSize = 20;

        [Header("// Config ")]
        public Text TargetText;
        public int UpdateTextEvery = 1;
        public int MaxTextLength = 5;
        public bool Smoothed = true;
        public bool ForceIntResult = true;
        public bool ZeroAllocOnly = false;

        [Header("// System FPS (updates once/sec)")]
        public bool UseSystemTick = false;

        [Header("// Color Config ")]
        public bool UseColors = true;
        public Color Good = Color.green;
        public Color Okay = Color.yellow;
        public Color Bad = Color.red;
        public int OkayBelow = 60;
        public int BadBelow = 30;

        public float Framerate { get { return _fps; } }

        protected float[] FpsSamples;
        protected int SampleIndex;
        protected int TextUpdateIndex;
        private float _fps;

        private int _sysLastSysTick;
        private int _sysLastFrameRate;
        private int _sysFrameRate;
        private string _localfps;

        private static readonly string[] FpsStringMap = {
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
            "10", "11", "12", "13", "14", "15", "16", "17", "18", "19",
            "20", "21", "22", "23", "24", "25", "26", "27", "28", "29",
            "30", "31", "32", "33", "34", "35", "36", "37", "38", "39",
            "40", "41", "42", "43", "44", "45", "46", "47", "48", "49",
            "50", "51", "52", "53", "54", "55", "56", "57", "58", "59",
            "60", "61", "62", "63", "64", "65", "66", "67", "68", "69",
            "70", "71", "72", "73", "74", "75", "76", "77", "78", "79",
            "80", "81", "82", "83", "84", "85", "86", "87", "88", "89",
            "90", "91", "92", "93", "94", "95", "96", "97", "98", "99",
            "100", "101", "102", "103", "104", "105", "106", "107", "108", "109",
            "110", "111", "122", "113", "114", "115", "116", "117", "118", "119",
            "120", "121", "132", "123", "124", "125", "126", "127", "128", "129",
            "130", "131", "142", "133", "134", "135", "136", "137", "138", "139",
            "140", "141", "152", "143", "144", "145", "146", "147", "148", "149",
            "150", "151", "162", "153", "154", "155", "156", "157", "158", "159",
            "160", "161", "172", "163", "164", "165", "166", "167", "168", "169",
            "170", "171", "182", "173", "174", "175", "176", "177", "178", "179",
            "180", "181", "192", "183", "184", "185", "186", "187", "188", "189",
            "190", "191", "192", "193", "194", "195", "196", "197", "198", "199",
            "200", "201", "202", "203", "204", "205", "206", "207", "208", "209",
            "210", "211", "222", "213", "214", "215", "216", "217", "218", "219",
            "220", "221", "222", "223", "224", "225", "226", "227", "228", "229",
            "230", "231", "232", "233", "234", "235", "236", "237", "238", "239",
            "240", "241", "242", "243", "244", "245", "246", "247", "248", "249",
            "250", "251", "252", "253", "254", "255", "256", "257", "258", "259",
            "260", "261", "262", "263", "264", "265", "266", "267", "268", "269",
            "270", "271", "272", "273", "274", "275", "276", "277", "278", "279",
            "280", "281", "282", "283", "284", "285", "286", "287", "288", "289",
            "290", "291", "292", "293", "294", "295", "296", "297", "298", "299+"
        };

        protected virtual void Reset()
        {
            SampleSize = 20;
            UpdateTextEvery = 1;
            MaxTextLength = 5;
            Smoothed = true;
            UseColors = true;
            Good = Color.green;
            Okay = Color.yellow;
            Bad = Color.red;
            OkayBelow = 60;
            BadBelow = 30;
            UseSystemTick = false;
            ForceIntResult = true;
        }

        protected virtual void Start()
        {
            FpsSamples = new float[SampleSize];
            for (int i = 0; i < FpsSamples.Length; i++) FpsSamples[i] = 0.001f;
            if (!TargetText) enabled = false;
        }

        protected virtual void Update()
        {
            if (GroupSampling) Group();
            else SingleFrame();

            _localfps = ZeroAllocOnly ? FpsStringMap[Mathf.Clamp((int)_fps, 0, 299)] : _fps.ToString(CultureInfo.CurrentCulture);

            SampleIndex = SampleIndex < SampleSize - 1 ? SampleIndex + 1 : 0;
            TextUpdateIndex = TextUpdateIndex > UpdateTextEvery ? 0 : TextUpdateIndex + 1;
            if (TextUpdateIndex == UpdateTextEvery) TargetText.text = _localfps.Substring(0, _localfps.Length < 5 ? _localfps.Length : 5);

            if (!UseColors) return;
            if (_fps < BadBelow)
            {
                TargetText.color = Bad;
                return;
            }
            TargetText.color = _fps < OkayBelow ? Okay : Good;
        }

        protected virtual void SingleFrame()
        {
            _fps = UseSystemTick
                ? GetSystemFramerate()
                : Smoothed ? 1 / Time.smoothDeltaTime : 1 / Time.deltaTime;
            if (ForceIntResult) _fps = (int)_fps;
        }

        protected virtual void Group()
        {
            FpsSamples[SampleIndex] = UseSystemTick
                ? GetSystemFramerate()
                : Smoothed ? 1 / Time.smoothDeltaTime : 1 / Time.deltaTime;

            _fps = 0;
            bool loop = true;
            int i = 0;
            while (loop)
            {
                if (i == SampleSize - 1) loop = false;
                _fps += FpsSamples[i];
                i++;
            }
            _fps /= FpsSamples.Length;
            if (ForceIntResult) _fps = (int)_fps;
        }

        protected virtual int GetSystemFramerate()
        {
            if (System.Environment.TickCount - _sysLastSysTick >= 1000)
            {
                _sysLastFrameRate = _sysFrameRate;
                _sysFrameRate = 0;
                _sysLastSysTick = System.Environment.TickCount;
            }
            _sysFrameRate++;
            return _sysLastFrameRate;
        }
    }
}