namespace TransportMux
{
    using DVBToolsCommon;

    internal class ProgramTables
    {
        public ushort TransportStreamId = 0;
        public ushort ProgramMapPID = 0x1E0;
        public ushort ProgramNumber = 1;
        private TransportPacket pat;
        private TransportPacket pmt;

        public ulong Interval = 27000000 / 10;
        private ulong NextPatTime;
        private ulong NextPmtTime;

	    public ulong NextPacketTime
	    {
            get
            {
		        if(NextPatTime <= NextPmtTime)
			        return NextPatTime;
		        return NextPmtTime;
            }
	    }

	    public TransportPacket NextPacket()
	    {
		    if(NextPatTime <= NextPmtTime)
		    {
			    NextPatTime += Interval;
			    pat.IncrementContinuityCounter();
			    return pat;
		    }
		    NextPmtTime += Interval;
		    pmt.IncrementContinuityCounter();
		    return pmt;
	    }

	    public void GenerateProgramAssociationTable()
        {
            ByteArray output = new ByteArray();

        	// sync_byte = 0x47						(8 bits)
            output.Append((byte) 0x47);
	
	        output.EnterBitMode();

	        // transport_error_indicator = '0b'		(1 bit)
	        output.AppendBit(0);

	        // payload_unit_start_indicator = '1b'	(1 bit)
	        output.AppendBit(1);

	        // transport_priority = '0b'			(1 bit)
	        output.AppendBit(0);

	        // PID									(13 bits)
	        output.AppendBits((ushort) 0x0000, 12, 0);

	        // transport_scrambling_control = '00b'	(2 bits)
	        output.AppendBits((byte) 0x00, 1, 0);

	        // adaptation_field_control = '01b'		(2 bits) 
	        //output.appendBits((byte) 0x03, 1, 0);
	        output.AppendBits((byte) 0x01, 1, 0);

	        // continuity_counter					(4 bits)
	        output.AppendBits((byte) 0x00, 3, 0);	

	        // pointer_field
	        output.AppendBits((byte) 0x00, 7, 0);

	        long packetStart = output.length;

	        // table_id = 0x00 (program_association_section)	(8 bits)
	        output.AppendBits((byte) 0x00, 7, 0);

	        // section_syntax_indicator = 1			(1 bit)
	        output.AppendBit(1);

	        // '0'									(1 bit)
	        output.AppendBit(0);

	        // Reserved = '11'						(2 bits)
	        output.AppendBits((byte) 0x3, 1, 0);

	        // section_length						(12 bits)
	        long sectionLengthPosition = output.length;
	        output.AppendBits((ushort) 0x000, 11, 0);

	        // transport_stream_id					(16 bits)
	        output.AppendBits(TransportStreamId, 15, 0);

	        // Reserved = '11'						(2 bits)
	        output.AppendBits((byte) 0x3, 1, 0);

	        // version_number = 0x00				(5 bits)
	        output.AppendBits((byte) 0x00, 4, 0);

	        // current_next_indicator = 1			(1 bit)
	        output.AppendBit(1);

	        // section_number = 0					(8 bits)
	        output.AppendBits((byte) 0x00, 7, 0);

	        // last_section_number = 0				(8 bits)
	        output.AppendBits((byte) 0x00, 7, 0);

	        // Loop here for more than one program, not supported at the moment
	        {
		        // program_number					(16 bits)
		        output.AppendBits(ProgramNumber, 15, 0);

		        // reserved							(3 bits)
		        output.AppendBits((byte) 7, 2, 0);

		        // program_map_PID					(13 bits)
		        output.AppendBits(ProgramMapPID, 12, 0);
	        }

	        output.LeaveBitMode();

	        ushort sectionLength = (ushort) (output.length - sectionLengthPosition + 2);
            output[sectionLengthPosition] = (byte)(output[sectionLengthPosition] | ((sectionLength >> 8) & 0xF));
            output[sectionLengthPosition + 1] = (byte)(sectionLength & 0xFF);

            uint crc = CRC.Calculate(output.buffer, packetStart, output.length - packetStart);
	        output.Append(crc);

	        while(output.length < 188)
		        output.Append((byte) 0xFF);

            byte [] buffer = new byte[188];
            for(int i=0; i<188; i++)
                buffer[i] = output.buffer[i];

            pat = new TransportPacket(buffer);
        }

