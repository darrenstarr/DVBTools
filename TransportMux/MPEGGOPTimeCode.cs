namespace TransportMux
{
    using System;

    public class MPEGGOPTimeCode
    {
	    public bool DropFrameFlag;
	    public byte Hours;
	    public byte Minutes;
	    public byte Seconds;
	    public byte Pictures;

        public bool DecodeFromGOPHeaderValue(uint input)
	    {
		    // drop_frame_flag			(1 bit)		(01)	>> 31
		    // time_code_hours			(5 bits)	(06)	>> 26
		    // time_code_minutes		(6 bits)	(12)	>> 20
		    // marker_bit				(1 bit)		(13)	>> 19
		    // time_code_seconds		(6 bits)	(19)	>> 13
		    // time_code_pictures		(6 bits)	(25)	>> 7
		    // remaining				(7 bits)	(32)	>> 0

		    DropFrameFlag = (input & 0x80000000) == 0x80000000 ? true : false;
		    Hours = (byte)((input >> 26) & 0x1F);
		    Minutes = (byte)((input >> 20) & 0x3F);
		    Seconds = (byte)((input >> 13) & 0x3F);
		    Pictures = (byte)((input >> 7) & 0x3F);

    		return true;
	    }

        public override string  ToString()
        {
            return String.Format("{0:d2}", Hours) + String.Format(":{0:d2}", Minutes) + String.Format(":{0:d2}", Seconds) + String.Format(":{0:d2}", Pictures);
        }
    }
}
