namespace Dynamic_Reader.Model
{
    public class Bookmark
    {
        public int Chapter { get; set; }
        public int PositionInChapter { get; set; }
        public double PercentageRead { get; set; }
        public string Excerpt { get; set; }
    }
}