        public void GenerateProgramMap(ushort pid, ushort programNumber, InputStreams streams)
        {
	        ProgramMapPID = pid;
	        ProgramNumber = programNumber;

            ByteArray output = new ByteArray();

	        // sync_byte = 0x47						(8 bits)
	        output.Append((byte) 0x47);

	        output.EnterBitMode();

	        // transport_error_indicator = '0b'		(1 bit)
	        output.AppendBit(0);

	        // payload_unit_start_indicator = '1b'	(1 bit)
	        output.AppendBit(1);

	        // transport_priority = '0b'			(1 bit)
	        output.AppendBit(0);

	        // PID									(13 bits)
	        output.AppendBits(pid, 12, 0);

	        // transport_scrambling_control = '00b'	(2 bits)
	        output.AppendBits((byte) 0x00, 1, 0);

	        // adaptation_field_control = '01b'		(2 bits) 
	        output.AppendBits((byte) 0x01, 1, 0);

	        // continuity_counter					(4 bits)
	        output.AppendBits((byte) 0x00, 3, 0);

	        // pointer_field
	        output.AppendBits((byte) 0x00, 7, 0);

	        long packetStart = output.length;

	        // table_id = 0x02					(8 bits)
	        output.AppendBits((byte) 0x02, 7, 0);

	        // section_syntax_indicator	'1'		(1 bit)
	        output.AppendBit(1);

	        // '0'								(1 bit)
	        output.AppendBit(0);

	        // Reserved = '11'					(2 bits)
	        output.AppendBits((byte) 0x3, 1, 0);

	        // section_length (calculate later)	(12 bits)
	        long sectionLengthPosition = output.length;
	        output.AppendBits((ushort) 0x000, 11, 0);
        	
	        // program_number					(16 bits)
	        output.AppendBits(programNumber, 15, 0);

	        // Reserved = '11'					(2 bits)
	        output.AppendBits((byte) 0x3, 1, 0);

	        // version_number = 0x0				(5 bits)
	        output.AppendBits((byte) 0x00, 4, 0);

	        // current_next_indicator = 0x0		(1 bit)
	        output.AppendBit(1);

	        // section_number = 0x00			(8 bits)
	        output.AppendBits((byte) 0x00, 7, 0);

	        // current_section_number = 0x00	(8 bits)
	        output.AppendBits((byte) 0x00, 7, 0);

	        // Reserved = '111'					(3 bits)
	        output.AppendBits((byte) 0x7, 2, 0);

	        // PCR_ID							(13 bits)
	        output.AppendBits(streams.PcrPID, 12, 0);
        	
	        // Reserved = '1111'				(4 bits)
	        output.AppendBits((byte) 0xF, 3, 0);

	        // program_info_length = 0x00		(12 bits)
	        // - we ignore this field since it doesn't really matter
	        output.AppendBits((ushort) 0x000, 11, 0);

	        for(long i=0; i<streams.Count; i++)
		        streams[i].GenerateProgramMap(output);

	        output.LeaveBitMode();

	        ushort sectionLength = (ushort) (output.length - sectionLengthPosition + 2);
            output[sectionLengthPosition] = (byte)(output[sectionLengthPosition] | ((sectionLength >> 8) & 0xF));
            output[sectionLengthPosition + 1] = (byte)(sectionLength & 0xFF);

            uint crc = CRC.Calculate(output.buffer, packetStart, output.length - packetStart);
	        output.Append(crc);

            while (output.length < 188)
                output.Append((byte)0xFF);

            byte[] buffer = new byte[188];
            for (int i = 0; i < 188; i++)
                buffer[i] = output.buffer[i];

            pmt = new TransportPacket(buffer);

	        GenerateProgramAssociationTable();
        }
    }
}
