namespace MPEG.PSI
{
    public abstract class Descriptor
    {
        public abstract byte DescriptorTag { get; }
        public abstract byte DescriptorLength { get; }
    }
}
