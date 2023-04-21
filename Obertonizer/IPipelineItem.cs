namespace Obertonizer
{
    public interface IPipelineItem
    {
        bool Enabled { get; set; }
        object Process(object input);
        Type InputType { get; }
        Type OutputType { get; }


    }
}
