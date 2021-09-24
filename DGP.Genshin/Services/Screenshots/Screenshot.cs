namespace DGP.Genshin.Services.Screenshots
{
    public class Screenshot
    {
        public Screenshot(string path)
        {
            this.Path = path;
        }
        public string Path { get; set; }
        public string Name { get; set; }
    }
}
