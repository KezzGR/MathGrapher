namespace MathGrapher.Core.Models
{
    public class GraphRecord
    {
        public int Id { get; set; }
        public string Expression { get; set; }
        public double XMin { get; set; }
        public double XMax { get; set; }
        public double Step { get; set; }
        public double? Area { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}