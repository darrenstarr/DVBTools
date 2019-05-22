namespace DVBToolsCommon.MPEG
{
    // ISO13818-2 Table 6-11 — time_code
    /// <summary>
    /// Implements the fields found in the Group of Pictures Header time code.
    /// </summary>
    public class TimeCode
    {
        public int dropFrameFlag;
        public int hours;
        public int minutes;
        public int seconds;
        public int pictures;
    }
}
